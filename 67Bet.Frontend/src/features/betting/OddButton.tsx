import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';

interface OddButtonProps {
  name: string;
  odd: number;
  isSelected: boolean;
  onClick: () => void;
}

const OddButton: React.FC<OddButtonProps> = ({ name, odd, isSelected, onClick }) => {
  const numericOdd = Number(odd) || 0;
  const [prevOdd, setPrevOdd] = useState(numericOdd);
  const [change, setChange] = useState<'up' | 'down' | null>(null);

  useEffect(() => {
    if (numericOdd > prevOdd) {
      setChange('up');
      const timer = setTimeout(() => setChange(null), 2000);
      return () => clearTimeout(timer);
    } else if (numericOdd < prevOdd) {
      setChange('down');
      const timer = setTimeout(() => setChange(null), 2000);
      return () => clearTimeout(timer);
    }
    setPrevOdd(numericOdd);
  }, [numericOdd, prevOdd]);

  return (
    <button
      onClick={onClick}
      className={`relative flex flex-col items-center justify-center w-24 py-2.5 rounded-xl border transition-all overflow-hidden ${
        isSelected
          ? 'bg-primary-600 border-primary-500 text-white shadow-lg shadow-primary-600/20'
          : 'bg-dark-700 border-dark-600 text-gray-400 hover:border-primary-500 hover:text-white'
      }`}
    >
      <AnimatePresence>
        {change && (
          <motion.div
            initial={{ opacity: 0, scale: 0.8 }}
            animate={{ opacity: 0.2, scale: 1.2 }}
            exit={{ opacity: 0 }}
            className={`absolute inset-0 ${
              change === 'up' ? 'bg-accent-success' : 'bg-accent-danger'
            }`}
          />
        )}
      </AnimatePresence>

      <span className="text-[10px] font-bold uppercase mb-0.5 opacity-60 z-10">
        {name}
      </span>
      <span className={`text-sm font-black tracking-tighter z-10 flex items-center gap-1 transition-colors ${
        change === 'up' ? 'text-accent-success' : change === 'down' ? 'text-accent-danger' : ''
      }`}>
        {numericOdd > 0 ? Number(numericOdd).toFixed(2) : '-'}
        {change && (
          <motion.span
            initial={{ y: change === 'up' ? 5 : -5, opacity: 0 }}
            animate={{ y: 0, opacity: 1 }}
            className="text-[10px]"
          >
            {change === 'up' ? '▲' : '▼'}
          </motion.span>
        )}
      </span>
    </button>
  );
};

export default OddButton;
