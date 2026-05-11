import React, { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import type { AppDispatch, RootState } from '../../app/store';
import { fetchPendingRequestsAsync, acceptRequestAsync, rejectRequestAsync } from './adminSlice';
import { ShieldCheck, Users, Activity, DollarSign, Check, X, Clock, Zap } from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import toast from 'react-hot-toast';

const AdminDashboard: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const { pendingRequests, stats, loading } = useSelector((state: RootState) => state.admin);
  const [oddsInput, setOddsInput] = useState<Record<string, string>>({});

  useEffect(() => {
    dispatch(fetchPendingRequestsAsync());
  }, [dispatch]);

  const handleAccept = (id: string) => {
    const odds = parseFloat(oddsInput[id]);
    if (isNaN(odds) || odds <= 1) {
      toast.error('Please enter valid odds greater than 1.0');
      return;
    }
    dispatch(acceptRequestAsync({ id, odds, note: 'Approved by Admin' }));
    toast.success('Custom bet accepted and published!');
  };

  const handleReject = (id: string) => {
    dispatch(rejectRequestAsync({ id, reason: 'Does not meet platform guidelines' }));
    toast.error('Custom bet rejected.');
  };

  return (
    <div className="max-w-6xl mx-auto space-y-10 pb-12">
      {/* Header */}
      <div className="bg-gradient-to-r from-primary-900/40 to-transparent p-8 rounded-3xl border border-primary-500/20 flex flex-col md:flex-row md:items-center justify-between gap-6 relative overflow-hidden">
        <div className="relative z-10">
          <h1 className="text-4xl font-black text-white flex items-center gap-3 mb-2">
            <ShieldCheck className="w-10 h-10 text-primary-500" /> Control Center
          </h1>
          <p className="text-gray-400 text-sm max-w-xl leading-relaxed">
            Welcome to the administrator dashboard. Monitor platform activity, manage user requests, and oversee AI-generated custom bets in real-time.
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
            <p className="text-gray-400 text-xs font-bold uppercase tracking-widest mb-1">Total Users</p>
            <p className="text-3xl font-black text-white">{stats.totalUsers.toLocaleString()}</p>
          </div>
        </div>
        <div className="bg-dark-800/80 backdrop-blur-md border border-dark-600 rounded-3xl p-6 flex items-center gap-5 shadow-lg shadow-dark-900/50">
          <div className="w-14 h-14 bg-accent-success/20 rounded-2xl flex items-center justify-center">
            <Activity className="w-7 h-7 text-accent-success" />
          </div>
          <div>
            <p className="text-gray-400 text-xs font-bold uppercase tracking-widest mb-1">Active Bets</p>
            <p className="text-3xl font-black text-white">{stats.activeBets.toLocaleString()}</p>
          </div>
        </div>
        <div className="bg-dark-800/80 backdrop-blur-md border border-dark-600 rounded-3xl p-6 flex items-center gap-5 shadow-lg shadow-dark-900/50">
          <div className="w-14 h-14 bg-yellow-500/20 rounded-2xl flex items-center justify-center">
            <DollarSign className="w-7 h-7 text-yellow-500" />
          </div>
          <div>
            <p className="text-gray-400 text-xs font-bold uppercase tracking-widest mb-1">Revenue (24h)</p>
            <p className="text-3xl font-black text-white">${stats.revenue.toLocaleString(undefined, {minimumFractionDigits: 2})}</p>
          </div>
        </div>
      </div>

      {/* Pending Custom Bets */}
      <section className="bg-dark-800/50 border border-dark-600 rounded-3xl overflow-hidden shadow-2xl">
        <div className="p-6 md:px-8 border-b border-dark-600 flex items-center justify-between bg-dark-800">
          <h2 className="text-xl font-bold text-white flex items-center gap-3">
            <Clock className="w-5 h-5 text-primary-500" /> Custom Bet Queue
          </h2>
          <div className="bg-dark-900 border border-dark-600 px-4 py-1.5 rounded-full flex items-center gap-2">
            <div className="w-2 h-2 rounded-full bg-primary-500 animate-pulse" />
            <span className="text-xs font-bold text-gray-300 uppercase tracking-widest">{pendingRequests.length} Pending</span>
          </div>
        </div>

        <div className="p-6 md:p-8">
          {pendingRequests.length === 0 ? (
            <div className="text-center py-16 bg-dark-900/50 rounded-2xl border border-dark-700 border-dashed">
              <ShieldCheck className="w-16 h-16 text-dark-600 mx-auto mb-4" />
              <p className="text-lg font-bold text-gray-400">All caught up!</p>
              <p className="text-sm text-gray-500 mt-1">There are no pending custom bet requests at the moment.</p>
            </div>
          ) : (
            <div className="grid gap-6">
              <AnimatePresence mode="popLayout">
                {pendingRequests.map((request) => (
                  <motion.div 
                    key={request.id}
                    layout
                    initial={{ opacity: 0, scale: 0.98 }}
                    animate={{ opacity: 1, scale: 1 }}
                    exit={{ opacity: 0, scale: 0.95, transition: { duration: 0.2 } }}
                    className="bg-dark-800 border border-dark-600 rounded-2xl p-6 flex flex-col xl:flex-row gap-8 xl:items-center justify-between hover:border-primary-500/50 transition-colors shadow-lg"
                  >
                    <div className="flex-1 space-y-4">
                      <div className="flex flex-wrap items-center gap-3">
                        <span className="text-[10px] font-black uppercase tracking-widest bg-primary-600/20 text-primary-400 px-3 py-1 rounded-lg">
                          Request ID: {request.id.split('-').slice(0,2).join('-')}
                        </span>
                        <span className="text-[10px] font-bold uppercase tracking-widest text-gray-500">
                          User: {request.userId}
                        </span>
                        <span className="text-[10px] font-bold text-gray-600">
                          • {new Date(request.createdAt).toLocaleString()}
                        </span>
                      </div>
                      
                      <div className="bg-dark-900/50 border border-dark-700 p-4 rounded-xl">
                        <p className="text-base text-white font-medium italic leading-relaxed">
                          "{request.description}"
                        </p>
                      </div>
                    </div>

                    <div className="flex items-center gap-4 shrink-0 bg-dark-900 p-3 rounded-2xl border border-dark-700">
                      <div className="flex flex-col gap-1">
                        <label className="text-[10px] font-bold text-gray-500 uppercase tracking-widest px-1">Set Odds</label>
                        <div className="relative">
                          <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500 font-black text-sm">@</span>
                          <input 
                            type="number"
                            placeholder="e.g. 2.50"
                            step="0.01"
                            min="1.01"
                            value={oddsInput[request.id] || ''}
                            onChange={(e) => setOddsInput(prev => ({ ...prev, [request.id]: e.target.value }))}
                            className="w-32 bg-dark-800 border border-dark-600 rounded-xl py-3 pl-8 pr-4 text-sm font-black text-white focus:outline-none focus:border-primary-500 transition-colors shadow-inner"
                          />
                        </div>
                      </div>
                      
                      <div className="flex items-center gap-2 mt-5">
                        <button 
                          onClick={() => handleAccept(request.id)}
                          className="flex items-center gap-2 bg-accent-success hover:bg-green-400 text-dark-900 font-black px-4 py-3 rounded-xl transition-all active:scale-95 shadow-lg shadow-accent-success/20"
                        >
                          <Check className="w-4 h-4" /> Accept
                        </button>
                        <button 
                          onClick={() => handleReject(request.id)}
                          className="flex items-center justify-center bg-dark-800 border border-dark-600 hover:bg-accent-danger/20 hover:text-accent-danger hover:border-accent-danger/50 text-gray-400 p-3 rounded-xl transition-all active:scale-95"
                          title="Reject Request"
                        >
                          <X className="w-5 h-5" />
                        </button>
                      </div>
                    </div>
                  </motion.div>
                ))}
              </AnimatePresence>
            </div>
          )}
        </div>
      </section>
    </div>
  );
};

export default AdminDashboard;