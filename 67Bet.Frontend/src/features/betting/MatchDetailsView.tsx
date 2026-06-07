import React, { useState, useEffect } from "react";
import { useParams, Link } from "react-router-dom";
import { useDispatch, useSelector } from "react-redux";
import { ChevronLeft, Loader2, Play, LayoutGrid, Activity } from "lucide-react";
import type { RootState, AppDispatch } from "../../app/store";
import { fetchEventsAsync } from "./bettingSlice";
import { addSelection, removeSelection } from "../betslip/betslipSlice";
import {
  updateMatchState,
  clearMatchState,
  setConnectionStatus,
} from "./liveTrackerSlice";
import {
  startSignalRConnection,
  subscribeToMatch,
  unsubscribeFromMatch,
  onMatchUpdate,
  offMatchUpdate,
} from "../../api/signalr";
import { motion, AnimatePresence } from "framer-motion";
import OddButton from "./OddButton";
import LivePitch from "./LivePitch";
import StatsBars from "./StatsBars";
import MatchTimeline from "./MatchTimeline";
import AiMatchInsights from "./AiMatchInsights";

const MatchDetailsView: React.FC = () => {
  const { matchId } = useParams<{ matchId: string }>();
  const dispatch = useDispatch<AppDispatch>();
  const { events } = useSelector((state: RootState) => state.betting);
  const { currentMatch } = useSelector((state: RootState) => state.liveTracker);
  const selections = useSelector(
    (state: RootState) => state.betslip.selections,
  );

  // Mode: 'viz' for 2D Pitch, 'stream' for Live Video
  const [viewMode, setViewMode] = useState<"viz" | "stream">("viz");
  const [isPiP, setIsPiP] = useState(false);

  const event = events.find((e) => e.id === matchId);
  const isSelected = (outcomeId: string) =>
    selections.some((s) => s.outcomeId === outcomeId);

  useEffect(() => {
    if (!event) {
      dispatch(fetchEventsAsync());
    }
  }, [dispatch, event]);

  useEffect(() => {
    if (matchId) {
      const setupSignalR = async () => {
        await startSignalRConnection();
        dispatch(setConnectionStatus(true));
        await subscribeToMatch(matchId);

        onMatchUpdate((update) => {
          dispatch(updateMatchState(update));
          // Auto-switch to stream if available and not yet set
          if (update.streamUrl && viewMode === "viz") {
            // setViewMode('stream'); // Optional: auto-switch
          }
        });
      };

      setupSignalR();

      return () => {
        unsubscribeFromMatch(matchId);
        offMatchUpdate();
        dispatch(clearMatchState());
      };
    }
  }, [matchId, dispatch]);

  if (!event) {
    return (
      <div className="flex flex-col items-center justify-center h-64 space-y-4">
        <Loader2 className="w-10 h-10 text-primary-500 animate-spin" />
        <p className="text-gray-400 font-bold">Loading match details...</p>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto space-y-6 pb-12">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Link
            to="/"
            className="text-gray-500 hover:text-white transition-colors"
          >
            <ChevronLeft className="w-5 h-5" />
          </Link>
          <div>
            <h1 className="text-2xl font-black text-white">{event.name}</h1>
            <div className="flex items-center gap-2">
              <span className="text-xs font-bold text-primary-500 bg-primary-500/10 px-2 py-0.5 rounded uppercase tracking-tighter">
                {event.league}
              </span>
            </div>
          </div>
        </div>

        {/* View Mode Switcher */}
        {currentMatch?.streamUrl && (
          <div className="flex items-center gap-3">
            <div className="flex bg-dark-800 p-1 rounded-xl border border-dark-700">
              <button
                onClick={() => {
                  setViewMode("viz");
                  setIsPiP(false);
                }}
                className={`flex items-center gap-2 px-4 py-2 rounded-lg text-xs font-bold transition-all ${viewMode === "viz" && !isPiP ? "bg-primary-600 text-white shadow-lg" : "text-gray-400 hover:text-white"}`}
              >
                <LayoutGrid className="w-4 h-4" /> VIZ
              </button>
              <button
                onClick={() => {
                  setViewMode("stream");
                  setIsPiP(false);
                }}
                className={`flex items-center gap-2 px-4 py-2 rounded-lg text-xs font-bold transition-all ${viewMode === "stream" ? "bg-red-600 text-white shadow-lg" : "text-gray-400 hover:text-white"}`}
              >
                <Play className="w-4 h-4" /> LIVE STREAM
              </button>
            </div>

            <button
              onClick={() => setIsPiP(!isPiP)}
              className={`p-2.5 rounded-xl border transition-all ${isPiP ? "bg-primary-500 border-primary-400 text-white animate-pulse" : "bg-dark-800 border-dark-700 text-gray-400 hover:text-white"}`}
              title="Toggle Picture-in-Picture"
            >
              <Activity className="w-5 h-5" />
            </button>
          </div>
        )}
      </div>

      {/* Floating PiP Window */}
      <AnimatePresence>
        {isPiP && currentMatch?.streamUrl && (
          <motion.div
            drag
            dragConstraints={{ left: -500, right: 0, top: -500, bottom: 0 }}
            initial={{ opacity: 0, scale: 0.5, y: 100 }}
            animate={{ opacity: 1, scale: 1, y: 0 }}
            exit={{ opacity: 0, scale: 0.5 }}
            className="fixed bottom-6 right-6 w-80 aspect-video bg-black rounded-2xl overflow-hidden shadow-2xl border-2 border-primary-500 z-[9999] cursor-move"
          >
            <iframe
              src={getSanitizedStreamUrl(currentMatch.streamUrl)}
              className="w-full h-full border-0 pointer-events-none"
              allow="autoplay; encrypted-media"
            />
            <div className="absolute top-2 right-2 flex gap-1">
              <button
                onClick={() => setIsPiP(false)}
                className="bg-black/50 hover:bg-red-500 p-1.5 rounded-lg text-white transition-colors"
              >
                <ChevronLeft className="w-3 h-3 rotate-180" />
              </button>
            </div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Main Layout */}
      <div className="grid grid-cols-1 xl:grid-cols-3 gap-6">
        {/* Left: Timeline */}
        <div className="bg-dark-800 border border-dark-700 rounded-3xl p-6 h-fit">
          <h2 className="text-sm font-bold text-white uppercase tracking-wider mb-6">
            Match History
          </h2>
          <MatchTimeline events={currentMatch?.timelineEvents || []} />

          {/* AI Assistant Section */}
          <AiMatchInsights eventId={matchId || ""} />
        </div>

        {/* Center: Main View (Pitch or Stream) */}
        <div className="xl:col-span-2 space-y-6">
          <div className="bg-dark-800 border border-dark-700 rounded-3xl p-6 overflow-hidden">
            <div className="flex items-center justify-between mb-4">
              <div className="flex items-center gap-2">
                <div
                  className={`w-2 h-2 rounded-full animate-pulse ${viewMode === "stream" ? "bg-red-500" : "bg-primary-500"}`}
                />
                <h2 className="text-sm font-bold text-white uppercase tracking-wider">
                  {viewMode === "stream"
                    ? "Live Transmission"
                    : "Live Action Tracker"}
                </h2>
              </div>
              {currentMatch && (
                <div className="text-sm font-black text-primary-500 bg-primary-500/10 px-3 py-1 rounded-lg">
                  {currentMatch.currentTime}
                </div>
              )}
            </div>

            <div className="relative rounded-2xl overflow-hidden">
              <AnimatePresence mode="wait">
                {viewMode === "viz" ? (
                  <motion.div
                    key="viz"
                    initial={{ opacity: 0, x: 20 }}
                    animate={{ opacity: 1, x: 0 }}
                    exit={{ opacity: 0, x: -20 }}
                  >
                    <LivePitch
                      sportKey={event.sportKey}
                      zone={currentMatch?.currentZone || "Midfield"}
                      action={currentMatch?.currentAction || "Waiting..."}
                    />
                  </motion.div>
                ) : (
                  <motion.div
                    key="stream"
                    initial={{ opacity: 0, x: 20 }}
                    animate={{ opacity: 1, x: 0 }}
                    exit={{ opacity: 0, x: -20 }}
                    className="aspect-[16/9] bg-black rounded-2xl overflow-hidden shadow-2xl"
                  >
                    <iframe
                      src={currentMatch?.streamUrl || ""}
                      className="w-full h-full border-0"
                      allowFullScreen
                      allow="autoplay; encrypted-media"
                    />
                  </motion.div>
                )}
              </AnimatePresence>
            </div>

            {/* Bottom Info: Momentum & Stats */}
            <div className="mt-8 grid grid-cols-1 md:grid-cols-2 gap-8 items-center border-t border-dark-700 pt-8">
              <div className="space-y-4">
                <div className="flex justify-between items-end">
                  <h3 className="text-[10px] font-black text-gray-500 uppercase tracking-widest">
                    Pressure Momentum
                  </h3>
                  <div className="text-[10px] font-bold text-primary-400">
                    {currentMatch?.momentum}%
                  </div>
                </div>
                <div className="h-4 bg-dark-900 rounded-full overflow-hidden flex border border-dark-700 p-0.5">
                  <motion.div
                    animate={{ width: `${currentMatch?.momentum || 50}%` }}
                    className="h-full bg-gradient-to-r from-primary-600 to-primary-400 rounded-full shadow-[0_0_10px_rgba(20,184,166,0.3)]"
                  />
                </div>
                <div className="flex justify-between text-[10px] font-bold text-gray-400 px-1">
                  <span>HOME TEAM</span>
                  <span>AWAY TEAM</span>
                </div>
              </div>
              <StatsBars
                sportKey={event.sportKey}
                stats={currentMatch?.statistics || {}}
              />
            </div>
          </div>

          {/* Markets Grid */}
          <div className="bg-dark-800 border border-dark-700 rounded-3xl p-6">
            <h2 className="text-sm font-bold text-white uppercase tracking-wider mb-6 flex items-center gap-2">
              <span className="w-1 h-4 bg-primary-500 rounded-full" />
              Available Markets
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {Array.isArray(event.markets) && event.markets.length > 0 ? (
                event.markets.map((market: any, mIndex: number) => (
                  <div
                    key={market.id || mIndex}
                    className="bg-dark-900 p-5 rounded-2xl border border-dark-700 group hover:border-primary-500/30 transition-all duration-300 shadow-sm hover:shadow-primary-500/5"
                  >
                    <h3 className="text-[10px] font-black text-gray-500 uppercase mb-4 tracking-widest group-hover:text-primary-400 transition-colors">
                      {market.name}
                    </h3>
                    <div className="flex flex-wrap gap-2">
                      {Array.isArray(market.outcomes) &&
                        market.outcomes.map((outcome: any, oIndex: number) => (
                          <OddButton
                            key={outcome.id || oIndex}
                            name={outcome.name || "-"}
                            odd={outcome.odd || 0}
                            isSelected={
                              outcome.id ? isSelected(outcome.id) : false
                            }
                            onClick={() => {
                              if (outcome.id) {
                                if (isSelected(outcome.id)) {
                                  dispatch(removeSelection(outcome.id));
                                } else {
                                  dispatch(
                                    addSelection({
                                      eventId: event.id,
                                      eventName: event.name || "Unknown Event",
                                      marketId: market.id,
                                      marketName:
                                        market.name || "Unknown Market",
                                      outcomeId: outcome.id,
                                      outcomeName:
                                        outcome.name === "1"
                                          ? event.name?.split(" vs ")[0] ||
                                            "Team 1"
                                          : outcome.name === "2"
                                            ? event.name?.split(" vs ")[1] ||
                                              "Team 2"
                                            : outcome.name,
                                      odd: outcome.odd || 0,
                                    }),
                                  );
                                }
                              }
                            }}
                          />
                        ))}
                    </div>
                  </div>
                ))
              ) : (
                <div className="col-span-full text-center text-gray-500 py-10 border border-dashed border-dark-600 rounded-2xl">
                  No odds available for this event yet.
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default MatchDetailsView;
