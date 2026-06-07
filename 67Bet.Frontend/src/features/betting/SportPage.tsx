import React, { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { useDispatch, useSelector } from "react-redux";
import { addSelection, removeSelection } from "../betslip/betslipSlice";
import { fetchEventsAsync } from "./bettingSlice";
import type { RootState, AppDispatch } from "../../app/store";
import { motion, AnimatePresence } from "framer-motion";
import { Clock, ChevronLeft, Loader2, Info } from "lucide-react";
import OddButton from "./OddButton";

const SportPage: React.FC = () => {
  const { sportName } = useParams<{ sportName: string }>();
  const dispatch = useDispatch<AppDispatch>();
  const selections = useSelector(
    (state: RootState) => state.betslip.selections,
  );
  const { events, loading, error } = useSelector(
    (state: RootState) => state.betting,
  );
  const [now] = useState(() => Date.now());

  useEffect(() => {
    if (events.length === 0) {
      dispatch(fetchEventsAsync());
    }
  }, [dispatch, events.length]);

  // Try to match sportName with League or Event Name to simulate categorization
  // In a real app, 'Sport' would be a direct property on the Event object from the API.
  const filteredEvents = Array.isArray(events)
    ? events.filter((e) => {
        if (!e) return false;
        const safeSportName = sportName?.toLowerCase() || "";
        const safeLeague = e.league?.toLowerCase() || "";
        const safeName = e.name?.toLowerCase() || "";
        const safeSportKey = e.sportKey?.toLowerCase() || "";

        if (safeSportName === "popular") return true;

        // Live: Wydarzenia trwające lub zaczynające się za mniej niż 2 godziny
        if (safeSportName === "live") {
          const eventTime = new Date(e.rawTime).getTime();
          return eventTime <= now + 2 * 60 * 60 * 1000;
        }

        // Exact mapping based on sportKey
        if (safeSportName === "football" && safeSportKey.includes("soccer"))
          return true;
        if (
          safeSportName === "basketball" &&
          safeSportKey.includes("basketball")
        )
          return true;
        if (safeSportName === "esports" && safeSportKey.includes("esports"))
          return true;
        if (safeSportName === "mma" && safeSportKey.includes("mma"))
          return true;

        // Fallback for custom search/clicks
        if (
          safeLeague.includes(safeSportName) ||
          safeName.includes(safeSportName)
        )
          return true;

        return false;
      })
    : [];

  const isSelected = (outcomeId: string) =>
    selections.some((s) => s.outcomeId === outcomeId);

  return (
    <div className="max-w-4xl mx-auto space-y-6 pb-12">
      <div className="flex items-center gap-4 mb-2">
        <Link
          to="/"
          className="text-gray-500 hover:text-white transition-colors"
        >
          <ChevronLeft className="w-5 h-5" />
        </Link>
        <h1 className="text-3xl font-black text-white">{sportName}</h1>
      </div>

      <div className="flex items-center justify-between">
        <span className="text-sm text-gray-500 font-bold uppercase">
          {filteredEvents.length} Active Events
        </span>
        <button
          onClick={() => dispatch(fetchEventsAsync())}
          className="text-xs font-bold text-gray-400 hover:text-white transition-colors"
        >
          Refresh Data
        </button>
      </div>

      {error && (
        <div className="bg-red-500/10 border border-red-500/50 text-red-500 text-xs font-bold p-4 rounded-xl text-center">
          {error}
        </div>
      )}

      <div className="grid gap-4">
        {loading && events.length === 0 ? (
          <div className="flex flex-col items-center justify-center h-64 space-y-4">
            <Loader2 className="w-10 h-10 text-primary-500 animate-spin" />
            <p className="text-gray-400 font-bold">Loading events...</p>
          </div>
        ) : filteredEvents.length > 0 ? (
          <AnimatePresence mode="popLayout">
            {filteredEvents.map((event) => (
              <motion.div
                key={event.id}
                layout
                initial={{ opacity: 0, y: 10 }}
                animate={{ opacity: 1, y: 0 }}
                className="bg-dark-800 border border-dark-700 rounded-2xl p-5 hover:border-dark-600 transition-colors"
              >
                <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                  <div className="flex-1">
                    <div className="flex items-center gap-3 text-xs font-bold text-gray-500 mb-2 uppercase">
                      <span>{event.league}</span>
                      {event.source === "external" && (
                        <span className="rounded-full border border-blue-500/30 bg-blue-500/10 px-2 py-0.5 text-[9px] text-blue-300">
                          External API
                        </span>
                      )}
                      <div className="w-1 h-1 bg-dark-600 rounded-full" />
                      <div className="flex items-center gap-1">
                        <Clock className="w-3 h-3" />
                        {event.time || "Scheduled"}
                      </div>
                    </div>
                    <Link to={`/match/${event.id}`}>
                      <h3 className="text-lg font-bold text-white hover:text-primary-500 cursor-pointer transition-colors">
                        {event.name}
                      </h3>
                    </Link>
                  </div>

                  <div className="flex items-center gap-2">
                    {Array.isArray(event.markets) &&
                    event.markets.length > 0 &&
                    Array.isArray(event.markets[0]?.outcomes) ? (
                      event.markets[0].outcomes.map((outcome, index) => (
                        <OddButton
                          key={outcome?.id || index}
                          name={outcome?.name || "-"}
                          odd={outcome?.odd || 0}
                          isSelected={
                            outcome?.id && outcome?.isBettable
                              ? isSelected(outcome.id)
                              : false
                          }
                          disabled={!event.isBettable || !outcome?.isBettable}
                          title={
                            event.isBettable && outcome?.isBettable
                              ? undefined
                              : "Kurs z zewnętrznego API jest tylko podglądem. Do kuponu można dodawać rynki z Betting API i Virtual Racing."
                          }
                          onClick={() => {
                            const market = event.markets[0];
                            if (
                              market &&
                              event.isBettable &&
                              outcome?.id &&
                              outcome?.isBettable
                            ) {
                              if (isSelected(outcome.id)) {
                                dispatch(removeSelection(outcome.id));
                              } else {
                                dispatch(
                                  addSelection({
                                    eventId: event.id,
                                    eventName: event.name || "Unknown Event",
                                    marketId: market.id,
                                    marketName: market.name || "Unknown Market",
                                    outcomeId: outcome.id,
                                    outcomeName:
                                      outcome.name === "1"
                                        ? event.name?.split(" vs ")[0] ||
                                          "Team 1"
                                        : outcome.name === "2"
                                          ? event.name?.split(" vs ")[1] ||
                                            "Team 2"
                                          : "Draw",
                                    odd: outcome.odd || 0,
                                  }),
                                );
                              }
                            }
                          }}
                        />
                      ))
                    ) : (
                      <span className="text-xs text-gray-500 font-bold border border-dark-600 border-dashed px-4 py-2 rounded-xl">
                        Odds upcoming
                      </span>
                    )}
                  </div>
                </div>
              </motion.div>
            ))}
          </AnimatePresence>
        ) : (
          <div className="h-64 flex flex-col items-center justify-center text-center p-6 space-y-4 bg-dark-800 rounded-3xl border border-dark-700 border-dashed">
            <div className="w-16 h-16 bg-dark-700 rounded-full flex items-center justify-center">
              <Info className="w-8 h-8 text-gray-500" />
            </div>
            <div>
              <p className="text-lg font-bold text-white">
                No active events for {sportName}
              </p>
              <p className="text-sm text-gray-500 mt-1">
                Check back later or try our AI Custom Bet feature!
              </p>
            </div>
            <Link
              to="/custom-bet"
              className="text-primary-500 font-bold hover:underline text-sm mt-4 inline-block"
            >
              Create Custom Bet
            </Link>
          </div>
        )}
      </div>
    </div>
  );
};

export default SportPage;
