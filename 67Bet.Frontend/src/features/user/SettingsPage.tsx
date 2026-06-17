import React, { useState } from "react";
import { useSelector } from "react-redux";
import type { RootState } from "../../app/store";
import { User, Mail, Shield, Bell, Save, ChevronLeft } from "lucide-react";
import { Link } from "react-router-dom";
import toast from "react-hot-toast";

const SettingsPage: React.FC = () => {
  const { user } = useSelector((state: RootState) => state.auth);
  const [notifications, setNotifications] = useState(true);

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
