import React from "react";
import { useSelector, useDispatch } from "react-redux";
import type { RootState, AppDispatch } from "../../app/store";
import { X, Trash2, Info, Loader2 } from "lucide-react";
import {
  removeSelection,
  clearBetslip,
  setStake,
  toggleBetslip,
  placeBetAsync,
} from "./betslipSlice";
import { fetchBalanceAsync } from "../wallet/walletSlice";
import { motion, AnimatePresence } from "framer-motion";
import toast from "react-hot-toast";

const BetSlip: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const { selections, stake, loading } = useSelector(
    (state: RootState) => state.betslip,
  );
  const { isAuthenticated } = useSelector((state: RootState) => state.auth);

  const totalOdds = selections.reduce((acc, curr) => acc * curr.odd, 1);
  const potentialPayout = stake * totalOdds;

  const handlePlaceBet = async () => {
    if (!isAuthenticated) {
      toast.error("You must be logged in to place a bet.");
      return;
    }

    if (stake <= 0) {
      toast.error("Stake must be greater than 0.");
      return;
    }

    const resultAction = await dispatch(placeBetAsync());

    if (placeBetAsync.fulfilled.match(resultAction)) {
      toast.success("Bet placed successfully!");
      dispatch(fetchBalanceAsync()); // Refresh balance after bet
    } else {
      toast.error((resultAction.payload as string) || "Failed to place bet.");
    }
  };

  return (
    <div className="flex flex-col h-full bg-dark-800">
      <div className="p-4 border-b border-dark-700 flex items-center justify-between">
        <div className="flex items-center gap-2">
          <h2 className="font-bold text-sm uppercase tracking-wider">
            Bet Slip
          </h2>
          <span className="bg-primary-600 text-[10px] font-bold px-1.5 py-0.5 rounded">
            {selections.length}
          </span>
        </div>
        <button
          onClick={() => dispatch(toggleBetslip())}
          className="text-gray-500 hover:text-white transition-colors"
        >
          <X className="w-5 h-5" />
        </button>
      </div>

      <div className="flex-1 overflow-y-auto p-3 space-y-3">
        {selections.length === 0 ? (
          <div className="h-full flex flex-col items-center justify-center text-center p-6 space-y-4">
            <div className="w-16 h-16 bg-dark-700 rounded-full flex items-center justify-center">
              <Info className="w-8 h-8 text-gray-500" />
            </div>
            <div>
              <p className="text-sm font-bold text-gray-300">
                Your slip is empty
              </p>
              <p className="text-xs text-gray-500 mt-1">
                Add events to start betting
              </p>
            </div>
          </div>
        ) : (
          <AnimatePresence>
            {selections.map((selection) => (
              <motion.div
                key={selection.outcomeId}
                initial={{ opacity: 0, x: 20 }}
                animate={{ opacity: 1, x: 0 }}
                exit={{ opacity: 0, x: -20 }}
                className="bg-dark-700 rounded-xl p-3 border border-dark-600 relative group"
              >
                <button
                  onClick={() => dispatch(removeSelection(selection.outcomeId))}
                  className="absolute top-2 right-2 text-gray-500 hover:text-accent-danger opacity-0 group-hover:opacity-100 transition-opacity"
                >
                  <Trash2 className="w-4 h-4" />
                </button>
                <div className="pr-6">
                  <p className="text-[10px] font-bold text-primary-500 uppercase">
                    {selection.marketName}
                  </p>
                  <p className="text-xs font-bold mt-0.5">
                    {selection.outcomeName}
                  </p>
                  <p className="text-[10px] text-gray-400 mt-1">
                    {selection.eventName}
                  </p>
                </div>
                <div className="mt-2 flex items-center justify-between">
                  <span className="text-xs font-black text-white bg-dark-600 px-2 py-1 rounded">
                    @{Number(selection.odd).toFixed(2)}
                  </span>
                </div>
              </motion.div>
            ))}
          </AnimatePresence>
        )}
      </div>

      {selections.length > 0 && (
        <div className="p-4 bg-dark-700/50 border-t border-dark-700 space-y-4">
          <div className="flex items-center justify-between text-xs font-bold text-gray-400">
            <span>Total Odds</span>
            <span className="text-white">@{Number(totalOdds).toFixed(2)}</span>
          </div>

          <div className="space-y-2">
            <label className="text-[10px] font-bold text-gray-500 uppercase px-1">
              Stake
            </label>
            <div className="relative">
              <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500 font-bold text-xs">
                PLN
              </span>
              <input
                type="number"
                value={stake || ""}
                onChange={(e) => dispatch(setStake(Number(e.target.value)))}
                placeholder="0.00"
                className="w-full bg-dark-900 border border-dark-600 rounded-lg py-3 pl-12 pr-4 text-sm font-bold focus:outline-none focus:border-primary-500 transition-colors"
                disabled={loading}
              />
            </div>
          </div>

          <div className="pt-2">
            <div className="flex items-center justify-between mb-4">
              <span className="text-xs font-bold text-gray-400">
                Potential Payout
              </span>
              <span className="text-lg font-black text-accent-success">
                {Number(potentialPayout).toFixed(2)} PLN
              </span>
            </div>

            <button
              onClick={handlePlaceBet}
              disabled={loading}
              className="w-full flex items-center justify-center gap-2 bg-primary-600 hover:bg-primary-700 disabled:bg-primary-800 disabled:opacity-50 py-4 rounded-xl font-black text-sm uppercase tracking-widest shadow-lg shadow-primary-600/20 active:scale-95 transition-all"
            >
              {loading && <Loader2 className="w-4 h-4 animate-spin" />}
              {loading ? "Placing..." : "Place Bet"}
            </button>

            <button
              onClick={() => dispatch(clearBetslip())}
              disabled={loading}
              className="w-full mt-2 text-[10px] font-bold text-gray-500 hover:text-gray-300 disabled:opacity-50 uppercase tracking-tighter transition-colors"
            >
              Clear All
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default BetSlip;
