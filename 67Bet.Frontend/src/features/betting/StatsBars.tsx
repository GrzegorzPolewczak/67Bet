import React from 'react';
import { motion } from 'framer-motion';

interface StatsBarsProps {
  sportKey: string;
  stats: Record<string, number>;
}

const StatsBars: React.FC<StatsBarsProps> = ({ sportKey, stats }) => {
  const isSoccer = sportKey.includes('soccer');
  const isBasketball = sportKey.includes('basketball');
  const isTennis = sportKey.includes('tennis');
  const isEsport = sportKey.includes('esport');

  // Funkcja do renderowania paska porównawczego (np. Posiadanie, Celność)
  const renderComparisonBar = (label: string, homeVal: number, awayVal: number, unit: string = "") => {
    const total = homeVal + awayVal;
    const homePercent = total > 0 ? (homeVal / total) * 100 : 50;
    
    return (
      <div className="space-y-2">
        <div className="flex justify-between text-[10px] font-black text-gray-500 uppercase tracking-widest">
          <span>{label}</span>
          <span>{homeVal}{unit} - {awayVal}{unit}</span>
        </div>
        <div className="h-1.5 bg-dark-900 rounded-full overflow-hidden flex border border-dark-700/50">
          <motion.div 
            initial={{ width: 0 }}
            animate={{ width: `${homePercent}%` }}
            className="h-full bg-primary-500 shadow-[0_0_8px_rgba(20,184,166,0.4)]"
          />
        </div>
      </div>
    );
  };

  return (
    <div className="space-y-5">
      {isSoccer && (
        <>
          {renderComparisonBar("Possession", stats["PossessionHome"] || 50, 100 - (stats["PossessionHome"] || 50), "%")}
          <div className="grid grid-cols-2 gap-3">
             <StatBox label="Corners" val={stats["Corners"] || 0} />
             <StatBox label="Shots" val={stats["ShotsOnTarget"] || 0} />
          </div>
        </>
      )}

      {isBasketball && (
        <>
          {renderComparisonBar("Field Goals", stats["FGPercentHome"] || 45, stats["FGPercentAway"] || 42, "%")}
          <div className="grid grid-cols-2 gap-3">
             <StatBox label="3-Pointers" val={stats["ThreePointers"] || 0} />
             <StatBox label="Rebounds" val={stats["Rebounds"] || 0} />
          </div>
        </>
      )}

      {isTennis && (
        <>
          {renderComparisonBar("Win Probability", stats["WinProbHome"] || 50, 100 - (stats["WinProbHome"] || 50), "%")}
          <div className="grid grid-cols-2 gap-3">
             <StatBox label="Aces" val={stats["Aces"] || 0} />
             <StatBox label="Double Faults" val={stats["DoubleFaults"] || 0} />
          </div>
        </>
      )}

      {isEsport && (
        <>
          {sportKey.includes('csgo') ? (
            <>
              {renderComparisonBar("Team Economy", stats["EconomyHome"] || 50, stats["EconomyAway"] || 48, "$k")}
              <div className="grid grid-cols-2 gap-3">
                 <StatBox label="Rounds" val={`${stats["RoundsHome"] || 0} - ${stats["RoundsAway"] || 0}`} isString />
                 <StatBox label="Bomb Status" val={stats["BombPlanted"] === 1 ? "PLANTED" : "CLEAR"} isString />
              </div>
            </>
          ) : (
            <>
              {renderComparisonBar("Map Control", stats["MapControl"] || 50, 100 - (stats["MapControl"] || 50), "%")}
              <div className="grid grid-cols-2 gap-3">
                 <StatBox label="Objectives" val={stats["Objectives"] || 0} />
                 <StatBox label="Gold Lead" val={stats["GoldLead"] || 0} unit="k" />
              </div>
            </>
          )}
        </>
      )}

      {!isSoccer && !isBasketball && !isTennis && !isEsport && (
        <div className="grid grid-cols-2 gap-3">
           {Object.entries(stats).slice(0, 4).map(([key, val]) => (
             <StatBox key={key} label={key} val={val} />
           ))}
        </div>
      )}
    </div>
  );
};

const StatBox = ({ label, val, unit = "", isString = false }: { label: string, val: number | string, unit?: string, isString?: boolean }) => (
  <div className="bg-dark-900/40 p-2.5 rounded-xl border border-dark-700/50 text-center hover:bg-dark-800 transition-colors">
    <span className="block text-[9px] text-gray-500 font-black uppercase mb-0.5 tracking-tighter">{label}</span>
    <span className="text-base font-black text-white">{val}{!isString && unit}</span>
  </div>
);

export default StatsBars;
