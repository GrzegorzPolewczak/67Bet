import React, { useState, useEffect } from "react";
import { Link, useNavigate } from "react-router-dom";
import { Wallet, Menu, Search, Bell, LogOut, Gift } from "lucide-react";
import { useSelector, useDispatch } from "react-redux";
import type { RootState, AppDispatch } from "../../app/store";
import { toggleBetslip } from "../../features/betslip/betslipSlice";
import { logout } from "../../features/auth/authSlice";
import { fetchBalanceAsync } from "../../features/wallet/walletSlice";
import { motion } from "framer-motion";

const Navbar: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const navigate = useNavigate();
  const { isAuthenticated, user } = useSelector(
    (state: RootState) => state.auth,
  );
  const { balance, freebetBalance } = useSelector(
    (state: RootState) => state.wallet,
  );
  const betSelectionsCount = useSelector(
    (state: RootState) => state.betslip.selections.length,
  );
  const [isLogoHovered, setIsLogoHovered] = useState(false);

  useEffect(() => {
    if (isAuthenticated) {
      dispatch(fetchBalanceAsync());
    }
  }, [isAuthenticated, dispatch]);

  const handleLogout = () => {
    dispatch(logout());
    navigate("/login");
  };

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
              animate={
                isLogoHovered
                  ? {
                      y: [-8, 8, -8],
                      rotate: [-5, 5, -5],
                    }
                  : { y: 0, rotate: 0 }
              }
              transition={
                isLogoHovered
                  ? {
                      duration: 0.6,
                      repeat: Infinity,
                      ease: "easeInOut",
                    }
                  : {
                      type: "spring",
                      stiffness: 200,
                      damping: 25,
                      mass: 1.2,
                    }
              }
              className="inline-block"
            >
              6
            </motion.span>
            <motion.span
              animate={
                isLogoHovered
                  ? {
                      y: [8, -8, 8],
                      rotate: [5, -5, 5],
                    }
                  : { y: 0, rotate: 0 }
              }
              transition={
                isLogoHovered
                  ? {
                      duration: 0.6,
                      repeat: Infinity,
                      ease: "easeInOut",
                    }
                  : {
                      type: "spring",
                      stiffness: 200,
                      damping: 25,
                      mass: 1.2,
                    }
              }
              className="inline-block"
            >
              7
            </motion.span>
          </div>
          <span className="text-white ml-1 group-hover:text-primary-400 transition-colors">
            BET
          </span>
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
            <div className="flex items-center gap-4">
              <div className="flex flex-col items-end">
                <span className="text-[10px] text-gray-400 font-bold uppercase tracking-wider">
                  Saldo
                </span>
                <div className="flex flex-col items-end gap-0.5">
                  <div className="flex items-center gap-1.5 text-accent-success font-black text-sm">
                    <Wallet className="w-3.5 h-3.5" />
                    <span>{Number(balance || 0).toFixed(2)} PLN</span>
                  </div>
                  {freebetBalance > 0 && (
                    <div className="flex items-center gap-1 text-primary-400 font-bold text-[10px] bg-primary-500/10 px-1.5 rounded-full border border-primary-500/20">
                      <Gift className="w-2.5 h-2.5" />
                      <span>{Number(freebetBalance).toFixed(2)} FREEBET</span>
                    </div>
                  )}
                </div>
              </div>
              <div className="flex flex-col gap-1">
                <Link
                  to="/deposit"
                  className="bg-accent-success hover:bg-green-600 text-white px-3 py-1 rounded-lg text-[9px] font-black transition-all transform hover:scale-105 active:scale-95 text-center"
                >
                  DEPOSIT
                </Link>
                <Link
                  to="/withdraw"
                  className="bg-dark-600 hover:bg-dark-500 text-gray-200 px-3 py-1 rounded-lg text-[9px] font-black transition-all transform hover:scale-105 active:scale-95 text-center border border-dark-500"
                >
                  WITHDRAW
                </Link>
              </div>
            </div>

            <Link
              to="/settings"
              className="relative text-gray-400 hover:text-white transition-colors"
            >
              <Bell className="w-6 h-6" />
              <span className="absolute -top-1 -right-1 bg-primary-500 text-white text-[10px] w-4 h-4 rounded-full flex items-center justify-center">
                2
              </span>
            </Link>

            <button
              onClick={() => dispatch(toggleBetslip())}
              className="relative p-2 bg-dark-700 rounded-lg hover:bg-dark-600 transition-colors"
            >
              <div className="text-xs font-bold px-2 py-1 bg-primary-600 rounded text-white">
                SLIP
              </div>
              {betSelectionsCount > 0 && (
                <span className="absolute -top-2 -right-2 bg-accent-danger text-white text-[10px] w-5 h-5 rounded-full flex items-center justify-center border-2 border-dark-800 font-bold">
                  {betSelectionsCount}
                </span>
              )}
            </button>

            <div className="flex items-center gap-3 pl-4 border-l border-dark-600">
              <Link
                to="/settings"
                className="w-9 h-9 bg-primary-600 rounded-full flex items-center justify-center text-sm font-bold hover:bg-primary-500 transition-colors cursor-pointer text-white"
              >
                {user?.username?.[0]?.toUpperCase() || "U"}
              </Link>
              <button
                onClick={handleLogout}
                className="text-gray-500 hover:text-white transition-colors"
                title="Log Out"
              >
                <LogOut className="w-5 h-5" />
              </button>
            </div>
          </>
        ) : (
          <div className="flex items-center gap-4">
            <Link
              to="/login"
              className="text-sm font-semibold hover:text-primary-500 transition-colors"
            >
              Login
            </Link>
            <Link
              to="/register"
              className="bg-primary-600 hover:bg-primary-700 text-white px-5 py-2 rounded-lg text-sm font-bold transition-all transform hover:scale-105 active:scale-95"
            >
              Join Now
            </Link>
          </div>
        )}
      </div>
    </nav>
  );
};

export default Navbar;
