import React from "react";
import { motion, AnimatePresence } from "framer-motion";

interface LivePitchProps {
  sportKey: string;
  zone: string;
  action: string;
  homeTeam?: string;
  awayTeam?: string;
  homeScore?: string | number;
  awayScore?: string | number;
  currentTime?: string;
}

const LivePitch: React.FC<LivePitchProps> = ({
  sportKey,
  zone,
  action,
  homeTeam,
  awayTeam,
  homeScore,
  awayScore,
  currentTime,
}) => {
  const isSoccer = sportKey.includes("soccer");
  const isBasketball = sportKey.includes("basketball");
  const isEsport = sportKey.includes("esport");

  const getZoneOverlayClass = () => {
    switch (zone) {
      case "HomeBox":
        return "left-0 w-1/4 bg-red-500/20";
      case "HomeDef":
        return "left-0 w-1/2 bg-red-500/10";
      case "Midfield":
        return "left-1/4 w-1/2 bg-blue-500/10";
      case "AwayDef":
        return "right-0 w-1/2 bg-red-500/10";
      case "AwayBox":
        return "right-0 w-1/4 bg-red-500/20";
      default:
        return "hidden";
    }
  };

  const renderLines = () => {
    if (isSoccer) {
      return (
        <svg className="w-full h-full" viewBox="0 0 100 60">
          <rect
            x="0"
            y="0"
            width="100"
            height="60"
            fill="none"
            stroke="white"
            strokeWidth="0.5"
          />
          <line
            x1="50"
            y1="0"
            x2="50"
            y2="60"
            stroke="white"
            strokeWidth="0.5"
          />
          <circle
            cx="50"
            cy="30"
            r="8"
            fill="none"
            stroke="white"
            strokeWidth="0.5"
          />
          <rect
            x="0"
            y="15"
            width="12"
            height="30"
            fill="none"
            stroke="white"
            strokeWidth="0.5"
          />
          <rect
            x="88"
            y="15"
            width="12"
            height="30"
            fill="none"
            stroke="white"
            strokeWidth="0.5"
          />
        </svg>
      );
    }
    if (isBasketball) {
      return (
        <svg className="w-full h-full" viewBox="0 0 100 60">
          <rect
            x="0"
            y="0"
            width="100"
            height="60"
            fill="none"
            stroke="white"
            strokeWidth="0.8"
          />
          <line
            x1="50"
            y1="0"
            x2="50"
            y2="60"
            stroke="white"
            strokeWidth="0.8"
          />
          <circle
            cx="50"
            cy="30"
            r="10"
            fill="none"
            stroke="white"
            strokeWidth="0.8"
          />
          {/* Trumny */}
          <rect
            x="0"
            y="20"
            width="19"
            height="20"
            fill="none"
            stroke="white"
            strokeWidth="0.8"
          />
          <rect
            x="81"
            y="20"
            width="19"
            height="20"
            fill="none"
            stroke="white"
            strokeWidth="0.8"
          />
          {/* Linie za 3 */}
          <path
            d="M 0 5 Q 40 30 0 55"
            fill="none"
            stroke="white"
            strokeWidth="0.8"
          />
          <path
            d="M 100 5 Q 60 30 100 55"
            fill="none"
            stroke="white"
            strokeWidth="0.8"
          />
        </svg>
      );
    }
    return (
      <div className="absolute inset-0 flex items-center justify-center">
        <div className="w-full h-full bg-[radial-gradient(circle,_var(--tw-gradient-stops))] from-primary-900/20 via-transparent to-transparent" />
        <div className="grid grid-cols-8 grid-rows-4 w-full h-full opacity-10">
          {Array.from({ length: 32 }).map((_, i) => (
            <div key={i} className="border border-primary-500/20" />
          ))}
        </div>
      </div>
    );
  };

  return (
    <div
      className={`relative w-full aspect-[16/9] rounded-2xl overflow-hidden border-2 border-dark-700 shadow-2xl transition-colors duration-700 ${isSoccer ? "bg-green-950" : isBasketball ? "bg-amber-950" : "bg-dark-900"}`}
    >
      {/* Scoreboard Overlay */}
      {(homeTeam || awayTeam) && (
        <div className="absolute top-4 left-0 w-full flex flex-col items-center z-40">
          <div className="bg-black/80 backdrop-blur-md border border-dark-600 rounded-2xl flex items-center px-6 py-2 shadow-xl">
            <div className="text-white font-bold text-sm md:text-base mr-4 text-right min-w-[100px] truncate">
              {homeTeam}
            </div>
            <div className="flex flex-col items-center">
              {currentTime && (
                <div className="text-[10px] font-black text-primary-400 mb-0.5 tracking-wider animate-pulse uppercase">
                  {currentTime}
                </div>
              )}
              <div className="bg-primary-600 text-white font-black text-xl px-4 py-1 rounded-lg leading-none">
                {homeScore} <span className="text-primary-300 mx-1">-</span>{" "}
                {awayScore}
              </div>
            </div>
            <div className="text-white font-bold text-sm md:text-base ml-4 text-left min-w-[100px] truncate">
              {awayTeam}
            </div>
          </div>
        </div>
      )}

      {/* Pitch Lines */}
      <div className="absolute inset-0 opacity-40">{renderLines()}</div>

      {/* Dynamic Zone Overlay */}
      <motion.div
        key={zone}
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        className={`absolute top-0 h-full transition-all duration-1000 ${getZoneOverlayClass()}`}
      />

      {/* Action Indicator */}
      <div className="absolute inset-0 flex items-center justify-center pointer-events-none z-30">
        <AnimatePresence mode="wait">
          <motion.div
            key={action}
            initial={{ scale: 0.5, opacity: 0, rotateX: 45 }}
            animate={{ scale: 1, opacity: 1, rotateX: 0 }}
            exit={{ scale: 1.5, opacity: 0 }}
            className={`px-8 py-4 rounded-xl border-2 text-white font-black text-xl shadow-2xl uppercase tracking-tighter backdrop-blur-md ${
              action.includes("GOAL") ||
              action.includes("POINTER") ||
              action.includes("ELIMINATED")
                ? "bg-red-600 border-red-400 animate-bounce"
                : "bg-primary-600/80 border-primary-400"
            }`}
          >
            {action}
          </motion.div>
        </AnimatePresence>
      </div>

      {/* Ball / Focus Point */}
      <motion.div
        animate={{
          x: zone.includes("Away")
            ? "80%"
            : zone.includes("Home")
              ? "15%"
              : "48%",
          y: "45%",
        }}
        transition={{ type: "spring", stiffness: 50 }}
        className={`absolute w-6 h-6 rounded-full shadow-2xl border-2 z-20 ${isEsport ? "bg-primary-400 border-primary-200" : "bg-white border-primary-500"}`}
      >
        <div className="absolute inset-0 bg-primary-500 rounded-full animate-ping opacity-75" />
      </motion.div>

      {/* Ambient particles for Esport */}
      {isEsport && (
        <div className="absolute inset-0 pointer-events-none">
          <div className="absolute top-10 left-10 w-24 h-24 bg-primary-500/10 blur-3xl animate-pulse" />
          <div className="absolute bottom-10 right-10 w-32 h-32 bg-blue-500/10 blur-3xl animate-pulse" />
        </div>
      )}
    </div>
  );
};

export default LivePitch;
