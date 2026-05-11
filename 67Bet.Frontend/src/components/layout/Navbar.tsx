import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { Wallet, Menu, Search, Bell } from 'lucide-react';
import { useSelector, useDispatch } from 'react-redux';
import type { RootState } from '../../app/store';
import { toggleBetslip } from '../../features/betslip/betslipSlice';
import { motion, AnimatePresence } from 'framer-motion';

const Navbar: React.FC = () => {
  const dispatch = useDispatch();
  const { isAuthenticated, user } = useSelector((state: RootState) => state.auth);
  const betSelectionsCount = useSelector((state: RootState) => state.betslip.selections.length);
  const [isLogoHovered, setIsLogoHovered] = useState(false);

  return (
    <nav className="bg-dark-800 border-b border-dark-700 h-16 flex items-center justify-between px-6 z-50">
      <div className="flex items-center gap-4">
        <Menu className="text-gray-400 cursor-pointer lg:hidden" />
        <Link 
          to="/" 
          className="text-3xl font-black text-primary-500 tracking-tighter italic flex items-center group"
          onMouseEnter={() => setIsLogoHovered(true)}
          onMouseLeave={() => setIsLogoHovered(false)}
        >
          <div className="flex items-center">
            <motion.span
              animate={isLogoHovered ? { 
                y: [-8, 8, -8], 
                rotate: [-5, 5, -5] 
              } : { y: 0, rotate: 0 }}
              transition={isLogoHovered ? { 
                duration: 0.6, 
                repeat: Infinity, 
                ease: "easeInOut" 
              } : { 
                type: "spring", 
                stiffness: 200, 
                damping: 25,
                mass: 1.2
              }}
              className="inline-block"
            >
              6
            </motion.span>
            <motion.span
              animate={isLogoHovered ? { 
                y: [8, -8, 8], 
                rotate: [5, -5, 5] 
              } : { y: 0, rotate: 0 }}
              transition={isLogoHovered ? { 
                duration: 0.6, 
                repeat: Infinity, 
                ease: "easeInOut" 
              } : { 
                type: "spring", 
                stiffness: 200, 
                damping: 25,
                mass: 1.2
              }}
              className="inline-block"
            >
              7
            </motion.span>
          </div>
          <span className="text-white ml-1 group-hover:text-primary-400 transition-colors">BET</span>
        </Link>
      </div>

      <div className="hidden md:flex items-center flex-1 max-w-xl mx-8">
        <div className="relative w-full">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500 w-4 h-4" />
          <input
            type="text"
            placeholder="Search for sports, teams or events..."
            className="w-full bg-dark-900 border border-dark-600 rounded-full py-2 pl-10 pr-4 text-sm focus:outline-none focus:border-primary-500 transition-colors"
          />
        </div>
      </div>

      <div className="flex items-center gap-6">
        {isAuthenticated ? (
          <>
            <div className="flex flex-col items-end">
              <span className="text-xs text-gray-400 font-medium">Balance</span>
              <div className="flex items-center gap-2 text-accent-success font-bold">
                <Wallet className="w-4 h-4" />
                <span>$1,240.00</span>
              </div>
            </div>
            
            <button className="relative text-gray-400 hover:text-white transition-colors">
              <Bell className="w-6 h-6" />
              <span className="absolute -top-1 -right-1 bg-primary-500 text-white text-[10px] w-4 h-4 rounded-full flex items-center justify-center">2</span>
            </button>

            <button 
              onClick={() => dispatch(toggleBetslip())}
              className="relative p-2 bg-dark-700 rounded-lg hover:bg-dark-600 transition-colors"
            >
              <div className="text-xs font-bold px-2 py-1 bg-primary-600 rounded text-white mb-1">SLIP</div>
              {betSelectionsCount > 0 && (
                <span className="absolute -top-2 -right-2 bg-accent-danger text-white text-[10px] w-5 h-5 rounded-full flex items-center justify-center border-2 border-dark-800 font-bold">
                  {betSelectionsCount}
                </span>
              )}
            </button>

            <div className="flex items-center gap-3 pl-4 border-l border-dark-600">
              <div className="w-9 h-9 bg-primary-600 rounded-full flex items-center justify-center text-sm font-bold">
                {user?.username?.[0]?.toUpperCase() || 'U'}
              </div>
            </div>
          </>
        ) : (
          <div className="flex items-center gap-4">
            <Link to="/login" className="text-sm font-semibold hover:text-primary-500 transition-colors">Login</Link>
            <Link to="/register" className="bg-primary-600 hover:bg-primary-700 text-white px-5 py-2 rounded-lg text-sm font-bold transition-all transform hover:scale-105 active:scale-95">
              Join Now
            </Link>
          </div>
        )}
      </div>
    </nav>
  );
};

export default Navbar;
