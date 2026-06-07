import React, { useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";
import type { AppDispatch, RootState } from "../../app/store";
import { fetchHistoryAsync } from "./historySlice";
import {
  History,
  ChevronLeft,
  Loader2,
  Info,
  Share2,
  Clock,
  CheckCircle2,
  XCircle,
} from "lucide-react";
import { Link } from "react-router-dom";
import { motion } from "framer-motion";

const getStatusLabel = (status?: string) => {
  switch ((status || "").toLowerCase()) {
    case "won":
      return "WIN";
    case "lost":
      return "LOST";
    case "cancelled":
      return "CANCELLED";
    default:
      return "W TRAKCIE";
  }
};

const getStatusClasses = (status?: string) => {
  switch ((status || "").toLowerCase()) {
    case "won":
      return "bg-accent-success/20 text-accent-success";
    case "lost":
    case "cancelled":
      return "bg-accent-danger/20 text-accent-danger";
    default:
      return "bg-primary-500/20 text-primary-500";
  }
};

const BetHistoryPage: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const { tickets, loading, error } = useSelector(
    (state: RootState) => state.history,
  );

  useEffect(() => {
    dispatch(fetchHistoryAsync());
  }, [dispatch]);

  const handleShare = (ticketId: string) => {
    const url = `${window.location.origin}/share-ticket/${ticketId}`;
    navigator.clipboard.writeText(url);
    alert("Link to betslip copied to clipboard!");
  };

  return (
    <div className="max-w-4xl mx-auto space-y-8 pb-12">
      <Link
        to="/"
        className="inline-flex items-center gap-2 text-gray-400 hover:text-white transition-colors text-sm font-bold"
      >
        <ChevronLeft className="w-4 h-4" /> Back to Betting
      </Link>

      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-black text-white flex items-center gap-3">
            <History className="w-8 h-8 text-primary-500" /> Bet History
          </h1>
          <p className="text-gray-400 text-sm">
            Review your past and active bets.
          </p>
        </div>
        <button
          onClick={() => dispatch(fetchHistoryAsync())}
          className="text-xs font-bold text-gray-400 hover:text-white transition-colors"
        >
          Refresh
        </button>
      </div>

      {error && (
        <div className="bg-red-500/10 border border-red-500/50 text-red-500 text-xs font-bold p-4 rounded-xl text-center">
          {error}
        </div>
      )}

      {loading && tickets.length === 0 ? (
        <div className="flex flex-col items-center justify-center h-64 space-y-4">
          <Loader2 className="w-10 h-10 text-primary-500 animate-spin" />
        </div>
      ) : tickets.length === 0 ? (
        <div className="flex flex-col items-center justify-center h-64 bg-dark-800 rounded-3xl border border-dark-700 border-dashed text-center p-6 space-y-4">
          <Info className="w-10 h-10 text-gray-500" />
          <div>
            <p className="text-lg font-bold text-gray-300">No bets found</p>
            <p className="text-sm text-gray-500 mt-1">
              You haven't placed any bets yet.
            </p>
          </div>
        </div>
      ) : (
        <div className="grid gap-4">
          {tickets.map((ticket) => (
            <motion.div
              key={ticket.id}
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              className="bg-dark-800 border border-dark-700 rounded-3xl p-6"
            >
              <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-6 border-b border-dark-700 pb-4">
                <div>
                  <span className="text-xs font-bold text-gray-500 uppercase tracking-widest block mb-1">
                    Ticket ID
                  </span>
                  <span className="text-sm font-mono text-gray-300">
                    {ticket.id.split("-")[0]}...
                  </span>
                </div>
                <div className="flex items-center gap-6">
                  <button
                    onClick={() => handleShare(ticket.id)}
                    className="p-2 hover:bg-dark-700 rounded-lg transition-colors text-gray-400 hover:text-primary-500"
                    title="Share Ticket"
                  >
                    <Share2 className="w-5 h-5" />
                  </button>
                  <div className="text-right">
                    <span className="text-xs font-bold text-gray-500 uppercase tracking-widest block mb-1">
                      Total Odds
                    </span>
                    <span className="text-sm font-black text-white">
                      @{Number(ticket.totalOdds || 0).toFixed(2)}
                    </span>
                  </div>
                  <div className="text-right">
                    <span className="text-xs font-bold text-gray-500 uppercase tracking-widest block mb-1">
                      Stake
                    </span>
                    <span className="text-sm font-black text-white">
                      ${Number(ticket.stake || 0).toFixed(2)}
                    </span>
                  </div>
                  <div className="text-right">
                    <span className="text-xs font-bold text-gray-500 uppercase tracking-widest block mb-1">
                      Payout
                    </span>
                    <span className="text-sm font-black text-accent-success">
                      ${Number(ticket.potentialWinning || 0).toFixed(2)}
                    </span>
                  </div>
                  <div className="text-right">
                    <span
                      className={`text-xs font-bold uppercase tracking-widest px-3 py-1 rounded-lg ${getStatusClasses(ticket.status)}`}
                    >
                      {getStatusLabel(ticket.status)}
                    </span>
                  </div>
                </div>
              </div>

              <div className="space-y-3">
                <h4 className="text-xs font-bold text-gray-500 uppercase tracking-widest mb-2">
                  Selections ({ticket.bets.length})
                </h4>
                {ticket.bets.map((bet, i) => {
                  const resultStatus = bet.resultStatus || bet.status;
                  const isFinished =
                    resultStatus === "Won" || resultStatus === "Lost";

                  return (
                    <div
                      key={i}
                      className="flex items-center justify-between bg-dark-900 rounded-xl p-4 border border-dark-600"
                    >
                      <div className="space-y-1">
                        <p className="text-[10px] font-black text-primary-500 uppercase tracking-wider">
                          {bet.eventName}
                        </p>
                        <p className="text-xs font-bold text-white">
                          Obstawiono: {bet.marketName}: {bet.outcomeName}
                        </p>
                        {isFinished && bet.winningOutcomeName && (
                          <p className="text-[11px] font-bold text-gray-300">
                            Wynik: wygrał {bet.winningOutcomeName}
                          </p>
                        )}
                        <div className="flex items-center gap-2 text-[10px] text-gray-500">
                          <Clock className="w-3 h-3" />
                          {new Date(bet.startTime).toLocaleString()}
                        </div>
                      </div>
                      <div className="flex items-center gap-3">
                        <span className="text-sm font-black text-white bg-dark-700 px-3 py-1 rounded-lg">
                          @{Number(bet.fixedPrice).toFixed(2)}
                        </span>
                        <span
                          className={`text-[10px] font-black uppercase tracking-widest px-2 py-1 rounded-lg ${getStatusClasses(resultStatus)}`}
                        >
                          {getStatusLabel(resultStatus)}
                        </span>
                        {resultStatus === "Won" ? (
                          <CheckCircle2 className="w-4 h-4 text-accent-success" />
                        ) : resultStatus === "Lost" ? (
                          <XCircle className="w-4 h-4 text-accent-danger" />
                        ) : (
                          <Clock className="w-4 h-4 text-gray-500" />
                        )}
                      </div>
                    </div>
                  );
                })}
              </div>
            </motion.div>
          ))}
        </div>
      )}
    </div>
  );
};

export default BetHistoryPage;
