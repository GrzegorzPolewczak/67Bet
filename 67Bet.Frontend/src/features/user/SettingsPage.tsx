import React, { useState, useEffect } from "react";
import { useSelector } from "react-redux";
import type { RootState } from "../../app/store";
import { User, Mail, Shield, Bell, Save, ChevronLeft, Gift, Trophy } from "lucide-react";
import { Link } from "react-router-dom";
import toast from "react-hot-toast";
import { referralApi } from "../../api/referral";

const SettingsPage: React.FC = () => {
  const { user } = useSelector((state: RootState) => state.auth);
  const [notifications, setNotifications] = useState(true);
  
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

  const handleSave = () => {
    toast.success("Settings saved successfully!");
  };

  const milestones = [5, 15, 25, 50, 100, 250];
  const currentCount = referralStatus?.referralCount || 0;
  const nextMilestone = milestones.find(m => m > currentCount) || 250;
  const progress = (currentCount / nextMilestone) * 100;

  return (
    <div className="max-w-4xl mx-auto space-y-8 pb-12">
      <Link to="/" className="inline-flex items-center gap-2 text-gray-400 hover:text-white transition-colors text-sm font-bold">
        <ChevronLeft className="w-4 h-4" /> Back to Betting
      </Link>

      <div>
        <h1 className="text-3xl font-black text-white">Profile Settings</h1>
        <p className="text-gray-400 text-sm">Manage your account details and preferences.</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
        <div className="md:col-span-2 space-y-6">
          {/* Personal Info */}
          <section className="bg-dark-800 border border-dark-700 rounded-3xl p-6">
            <h2 className="text-lg font-bold text-white mb-4 flex items-center gap-2">
              <User className="w-5 h-5 text-primary-500" /> Personal Information
            </h2>
            <div className="space-y-4">
              <div>
                <label className="text-xs font-bold text-gray-500 uppercase px-1">Username</label>
                <input
                  type="text"
                  value={user?.username || ""}
                  disabled
                  className="w-full bg-dark-900 border border-dark-600 rounded-xl py-3 px-4 text-gray-400 opacity-70 cursor-not-allowed"
                />
              </div>
              <div>
                <label className="text-xs font-bold text-gray-500 uppercase px-1">Email Address</label>
                <div className="relative">
                  <Mail className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-500 w-4 h-4" />
                  <input
                    type="email"
                    value={user?.email || ""}
                    disabled
                    className="w-full bg-dark-900 border border-dark-600 rounded-xl py-3 pl-10 pr-4 text-gray-400 opacity-70 cursor-not-allowed"
                  />
                </div>
              </div>
            </div>
          </section>

          {/* Referral & Promo Section */}
          <section className="bg-dark-800 border border-dark-700 rounded-3xl p-6 overflow-hidden">
            <h2 className="text-lg font-bold text-white mb-4 flex items-center gap-2">
              <Gift className="w-5 h-5 text-accent-success" /> Kody poleceń i Promocje
            </h2>
            
            <div className="grid grid-cols-1 gap-6">
              {/* Creator Code */}
              <div className="bg-dark-900 p-5 rounded-2xl border border-dark-600">
                <p className="text-sm font-bold text-white mb-2">Twój Kod Twórcy</p>
                {referralStatus?.myCode ? (
                  <div className="flex items-center justify-between">
                    <span className="text-2xl font-black text-primary-500 tracking-widest uppercase">{referralStatus.myCode}</span>
                    <div className="text-right">
                      <p className="text-[10px] text-gray-500 uppercase font-bold">Poleconych osób</p>
                      <p className="text-lg font-black text-white">{currentCount}</p>
                    </div>
                  </div>
                ) : (
                  <div className="flex gap-2">
                    <input
                      type="text"
                      maxLength={10}
                      placeholder="Wpisz swój kod (np. AS)"
                      value={newCreatorCode}
                      onChange={(e) => setNewCreatorCode(e.target.value.toUpperCase())}
                      className="flex-1 bg-dark-800 border border-dark-600 rounded-xl py-2 px-4 text-white focus:border-primary-500 outline-none"
                    />
                    <button 
                      onClick={handleCreateCode}
                      disabled={loading || !newCreatorCode}
                      className="bg-primary-600 hover:bg-primary-700 disabled:opacity-50 text-white px-4 rounded-xl font-bold text-xs"
                    >
                      Utwórz
                    </button>
                  </div>
                )}

                {referralStatus?.myCode && (
                  <div className="mt-6">
                    <div className="flex justify-between items-end mb-2">
                      <p className="text-xs font-bold text-gray-400 flex items-center gap-1">
                        <Trophy className="w-3 h-3 text-yellow-500" /> Postęp do nagrody
                      </p>
                      <p className="text-xs font-black text-white">{currentCount} / {nextMilestone}</p>
                    </div>
                    <div className="w-full bg-dark-800 h-2 rounded-full overflow-hidden">
                      <div 
                        className="h-full bg-gradient-to-r from-primary-600 to-primary-400 transition-all duration-1000"
                        style={{ width: `${Math.min(progress, 100)}%` }}
                      />
                    </div>
                    <div className="flex justify-between mt-1">
                      {milestones.map(m => (
                        <span key={m} className={`text-[8px] font-bold ${currentCount >= m ? "text-primary-500" : "text-gray-600"}`}>{m}</span>
                      ))}
                    </div>
                  </div>
                )}
              </div>

              {/* Apply Code */}
              <div className="bg-dark-900 p-5 rounded-2xl border border-dark-600">
                <p className="text-sm font-bold text-white mb-2">Wprowadź kod polecenia lub promo</p>
                <div className="flex gap-2">
                  <input
                    type="text"
                    placeholder="KOD-ZNAM-LUB-PROMO"
                    value={applyCode}
                    onChange={(e) => setApplyCode(e.target.value.toUpperCase())}
                    className="flex-1 bg-dark-800 border border-dark-600 rounded-xl py-2 px-4 text-white focus:border-primary-500 outline-none"
                  />
                  <button 
                    onClick={handleApplyCode}
                    disabled={loading || !applyCode}
                    className="bg-accent-success hover:bg-green-600 disabled:opacity-50 text-dark-900 px-6 rounded-xl font-black text-xs uppercase"
                  >
                    Użyj
                  </button>
                </div>
                <p className="text-[10px] text-gray-500 mt-2 italic">*Kody poleceń od znajomych można wpisać tylko raz na konto.</p>
              </div>
            </div>
          </section>

          {/* Preferences */}
          <section className="bg-dark-800 border border-dark-700 rounded-3xl p-6">
            <h2 className="text-lg font-bold text-white mb-4 flex items-center gap-2">
              <Bell className="w-5 h-5 text-primary-500" /> Notifications
            </h2>
            <div className="space-y-4">
              <label className="flex items-center justify-between cursor-pointer group">
                <div>
                  <p className="font-bold text-gray-300 group-hover:text-white transition-colors">Bet Updates</p>
                  <p className="text-xs text-gray-500">Get notified when your bet is settled.</p>
                </div>
                <input
                  type="checkbox"
                  checked={notifications}
                  onChange={(e) => setNotifications(e.target.checked)}
                  className="w-5 h-5 rounded border-dark-600 text-primary-600 bg-dark-900 focus:ring-primary-500"
                />
              </label>
            </div>
          </section>

          <button
            onClick={handleSave}
            className="w-full bg-primary-600 hover:bg-primary-700 text-white py-4 rounded-xl font-black text-sm flex items-center justify-center gap-2 transition-all active:scale-95"
          >
            <Save className="w-4 h-4" /> Save Changes
          </button>
        </div>

        {/* Security Summary */}
        <div className="space-y-6">
          <section className="bg-dark-800 border border-dark-700 rounded-3xl p-6">
            <h2 className="text-lg font-bold text-white mb-4 flex items-center gap-2">
              <Shield className="w-5 h-5 text-primary-500" /> Security
            </h2>
            <div className="space-y-4">
              <div className="bg-dark-900 p-4 rounded-xl border border-dark-600 flex items-start gap-3">        
                <div className="w-2 h-2 rounded-full bg-accent-success mt-1.5" />
                <div>
                  <p className="font-bold text-sm text-white">Account Verified</p>
                  <p className="text-xs text-gray-500 mt-1">Your identity has been verified allowing full platform access.</p>
                </div>
              </div>
            </div>
          </section>
        </div>
      </div>
    </div>
  );
};

export default SettingsPage;
