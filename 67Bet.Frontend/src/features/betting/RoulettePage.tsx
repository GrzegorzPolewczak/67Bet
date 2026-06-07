import React, { useEffect, useState } from "react";
import { useSelector } from "react-redux";
import { motion, AnimatePresence } from "framer-motion";
import {
  Coins,
  AlertCircle,
  Trophy,
  RefreshCw,
  X,
  ChevronRight,
} from "lucide-react";
import type { RootState } from "../../app/store";
import { bettingApi, walletApi } from "../../api/axios";

const RED_NUMBERS = new Set([
  1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36,
]);

type BetType =
  | "StraightUp"
  | "Red"
  | "Black"
  | "Even"
  | "Odd"
  | "Low"
  | "High"
  | "DozenFirst"
  | "DozenSecond"
  | "DozenThird"
  | "ColumnFirst"
  | "ColumnSecond"
  | "ColumnThird";

const BET_TYPE_API: Record<BetType, number> = {
  StraightUp: 0,
  Red: 1,
  Black: 2,
  Even: 3,
  Odd: 4,
  Low: 5,
  High: 6,
  DozenFirst: 7,
  DozenSecond: 8,
  DozenThird: 9,
  ColumnFirst: 10,
  ColumnSecond: 11,
  ColumnThird: 12,
};

interface PlacedBet {
  id: string;
  betType: BetType;
  chosenNumber?: number;
  stake: number;
  label: string;
}

interface RouletteBetDto {
  id: string;
  betType: number;
  chosenNumber: number | null;
  stake: number;
  isWon: boolean;
  payout: number;
}

interface RouletteRoundDto {
  id: string;
  spinResult: number;
  totalStake: number;
  totalPayout: number;
  isPayoutSettled: boolean;
  createdAtUtc: string;
  bets: RouletteBetDto[];
}

function betLabel(betType: BetType, chosenNumber?: number): string {
  if (betType === "StraightUp") return `Numer ${chosenNumber}`;
  const map: Record<BetType, string> = {
    StraightUp: "",
    Red: "Rouge",
    Black: "Noir",
    Even: "Pair",
    Odd: "Impair",
    Low: "Manque (1–18)",
    High: "Passe (19–36)",
    DozenFirst: "1ère douzaine (1–12)",
    DozenSecond: "2ème douzaine (13–24)",
    DozenThird: "3ème douzaine (25–36)",
    ColumnFirst: "Colonne 1",
    ColumnSecond: "Colonne 2",
    ColumnThird: "Colonne 3",
  };
  return map[betType];
}

function numBg(n: number): string {
  if (n === 0)
    return "bg-green-600 hover:bg-green-500 text-white border-green-400";
  return RED_NUMBERS.has(n)
    ? "bg-red-600 hover:bg-red-500 text-white border-red-400"
    : "bg-zinc-900 hover:bg-zinc-800 text-white border-zinc-700";
}

const getApiError = (err: unknown, fallback: string): string => {
  const e = err as any;
  return (
    e?.response?.data?.error ||
    e?.response?.data?.message ||
    e?.response?.data ||
    e?.message ||
    fallback
  );
};

const betTypeFromApiValue = (apiValue: number): BetType =>
  (Object.keys(BET_TYPE_API) as BetType[]).find(
    (k) => BET_TYPE_API[k] === apiValue,
  ) ?? "Red";

const NUMBER_GRID: number[][] = [
  [3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36],
  [2, 5, 8, 11, 14, 17, 20, 23, 26, 29, 32, 35],
  [1, 4, 7, 10, 13, 16, 19, 22, 25, 28, 31, 34],
];

const OUTSIDE_BETS: [BetType, string, string][] = [
  ["Low", "1–18", "bg-dark-700 border-dark-600 text-gray-200"],
  ["Even", "Pair", "bg-dark-700 border-dark-600 text-gray-200"],
  ["Red", "Rouge", "bg-red-600 border-red-400 text-white"],
  ["Black", "Noir", "bg-zinc-900 border-zinc-700 text-white"],
  ["Odd", "Impair", "bg-dark-700 border-dark-600 text-gray-200"],
  ["High", "19–36", "bg-dark-700 border-dark-600 text-gray-200"],
];

const DOZEN_BETS: [BetType, string][] = [
  ["DozenFirst", "1–12"],
  ["DozenSecond", "13–24"],
  ["DozenThird", "25–36"],
];

