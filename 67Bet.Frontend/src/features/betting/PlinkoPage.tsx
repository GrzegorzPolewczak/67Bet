import React, { useEffect, useState } from "react";
import { useSelector } from "react-redux";
import { motion, AnimatePresence } from "framer-motion";
import {
  CircleDot,
  Coins,
  RefreshCw,
  Trophy,
  Zap,
  AlertCircle,
  SlidersHorizontal,
} from "lucide-react";
import type { RootState } from "../../app/store";
import { bettingApi, walletApi } from "../../api/axios";

type RiskLevel = "Low" | "Medium" | "High";

interface PlinkoRound {
  id: string;
  batchId: string;
  stake: number;
  riskLevel: RiskLevel;
  rows: number;
  landingSlot: number;
  multiplier: number;
  payout: number;
  path: string;
  createdAt: Date;
}

interface PlinkoBatchSummary {
  id: string;
  balls: number;
  completed: number;
  totalStake: number;
  boardFee: number;
  totalPayout: number;
}

interface StoredPlinkoQueue {
  pendingRounds: Array<Omit<PlinkoRound, "createdAt"> & { createdAt: string }>;
  activeBatch: PlinkoBatchSummary | null;
  balance: number;
  freebetBalance: number;
}

const riskOptions: RiskLevel[] = ["Low", "Medium", "High"];
const riskApiValue: Record<RiskLevel, number> = { Low: 1, Medium: 2, High: 3 };
const riskLabel = (riskLevel: RiskLevel | number): RiskLevel => {
  if (riskLevel === 1) return "Low";
  if (riskLevel === 3) return "High";
  if (riskLevel === "Low" || riskLevel === "High" || riskLevel === "Medium")
    return riskLevel;
  return "Medium";
};

const calculateBoardFee = (stake: number, balls: number, rows: number) => {
  const missingRows = Math.max(0, 16 - rows);
  const feeRate = missingRows * 0.08;
  return Math.round(stake * balls * feeRate * 100) / 100;
};

const toMoney = (value: number) => Math.round(value * 100) / 100;
const plinkoQueueStorageKey = (token: string) => `67bet:plinko:queue:${token}`;

const createMultipliers = (riskLevel: RiskLevel, rows: number) => {
  const edge = riskLevel === "Low" ? 2.2 : riskLevel === "High" ? 28 : 6;
  const center = riskLevel === "Low" ? 0.55 : riskLevel === "High" ? 0 : 0.18;
  const houseEdge =
    riskLevel === "Low" ? 0.78 : riskLevel === "High" ? 0.5 : 0.62;
  const curvePower =
    riskLevel === "Low" ? 2.2 : riskLevel === "High" ? 3.6 : 2.8;

  return Array.from({ length: rows + 1 }, (_, slot) => {
    const distanceFromCenter = Math.abs(slot - rows / 2) / (rows / 2);
    const rawValue =
      center + (edge - center) * Math.pow(distanceFromCenter, curvePower);
    return Math.round(rawValue * houseEdge * 100) / 100;
  });
};

const createDemoRound = (
  stake: number,
  riskLevel: RiskLevel,
  rows: number,
  batchId: string,
): PlinkoRound => {
  const path = Array.from({ length: rows }, () =>
    Math.random() < 0.5 ? "L" : "R",
  ).join("");
  const landingSlot = [...path].filter((move) => move === "R").length;
  const multiplier = createMultipliers(riskLevel, rows)[landingSlot];
  const payout = Math.round(stake * multiplier * 100) / 100;

  return {
    id: crypto.randomUUID(),
    batchId,
    stake,
    riskLevel,
    rows,
    landingSlot,
    multiplier,
    payout,
    path,
    createdAt: new Date(),
  };
};

