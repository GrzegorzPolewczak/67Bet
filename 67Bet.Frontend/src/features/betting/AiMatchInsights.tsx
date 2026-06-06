import React, { useState } from "react";
import { Sparkles, Loader2, Info } from "lucide-react";
import { motion, AnimatePresence } from "framer-motion";
import { bettingApi } from "../../api/axios";

interface AiMatchInsightsProps {
  eventId: string;
}

const AiMatchInsights: React.FC<AiMatchInsightsProps> = ({ eventId }) => {
  const [insight, setInsight] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleFetchInsight = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await bettingApi.get(
        `/AiAssistant/event/${eventId}/insight`,
      );
      setInsight(response.data.insight);
    } catch (err) {
      console.error("Error fetching AI insight:", err);
      setError("Nie udało się pobrać analizy AI. Spróbuj ponownie później.");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="bg-dark-800 border border-dark-700 rounded-3xl p-6 h-fit mt-6">
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-sm font-bold text-white uppercase tracking-wider flex items-center gap-2">
          <Sparkles className="w-4 h-4 text-primary-400" />
          AI Match Insights
        </h2>
        {!insight && !isLoading && (
          <button
            onClick={handleFetchInsight}
            className="text-[10px] font-black bg-primary-600 hover:bg-primary-500 text-white px-3 py-1 rounded-lg transition-colors flex items-center gap-1 uppercase tracking-tighter"
          >
            Zapytaj AI
          </button>
        )}
      </div>

      <AnimatePresence mode="wait">
        {isLoading ? (
          <motion.div
            key="loading"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="flex items-center gap-3 text-gray-400 py-4"
          >
            <Loader2 className="w-5 h-5 animate-spin text-primary-500" />
            <span className="text-xs font-medium italic">
              Gemini analizuje dane meczowe...
            </span>
          </motion.div>
        ) : insight ? (
          <motion.div
            key="content"
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            className="bg-primary-500/5 border border-primary-500/20 rounded-2xl p-4"
          >
            <p className="text-sm text-gray-200 leading-relaxed italic">
              "{insight}"
            </p>
            <div className="mt-3 flex items-center gap-1.5 opacity-50">
              <Info className="w-3 h-3 text-primary-400" />
              <span className="text-[10px] text-gray-400 font-bold uppercase tracking-tighter">
                Powered by Gemini 1.5 Flash
              </span>
            </div>
          </motion.div>
        ) : error ? (
          <motion.div
            key="error"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            className="text-xs text-red-400 bg-red-500/10 border border-red-500/20 rounded-xl p-3"
          >
            {error}
          </motion.div>
        ) : (
          <motion.div key="empty" className="text-xs text-gray-500 italic py-2">
            Kliknij przycisk powyżej, aby wygenerować inteligentną analizę tego
            spotkania.
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};

export default AiMatchInsights;
