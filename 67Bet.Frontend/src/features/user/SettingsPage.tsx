import React, { useState } from "react";
import { useSelector, useDispatch } from "react-redux";
import type { RootState, AppDispatch } from "../../app/store";
import {
  User,
  Mail,
  Shield,
  Bell,
  Save,
  ChevronLeft,
  Trophy,
  Star,
  CheckCircle2,
} from "lucide-react";
import { Link } from "react-router-dom";
import toast from "react-hot-toast";
import { fetchAchievements } from "../../features/gamification/gamificationSlice";

const SettingsPage: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const { user } = useSelector((state: RootState) => state.auth);
  const { achievements } = useSelector(
    (state: RootState) => state.gamification,
  );
  const [notifications, setNotifications] = useState(true);

  React.useEffect(() => {
    dispatch(fetchAchievements());
  }, [dispatch]);

  const handleSave = () => {
    toast.success("Settings saved successfully!");
  };

  return (
    <div className="max-w-4xl mx-auto space-y-8 pb-12">
      <Link
        to="/"
        className="inline-flex items-center gap-2 text-gray-400 hover:text-white transition-colors text-sm font-bold"
      >
        <ChevronLeft className="w-4 h-4" /> Back to Betting
      </Link>

      <div>
        <h1 className="text-3xl font-black text-white">Profile Settings</h1>
        <p className="text-gray-400 text-sm">
          Manage your account details and preferences.
        </p>
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
                <label className="text-xs font-bold text-gray-500 uppercase px-1">
                  Username
                </label>
                <input
                  type="text"
                  value={user?.username || ""}
                  disabled
                  className="w-full bg-dark-900 border border-dark-600 rounded-xl py-3 px-4 text-gray-400 opacity-70 cursor-not-allowed"
                />
              </div>
              <div>
                <label className="text-xs font-bold text-gray-500 uppercase px-1">
                  Email Address
                </label>
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

          {/* Preferences */}
          <section className="bg-dark-800 border border-dark-700 rounded-3xl p-6">
            <h2 className="text-lg font-bold text-white mb-4 flex items-center gap-2">
              <Bell className="w-5 h-5 text-primary-500" /> Notifications
            </h2>
            <div className="space-y-4">
              <label className="flex items-center justify-between cursor-pointer group">
                <div>
                  <p className="font-bold text-gray-300 group-hover:text-white transition-colors">
                    Bet Updates
                  </p>
                  <p className="text-xs text-gray-500">
                    Get notified when your bet is settled.
                  </p>
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

          {/* Achievements */}
          <section className="bg-dark-800 border border-dark-700 rounded-3xl p-6">
            <h2 className="text-lg font-bold text-white mb-4 flex items-center gap-2">
              <Trophy className="w-5 h-5 text-primary-500" /> My Achievements
            </h2>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              {achievements && achievements.length > 0 ? (
                achievements.map((achievement) => (
                  <div
                    key={achievement.achievementId}
                    className={`p-4 rounded-2xl border transition-all ${
                      achievement.isUnlocked
                        ? "bg-primary-500/10 border-primary-500/30"
                        : "bg-dark-900 border-dark-600 opacity-60"
                    }`}
                  >
                    <div className="flex items-start gap-3">
                      <div
                        className={`w-10 h-10 rounded-full flex items-center justify-center shrink-0 ${
                          achievement.isUnlocked
                            ? "bg-primary-500 text-white"
                            : "bg-dark-700 text-gray-500"
                        }`}
                      >
                        {achievement.isUnlocked ? (
                          <CheckCircle2 className="w-5 h-5" />
                        ) : (
                          <Star className="w-5 h-5" />
                        )}
                      </div>
                      <div className="min-w-0">
                        <p className="font-bold text-sm text-white truncate">
                          {achievement.name}
                        </p>
                        <p className="text-[10px] text-gray-500 mt-0.5 leading-tight">
                          {achievement.description}
                        </p>
                        {!achievement.isUnlocked && (
                          <div className="mt-2">
                            <div className="w-full h-1 bg-dark-700 rounded-full overflow-hidden">
                              <div
                                className="h-full bg-gray-600"
                                style={{
                                  width: `${Math.min(
                                    100,
                                    (Number(achievement.currentProgress) /
                                      Number(achievement.threshold)) *
                                      100,
                                  )}%`,
                                }}
                              ></div>
                            </div>
                            <p className="text-[9px] text-gray-600 mt-1 font-bold">
                              {Number(achievement.currentProgress).toFixed(0)} /{" "}
                              {Number(achievement.threshold).toFixed(0)}
                            </p>
                          </div>
                        )}
                      </div>
                    </div>
                  </div>
                ))
              ) : (
                <p className="text-gray-500 text-xs col-span-2 text-center py-4">
                  No achievements available yet.
                </p>
              )}
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
              {user?.isKycVerified ? (
                <div className="bg-dark-900 p-4 rounded-xl border border-dark-600 flex items-start gap-3">
                  <div className="w-2 h-2 rounded-full bg-accent-success mt-1.5" />
                  <div>
                    <p className="font-bold text-sm text-white">
                      Account Verified
                    </p>
                    <p className="text-xs text-gray-500 mt-1">
                      Your identity has been verified allowing full platform
                      access.
                    </p>
                  </div>
                </div>
              ) : (
                <div className="bg-dark-900 p-4 rounded-xl border border-dark-600 flex flex-col gap-3">
                  <div className="flex items-start gap-3">
                    <div className="w-2 h-2 rounded-full bg-yellow-500 mt-1.5" />
                    <div>
                      <p className="font-bold text-sm text-white">
                        Verification Required
                      </p>
                      <p className="text-xs text-gray-500 mt-1">
                        Please verify your identity to unlock all features.
                      </p>
                    </div>
                  </div>
                  <Link
                    to="/kyc-verify"
                    className="w-full bg-primary-600/20 text-primary-500 hover:bg-primary-600 hover:text-white transition-colors py-2 rounded-lg font-bold text-xs text-center border border-primary-600/30"
                  >
                    Start KYC Verification
                  </Link>
                </div>
              )}
            </div>
          </section>
        </div>
      </div>
    </div>
  );
};

export default SettingsPage;
