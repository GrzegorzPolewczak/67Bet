import React, { useState, useEffect } from "react";
import { useSelector } from "react-redux";
import type { RootState } from "../../app/store";
import { ChevronLeft, Gift, Trophy } from "lucide-react";
import { Link } from "react-router-dom";
import toast from "react-hot-toast";
import { referralApi } from "../../api/referral";

const PromotionsPage: React.FC = () => {
  const [referralStatus, setReferralStatus] = useState<any>(null);
  const [newCreatorCode, setNewCreatorCode] = useState("");
  const [applyCode, setApplyCode] = useState("");
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    fetchReferralStatus();
  }, []);

  const fetchReferralStatus = async () => {
    try {
      const response = await referralApi.getStatus();
      setReferralStatus(response.data);
    } catch (error) {
      console.error("Failed to fetch referral status", error);
    }
  };

  const handleCreateCode = async () => {
    if (!newCreatorCode) return;
    setLoading(true);
    try {
      await referralApi.createCode(newCreatorCode);
      toast.success("Twój kod twórcy został utworzony!");
      fetchReferralStatus();
    } catch (error: any) {
      toast.error(error.response?.data || "Błąd podczas tworzenia kodu");
    } finally {
      setLoading(false);
    }
  };

  const handleApplyCode = async () => {
    if (!applyCode) return;
    setLoading(true);
    try {
      const response = await referralApi.applyCode(applyCode);
      toast.success(response.data.message);
      setApplyCode("");
      fetchReferralStatus();
    } catch (error: any) {
      toast.error(error.response?.data || "Nieprawidłowy kod");
    } finally {
      setLoading(false);
    }
  };

  const milestones = referralStatus?.milestones || [5, 15, 25, 50, 100, 250];
  const currentCount = referralStatus?.referralCount || 0;
  const nextMilestone = referralStatus?.nextMilestone || milestones.find((m: number) => m > currentCount) || 250;
  const progress = (currentCount / nextMilestone) * 100;

  return (
    <div className="max-w-4xl mx-auto space-y-8 pb-12">
      <Link to="/" className="inline-flex items-center gap-2 text-gray-400 hover:text-white transition-colors text-sm font-bold">
        <ChevronLeft className="w-4 h-4" /> Back to Betting
      </Link>

      <div>
        <h1 className="text-3xl font-black text-white flex items-center gap-3">
          <Gift className="w-8 h-8 text-primary-500" /> Promocje i Bonusy
        </h1>
        <p className="text-gray-400 text-sm">Zgarnij dodatkowe środki na grę dzięki kodom poleceń i promocjom.</p>
      </div>

      <div className="grid grid-cols-1 gap-8">
        {/* Referral & Promo Section */}
        <section className="bg-dark-800 border border-dark-700 rounded-3xl p-8 overflow-hidden">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            {/* Creator Code */}
            <div className="bg-dark-900 p-6 rounded-2xl border border-dark-600 flex flex-col justify-between">
              <div>
                <h3 className="text-lg font-bold text-white mb-2">Program Poleceń</h3>
                <p className="text-xs text-gray-400 mb-6">Udostępnij swój kod znajomym. Gdy go użyją, obaj otrzymacie bonus!</p>
                
                {referralStatus?.myCode ? (
                  <div className="bg-dark-800 p-4 rounded-xl border border-dashed border-primary-500/50 flex items-center justify-between mb-6">
                    <div>
                      <p className="text-[10px] text-gray-500 uppercase font-bold mb-1">Twój Kod</p>
                      <span className="text-2xl font-black text-primary-500 tracking-widest uppercase">{referralStatus.myCode}</span>
                    </div>
                    <div className="text-right">
                      <p className="text-[10px] text-gray-500 uppercase font-bold">Poleconych</p>
                      <p className="text-xl font-black text-white">{currentCount}</p>
                    </div>
                  </div>
                ) : (
                  <div className="space-y-4 mb-6">
                    <p className="text-sm font-bold text-gray-300">Utwórz swój własny kod:</p>
                    <div className="flex gap-2">
                      <input
                        type="text"
                        maxLength={10}
                        placeholder="np. TWOJ-NICK"
                        value={newCreatorCode}
                        onChange={(e) => setNewCreatorCode(e.target.value.toUpperCase())}
                        className="flex-1 bg-dark-800 border border-dark-600 rounded-xl py-3 px-4 text-white focus:border-primary-500 outline-none font-bold"
                      />
                      <button 
                        onClick={handleCreateCode}
                        disabled={loading || !newCreatorCode}
                        className="bg-primary-600 hover:bg-primary-700 disabled:opacity-50 text-white px-6 rounded-xl font-black text-xs uppercase transition-all active:scale-95"
                      >
                        Utwórz
                      </button>
                    </div>
                  </div>
                )}
              </div>

              {referralStatus?.myCode && (
                <div>
                  <div className="flex justify-between items-end mb-2">
                    <p className="text-xs font-bold text-gray-400 flex items-center gap-1">
                      <Trophy className="w-3 h-3 text-yellow-500" /> Postęp do nagrody
                    </p>
                    <p className="text-xs font-black text-white">{currentCount} / {nextMilestone}</p>
                  </div>
                  <div className="w-full bg-dark-800 h-3 rounded-full overflow-hidden">
                    <div 
                      className="h-full bg-gradient-to-r from-primary-600 to-primary-400 transition-all duration-1000"
                      style={{ width: `${Math.min(progress, 100)}%` }}
                    />
                  </div>
                  <div className="flex justify-between mt-2">
                    {milestones.map((m: number) => (
                      <span key={m} className={`text-[8px] font-bold ${currentCount >= m ? "text-primary-500" : "text-gray-600"}`}>{m}</span>
                    ))}
                  </div>
                </div>
              )}
            </div>

            {/* Apply Code */}
            <div className="bg-dark-900 p-6 rounded-2xl border border-dark-600 flex flex-col">
              <h3 className="text-lg font-bold text-white mb-2">Aktywuj Kod</h3>
              <p className="text-xs text-gray-400 mb-8">Masz kod od znajomego lub kod promocyjny? Wpisz go tutaj, aby odebrać bonus.</p>
              
              <div className="space-y-4">
                <input
                  type="text"
                  placeholder="KOD-PROMOCYJNY"
                  value={applyCode}
                  onChange={(e) => setApplyCode(e.target.value.toUpperCase())}
                  className="w-full bg-dark-800 border border-dark-600 rounded-xl py-4 px-4 text-white focus:border-primary-500 outline-none text-center font-black tracking-widest"
                />
                <button 
                  onClick={handleApplyCode}
                  disabled={loading || !applyCode}
                  className="w-full bg-accent-success hover:bg-green-600 disabled:opacity-50 text-dark-900 py-4 rounded-xl font-black text-sm uppercase transition-all active:scale-95"
                >
                  Odbierz Bonus
                </button>
              </div>
              
              <div className="mt-auto pt-8">
                <div className="bg-dark-800/50 p-4 rounded-xl border border-dark-700">
                  <h4 className="text-xs font-bold text-gray-300 mb-1">Zasady:</h4>
                  <ul className="text-[10px] text-gray-500 space-y-1 list-disc pl-3">
                    <li>Kody poleceń od znajomych działają tylko dla nowych kont.</li>
                    <li>Możesz użyć tylko jednego kodu polecenia.</li>
                    <li>Kody promocyjne mogą mieć datę ważności.</li>
                    <li>Bonusy są przyznawane w formie Freebetów.</li>
                  </ul>
                </div>
              </div>
            </div>
          </div>
        </section>

        {/* Info Cards */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div className="bg-dark-800 border border-dark-700 p-6 rounded-3xl">
            <div className="w-10 h-10 bg-primary-500/10 rounded-xl flex items-center justify-center mb-4">
              <Gift className="w-6 h-6 text-primary-500" />
            </div>
            <h4 className="font-bold text-white mb-1">Darmowe Freebety</h4>
            <p className="text-xs text-gray-400">Każde polecenie to 20 PLN Freebetu dla Ciebie i Twojego znajomego.</p>
          </div>
          <div className="bg-dark-800 border border-dark-700 p-6 rounded-3xl">
            <div className="w-10 h-10 bg-yellow-500/10 rounded-xl flex items-center justify-center mb-4">
              <Trophy className="w-6 h-6 text-yellow-500" />
            </div>
            <h4 className="font-bold text-white mb-1">Kamienie Milowe</h4>
            <p className="text-xs text-gray-400">Osiągaj progi poleceń (5, 15, 25...) i zgarniaj jeszcze większe nagrody!</p>
          </div>
          <div className="bg-dark-800 border border-dark-700 p-6 rounded-3xl">
            <div className="w-10 h-10 bg-accent-success/10 rounded-xl flex items-center justify-center mb-4">
              <Gift className="w-6 h-6 text-accent-success" />
            </div>
            <h4 className="font-bold text-white mb-1">Kody Promo</h4>
            <p className="text-xs text-gray-400">Śledź nasze social media, aby nie przegapić limitowanych kodów promo.</p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default PromotionsPage;