const getSlotClassName = (
  index: number,
  totalSlots: number,
  multiplier: number,
  isHit: boolean,
) => {
  const isHighestEdge = index === 0 || index === totalSlots - 1;
  const isSecondEdge = index === 1 || index === totalSlots - 2;
  const isBreakEvenOrSmallWin = multiplier >= 1 && multiplier <= 1.5;

  const colorClass = isHighestEdge
    ? "bg-red-600 text-white border-red-400"
    : isSecondEdge
      ? "bg-orange-500 text-dark-900 border-orange-300"
      : isBreakEvenOrSmallWin
        ? "bg-emerald-600/80 text-white border-emerald-400"
        : "bg-dark-700 text-gray-300 border-dark-600";

  const hitClass = isHit
    ? "ring-4 ring-yellow-300 shadow-[0_0_24px_rgba(234,179,8,0.65)] scale-105"
    : "";

  return `min-w-0 rounded-xl py-3 text-center text-[10px] md:text-sm font-black border-2 transition-all ${colorClass} ${hitClass}`;
};

const getApiErrorMessage = (err: any, fallback: string) =>
  err.response?.data?.error ||
  err.response?.data?.message ||
  err.response?.data ||
  err.message ||
  fallback;

const createBallAnimation = (round: PlinkoRound) => {
  let slot = 0;
  const top: Array<string | number> = [20];
  const left: Array<string | number> = ["50%"];
  const rotate = [0];
  const scale = [1];

  [...round.path].forEach((move, index) => {
    const currentRow = index + 1;
    const impactProgress = currentRow / (round.rows + 1);
    const impactOffset = slot - index / 2;
    const bounceDirection = move === "R" ? 1 : -1;
    const horizontalSpread = 42 / Math.max(1, round.rows / 2);

    top.push(`${10 + impactProgress * 68}%`);
    left.push(`${50 + impactOffset * horizontalSpread}%`);
    rotate.push(rotate[rotate.length - 1] + bounceDirection * 28);
    scale.push(0.86);

    if (move === "R") {
      slot += 1;
    }

    const exitOffset = slot - currentRow / 2;

    top.push(`${12 + impactProgress * 68}%`);
    left.push(`${50 + exitOffset * horizontalSpread}%`);
    rotate.push(rotate[rotate.length - 1] + bounceDirection * 42);
    scale.push(1.08);
  });

  top.push("88%");
  left.push(`${8 + (round.landingSlot / round.rows) * 84}%`);
  rotate.push(rotate[rotate.length - 1] + 180);
  scale.push(1);

  return { top, left, rotate, scale };
};

