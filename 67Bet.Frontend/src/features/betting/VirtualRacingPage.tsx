import React, { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { bettingApi } from "../../api/axios";
import { addSelection, removeSelection } from "../betslip/betslipSlice";
import type { RootState, AppDispatch } from "../../app/store";
import { motion, AnimatePresence } from "framer-motion";
import {
  Flag,
  Trophy,
  Clock,
  Medal,
  Zap,
  AlertCircle,
  RefreshCw,
  CheckCircle2,
} from "lucide-react";

interface VirtualRaceParticipantDto {
  id: string;
  horseId: string;
  horseName: string;
  odds: number;
}

interface VirtualRaceDto {
  id: string;
  name: string;
  startTime: string;
  isFinished: boolean;
  winningHorseId: string | null;
  participants: VirtualRaceParticipantDto[];
}

const VirtualRacingPage: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const selections = useSelector(
    (state: RootState) => state.betslip.selections,
  );
  const [activeRaces, setActiveRaces] = useState<VirtualRaceDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isGenerating, setIsGenerating] = useState(false);
  const [isSimulating, setIsSimulating] = useState<string | null>(null);

  // Track animation state
  const [runningRaces, setRunningRaces] = useState<
    Record<string, { winnerId: string }>
  >({});
  const [finishedRaces, setFinishedRaces] = useState<Record<string, string>>(
    {},
  );

  useEffect(() => {
    fetchActiveRaces();
  }, []);

  const fetchActiveRaces = async () => {
    try {
      setLoading(true);
      const response = await bettingApi.get<VirtualRaceDto[]>(
        "/virtualracing/active",
      );
      setActiveRaces(response.data);
      setError(null);
    } catch (err: any) {
      setError(
        err.response?.data?.error || err.message || "Error fetching races",
      );
    } finally {
      setLoading(false);
    }
  };

  const generateRace = async () => {
    try {
      setIsGenerating(true);
      await bettingApi.post("/virtualracing/generate");
      await fetchActiveRaces();
    } catch (err: any) {
      alert(
        "Error generating race: " +
          (err.response?.data?.error || err.message || ""),
      );
    } finally {
      setIsGenerating(false);
    }
  };

  const simulateRace = async (id: string) => {
    try {
      setIsSimulating(id);
      const response = await bettingApi.post(`/virtualracing/${id}/simulate`);
      const winnerId =
        response.data.winningHorseId || response.data.WinningHorseId;

      // Rozpocznij animację wirtualnego toru
      setRunningRaces((prev) => ({ ...prev, [id]: { winnerId } }));

      // Czas trwania biegu koni to 4 sekundy
      setTimeout(() => {
        // Pokaż zwycięzcę (puchar obok nazwy i na złoto)
        setFinishedRaces((prev) => ({ ...prev, [id]: winnerId }));

        // Po kolejnych 5 sekundach usuń wyścig z listy aktywnych
        setTimeout(() => {
          fetchActiveRaces();
          setRunningRaces((prev) => {
            const newState = { ...prev };
            delete newState[id];
            return newState;
          });
          setFinishedRaces((prev) => {
            const newState = { ...prev };
            delete newState[id];
            return newState;
          });
          setIsSimulating(null);
        }, 5000);
      }, 4000);
    } catch (err: any) {
      alert(
        "Error simulating race: " +
          (err.response?.data?.error || err.message || ""),
      );
      setIsSimulating(null);
    }
  };

  const isSelected = (outcomeId: string) =>
    selections.some((s) => s.outcomeId === outcomeId);

  const toggleBet = (
    race: VirtualRaceDto,
    participant: VirtualRaceParticipantDto,
  ) => {
    const pId = participant.id || (participant as any).Id;
    if (isSelected(pId)) {
      dispatch(removeSelection(pId));
    } else {
      dispatch(
        addSelection({
          eventId: race.id || (race as any).Id,
          eventName: race.name || (race as any).Name,
          marketId: `virtual-winner-${race.id || (race as any).Id}`,
          marketName: "Race Winner",
          outcomeId: pId,
          outcomeName: participant.horseName || (participant as any).HorseName,
          odd: participant.odds || (participant as any).Odds,
        }),
      );
    }
  };

  return (
    <div className="max-w-[1600px] w-full px-4 xl:px-8 mx-auto space-y-10">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-6 bg-dark-800 p-8 rounded-3xl border border-dark-700 shadow-2xl">
        <div>
          <div className="flex items-center gap-4 mb-3">
            <div className="w-12 h-12 rounded-2xl bg-purple-500/20 flex items-center justify-center">
              <Flag className="w-7 h-7 text-purple-500" />
            </div>
            <h1 className="text-3xl md:text-4xl font-black text-white italic tracking-tight">
              VIRTUAL RACING
            </h1>
          </div>
          <p className="text-gray-400 text-base">
            Experience the thrill of AI-generated horse racing 24/7 in our
            premium simulator.
          </p>
        </div>

        <button
          onClick={generateRace}
          disabled={isGenerating}
          className="flex items-center gap-3 bg-purple-600 hover:bg-purple-700 disabled:bg-purple-600/50 disabled:cursor-not-allowed text-white px-8 py-4 rounded-2xl font-bold text-lg transition-all transform hover:scale-105 active:scale-95 shadow-[0_0_20px_rgba(147,51,234,0.3)] hover:shadow-[0_0_30px_rgba(147,51,234,0.5)]"
        >
          {isGenerating ? (
            <RefreshCw className="w-6 h-6 animate-spin" />
          ) : (
            <Zap className="w-6 h-6" />
          )}
          Generate Premium Race
        </button>
      </div>

      {error && (
        <div className="bg-red-500/10 border border-red-500/50 rounded-2xl p-6 flex items-center gap-4 text-red-500 text-lg">
          <AlertCircle className="w-6 h-6 flex-shrink-0" />
          <p className="font-medium">{error}</p>
        </div>
      )}

      {loading ? (
        <div className="flex justify-center items-center py-32">
          <RefreshCw className="w-12 h-12 text-purple-500 animate-spin" />
        </div>
      ) : activeRaces.length === 0 ? (
        <motion.div
          initial={{ opacity: 0, y: 30 }}
          animate={{ opacity: 1, y: 0 }}
          className="bg-dark-800 border border-dark-700 rounded-3xl p-16 text-center shadow-2xl"
        >
          <div className="w-24 h-24 bg-dark-700 rounded-full flex items-center justify-center mx-auto mb-6">
            <Trophy className="w-12 h-12 text-gray-500" />
          </div>
          <h2 className="text-3xl font-bold text-white mb-4">
            No Active Races
          </h2>
          <p className="text-gray-400 text-lg mb-8 max-w-xl mx-auto">
            The track is currently empty. Be the first to generate a new virtual
            race and watch the action unfold in ultra-high resolution!
          </p>
          <button
            onClick={generateRace}
            disabled={isGenerating}
            className="bg-primary-600 hover:bg-primary-700 text-white px-10 py-4 rounded-2xl font-bold text-lg transition-all"
          >
            Start a Race Now
          </button>
        </motion.div>
      ) : (
        <div className="grid grid-cols-1 xl:grid-cols-2 2xl:grid-cols-2 gap-8 md:gap-12">
          <AnimatePresence>
            {activeRaces.map((raceRaw) => {
              const race = {
                id: raceRaw.id || (raceRaw as any).Id,
                name: raceRaw.name || (raceRaw as any).Name,
                startTime: raceRaw.startTime || (raceRaw as any).StartTime,
                isFinished: raceRaw.isFinished || (raceRaw as any).IsFinished,
                winningHorseId:
                  raceRaw.winningHorseId || (raceRaw as any).WinningHorseId,
                participants:
                  raceRaw.participants || (raceRaw as any).Participants || [],
              } as VirtualRaceDto;

              return (
                <motion.div
                  key={race.id}
                  initial={{ opacity: 0, scale: 0.95 }}
                  animate={{ opacity: 1, scale: 1 }}
                  exit={{ opacity: 0, scale: 0.9, filter: "blur(4px)" }}
                  className="bg-dark-800 rounded-3xl border border-dark-700 overflow-hidden flex flex-col shadow-[0_10px_40px_rgba(0,0,0,0.5)]"
                >
                  <div className="bg-dark-900 p-6 border-b border-dark-700 flex justify-between items-center relative overflow-hidden">
                    <div className="absolute inset-0 opacity-10 bg-[url('https://www.transparenttextures.com/patterns/diagonal-stripes.png')]" />
                    <div className="flex items-center gap-4 relative z-10">
                      <div className="w-10 h-10 rounded-xl bg-purple-500/20 flex items-center justify-center">
                        <Clock className="w-5 h-5 text-purple-400" />
                      </div>
                      <div>
                        <h3 className="text-xl md:text-2xl font-black text-white leading-tight">
                          {race.name}
                        </h3>
                        <p className="text-sm md:text-base text-gray-400 mt-1">
                          Starts:{" "}
                          {new Date(race.startTime).toLocaleTimeString([], {
                            hour: "2-digit",
                            minute: "2-digit",
                          })}
                        </p>
                      </div>
                    </div>
                    <div className="px-4 py-2 rounded-lg bg-accent-success/10 text-accent-success text-xs md:text-sm uppercase tracking-widest font-black border border-accent-success/20 relative z-10">
                      Accepting Bets
                    </div>
                  </div>

                  <div className="p-6 md:p-8 flex-1">
                    {runningRaces[race.id] ? (
                      // WIDOK TORU WYŚCIGOWEGO (ANIMACJA)
                      <div className="relative w-full h-[500px] bg-green-900/40 border-4 border-green-800/50 rounded-2xl overflow-hidden mb-8 flex flex-col justify-evenly py-4 shadow-inner">
                        {/* Pasy toru */}
                        {race.participants.map((_, i) => (
                          <div
                            key={i}
                            className="absolute w-full h-[2px] bg-white/10"
                            style={{
                              top: `${(i + 1) * (100 / (race.participants.length + 1))}%`,
                            }}
                          />
                        ))}

                        {/* Linia startu */}
                        <div className="absolute left-[5%] top-0 bottom-0 w-2 bg-white/20 border-r-2 border-white/50 border-dashed z-0" />

                        {/* Linia mety (Szachownica) */}
                        <div className="absolute right-[10%] top-0 bottom-0 w-6 z-0 flex flex-col border-l-4 border-white shadow-[0_0_15px_rgba(255,255,255,0.3)]">
                          {Array.from({ length: 16 }).map((_, i) => (
                            <div key={i} className="flex-1 flex">
                              <div
                                className={`flex-1 ${i % 2 === 0 ? "bg-black" : "bg-white"}`}
                              />
                              <div
                                className={`flex-1 ${i % 2 === 0 ? "bg-white" : "bg-black"}`}
                              />
                            </div>
                          ))}
                        </div>

                        {race.participants.map((pRaw, index) => {
                          const p = {
                            id: pRaw.id || (pRaw as any).Id,
                            horseId: pRaw.horseId || (pRaw as any).HorseId,
                            horseName:
                              pRaw.horseName || (pRaw as any).HorseName,
                            odds: pRaw.odds || (pRaw as any).Odds,
                          } as VirtualRaceParticipantDto;

                          const winnerId = runningRaces[race.id].winnerId;
                          const isWinner = winnerId === p.horseId;
                          const isFinished =
                            finishedRaces[race.id] !== undefined;

                          const duration = isWinner
                            ? 4
                            : 4.2 + Math.random() * 1.5;

                          return (
                            <div
                              key={p.id}
                              className="relative z-10 flex items-center h-14 w-full"
                            >
                              <motion.div
                                initial={{ left: "5%" }}
                                animate={{
                                  left: isFinished
                                    ? isWinner
                                      ? "90%"
                                      : `${65 + Math.random() * 20}%`
                                    : "90%",
                                }}
                                transition={{
                                  duration: duration,
                                  ease: isWinner ? "easeOut" : "easeInOut",
                                }}
                                className="absolute text-5xl md:text-6xl drop-shadow-2xl z-20 flex flex-col items-center"
                                style={{
                                  transform:
                                    "translateX(-50%) translateY(-15%)",
                                }}
                              >
                                <span className="text-[10px] md:text-xs font-black bg-dark-900/95 border-2 border-dark-600 text-white px-2.5 py-1 rounded-md shadow-2xl whitespace-nowrap mb-[-8px] z-30">
                                  {index + 1}. {p.horseName}
                                </span>
                                🐎
                              </motion.div>
                            </div>
                          );
                        })}
                      </div>
                    ) : (
                      // WIDOK LISTY KONI (PRZED WYŚCIGIEM I PO ROZSTRZYGNIĘCIU)
                      <div className="space-y-3 md:space-y-4 mb-8">
                        {race.participants.map((pRaw, index) => {
                          const p = {
                            id: pRaw.id || (pRaw as any).Id,
                            horseId: pRaw.horseId || (pRaw as any).HorseId,
                            horseName:
                              pRaw.horseName || (pRaw as any).HorseName,
                            odds: pRaw.odds || (pRaw as any).Odds,
                          } as VirtualRaceParticipantDto;

                          const isWinner = finishedRaces[race.id] === p.horseId;
                          const selected = isSelected(p.id);

                          return (
                            <div
                              key={p.id}
                              onClick={() =>
                                !finishedRaces[race.id] && toggleBet(race, p)
                              }
                              className={`flex items-center justify-between p-4 md:p-6 rounded-2xl transition-all border-2 cursor-pointer
                              ${
                                isWinner
                                  ? "bg-yellow-500/20 border-yellow-500 shadow-[0_0_20px_rgba(234,179,8,0.4)]"
                                  : selected
                                    ? "bg-primary-600/20 border-primary-500"
                                    : "bg-dark-700/40 border-transparent hover:border-dark-600 hover:bg-dark-700"
                              }
                              ${finishedRaces[race.id] && !isWinner ? "opacity-40 grayscale" : ""}
                            `}
                            >
                              <div className="flex items-center gap-4 md:gap-6">
                                <span
                                  className={`w-8 h-8 md:w-10 md:h-10 rounded-lg flex items-center justify-center text-sm md:text-base font-black shadow-inner
                                ${isWinner ? "bg-yellow-500 text-dark-900" : "bg-dark-900 text-gray-400"}
                              `}
                                >
                                  {index + 1}
                                </span>
                                <div className="flex flex-col">
                                  <span
                                    className={`font-black tracking-wide ${isWinner ? "text-yellow-500 text-xl md:text-2xl" : "text-gray-100 text-lg md:text-xl"}`}
                                  >
                                    {p.horseName}
                                  </span>
                                  <span className="text-xs md:text-sm text-gray-500 font-bold uppercase tracking-widest mt-0.5">
                                    Stallion / Skill Level 8
                                  </span>
                                </div>
                                {isWinner && (
                                  <Trophy className="w-7 h-7 text-yellow-500 ml-2 animate-bounce" />
                                )}
                              </div>

                              <div
                                className={`px-6 py-3 rounded-xl font-black text-lg md:text-xl border-2 transition-colors shadow-md
                              ${
                                isWinner
                                  ? "bg-yellow-500 text-dark-900 border-yellow-400"
                                  : selected
                                    ? "bg-primary-500 text-white border-primary-400"
                                    : "bg-dark-800 text-primary-400 border-primary-500/30"
                              }
                            `}
                              >
                                {p.odds.toFixed(2)}
                              </div>
                            </div>
                          );
                        })}
                      </div>
                    )}

                    {finishedRaces[race.id] ? (
                      <div className="w-full bg-yellow-500/20 border-2 border-yellow-500/50 text-yellow-500 font-black text-xl py-5 rounded-2xl flex items-center justify-center gap-3 animate-pulse shadow-[0_0_30px_rgba(234,179,8,0.3)]">
                        <CheckCircle2 className="w-7 h-7" />
                        RACE FINISHED - WINNER CROWNED!
                      </div>
                    ) : runningRaces[race.id] ? (
                      <div className="w-full bg-purple-600/20 border-2 border-purple-500/50 text-purple-400 font-black text-xl py-5 rounded-2xl flex items-center justify-center gap-3">
                        <Flag className="w-7 h-7 animate-bounce" />
                        RACE IN PROGRESS... GO GO GO!
                      </div>
                    ) : (
                      <button
                        onClick={() => simulateRace(race.id)}
                        disabled={isSimulating === race.id}
                        className="w-full relative overflow-hidden group bg-dark-700 hover:bg-dark-600 border-2 border-dark-600 text-white font-black py-5 md:py-6 rounded-2xl transition-all disabled:opacity-50 disabled:cursor-not-allowed shadow-lg"
                      >
                        <div className="relative z-10 flex items-center justify-center gap-3 text-xl tracking-wide uppercase">
                          {isSimulating === race.id ? (
                            <>
                              <RefreshCw className="w-7 h-7 animate-spin text-purple-500" />
                              <span>Starting Engine...</span>
                            </>
                          ) : (
                            <>
                              <Medal className="w-7 h-7 text-purple-400 group-hover:scale-125 transition-transform duration-300" />
                              <span>Start Race & Watch Live</span>
                            </>
                          )}
                        </div>
                        <div className="absolute inset-0 bg-gradient-to-r from-purple-600/0 via-purple-600/20 to-purple-600/0 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700" />
                      </button>
                    )}
                  </div>
                </motion.div>
              );
            })}
          </AnimatePresence>
        </div>
      )}
    </div>
  );
};

export default VirtualRacingPage;
