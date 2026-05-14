import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { addSelection } from '../betslip/betslipSlice';
import { fetchEventsAsync } from './bettingSlice';
import type { RootState, AppDispatch } from '../../app/store';
import { motion, AnimatePresence } from 'framer-motion';
import { Trophy, Clock, ChevronRight, Zap, Loader2 } from 'lucide-react';
import { Link } from 'react-router-dom';
import OddButton from './OddButton';

const Home: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const selections = useSelector((state: RootState) => state.betslip.selections);
  const { events, loading, error } = useSelector((state: RootState) => state.betting);

  useEffect(() => {
    dispatch(fetchEventsAsync());
  }, [dispatch]);

  const isSelected = (outcomeId: string) => 
    selections.some(s => s.outcomeId === outcomeId);

  if (loading && events.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center h-64 space-y-4">
        <Loader2 className="w-10 h-10 text-primary-500 animate-spin" />
        <p className="text-gray-400 font-bold">Loading exciting matches...</p>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto space-y-8 pb-12 relative">
      {/* Background Glows */}
      <div className="absolute top-0 left-1/4 w-64 h-64 bg-primary-600/10 blur-[120px] rounded-full -z-10" />
      <div className="absolute bottom-1/4 right-1/4 w-96 h-96 bg-primary-900/10 blur-[150px] rounded-full -z-10" />

      {/* Hero Banner */}
      <section className="relative h-64 rounded-3xl overflow-hidden group cursor-pointer border border-dark-700">
        <img 
          src="https://images.unsplash.com/photo-1508098682722-e99c43a406b2?auto=format&fit=crop&q=80&w=1200" 
          alt="Sports"
          className="absolute inset-0 w-full h-full object-cover transition-transform duration-700 group-hover:scale-105"
        />
        <div className="absolute inset-0 bg-gradient-to-r from-dark-900 via-dark-900/40 to-transparent" />
        <div className="relative h-full flex flex-col justify-center px-10 space-y-4">
          <motion.div 
            initial={{ opacity: 0, x: -20 }}
            animate={{ opacity: 1, x: 0 }}
            className="inline-flex items-center gap-2 bg-primary-600/20 text-primary-400 px-3 py-1 rounded-full text-xs font-bold border border-primary-500/30 w-fit"
          >
            <Trophy className="w-3 h-3" />
            Champions League Special
          </motion.div>
          <motion.h1 
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.1 }}
            className="text-4xl font-black text-white max-w-md leading-tight"
          >
            Boost Your Odds by <span className="text-primary-500">25%</span> on AKO!
          </motion.h1>
          <motion.button 
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            transition={{ delay: 0.2 }}
            className="bg-white text-dark-900 px-6 py-2.5 rounded-xl font-bold text-sm w-fit hover:bg-primary-500 hover:text-white transition-all transform active:scale-95"
          >
            Bet Now
          </motion.button>
        </div>
      </section>

      {/* Featured Matches */}
      <section className="space-y-4">
        <div className="flex items-center justify-between">
          <h2 className="text-xl font-black flex items-center gap-2">
            <div className="w-1.5 h-6 bg-primary-500 rounded-full" />
            Top Matches
          </h2>
          <button 
            onClick={() => dispatch(fetchEventsAsync())}
            className="text-xs font-bold text-gray-400 hover:text-white flex items-center gap-1 transition-colors"
          >
            Refresh <ChevronRight className="w-4 h-4" />
          </button>
        </div>

        {error && (
          <div className="bg-red-500/10 border border-red-500/50 text-red-500 text-xs font-bold p-4 rounded-xl text-center">
            {error}. Using mock data for preview.
          </div>
        )}

        <div className="grid gap-4">
          <AnimatePresence mode="popLayout">
            {events.map((event) => (
              <motion.div 
                key={event.id}
                layout
                initial={{ opacity: 0, scale: 0.95 }}
                animate={{ opacity: 1, scale: 1 }}
                className="bg-dark-800 border border-dark-700 rounded-2xl p-5 hover:border-dark-600 transition-colors backdrop-blur-sm bg-dark-800/80"
              >
                <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                  <div className="flex-1">
                    <div className="flex items-center gap-3 text-xs font-bold text-gray-500 mb-2 uppercase tracking-tighter">
                      <span>{event.league}</span>
                      <div className="w-1 h-1 bg-dark-600 rounded-full" />
                      <div className="flex items-center gap-1">
                        <Clock className="w-3 h-3" />
                        {event.time}
                      </div>
                    </div>
                    <h3 className="text-lg font-bold text-white">{event.name}</h3>
                  </div>

                  <div className="flex items-center gap-2">
                    {Array.isArray(event.markets) && event.markets.length > 0 && Array.isArray(event.markets[0]?.outcomes) ? (
                      event.markets[0].outcomes.map((outcome: any, index: number) => (
                        <OddButton
                          key={outcome?.id || index}
                          name={outcome?.name || '-'}
                          odd={outcome?.odd || 0}
                          isSelected={outcome?.id ? isSelected(outcome.id) : false}
                          onClick={() => {
                            const market = event.markets[0];
                            if (market && outcome?.id) {
                              if (isSelected(outcome.id)) {
                                dispatch(removeSelection(outcome.id));
                              } else {
                                dispatch(addSelection({
                                  eventId: event.id,
                                  eventName: event.name || 'Unknown Event',
                                  marketId: market.id,
                                  marketName: market.name || 'Unknown Market',
                                  outcomeId: outcome.id,
                                  outcomeName: outcome.name === '1' ? (event.name?.split(' vs ')[0] || 'Team 1') : outcome.name === '2' ? (event.name?.split(' vs ')[1] || 'Team 2') : outcome.name,
                                  odd: outcome.odd || 0
                                }));
                              }
                            }
                          }}
                        />
                      ))
                    ) : (
                      <span className="text-xs text-gray-500 font-bold border border-dark-600 border-dashed px-4 py-2 rounded-xl">Odds upcoming</span>
                    )}
                  </div>
                </div>
              </motion.div>
            ))}
          </AnimatePresence>
          {events.length === 0 && !loading && !error && (
            <p className="text-center text-gray-500 py-10">No matches available at the moment.</p>
          )}
        </div>
      </section>

      {/* AI Custom Bet Promo */}
      <section className="bg-dark-800 rounded-3xl p-8 border border-dark-700 relative overflow-hidden">
        <div className="relative z-10 flex flex-col md:flex-row md:items-center justify-between gap-8">
          <div className="max-w-md space-y-4">
            <h2 className="text-2xl font-black text-white flex items-center gap-3">
              Create Your Own Bet
              <motion.div
                animate={{ rotate: [0, 15, -15, 0] }}
                transition={{ repeat: Infinity, duration: 2 }}
              >
                <Zap className="w-6 h-6 text-yellow-500 fill-yellow-500" />
              </motion.div>
            </h2>
            <p className="text-gray-400 text-sm leading-relaxed">
              Don't see what you want to bet on? Use our <span className="text-primary-400 font-bold italic">AI-Driven Oddsmaker</span> to request a custom bet. Our models will calculate the price instantly!
            </p>
            <Link to="/custom-bet" className="flex items-center gap-2 text-primary-500 font-bold hover:gap-3 transition-all">
              Try Custom Bet <ChevronRight className="w-4 h-4" />
            </Link>
          </div>
          <div className="flex-shrink-0">
             <div className="w-32 h-32 bg-primary-600/10 rounded-full flex items-center justify-center relative">
                <motion.div
                  animate={{ scale: [1, 1.2, 1], opacity: [0.1, 0.3, 0.1] }}
                  transition={{ repeat: Infinity, duration: 3 }}
                  className="absolute inset-0 bg-primary-500 rounded-full"
                />
                <Zap className="w-16 h-16 text-primary-500 relative z-10" />
             </div>
          </div>
        </div>
        <div className="absolute top-0 right-0 w-64 h-64 bg-primary-600/5 blur-3xl rounded-full -translate-y-1/2 translate-x-1/2" />
      </section>
    </div>
  );
};

export default Home;
