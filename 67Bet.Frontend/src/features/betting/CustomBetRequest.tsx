import React, { useState } from 'react';
import { motion } from 'framer-motion';
import { Zap, Send, Info, ChevronLeft } from 'lucide-react';
import { Link } from 'react-router-dom';

const CustomBetRequest: React.FC = () => {
  const [request, setRequest] = useState('');
  const [submitted, setSubmitted] = useState(false);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!request.trim()) return;
    // Symulacja wysyłki do AI
    setSubmitted(true);
  };

  if (submitted) {
    return (
      <div className="max-w-2xl mx-auto h-[60vh] flex flex-col items-center justify-center text-center space-y-6">
        <motion.div 
          initial={{ scale: 0 }}
          animate={{ scale: 1 }}
          className="w-20 h-20 bg-accent-success/20 rounded-full flex items-center justify-center"
        >
          <Zap className="w-10 h-10 text-accent-success" />
        </motion.div>
        <h2 className="text-3xl font-black text-white">Request Sent!</h2>
        <p className="text-gray-400">
          Our AI Oddsmaker is now analyzing your request: <br />
          <span className="text-white italic font-medium">"{request}"</span>
        </p>
        <p className="text-sm text-gray-500">You will be notified once the odds are ready.</p>
        <Link to="/" className="text-primary-500 font-bold hover:underline">Back to Home</Link>
      </div>
    );
  }

  return (
    <div className="max-w-2xl mx-auto space-y-8">
      <Link to="/" className="inline-flex items-center gap-2 text-gray-400 hover:text-white transition-colors text-sm font-bold">
        <ChevronLeft className="w-4 h-4" /> Back to Betting
      </Link>

      <div className="space-y-2">
        <h1 className="text-4xl font-black text-white flex items-center gap-4">
          AI Custom Bet
          <Zap className="w-8 h-8 text-yellow-500 fill-yellow-500" />
        </h1>
        <p className="text-gray-400 text-lg">
          Can't find what you're looking for? Describe the event and we'll price it for you.
        </p>
      </div>

      <form onSubmit={handleSubmit} className="bg-dark-800 border border-dark-700 rounded-3xl p-8 space-y-6">
        <div className="space-y-4">
          <label className="text-sm font-bold text-gray-300 block">Describe your bet</label>
          <textarea
            value={request}
            onChange={(e) => setRequest(e.target.value)}
            placeholder="Example: Robert Lewandowski will score a header and get a yellow card in the first half of El Clasico."
            className="w-full bg-dark-900 border border-dark-600 rounded-2xl p-4 text-white placeholder:text-gray-600 focus:outline-none focus:border-primary-500 min-h-[150px] transition-colors"
          />
        </div>

        <div className="bg-primary-600/10 border border-primary-500/20 rounded-2xl p-4 flex gap-4">
          <Info className="w-6 h-6 text-primary-500 shrink-0" />
          <p className="text-xs text-primary-100 leading-relaxed">
            Our AI uses historical data and real-time statistics to calculate fair odds. 
            Custom bets are usually priced within 2-5 minutes and require administrator approval.
          </p>
        </div>

        <button 
          type="submit"
          className="w-full bg-primary-600 hover:bg-primary-700 text-white py-4 rounded-2xl font-black text-lg flex items-center justify-center gap-3 transition-all active:scale-[0.98] shadow-lg shadow-primary-600/20"
        >
          <Send className="w-5 h-5" />
          Request Odds
        </button>
      </form>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div className="p-6 bg-dark-800 border border-dark-700 rounded-2xl">
          <h4 className="font-bold text-white mb-2">Step 1: Describe</h4>
          <p className="text-xs text-gray-500">Be as specific as possible about the players, teams, and conditions.</p>
        </div>
        <div className="p-6 bg-dark-800 border border-dark-700 rounded-2xl">
          <h4 className="font-bold text-white mb-2">Step 2: AI Pricing</h4>
          <p className="text-xs text-gray-500">Our ML.NET models calculate probability based on thousands of data points.</p>
        </div>
      </div>
    </div>
  );
};

export default CustomBetRequest;
