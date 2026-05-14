import React from 'react';
import { motion } from 'framer-motion';

interface TimelineEvent {
  type: string;
  minute: string;
  description: string;
  team: string;
}

interface MatchTimelineProps {
  events: TimelineEvent[];
}

const MatchTimeline: React.FC<MatchTimelineProps> = ({ events }) => {
  if (!events || events.length === 0) return <div className="text-gray-600 italic text-sm text-center py-4">No events yet...</div>;

  return (
    <div className="space-y-3 max-h-[300px] overflow-y-auto pr-2 custom-scrollbar">
      {events.map((evt, idx) => (
        <motion.div 
          key={idx}
          initial={{ x: -20, opacity: 0 }}
          animate={{ x: 0, opacity: 1 }}
          className={`flex items-center gap-3 p-3 rounded-xl border ${evt.type === 'Goal' ? 'bg-primary-500/10 border-primary-500/30' : 'bg-dark-900/30 border-dark-700'}`}
        >
          <div className="text-xs font-black text-primary-500 w-8">{evt.minute}</div>
          <div className="flex-1">
            <p className={`text-sm font-bold ${evt.type === 'Goal' ? 'text-white' : 'text-gray-300'}`}>{evt.description}</p>
          </div>
          <div className="text-[10px] font-black text-gray-500 uppercase">{evt.team}</div>
        </motion.div>
      ))}
    </div>
  );
};

export default MatchTimeline;