const PlinkoPage: React.FC = () => {
  const { isAuthenticated, token } = useSelector(
    (state: RootState) => state.auth,
  );
  const isDemoMode = !isAuthenticated;
  const [stake, setStake] = useState(10);
  const [ballCount, setBallCount] = useState(1);
  const [riskLevel, setRiskLevel] = useState<RiskLevel>("Medium");
  const [rows, setRows] = useState(16);
  const [balance, setBalance] = useState(0);
  const [freebetBalance, setFreebetBalance] = useState(0);
  const [isBuying, setIsBuying] = useState(false);
  const [droppingRounds, setDroppingRounds] = useState<PlinkoRound[]>([]);
  const [pendingRounds, setPendingRounds] = useState<PlinkoRound[]>([]);
  const [activeRound, setActiveRound] = useState<PlinkoRound | null>(null);
  const [activeBatch, setActiveBatch] = useState<PlinkoBatchSummary | null>(
    null,
  );
  const [history, setHistory] = useState<PlinkoRound[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [multipliers, setMultipliers] = useState<number[]>([]);
  const [queueRestored, setQueueRestored] = useState(false);

  const ballsInPlay = droppingRounds.length;
  const queueStorageKey =
    !isDemoMode && token ? plinkoQueueStorageKey(token) : null;
  const dropDurationMs = 2200;
  const totalStake = Math.round(stake * ballCount * 100) / 100;
  const boardFee = calculateBoardFee(stake, ballCount, rows);
  const totalCost = Math.round((totalStake + boardFee) * 100) / 100;
  const boardFeePerBall = Math.round((boardFee / ballCount) * 100) / 100;

  useEffect(() => {
    if (isDemoMode) {
      setBalance(1000);
      setFreebetBalance(0);
      setPendingRounds([]);
      setActiveBatch(null);
      setQueueRestored(true);
      setError(null);
      return;
    }

    if (!queueStorageKey) return;

    const restored = restoreQueuedRounds(queueStorageKey);
    if (restored) {
      setPendingRounds(restored.pendingRounds);
      setActiveBatch(restored.activeBatch);
      setBalance(restored.balance);
      setFreebetBalance(restored.freebetBalance);
      setQueueRestored(true);
      setError(null);
      return;
    }

    setQueueRestored(true);
    fetchWalletBalance();
  }, [isDemoMode, queueStorageKey]);

  useEffect(() => {
    if (isDemoMode || !queueStorageKey || !queueRestored) return;

    if (pendingRounds.length === 0) {
      localStorage.removeItem(queueStorageKey);
      return;
    }

    const storedQueue: StoredPlinkoQueue = {
      pendingRounds: pendingRounds.map((round) => ({
        ...round,
        createdAt: round.createdAt.toISOString(),
      })),
      activeBatch,
      balance,
      freebetBalance,
    };

    localStorage.setItem(queueStorageKey, JSON.stringify(storedQueue));
  }, [
    activeBatch,
    balance,
    freebetBalance,
    isDemoMode,
    pendingRounds,
    queueRestored,
    queueStorageKey,
  ]);

  useEffect(() => {
    fetchBoard();
  }, [riskLevel, rows, isDemoMode]);

  const fetchWalletBalance = async () => {
    if (isDemoMode) return;

    try {
      const response = await walletApi.get("/wallet/balance");
      setBalance(Number(response.data.balance || response.data.Balance || 0));
      setFreebetBalance(
        Number(response.data.freebetBalance || response.data.FreebetBalance || 0),
      );
    } catch (err: any) {
      setError(
        err.response?.data?.message ||
          err.response?.data ||
          err.message ||
          "Wallet API is not available.",
      );
    }
  };

  const fetchBoard = async () => {
    if (isDemoMode) {
      setMultipliers(createMultipliers(riskLevel, rows));
      setError(null);
      return;
    }

    try {
      const response = await bettingApi.get("/plinko/board", {
        params: { riskLevel: riskApiValue[riskLevel], rows },
      });
      setMultipliers(
        response.data.multipliers || response.data.Multipliers || [],
      );
    } catch (err: any) {
      setError(
        err.response?.data?.message ||
          err.response?.data ||
          err.message ||
          "Plinko API is not available.",
      );
    }
  };

  const mapRound = (round: any, batchId: string): PlinkoRound => ({
    id: round.id || round.Id,
    batchId,
    stake: Number(round.stake ?? round.Stake),
    riskLevel: riskLabel(round.riskLevel ?? round.RiskLevel),
    rows: Number(round.rows ?? round.Rows),
    landingSlot: Number(round.landingSlot ?? round.LandingSlot),
    multiplier: Number(round.multiplier ?? round.Multiplier),
    payout: Number(round.payout ?? round.Payout),
    path: round.path || round.Path,
    createdAt: new Date(round.createdAtUtc || round.CreatedAtUtc || new Date()),
  });

  const settleRoundPayout = async (round: PlinkoRound) => {
    if (isDemoMode) {
      setBalance((current) => toMoney(current + round.payout));
      return;
    }

    try {
      await bettingApi.post(`/plinko/${round.id}/settle`);
      setBalance((current) => toMoney(current + round.payout));
    } catch (err: any) {
      setError(
        err.response?.data?.error ||
          err.response?.data ||
          err.message ||
          "Could not settle Plinko payout.",
      );
    }
  };

  const restoreQueuedRounds = (storageKey: string) => {
    try {
      const storedValue = localStorage.getItem(storageKey);
      if (!storedValue) return null;

      const parsed = JSON.parse(storedValue) as StoredPlinkoQueue;
      if (!Array.isArray(parsed.pendingRounds) || parsed.pendingRounds.length === 0) {
        localStorage.removeItem(storageKey);
        return null;
      }

      return {
        pendingRounds: parsed.pendingRounds.map((round) => ({
          ...round,
          createdAt: new Date(round.createdAt),
        })),
        activeBatch: parsed.activeBatch,
        balance: Number(parsed.balance || 0),
        freebetBalance: Number(parsed.freebetBalance || 0),
      };
    } catch {
      localStorage.removeItem(storageKey);
      return null;
    }
  };

  const chargeDisplayedWalletForBatch = (purchasedBallCount = ballCount) => {
    if (isDemoMode) {
      setBalance((current) =>
        toMoney(
          current -
            stake * purchasedBallCount -
            calculateBoardFee(stake, purchasedBallCount, rows),
        ),
      );
      return;
    }

    const costPerBall = toMoney(stake + boardFeePerBall);

    setFreebetBalance((currentFreebet) => {
      let nextFreebet = currentFreebet;
      let cashCharge = 0;

      for (let i = 0; i < purchasedBallCount; i += 1) {
        if (nextFreebet >= costPerBall) {
          nextFreebet = toMoney(nextFreebet - costPerBall);
        } else {
          cashCharge = toMoney(cashCharge + costPerBall);
        }
      }

      if (cashCharge > 0) {
        setBalance((currentBalance) =>
          toMoney(Math.max(0, currentBalance - cashCharge)),
        );
      }

      return nextFreebet;
    });
  };

  const buyBalls = async () => {
    if (!Number.isFinite(stake) || stake <= 0) {
      setError("Stake must be greater than zero.");
      return;
    }
    if (!Number.isInteger(ballCount) || ballCount < 1 || ballCount > 25) {
      setError("Choose between 1 and 25 balls.");
      return;
    }

    setError(null);
    setIsBuying(true);
    setActiveRound(null);
    const batchId = crypto.randomUUID();

    try {
      if (isDemoMode) {
        if (totalCost > balance) {
          setError("Demo balance is too low for this batch.");
          return;
        }

        const rounds = Array.from({ length: ballCount }, () =>
          createDemoRound(stake, riskLevel, rows, batchId),
        );

        chargeDisplayedWalletForBatch();
        setActiveBatch({
          id: batchId,
          balls: ballCount,
          completed: 0,
          totalStake,
          boardFee,
          totalPayout: 0,
        });
        setPendingRounds((current) => [...current, ...rounds]);
        return;
      }

      const responses = [];
      let failedPurchase: any = null;

      for (let i = 0; i < ballCount; i += 1) {
        try {
          const response = await bettingApi.post("/plinko/play", {
            stake,
            riskLevel: riskApiValue[riskLevel],
            rows,
            boardFee: boardFeePerBall,
          });
          responses.push(response);
        } catch (err: any) {
          failedPurchase = err;
          break;
        }
      }

      if (responses.length === 0 && failedPurchase) {
        throw failedPurchase;
      }

      const purchasedBallCount = responses.length;
      const purchasedTotalStake = toMoney(stake * purchasedBallCount);
      const purchasedBoardFee = calculateBoardFee(
        stake,
        purchasedBallCount,
        rows,
      );

      setActiveBatch({
        id: batchId,
        balls: purchasedBallCount,
        completed: 0,
        totalStake: purchasedTotalStake,
        boardFee: purchasedBoardFee,
        totalPayout: 0,
      });
      setPendingRounds((current) => [
        ...current,
        ...responses.map((response) => mapRound(response.data, batchId)),
      ]);
      chargeDisplayedWalletForBatch(purchasedBallCount);

      if (failedPurchase) {
        setError(
          `Bought ${purchasedBallCount} of ${ballCount} balls. ${getApiErrorMessage(
            failedPurchase,
            "The next ball was blocked.",
          )}`,
        );
      }
    } catch (err: any) {
      setError(getApiErrorMessage(err, "Could not buy Plinko balls."));
    } finally {
      setIsBuying(false);
    }
  };

  const startDrop = (round: PlinkoRound, delayMs = 0) => {
    setError(null);

    window.setTimeout(() => {
      setDroppingRounds((current) => [...current, round]);
    }, delayMs);

    window.setTimeout(() => {
      setDroppingRounds((current) =>
        current.filter((item) => item.id !== round.id),
      );
      setActiveRound(round);
      setActiveBatch((current) => {
        if (!current || current.id !== round.batchId) return current;

        return {
          ...current,
          completed: current.completed + 1,
          totalPayout:
            Math.round((current.totalPayout + round.payout) * 100) / 100,
        };
      });
      setHistory((current) => [round, ...current].slice(0, 8));
      void settleRoundPayout(round);
    }, delayMs + dropDurationMs);
  };

  const dropNextBall = () => {
    if (pendingRounds.length === 0) return;

    const [round, ...remainingRounds] = pendingRounds;
    setPendingRounds(remainingRounds);
    startDrop(round);
  };

  const dropAllBalls = () => {
    if (pendingRounds.length === 0) return;

    pendingRounds.forEach((round, index) => startDrop(round, index * 180));
    setPendingRounds([]);
  };

  return (
    <div className="max-w-[1600px] w-full px-4 xl:px-8 mx-auto space-y-10">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-6 bg-dark-800 p-8 rounded-3xl border border-dark-700 shadow-2xl">
        <div>
          <div className="flex items-center gap-4 mb-3">
            <div className="w-12 h-12 rounded-2xl bg-cyan-500/20 flex items-center justify-center">
              <CircleDot className="w-7 h-7 text-cyan-400" />
            </div>
            <h1 className="text-3xl md:text-4xl font-black text-white italic tracking-tight">
              PLINKO
            </h1>
          </div>
          <p className="text-gray-400 text-base">
            {isDemoMode
              ? "Try Plinko without logging in using a demo balance. Real wallet payouts are enabled after login."
              : "Drop the ball through the board, hit a multiplier and collect the payout from your wallet."}
          </p>
        </div>

        <div className="flex items-center gap-4 bg-dark-900 px-6 py-4 rounded-2xl border border-dark-700">
          <Coins className="w-6 h-6 text-accent-success" />
          <div>
            <p className="text-[10px] text-gray-500 font-black uppercase tracking-widest">
              {isDemoMode ? "Demo Balance" : "Wallet Balance"}
            </p>
            <p className="text-xl font-black text-white">
              {balance.toFixed(2)} PLN
            </p>
            {!isDemoMode && freebetBalance > 0 && (
              <p className="mt-1 text-xs font-black text-cyan-300 uppercase tracking-wider">
                Freebet {freebetBalance.toFixed(2)} PLN
              </p>
            )}
          </div>
        </div>
      </div>

      {error && (
        <div className="bg-red-500/10 border border-red-500/50 rounded-2xl p-5 flex items-center gap-4 text-red-500">
          <AlertCircle className="w-6 h-6 flex-shrink-0" />
          <p className="font-medium">{error}</p>
        </div>
      )}

      <div className="grid grid-cols-1 xl:grid-cols-[380px_minmax(0,1fr)] gap-8">
        <div className="bg-dark-800 rounded-3xl border border-dark-700 p-6 shadow-[0_10px_40px_rgba(0,0,0,0.5)]">
          <div className="flex items-center gap-3 mb-6">
            <SlidersHorizontal className="w-6 h-6 text-cyan-400" />
            <h2 className="text-2xl font-black text-white">Game Setup</h2>
          </div>

          <label className="block text-xs font-black text-gray-500 uppercase tracking-widest mb-2">
            Stake
          </label>
          <input
            type="number"
            min={1}
            step={0.01}
            value={stake}
            onChange={(event) => setStake(Number(event.target.value))}
            className="w-full bg-dark-900 border border-dark-600 rounded-2xl px-5 py-4 text-white text-lg font-bold focus:outline-none focus:border-cyan-500 transition-colors"
          />

          <label className="block text-xs font-black text-gray-500 uppercase tracking-widest mt-6 mb-2">
            Balls
          </label>
          <div className="grid grid-cols-[1fr_auto] gap-3">
            <input
              type="range"
              min={1}
              max={25}
              value={ballCount}
              onChange={(event) => setBallCount(Number(event.target.value))}
              className="w-full accent-cyan-500"
            />
            <div className="min-w-16 h-11 px-4 rounded-2xl bg-dark-900 border border-dark-600 flex items-center justify-center text-cyan-400 font-black">
              {ballCount}
            </div>
          </div>
          <div className="flex justify-between text-xs text-gray-500 font-bold mt-2">
            <span>Total stake</span>
            <span>{totalStake.toFixed(2)} PLN</span>
          </div>

          <label className="block text-xs font-black text-gray-500 uppercase tracking-widest mt-6 mb-2">
            Risk
          </label>
          <div className="grid grid-cols-3 gap-3">
            {riskOptions.map((option) => (
              <button
                key={option}
                onClick={() => setRiskLevel(option)}
                className={`py-3 rounded-2xl text-sm font-black transition-all border-2 ${
                  riskLevel === option
                    ? "bg-cyan-500 text-dark-900 border-cyan-400"
                    : "bg-dark-900 text-gray-300 border-dark-600 hover:border-cyan-500/50"
                }`}
              >
                {option}
              </button>
            ))}
          </div>

          <label className="block text-xs font-black text-gray-500 uppercase tracking-widest mt-6 mb-2">
            Rows
          </label>
          <input
            type="range"
            min={8}
            max={16}
            value={rows}
            onChange={(event) => setRows(Number(event.target.value))}
            className="w-full accent-cyan-500"
          />
          <div className="flex justify-between text-xs text-gray-500 font-bold mt-2">
            <span>8</span>
            <span className="text-cyan-400">{rows}</span>
            <span>16</span>
          </div>
          <div className="mt-4 bg-dark-900 border border-dark-700 rounded-2xl p-4 space-y-2">
            <div className="flex justify-between text-xs font-bold text-gray-400">
              <span>Board fee</span>
              <span
                className={boardFee > 0 ? "text-yellow-400" : "text-gray-500"}
              >
                {boardFee.toFixed(2)} PLN
              </span>
            </div>
            <div className="flex justify-between text-sm font-black text-white">
              <span>Total cost</span>
              <span>{totalCost.toFixed(2)} PLN</span>
            </div>
          </div>

          <button
            onClick={buyBalls}
            disabled={isBuying}
            className="w-full mt-8 relative overflow-hidden group bg-cyan-600 hover:bg-cyan-500 disabled:bg-cyan-600/50 disabled:cursor-not-allowed text-dark-900 font-black py-5 rounded-2xl transition-all shadow-[0_0_24px_rgba(8,145,178,0.28)]"
          >
            <div className="relative z-10 flex items-center justify-center gap-3 text-lg tracking-wide uppercase">
              {isBuying ? (
                <>
                  <RefreshCw className="w-6 h-6 animate-spin" />
                  {isDemoMode ? "Preparing..." : "Buying..."}
                </>
              ) : (
                <>
                  <Zap className="w-6 h-6 group-hover:scale-125 transition-transform" />
                  {isDemoMode ? "Get Demo Balls" : "Buy Balls"}
                </>
              )}
            </div>
          </button>

          <button
            onClick={dropNextBall}
            disabled={pendingRounds.length === 0}
            className="w-full mt-3 relative overflow-hidden group bg-dark-700 hover:bg-dark-600 disabled:opacity-50 disabled:cursor-not-allowed border-2 border-dark-600 text-white font-black py-5 rounded-2xl transition-all"
          >
            <div className="relative z-10 flex items-center justify-center gap-3 text-lg tracking-wide uppercase">
              <CircleDot className="w-6 h-6 text-cyan-400 group-hover:scale-125 transition-transform" />
              Drop Next Ball ({pendingRounds.length})
            </div>
          </button>

          <button
            onClick={dropAllBalls}
            disabled={pendingRounds.length === 0}
            className="w-full mt-3 relative overflow-hidden group bg-primary-600/20 hover:bg-primary-600/30 disabled:opacity-50 disabled:cursor-not-allowed border-2 border-primary-500/30 text-primary-200 font-black py-4 rounded-2xl transition-all"
          >
            <div className="relative z-10 flex items-center justify-center gap-3 text-base tracking-wide uppercase">
              <RefreshCw className="w-5 h-5 text-primary-300 group-hover:rotate-180 transition-transform" />
              Drop All Queued
            </div>
          </button>

          {(pendingRounds.length > 0 || ballsInPlay > 0) && (
            <div className="mt-4 grid grid-cols-2 gap-3 text-center">
              <div className="bg-dark-900 border border-dark-700 rounded-2xl p-3">
                <p className="text-[10px] text-gray-500 font-black uppercase tracking-widest">
                  Queued
                </p>
                <p className="text-xl font-black text-white">
                  {pendingRounds.length}
                </p>
              </div>
              <div className="bg-dark-900 border border-dark-700 rounded-2xl p-3">
                <p className="text-[10px] text-gray-500 font-black uppercase tracking-widest">
                  In Play
                </p>
                <p className="text-xl font-black text-cyan-400">
                  {ballsInPlay}
                </p>
              </div>
            </div>
          )}

          {activeRound && (
            <motion.div
              initial={{ opacity: 0, y: 12 }}
              animate={{ opacity: 1, y: 0 }}
              className="mt-6 bg-dark-900 border border-dark-700 rounded-2xl p-5"
            >
              <p className="text-xs text-gray-500 font-black uppercase tracking-widest mb-3">
                Last Result
              </p>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <p className="text-gray-500 text-xs font-bold">Multiplier</p>
                  <p className="text-2xl font-black text-yellow-400">
                    {activeRound.multiplier}x
                  </p>
                </div>
                <div>
                  <p className="text-gray-500 text-xs font-bold">Payout</p>
                  <p className="text-2xl font-black text-accent-success">
                    {activeRound.payout.toFixed(2)}
                  </p>
                </div>
              </div>
            </motion.div>
          )}

          {activeBatch && (
            <motion.div
              initial={{ opacity: 0, y: 12 }}
              animate={{ opacity: 1, y: 0 }}
              className="mt-4 bg-dark-900 border border-cyan-500/30 rounded-2xl p-5"
            >
              <div className="flex items-center justify-between mb-4">
                <p className="text-xs text-gray-500 font-black uppercase tracking-widest">
                  Current Batch
                </p>
                <p className="text-xs text-cyan-400 font-black">
                  {activeBatch.completed}/{activeBatch.balls}
                </p>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <p className="text-gray-500 text-xs font-bold">Total Stake</p>
                  <p className="text-xl font-black text-white">
                    {activeBatch.totalStake.toFixed(2)}
                  </p>
                </div>
                <div>
                  <p className="text-gray-500 text-xs font-bold">
                    Total Payout
                  </p>
                  <p className="text-xl font-black text-accent-success">
                    {activeBatch.totalPayout.toFixed(2)}
                  </p>
                </div>
              </div>
              <div className="mt-3 flex justify-between text-xs font-bold text-gray-500">
                <span>Board fee paid</span>
                <span className="text-yellow-400">
                  {activeBatch.boardFee.toFixed(2)} PLN
                </span>
              </div>
              <div className="mt-4 h-2 bg-dark-700 rounded-full overflow-hidden">
                <div
                  className="h-full bg-cyan-500 transition-all duration-500"
                  style={{
                    width: `${(activeBatch.completed / activeBatch.balls) * 100}%`,
                  }}
                />
              </div>
            </motion.div>
          )}
        </div>

        <div className="bg-dark-800 rounded-3xl border border-dark-700 overflow-hidden shadow-[0_10px_40px_rgba(0,0,0,0.5)]">
          <div className="bg-dark-900 p-6 border-b border-dark-700 flex justify-between items-center relative overflow-hidden">
            <div className="absolute inset-0 opacity-10 bg-[url('https://www.transparenttextures.com/patterns/diagonal-stripes.png')]" />
            <div className="relative z-10">
              <h3 className="text-2xl font-black text-white">
                Multiplier Board
              </h3>
              <p className="text-gray-400 text-sm mt-1">
                {riskLevel} risk / {rows} rows
              </p>
            </div>
            <div className="px-4 py-2 rounded-lg bg-cyan-500/10 text-cyan-400 text-xs uppercase tracking-widest font-black border border-cyan-500/20 relative z-10">
              {isDemoMode ? "Demo Mode" : "API Mode"}
            </div>
          </div>

          <div className="p-6 md:p-8">
            <div className="relative min-h-[520px] bg-dark-900/70 rounded-2xl border border-dark-700 overflow-hidden flex flex-col justify-end px-4 pt-8 pb-5">
              <AnimatePresence>
                {droppingRounds.map((round) => (
                  <motion.div
                    key={round.id}
                    initial={{ top: 20, left: "50%", opacity: 1, scale: 1 }}
                    animate={{
                      ...createBallAnimation(round),
                      opacity: 1,
                    }}
                    exit={{ opacity: 0, scale: 0.75 }}
                    transition={{
                      duration: dropDurationMs / 1000,
                      ease: "easeInOut",
                    }}
                    className="absolute z-20 w-8 h-8 rounded-full bg-cyan-300 shadow-[0_0_24px_rgba(103,232,249,0.9)] border-4 border-white"
                    style={{ x: "-50%", y: "-50%" }}
                  />
                ))}
              </AnimatePresence>

              <div className="flex-1 flex flex-col justify-center gap-3">
                {Array.from({ length: rows }, (_, row) => (
                  <div key={row} className="flex justify-center gap-6 md:gap-8">
                    {Array.from({ length: row + 2 }, (_, peg) => (
                      <div
                        key={peg}
                        className="w-3 h-3 rounded-full bg-gray-200 shadow-[0_0_14px_rgba(255,255,255,0.45)]"
                      />
                    ))}
                  </div>
                ))}
              </div>

              <div
                className="grid gap-1 md:gap-2 mt-6"
                style={{
                  gridTemplateColumns: `repeat(${multipliers.length}, minmax(0, 1fr))`,
                }}
              >
                {multipliers.map((multiplier, index) => {
                  const isHit = activeRound?.landingSlot === index;
                  return (
                    <div
                      key={`${index}-${multiplier}`}
                      className={getSlotClassName(
                        index,
                        multipliers.length,
                        multiplier,
                        isHit,
                      )}
                    >
                      {multiplier}x
                    </div>
                  );
                })}
              </div>
            </div>
          </div>
        </div>
      </div>

      <div className="bg-dark-800 rounded-3xl border border-dark-700 p-6 shadow-[0_10px_40px_rgba(0,0,0,0.35)]">
        <div className="flex items-center gap-3 mb-5">
          <Trophy className="w-6 h-6 text-yellow-500" />
          <h2 className="text-2xl font-black text-white">Recent Drops</h2>
        </div>

        {history.length === 0 ? (
          <div className="bg-dark-900 border border-dark-700 rounded-2xl p-10 text-center text-gray-500 font-bold">
            No Plinko rounds yet.
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-4">
            {history.map((round) => (
              <motion.div
                key={round.id}
                initial={{ opacity: 0, y: 12 }}
                animate={{ opacity: 1, y: 0 }}
                className="bg-dark-900 border border-dark-700 rounded-2xl p-5"
              >
                <div className="flex items-center justify-between mb-4">
                  <span className="text-xs font-black uppercase tracking-widest text-gray-500">
                    {round.riskLevel}
                  </span>
                  <span className="text-xs text-gray-500">
                    {round.createdAt.toLocaleTimeString([], {
                      hour: "2-digit",
                      minute: "2-digit",
                    })}
                  </span>
                </div>
                <div className="flex items-end justify-between gap-4">
                  <div>
                    <p className="text-gray-500 text-xs font-bold">Stake</p>
                    <p className="text-white text-lg font-black">
                      {round.stake.toFixed(2)}
                    </p>
                  </div>
                  <div className="text-right">
                    <p className="text-yellow-400 text-2xl font-black">
                      {round.multiplier}x
                    </p>
                    <p className="text-accent-success text-sm font-bold">
                      {round.payout.toFixed(2)} PLN
                    </p>
                  </div>
                </div>
              </motion.div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default PlinkoPage;