const COLUMN_BETS: [BetType, string][] = [
  ["ColumnFirst", "Col 1"],
  ["ColumnSecond", "Col 2"],
  ["ColumnThird", "Col 3"],
];

const QUICK_BETS: [BetType, string][] = [
  ["Red", "Rouge"],
  ["Black", "Noir"],
  ["Even", "Pair"],
  ["Odd", "Impair"],
  ["Low", "1–18"],
  ["High", "19–36"],
  ["DozenFirst", "1–12"],
  ["DozenSecond", "13–24"],
  ["DozenThird", "25–36"],
  ["ColumnFirst", "Col 1"],
  ["ColumnSecond", "Col 2"],
  ["ColumnThird", "Col 3"],
];

const RoulettePage: React.FC = () => {
  const { isAuthenticated } = useSelector((state: RootState) => state.auth);
  const [balance, setBalance] = useState(0);
  const [stake, setStake] = useState(10);
  const [bets, setBets] = useState<PlacedBet[]>([]);
  const [isSpinning, setIsSpinning] = useState(false);
  const [spinAnim, setSpinAnim] = useState(false);
  const [lastRound, setLastRound] = useState<RouletteRoundDto | null>(null);
  const [history, setHistory] = useState<RouletteRoundDto[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isAuthenticated) return;
    walletApi
      .get("/wallet/balance")
      .then((r) => setBalance(Number(r.data.balance ?? r.data.Balance ?? 0)))
      .catch(() => {});
  }, [isAuthenticated]);

  const addBet = (betType: BetType, chosenNumber?: number) => {
    if (stake <= 0) {
      setError("Stawka musi być większa od zera.");
      return;
    }
    if (bets.length >= 10) {
      setError("Maksymalnie 10 zakładów na jedną rundę.");
      return;
    }
    setBets((prev) => [
      ...prev,
      {
        id: crypto.randomUUID(),
        betType,
        chosenNumber,
        stake,
        label: betLabel(betType, chosenNumber),
      },
    ]);
    setError(null);
  };

  const totalStake =
    Math.round(bets.reduce((s, b) => s + b.stake, 0) * 100) / 100;

  const spin = async () => {
    if (bets.length === 0) {
      setError("Postaw przynajmniej jeden zakład.");
      return;
    }
    if (!isAuthenticated) {
      setError("Zaloguj się, aby grać.");
      return;
    }

    setError(null);
    setIsSpinning(true);
    setSpinAnim(true);
    setLastRound(null);

    try {
      const response = await bettingApi.post<RouletteRoundDto>(
        "/roulette/play",
        {
          bets: bets.map((b) => ({
            betType: BET_TYPE_API[b.betType],
            chosenNumber: b.chosenNumber ?? null,
            stake: b.stake,
          })),
        },
      );
      const round = response.data;

      await new Promise((res) => setTimeout(res, 2200));
      setSpinAnim(false);
      setLastRound(round);
      setBalance((prev) => Math.round((prev - round.totalStake) * 100) / 100);
      setBets([]);

      await bettingApi.post(`/roulette/${round.id}/settle`);
      setBalance((prev) => Math.round((prev + round.totalPayout) * 100) / 100);
      setHistory((prev) => [round, ...prev].slice(0, 10));
    } catch (err) {
      setSpinAnim(false);
      setError(getApiError(err, "Błąd podczas kręcenia. Spróbuj ponownie."));
    } finally {
      setIsSpinning(false);
    }
  };

  const spinResultColor = (n: number) =>
    n === 0
      ? "bg-green-600 border-green-400"
      : RED_NUMBERS.has(n)
        ? "bg-red-600 border-red-400"
        : "bg-zinc-900 border-zinc-600";

  return (
    <div className="max-w-[1400px] w-full px-4 xl:px-8 mx-auto space-y-8">
      {/* Header */}
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-6 bg-dark-800 p-8 rounded-3xl border border-dark-700 shadow-2xl">
        <div>
          <div className="flex items-center gap-4 mb-3">
            <div className="w-12 h-12 rounded-2xl bg-red-500/20 flex items-center justify-center text-2xl">
              🎯
            </div>
            <h1 className="text-3xl md:text-4xl font-black text-white italic tracking-tight">
              ROULETTE
            </h1>
          </div>
          <p className="text-gray-400 text-base">
            Europejska ruletka. Postaw zakłady na stole i zakręć kołem. Kurs
            35:1 na numer, 1:1 na kolor/parzystość, 2:1 na dziesiątkę/kolumnę.
          </p>
        </div>
        {isAuthenticated && (
          <div className="flex items-center gap-4 bg-dark-900 px-6 py-4 rounded-2xl border border-dark-700 shrink-0">
            <Coins className="w-6 h-6 text-accent-success" />
            <div>
              <p className="text-[10px] text-gray-500 font-black uppercase tracking-widest">
                Saldo
              </p>
              <p className="text-xl font-black text-white">
                {balance.toFixed(2)} PLN
              </p>
            </div>
          </div>
        )}
      </div>

      {error && (
        <div className="bg-red-500/10 border border-red-500/50 rounded-2xl p-5 flex items-center gap-4 text-red-400">
          <AlertCircle className="w-6 h-6 shrink-0" />
          <p className="font-medium">{error}</p>
        </div>
      )}

      <div className="grid grid-cols-1 xl:grid-cols-[1fr_380px] gap-8">
        {/* Betting table */}
        <div className="bg-dark-800 rounded-3xl border border-dark-700 p-6 space-y-5">
          <h2 className="text-xl font-black text-white">Stół do gry</h2>

          <div className="overflow-x-auto">
            <div className="min-w-[620px] space-y-1">
              {/* Zero */}
              <button
                onClick={() => addBet("StraightUp", 0)}
                className="w-full py-3 rounded-xl text-lg font-black bg-green-600 hover:bg-green-500 text-white border-2 border-green-400 transition-all hover:scale-[1.02]"
              >
                0
              </button>

              {/* Number rows */}
              {NUMBER_GRID.map((row, ri) => (
                <div key={ri} className="flex gap-1">
                  {row.map((num) => (
                    <button
                      key={num}
                      onClick={() => addBet("StraightUp", num)}
                      className={`flex-1 py-3 rounded-xl text-sm font-black border-2 transition-all hover:scale-105 ${numBg(num)}`}
                    >
                      {num}
                    </button>
                  ))}
                  <button
                    onClick={() => addBet(COLUMN_BETS[2 - ri][0])}
                    className="w-16 rounded-xl text-xs font-black bg-dark-700 hover:bg-dark-600 text-gray-300 border border-dark-600 transition-colors"
                  >
                    {COLUMN_BETS[2 - ri][1]}
                  </button>
                </div>
              ))}

              {/* Dozen bets */}
              <div className="flex gap-1 pt-1">
                {DOZEN_BETS.map(([type, label]) => (
                  <button
                    key={type}
                    onClick={() => addBet(type)}
                    className="flex-1 py-2 rounded-xl text-xs font-black bg-dark-700 hover:bg-dark-600 text-gray-300 border border-dark-600 transition-colors"
                  >
                    {label}
                  </button>
                ))}
                <div className="w-16" />
              </div>

              {/* Outside bets */}
              <div className="grid grid-cols-6 gap-1 pt-1">
                {OUTSIDE_BETS.map(([type, label, cls]) => (
                  <button
                    key={type}
                    onClick={() => addBet(type)}
                    className={`py-2 rounded-xl text-xs font-black border-2 transition-all hover:opacity-90 ${cls}`}
                  >
                    {label}
                  </button>
                ))}
              </div>
            </div>
          </div>

          {/* Quick bet selector */}
          <div className="bg-dark-900 rounded-2xl border border-dark-700 p-4">
            <p className="text-xs font-black text-gray-500 uppercase tracking-widest mb-3">
              Szybkie zakłady
            </p>
            <div className="flex flex-wrap gap-2">
              {QUICK_BETS.map(([type, label]) => (
                <button
                  key={type}
                  onClick={() => addBet(type)}
                  className={`px-3 py-1.5 rounded-xl text-xs font-black border transition-all hover:scale-105 ${
                    type === "Red"
                      ? "bg-red-600 border-red-400 text-white"
                      : type === "Black"
                        ? "bg-zinc-900 border-zinc-700 text-white"
                        : "bg-dark-800 border-dark-600 text-gray-300 hover:border-primary-500/50"
                  }`}
                >
                  {label}
                </button>
              ))}
            </div>
          </div>
        </div>

        {/* Right panel */}
        <div className="space-y-5">
          {/* Spin result */}
          <div className="bg-dark-800 rounded-3xl border border-dark-700 p-6 text-center">
            <p className="text-xs font-black text-gray-500 uppercase tracking-widest mb-5">
              {spinAnim ? "Kręci się..." : "Ostatni wynik"}
            </p>
            <motion.div
              animate={
                spinAnim ? { rotate: [0, 360, 720, 1080] } : { rotate: 0 }
              }
              transition={
                spinAnim
                  ? { duration: 2.2, ease: "easeOut" }
                  : { duration: 0.3 }
              }
              className={`w-28 h-28 rounded-full mx-auto flex items-center justify-center text-4xl font-black border-4 transition-colors ${
                spinAnim
                  ? "border-yellow-400 bg-dark-900"
                  : lastRound !== null
                    ? spinResultColor(lastRound.spinResult)
                    : "bg-dark-900 border-dark-700 text-gray-600"
              }`}
            >
              {spinAnim ? "?" : lastRound !== null ? lastRound.spinResult : "–"}
            </motion.div>

            <AnimatePresence>
              {lastRound && !spinAnim && (
                <motion.div
                  initial={{ opacity: 0, y: 8 }}
                  animate={{ opacity: 1, y: 0 }}
                  className="mt-5 space-y-1"
                >
                  <p
                    className={`text-3xl font-black ${lastRound.totalPayout - lastRound.totalStake >= 0 ? "text-accent-success" : "text-red-400"}`}
                  >
                    {lastRound.totalPayout - lastRound.totalStake >= 0
                      ? `+${(lastRound.totalPayout - lastRound.totalStake).toFixed(2)} PLN`
                      : `${(lastRound.totalPayout - lastRound.totalStake).toFixed(2)} PLN`}
                  </p>
                  <p className="text-xs text-gray-500">
                    Stawka {lastRound.totalStake.toFixed(2)} · Wypłata{" "}
                    {lastRound.totalPayout.toFixed(2)} PLN
                  </p>
                  <div className="mt-3 space-y-1">
                    {lastRound.bets.map((b, i) => (
                      <div
                        key={i}
                        className={`flex justify-between text-xs rounded-xl px-3 py-2 ${b.isWon ? "bg-accent-success/10 text-accent-success" : "bg-red-500/10 text-red-400"}`}
                      >
                        <span>
                          {betLabel(
                            betTypeFromApiValue(b.betType),
                            b.chosenNumber ?? undefined,
                          )}
                        </span>
                        <span className="font-black">
                          {b.isWon
                            ? `+${b.payout.toFixed(2)}`
                            : `-${b.stake.toFixed(2)}`}
                        </span>
                      </div>
                    ))}
                  </div>
                </motion.div>
              )}
            </AnimatePresence>
          </div>

          {/* Stake */}
          <div className="bg-dark-800 rounded-3xl border border-dark-700 p-6">
            <label className="block text-xs font-black text-gray-500 uppercase tracking-widest mb-2">
              Stawka
            </label>
            <input
              type="number"
              min={1}
              step={0.01}
              value={stake}
              onChange={(e) => setStake(Number(e.target.value))}
              className="w-full bg-dark-900 border border-dark-600 rounded-2xl px-5 py-4 text-white text-lg font-bold focus:outline-none focus:border-red-500 transition-colors"
            />
            <div className="flex gap-2 mt-3">
              {[5, 10, 25, 50, 100].map((v) => (
                <button
                  key={v}
                  onClick={() => setStake(v)}
                  className="flex-1 py-2 text-xs font-black rounded-xl bg-dark-700 hover:bg-dark-600 text-gray-300 border border-dark-600 transition-colors"
                >
                  {v}
                </button>
              ))}
            </div>
          </div>

          {/* Placed bets */}
          <div className="bg-dark-800 rounded-3xl border border-dark-700 p-6">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-base font-black text-white">
                Zakłady ({bets.length}/10)
              </h3>
              {bets.length > 0 && (
                <button
                  onClick={() => setBets([])}
                  className="text-xs text-gray-500 hover:text-red-400 font-bold transition-colors"
                >
                  Wyczyść
                </button>
              )}
            </div>

            {bets.length === 0 ? (
              <p className="text-center text-gray-500 text-sm py-6">
                Kliknij liczbę lub zakład zewnętrzny, aby dodać.
              </p>
            ) : (
              <div className="space-y-2">
                <AnimatePresence>
                  {bets.map((bet) => (
                    <motion.div
                      key={bet.id}
                      initial={{ opacity: 0, x: -8 }}
                      animate={{ opacity: 1, x: 0 }}
                      exit={{ opacity: 0, x: 8 }}
                      className="flex items-center justify-between bg-dark-900 rounded-xl px-4 py-3 border border-dark-700"
                    >
                      <div className="flex items-center gap-2 min-w-0">
                        <ChevronRight className="w-4 h-4 text-red-500 shrink-0" />
                        <span className="text-white text-sm font-bold truncate">
                          {bet.label}
                        </span>
                      </div>
                      <div className="flex items-center gap-3 shrink-0">
                        <span className="text-accent-success font-black text-sm">
                          {bet.stake.toFixed(2)}
                        </span>
                        <button
                          onClick={() =>
                            setBets((p) => p.filter((b) => b.id !== bet.id))
                          }
                          className="text-gray-600 hover:text-red-400 transition-colors"
                        >
                          <X className="w-4 h-4" />
                        </button>
                      </div>
                    </motion.div>
                  ))}
                </AnimatePresence>
                <div className="flex justify-between text-sm font-black text-white pt-2 border-t border-dark-700">
                  <span>Łączna stawka</span>
                  <span className="text-red-400">
                    {totalStake.toFixed(2)} PLN
                  </span>
                </div>
              </div>
            )}
          </div>

          {/* Spin button */}
          <button
            onClick={spin}
            disabled={isSpinning || bets.length === 0 || !isAuthenticated}
            className="w-full relative overflow-hidden group bg-red-600 hover:bg-red-500 disabled:bg-red-700/40 disabled:cursor-not-allowed text-white font-black py-5 rounded-2xl transition-all shadow-[0_0_28px_rgba(220,38,38,0.35)]"
          >
            <div className="relative z-10 flex items-center justify-center gap-3 text-lg tracking-wide uppercase">
              {isSpinning ? (
                <>
                  <RefreshCw className="w-6 h-6 animate-spin" /> Kręci się...
                </>
              ) : (
                <>
                  <span className="text-2xl">🎯</span>
                  {isAuthenticated ? "Zakręć!" : "Zaloguj się"}
                </>
              )}
            </div>
          </button>
        </div>
      </div>

      {/* History */}
      <div className="bg-dark-800 rounded-3xl border border-dark-700 p-6">
        <div className="flex items-center gap-3 mb-5">
          <Trophy className="w-6 h-6 text-yellow-500" />
          <h2 className="text-2xl font-black text-white">Historia rund</h2>
        </div>

        {history.length === 0 ? (
          <div className="bg-dark-900 border border-dark-700 rounded-2xl p-10 text-center text-gray-500 font-bold">
            Brak rozegranych rund.
          </div>
        ) : (
          <div className="grid grid-cols-2 md:grid-cols-5 gap-4">
            {history.map((round) => (
              <motion.div
                key={round.id}
                initial={{ opacity: 0, y: 8 }}
                animate={{ opacity: 1, y: 0 }}
                className="bg-dark-900 border border-dark-700 rounded-2xl p-4"
              >
                <div className="flex items-center justify-between mb-3">
                  <div
                    className={`w-10 h-10 rounded-full flex items-center justify-center text-sm font-black border-2 ${
                      round.spinResult === 0
                        ? "bg-green-600 border-green-400 text-white"
                        : RED_NUMBERS.has(round.spinResult)
                          ? "bg-red-600 border-red-400 text-white"
                          : "bg-zinc-900 border-zinc-700 text-white"
                    }`}
                  >
                    {round.spinResult}
                  </div>
                  <span
                    className={`text-sm font-black ${round.totalPayout - round.totalStake >= 0 ? "text-accent-success" : "text-red-400"}`}
                  >
                    {round.totalPayout - round.totalStake >= 0
                      ? `+${(round.totalPayout - round.totalStake).toFixed(2)}`
                      : (round.totalPayout - round.totalStake).toFixed(2)}
                  </span>
                </div>
                <p className="text-xs text-gray-500">
                  {round.totalStake.toFixed(2)} PLN · {round.bets.length} zak.
                </p>
              </motion.div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default RoulettePage;
