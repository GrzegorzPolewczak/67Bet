import React, { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { bettingApi } from "../../api/axios";
import {
  ChevronLeft,
  Loader2,
  Share2,
  Copy,
  CheckCircle2,
  XCircle,
  Clock,
} from "lucide-react";
import { motion } from "framer-motion";
import { useDispatch } from "react-redux";
import { addSelection } from "../betslip/betslipSlice";

interface BetDto {
  outcomeId: string;
  outcomeName: string;
  marketName: string;
  eventName: string;
  startTime: string;
  fixedPrice: number;
  status: string;
}

interface TicketDto {
  id: string;
  stake: number;
  totalOdds: number;
  potentialWinning: number;
  status: string;
  bets: BetDto[];
}

const SharedTicketPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [ticket, setTicket] = useState<TicketDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const dispatch = useDispatch();

  useEffect(() => {
    const fetchTicket = async () => {
      try {
        const response = await bettingApi.get(`/tickets/share/${id}`);
        setTicket(response.data);
      } catch (err: any) {
        setError(
          "Failed to load shared ticket. It might not exist or has been removed.",
        );
      } finally {
        setLoading(false);
      }
    };

    if (id) fetchTicket();
  }, [id]);

  const canCopy = ticket?.bets.every(
    (bet) => new Date(bet.startTime) > new Date(),
  );

  const handleCopyToBetslip = () => {
    if (!ticket) return;

    ticket.bets.forEach((bet) => {
      dispatch(
        addSelection({
          eventId: "", // We don't have eventId in BetDto but we can still add it if needed
          eventName: bet.eventName,
          marketId: "",
          marketName: bet.marketName,
          outcomeId: bet.outcomeId,
          outcomeName: bet.outcomeName,
          odd: bet.fixedPrice,
        }),
      );
    });
  };

  if (loading) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[60vh] space-y-4">
        <Loader2 className="w-10 h-10 text-primary-500 animate-spin" />
        <p className="text-gray-400 font-bold">Loading shared ticket...</p>
      </div>
    );
  }

  if (error || !ticket) {
    return (
      <div className="max-w-2xl mx-auto mt-20 text-center space-y-6">
        <div className="bg-red-500/10 border border-red-500/50 p-8 rounded-3xl">
          <h2 className="text-2xl font-black text-white mb-2">Oops!</h2>
          <p className="text-gray-400">{error || "Ticket not found."}</p>
        </div>
        <Link
          to="/"
          className="inline-flex items-center gap-2 text-primary-500 hover:text-primary-400 font-bold"
        >
          <ChevronLeft className="w-4 h-4" /> Return to Home
        </Link>
      </div>
    );
  }

  return (
    <div className="max-w-2xl mx-auto space-y-8 pb-12">
      <Link
        to="/"
        className="inline-flex items-center gap-2 text-gray-400 hover:text-white transition-colors text-sm font-bold"
      >
        <ChevronLeft className="w-4 h-4" /> Back to Betting
      </Link>

      <div className="bg-dark-800 border border-dark-700 rounded-3xl overflow-hidden shadow-2xl">
        <div className="bg-primary-600/10 border-b border-dark-700 p-8">
          <div className="flex items-center justify-between mb-6">
            <h1 className="text-3xl font-black text-white flex items-center gap-3">
              <Share2 className="w-8 h-8 text-primary-500" /> Shared Ticket
            </h1>
            <div
              className={`px-4 py-1.5 rounded-full text-xs font-black uppercase tracking-widest ${
                ticket.status === "Pending"
                  ? "bg-primary-500/20 text-primary-500"
                  : ticket.status === "Won"
                    ? "bg-accent-success/20 text-accent-success"
                    : "bg-accent-danger/20 text-accent-danger"
              }`}
            >
              {ticket.status}
            </div>
          </div>

          <div className="grid grid-cols-3 gap-4">
            <div className="bg-dark-900/50 rounded-2xl p-4 border border-dark-700/50">
              <span className="text-[10px] font-bold text-gray-500 uppercase tracking-widest block mb-1">
                Total Odds
              </span>
              <span className="text-xl font-black text-white">
                @{ticket.totalOdds.toFixed(2)}
              </span>
            </div>
            <div className="bg-dark-900/50 rounded-2xl p-4 border border-dark-700/50">
              <span className="text-[10px] font-bold text-gray-500 uppercase tracking-widest block mb-1">
                Stake
              </span>
              <span className="text-xl font-black text-white">
                ${ticket.stake.toFixed(2)}
              </span>
            </div>
            <div className="bg-dark-900/50 rounded-2xl p-4 border border-dark-700/50">
              <span className="text-[10px] font-bold text-gray-500 uppercase tracking-widest block mb-1">
                Potential Win
              </span>
              <span className="text-xl font-black text-accent-success">
                ${ticket.potentialWinning.toFixed(2)}
              </span>
            </div>
          </div>
        </div>

        <div className="p-8 space-y-4">
          <h2 className="text-xs font-bold text-gray-500 uppercase tracking-widest mb-4">
            Selections ({ticket.bets.length})
          </h2>
          {ticket.bets.map((bet, index) => {
            const hasStarted = new Date(bet.startTime) <= new Date();
            return (
              <motion.div
                key={index}
                initial={{ opacity: 0, x: -20 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: index * 0.1 }}
                className="bg-dark-900 rounded-2xl p-4 border border-dark-700 flex items-center justify-between relative overflow-hidden group"
              >
                <div className="space-y-1">
                  <p className="text-[10px] font-black text-primary-500 uppercase tracking-wider">
                    {bet.eventName}
                  </p>
                  <p className="text-sm font-bold text-white">
                    {bet.marketName}: {bet.outcomeName}
                  </p>
                  <div className="flex items-center gap-2 text-[10px] text-gray-500">
                    <Clock className="w-3 h-3" />
                    {new Date(bet.startTime).toLocaleString()}
                  </div>
                </div>
                <div className="flex items-center gap-4">
                  <span className="text-lg font-black text-white group-hover:text-primary-500 transition-colors">
                    @{bet.fixedPrice.toFixed(2)}
                  </span>
                  {hasStarted ? (
                    bet.status === "Won" ? (
                      <CheckCircle2 className="w-6 h-6 text-accent-success" />
                    ) : bet.status === "Lost" ? (
                      <XCircle className="w-6 h-6 text-accent-danger" />
                    ) : (
                      <Clock className="w-6 h-6 text-gray-500" />
                    )
                  ) : (
                    <div
                      className="w-6 h-6 rounded-full border-2 border-dashed border-gray-700"
                      title="Not started"
                    />
                  )}
                </div>
              </motion.div>
            );
          })}

          {canCopy && (
            <motion.button
              whileHover={{ scale: 1.02 }}
              whileTap={{ scale: 0.98 }}
              onClick={handleCopyToBetslip}
              className="w-full mt-6 bg-primary-600 hover:bg-primary-500 text-white font-black py-4 rounded-2xl flex items-center justify-center gap-2 shadow-lg shadow-primary-600/20 transition-all"
            >
              <Copy className="w-5 h-5" /> Copy to my Betslip
            </motion.button>
          )}

          {!canCopy && ticket.status === "Pending" && (
            <div className="mt-6 bg-dark-900/50 border border-dark-700 rounded-2xl p-4 text-center">
              <p className="text-sm text-gray-500 font-bold italic">
                Matches have already started. You cannot copy this ticket.
              </p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default SharedTicketPage;
