import React, { useEffect, useState, useRef } from "react";
import { motion, useAnimation } from "framer-motion";

const ROULETTE_NUMBERS = [
  0, 32, 15, 19, 4, 21, 2, 25, 17, 34, 6, 27, 13, 36, 11, 30, 8, 23, 10, 5, 24, 16, 33, 1, 20, 14, 31, 9, 22, 18, 29, 7, 28, 12, 35, 3, 26
];

const RED_NUMBERS = new Set([
  1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36,
]);

const getColor = (num: number) => {
  if (num === 0) return "bg-green-500 border-green-400";
  if (RED_NUMBERS.has(num)) return "bg-red-600 border-red-400";
  return "bg-zinc-900 border-zinc-700";
};

interface RouletteSliderProps {
  spinAnim: boolean;
  targetNumber: number | null;
  onSpinEnd: () => void;
}

const ITEM_WIDTH = 80; // width + gap/margin

const RouletteSlider: React.FC<RouletteSliderProps> = ({ spinAnim, targetNumber, onSpinEnd }) => {
  const containerRef = useRef<HTMLDivElement>(null);
  const controls = useAnimation();
  const [strip, setStrip] = useState<number[]>([]);

  useEffect(() => {
    // Generate an initial random strip to display
    const initialStrip = [];
    for (let i = 0; i < 5; i++) initialStrip.push(...ROULETTE_NUMBERS);
    setStrip(initialStrip);
  }, []);

  useEffect(() => {
    if (spinAnim && targetNumber !== null && containerRef.current) {
      // Create a strip where the target number is guaranteed to be around index 80-100
      const newStrip = [];
      for (let i = 0; i < 5; i++) newStrip.push(...ROULETTE_NUMBERS);
      
      // We want to land on the target number in the 3rd or 4th block
      let targetIndex = newStrip.findIndex((n, idx) => n === targetNumber && idx > 100);
      if (targetIndex === -1) targetIndex = 110; // Fallback

      setStrip(newStrip);

      const containerWidth = containerRef.current.clientWidth;
      
      // Calculate target X
      // We want the center of the item at targetIndex to be at containerWidth / 2
      // Add a tiny random offset to make it look realistic (not always perfectly centered)
      const randomOffset = (Math.random() - 0.5) * (ITEM_WIDTH - 10);
      const targetX = (containerWidth / 2) - (targetIndex * ITEM_WIDTH + ITEM_WIDTH / 2) + randomOffset;

      // Start the animation
      controls.set({ x: 0 });
      controls.start({
        x: targetX,
        transition: {
          duration: 5,
          ease: [0.15, 1, 0.3, 1], // Custom cubic bezier for smooth deceleration
        }
      }).then(() => {
        onSpinEnd();
      });
    }
  }, [spinAnim, targetNumber]);

  return (
    <div ref={containerRef} className="relative w-full h-32 bg-dark-900 rounded-2xl border border-dark-700 overflow-hidden shadow-inner flex items-center">
      {/* Center Pointer */}
      <div className="absolute left-1/2 top-0 bottom-0 w-1 bg-yellow-400 z-10 -translate-x-1/2 shadow-[0_0_10px_rgba(250,204,21,0.8)]" />
      <div className="absolute left-1/2 top-0 w-4 h-4 bg-yellow-400 z-10 -translate-x-1/2 rotate-45 -translate-y-2" />
      <div className="absolute left-1/2 bottom-0 w-4 h-4 bg-yellow-400 z-10 -translate-x-1/2 rotate-45 translate-y-2" />

      {/* Shadows for edges */}
      <div className="absolute left-0 top-0 bottom-0 w-16 bg-gradient-to-r from-dark-900 to-transparent z-10" />
      <div className="absolute right-0 top-0 bottom-0 w-16 bg-gradient-to-l from-dark-900 to-transparent z-10" />

      <motion.div
        animate={controls}
        className="flex items-center whitespace-nowrap"
        style={{ x: 0 }}
      >
        {strip.map((num, i) => (
          <div
            key={i}
            className={`shrink-0 flex items-center justify-center font-black text-2xl text-white rounded-xl mx-1 shadow-lg border-b-4 ${getColor(num)}`}
            style={{ width: ITEM_WIDTH - 8, height: ITEM_WIDTH - 8 }}
          >
            {num}
          </div>
        ))}
      </motion.div>
    </div>
  );
};

export default RouletteSlider;
