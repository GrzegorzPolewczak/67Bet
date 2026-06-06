import React, { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import type { AppDispatch, RootState } from "../../app/store";
import {
  fetchPendingRequestsAsync,
  acceptRequestAsync,
  rejectRequestAsync,
  fetchPromoCodesAsync,
  createPromoCodeAsync,
  togglePromoCodeStatusAsync,
} from "./adminSlice";
import {
  ShieldCheck,
  Users,
  Activity,
  DollarSign,
  Check,
  X,
  Clock,
  Zap,
  Ticket,
  Plus,
  Power,
  PowerOff,
} from "lucide-react";
import { motion, AnimatePresence } from "framer-motion";
import toast from "react-hot-toast";

const AdminDashboard: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const { pendingRequests, promoCodes, stats, loading } = useSelector(
    (state: RootState) => state.admin,
  );
  const [oddsInput, setOddsInput] = useState<Record<string, string>>({});

  // Promo code form state
  const [newPromoCode, setNewPromoCode] = useState("");
  const [newPromoReward, setNewPromoReward] = useState("25.00");

  useEffect(() => {
    dispatch(fetchPendingRequestsAsync());
    dispatch(fetchPromoCodesAsync());
  }, [dispatch]);

  const handleAccept = (id: string) => {
    const odds = parseFloat(oddsInput[id]);
    if (isNaN(odds) || odds <= 1) {
      toast.error("Please enter valid odds greater than 1.0");
      return;
    }
    dispatch(acceptRequestAsync({ id, odds, note: "Approved by Admin" }));
    toast.success("Custom bet accepted and published!");
  };

  const handleReject = (id: string) => {
    dispatch(
      rejectRequestAsync({ id, reason: "Does not meet platform guidelines" }),
    );
    toast.error("Custom bet rejected.");
  };

  const handleCreatePromo = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newPromoCode) {
      toast.error("Please enter a promo code.");
      return;
    }
    const reward = parseFloat(newPromoReward);
    if (isNaN(reward) || reward <= 0) {
      toast.error("Please enter a valid reward amount.");
      return;
    }

    try {
      await dispatch(
        createPromoCodeAsync({ code: newPromoCode, reward }),
      ).unwrap();
      toast.success(`Promo code ${newPromoCode} created!`);
      setNewPromoCode("");
    } catch (err: any) {
      toast.error(err || "Failed to create promo code");
    }
  };

  const handleTogglePromo = async (code: string, isActive: boolean) => {
    try {
      await dispatch(togglePromoCodeStatusAsync({ code, isActive })).unwrap();
      toast.success(
        `Promo code ${code} ${isActive ? "deactivated" : "activated"}!`,
      );
    } catch (err: any) {
      toast.error(err || "Failed to update promo code status");
    }
  };

  return (
    <div className="max-w-6xl mx-auto space-y-10 pb-12 px-4">
      {/* Header */}
      <div className="bg-gradient-to-r from-primary-900/40 to-transparent p-8 rounded-3xl border border-primary-500/20 flex flex-col md:flex-row md:items-center justify-between gap-6 relative overflow-hidden">
        <div className="relative z-10">
          <h1 className="text-4xl font-black text-white flex items-center gap-3 mb-2">
            <ShieldCheck className="w-10 h-10 text-primary-500" /> Control
            Center
          </h1>
          <p className="text-gray-400 text-sm max-w-xl leading-relaxed">
            Welcome to the administrator dashboard. Monitor platform activity,
            manage user requests, and oversee platform-wide promotional
            campaigns.
          </p>
        </div>
        <Zap className="absolute right-0 top-1/2 -translate-y-1/2 w-48 h-48 text-primary-500/10 blur-xl" />
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="bg-dark-800/80 backdrop-blur-md border border-dark-600 rounded-3xl p-6 flex items-center gap-5 shadow-lg shadow-dark-900/50">
          <div className="w-14 h-14 bg-primary-600/20 rounded-2xl flex items-center justify-center">
            <Users className="w-7 h-7 text-primary-500" />
          </div>
          <div>
            <p className="text-gray-400 text-xs font-bold uppercase tracking-widest mb-1">
              Total Users
            </p>
            <p className="text-3xl font-black text-white">
              {stats.totalUsers.toLocaleString()}
            </p>
          </div>
        </div>
        <div className="bg-dark-800/80 backdrop-blur-md border border-dark-600 rounded-3xl p-6 flex items-center gap-5 shadow-lg shadow-dark-900/50">
          <div className="w-14 h-14 bg-accent-success/20 rounded-2xl flex items-center justify-center">
            <Activity className="w-7 h-7 text-accent-success" />
          </div>
          <div>
            <p className="text-gray-400 text-xs font-bold uppercase tracking-widest mb-1">
              Active Bets
            </p>
            <p className="text-3xl font-black text-white">
              {stats.activeBets.toLocaleString()}
            </p>
          </div>
        </div>
        <div className="bg-dark-800/80 backdrop-blur-md border border-dark-600 rounded-3xl p-6 flex items-center gap-5 shadow-lg shadow-dark-900/50">
          <div className="w-14 h-14 bg-yellow-500/20 rounded-2xl flex items-center justify-center">
            <DollarSign className="w-7 h-7 text-yellow-500" />
          </div>
          <div>
            <p className="text-gray-400 text-xs font-bold uppercase tracking-widest mb-1">
              Revenue (24h)
            </p>
            <p className="text-3xl font-black text-white">
              $
              {stats.revenue.toLocaleString(undefined, {
                minimumFractionDigits: 2,
              })}
            </p>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-10">
        {/* Pending Custom Bets */}
        <section className="bg-dark-800/50 border border-dark-600 rounded-3xl overflow-hidden shadow-2xl flex flex-col">
          <div className="p-6 md:px-8 border-b border-dark-600 flex items-center justify-between bg-dark-800">
            <h2 className="text-xl font-bold text-white flex items-center gap-3">
              <Clock className="w-5 h-5 text-primary-500" /> Custom Bet Queue
            </h2>
            <div className="bg-dark-900 border border-dark-600 px-4 py-1.5 rounded-full flex items-center gap-2">
              <div className="w-2 h-2 rounded-full bg-primary-500 animate-pulse" />
              <span className="text-xs font-bold text-gray-300 uppercase tracking-widest">
                {pendingRequests.length}
              </span>
            </div>
          </div>

          <div className="p-6 md:p-8 flex-1">
            {pendingRequests.length === 0 ? (
              <div className="text-center py-16 bg-dark-900/50 rounded-2xl border border-dark-700 border-dashed">
                <ShieldCheck className="w-12 h-12 text-dark-600 mx-auto mb-4" />
                <p className="text-base font-bold text-gray-400">
                  All caught up!
                </p>
              </div>
            ) : (
              <div className="grid gap-4">
                <AnimatePresence mode="popLayout">
                  {pendingRequests.map((request) => (
                    <motion.div
                      key={request.id}
                      layout
                      initial={{ opacity: 0, scale: 0.98 }}
                      animate={{ opacity: 1, scale: 1 }}
                      exit={{ opacity: 0, scale: 0.95 }}
                      className="bg-dark-800 border border-dark-600 rounded-2xl p-5 flex flex-col gap-4 hover:border-primary-500/50 transition-colors shadow-lg"
                    >
                      <div className="flex-1">
                        <div className="flex items-center gap-3 mb-3">
                          <span className="text-[10px] font-black uppercase tracking-widest bg-primary-600/20 text-primary-400 px-2 py-0.5 rounded-md">
                            {request.id.split("-")[0]}
                          </span>
                          <span className="text-[10px] font-bold text-gray-600">
                            {new Date(request.createdAt).toLocaleTimeString()}
                          </span>
                        </div>
                        <p className="text-sm text-white font-medium italic leading-relaxed bg-dark-900/50 border border-dark-700 p-3 rounded-xl">
                          "{request.description}"
                        </p>
                      </div>

                      <div className="flex items-center gap-3 bg-dark-900 p-2 rounded-xl border border-dark-700">
                        <div className="relative flex-1">
                          <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500 font-black text-xs">
                            @
                          </span>
                          <input
                            type="number"
                            placeholder="Odds"
                            step="0.01"
                            value={oddsInput[request.id] || ""}
                            onChange={(e) =>
                              setOddsInput((prev) => ({
                                ...prev,
                                [request.id]: e.target.value,
                              }))
                            }
                            className="w-full bg-dark-800 border border-dark-600 rounded-lg py-2 pl-7 pr-3 text-sm font-black text-white focus:outline-none focus:border-primary-500"
                          />
                        </div>
                        <button
                          onClick={() => handleAccept(request.id)}
                          className="bg-accent-success hover:bg-green-400 text-dark-900 font-black px-4 py-2 rounded-lg text-sm transition-all"
                        >
                          Accept
                        </button>
                        <button
                          onClick={() => handleReject(request.id)}
                          className="bg-dark-800 border border-dark-600 hover:text-accent-danger p-2 rounded-lg transition-all"
                        >
                          <X className="w-4 h-4" />
                        </button>
                      </div>
                    </motion.div>
                  ))}
                </AnimatePresence>
              </div>
            )}
          </div>
        </section>

        {/* Promo Codes Management */}
        <section className="bg-dark-800/50 border border-dark-600 rounded-3xl overflow-hidden shadow-2xl flex flex-col">
          <div className="p-6 md:px-8 border-b border-dark-600 flex items-center justify-between bg-dark-800">
            <h2 className="text-xl font-bold text-white flex items-center gap-3">
              <Ticket className="w-5 h-5 text-primary-500" /> Promo Codes
            </h2>
            <div className="bg-dark-900 border border-dark-600 px-4 py-1.5 rounded-full flex items-center gap-2">
              <span className="text-xs font-bold text-gray-300 uppercase tracking-widest">
                {promoCodes.length} Codes
              </span>
            </div>
          </div>

          <div className="p-6 md:p-8 space-y-8 flex-1">
            {/* Create Promo Form */}
            <form
              onSubmit={handleCreatePromo}
              className="bg-dark-900 border border-dark-700 p-5 rounded-2xl space-y-4"
            >
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div className="space-y-1.5">
                  <label className="text-[10px] font-bold text-gray-500 uppercase tracking-widest px-1">
                    Code Name
                  </label>
                  <input
                    type="text"
                    placeholder="e.g. WORLDCUP26"
                    value={newPromoCode}
                    onChange={(e) =>
                      setNewPromoCode(e.target.value.toUpperCase())
                    }
                    className="w-full bg-dark-800 border border-dark-600 rounded-xl py-3 px-4 text-sm font-black text-white focus:outline-none focus:border-primary-500 uppercase"
                  />
                </div>
                <div className="space-y-1.5">
                  <label className="text-[10px] font-bold text-gray-500 uppercase tracking-widest px-1">
                    Freebet Reward ($)
                  </label>
                  <input
                    type="number"
                    step="0.01"
                    value={newPromoReward}
                    onChange={(e) => setNewPromoReward(e.target.value)}
                    className="w-full bg-dark-800 border border-dark-600 rounded-xl py-3 px-4 text-sm font-black text-white focus:outline-none focus:border-primary-500"
                  />
                </div>
              </div>
              <button
                type="submit"
                className="w-full bg-primary-600 hover:bg-primary-500 text-white font-black py-3 rounded-xl transition-all flex items-center justify-center gap-2 shadow-lg shadow-primary-900/20"
              >
                <Plus className="w-5 h-5" /> Add Promotional Code
              </button>
            </form>

            {/* Promo Codes List */}
            <div className="space-y-3 max-h-[400px] overflow-y-auto pr-2 custom-scrollbar">
              <AnimatePresence mode="popLayout">
                {promoCodes.map((promo) => (
                  <motion.div
                    key={promo.code}
                    layout
                    initial={{ opacity: 0, x: 20 }}
                    animate={{ opacity: 1, x: 0 }}
                    className={`flex items-center justify-between p-4 rounded-2xl border transition-all ${
                      promo.isActive
                        ? "bg-dark-800 border-dark-600 hover:border-primary-500/30"
                        : "bg-dark-900/50 border-dark-800 opacity-60"
                    }`}
                  >
                    <div className="flex items-center gap-4">
                      <div
                        className={`w-10 h-10 rounded-xl flex items-center justify-center ${
                          promo.isActive
                            ? "bg-primary-600/20 text-primary-500"
                            : "bg-dark-700 text-gray-600"
                        }`}
                      >
                        <Ticket className="w-5 h-5" />
                      </div>
                      <div>
                        <p className="text-sm font-black text-white tracking-wide">
                          {promo.code}
                        </p>
                        <p className="text-[10px] font-bold text-primary-500 uppercase tracking-widest">
                          ${promo.rewardAmount.toFixed(2)} Bonus
                        </p>
                      </div>
                    </div>

                    <button
                      onClick={() =>
                        handleTogglePromo(promo.code, promo.isActive)
                      }
                      className={`p-3 rounded-xl transition-all flex items-center gap-2 text-[10px] font-black uppercase tracking-tighter ${
                        promo.isActive
                          ? "bg-accent-danger/10 text-accent-danger hover:bg-accent-danger hover:text-white"
                          : "bg-accent-success/10 text-accent-success hover:bg-accent-success hover:text-dark-900"
                      }`}
                    >
                      {promo.isActive ? (
                        <>
                          Deactivate <PowerOff className="w-3.5 h-3.5" />
                        </>
                      ) : (
                        <>
                          Activate <Power className="w-3.5 h-3.5" />
                        </>
                      )}
                    </button>
                  </motion.div>
                ))}
              </AnimatePresence>
            </div>
          </div>
        </section>
      </div>
    </div>
  );
};

export default AdminDashboard;
