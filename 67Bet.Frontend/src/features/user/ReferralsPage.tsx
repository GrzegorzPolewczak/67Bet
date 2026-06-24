import React, { useState, useEffect } from "react";
import { ChevronLeft, Gift, Trophy } from "lucide-react";
import { Link } from "react-router-dom";
import toast from "react-hot-toast";
import { referralApi } from "../../api/referral";

const ReferralsPage: React.FC = () => {
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
      toast.success("Your creator code has been created!");
      fetchReferralStatus();
    } catch (error: any) {
      toast.error(error.response?.data || "Error creating code");
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
      toast.error(error.response?.data || "Invalid code");
    } finally {
      setLoading(false);
    }
  };

  const milestones = referralStatus?.milestones || [5, 15, 25, 50, 100, 250];
  const currentCount = referralStatus?.referralCount || 0;
  const nextMilestone =
    referralStatus?.nextMilestone ||
    milestones.find((m: number) => m > currentCount) ||
    250;

  // Segmented progress calculation
  const milestonesWithZero = [0, ...milestones];
  let segmentIndex = 0;
  for (let i = 0; i < milestonesWithZero.length - 1; i++) {
    if (currentCount >= milestonesWithZero[i]) {
      segmentIndex = i;
    }
  }

  let progress = 0;
  if (currentCount >= 250) {
    progress = 100;
  } else {
    const currentSegmentStart = milestonesWithZero[segmentIndex];
    const currentSegmentEnd = milestonesWithZero[segmentIndex + 1];
    const segmentProgress =
      (currentCount - currentSegmentStart) /
      (currentSegmentEnd - currentSegmentStart);
    progress =
      ((segmentIndex + segmentProgress) / (milestonesWithZero.length - 1)) *
      100;
  }

  return (
    <div className="max-w-4xl mx-auto space-y-8 pb-12">
      <Link
        to="/"
        className="inline-flex items-center gap-2 text-gray-400 hover:text-white transition-colors text-sm font-bold"
      >
        <ChevronLeft className="w-4 h-4" /> Back to Betting
      </Link>

      <div>
        <h1 className="text-3xl font-black text-white flex items-center gap-3">
          <Gift className="w-8 h-8 text-primary-500" /> Referrals & Rewards
        </h1>
        <p className="text-gray-400 text-sm">
          Earn extra credits by sharing your code and using promotional coupons.
        </p>
      </div>

      <div className="grid grid-cols-1 gap-8">
        {/* Referral & Promo Section */}
        <section className="bg-dark-800 border border-dark-700 rounded-3xl p-8 overflow-hidden">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            {/* Creator Code */}
            <div className="bg-dark-900 p-6 rounded-2xl border border-dark-600 flex flex-col justify-between">
              <div>
                <h3 className="text-lg font-bold text-white mb-2">
                  Affiliate Program
                </h3>
                <p className="text-xs text-gray-400 mb-6">
                  Share your unique code with friends. When they use it, you
                  both get a bonus!
                </p>

                {referralStatus?.myCode ? (
                  <div className="bg-dark-800 p-4 rounded-xl border border-dashed border-primary-500/50 flex items-center justify-between mb-6">
                    <div>
                      <p className="text-[10px] text-gray-500 uppercase font-bold mb-1">
                        Your Code
                      </p>
                      <span className="text-2xl font-black text-primary-500 tracking-widest uppercase">
                        {referralStatus.myCode}
                      </span>
                    </div>
                    <div className="text-right">
                      <p className="text-[10px] text-gray-500 uppercase font-bold">
                        Referrals
                      </p>
                      <p className="text-xl font-black text-white">
                        {currentCount}
                      </p>
                    </div>
                  </div>
                ) : (
                  <div className="space-y-4 mb-6">
                    <p className="text-sm font-bold text-gray-300">
                      Create your custom code:
                    </p>
                    <div className="flex gap-2">
                      <input
                        type="text"
                        maxLength={10}
                        placeholder="e.g. YOUR-NICK"
                        value={newCreatorCode}
                        onChange={(e) =>
                          setNewCreatorCode(e.target.value.toUpperCase())
                        }
                        className="flex-1 bg-dark-800 border border-dark-600 rounded-xl py-3 px-4 text-white focus:border-primary-500 outline-none font-bold"
                      />
                      <button
                        onClick={handleCreateCode}
                        disabled={loading || !newCreatorCode}
                        className="bg-primary-600 hover:bg-primary-700 disabled:opacity-50 text-white px-6 rounded-xl font-black text-xs uppercase transition-all active:scale-95"
                      >
                        Create
                      </button>
                    </div>
                  </div>
                )}
              </div>

              {referralStatus?.myCode && (
                <div>
                  <div className="flex justify-between items-end mb-2">
                    <p className="text-xs font-bold text-gray-400 flex items-center gap-1">
                      <Trophy className="w-3 h-3 text-yellow-500" /> Progress to
                      next reward
                    </p>
                    <p className="text-xs font-black text-white">
                      {currentCount} / {nextMilestone}
                    </p>
                  </div>
                  <div className="w-full bg-dark-800 h-3 rounded-full overflow-hidden">
                    <div
                      className="h-full bg-gradient-to-r from-primary-600 to-primary-400 transition-all duration-1000"
                      style={{ width: `${Math.min(progress, 100)}%` }}
                    />
                  </div>
                  <div className="flex justify-between mt-2">
                    {milestonesWithZero.map((m: number) => (
                      <span
                        key={m}
                        className={`text-[8px] font-bold ${currentCount >= m ? "text-primary-500" : "text-gray-600"}`}
                      >
                        {m}
                      </span>
                    ))}
                  </div>
                </div>
              )}
            </div>

            {/* Apply Code */}
            <div className="bg-dark-900 p-6 rounded-2xl border border-dark-600 flex flex-col">
              <h3 className="text-lg font-bold text-white mb-2">
                Activate Code
              </h3>
              <p className="text-xs text-gray-400 mb-8">
                Have a friend's referral code or a promotional coupon? Enter it
                below to claim your bonus.
              </p>

              {referralStatus?.usedReferralCode && (
                <div className="mb-6 p-4 bg-primary-500/10 border border-primary-500/30 rounded-xl flex items-center justify-between text-xs">
                  <span className="text-gray-400 font-bold">
                    Used Referral Code:
                  </span>
                  <span className="text-primary-400 font-black tracking-wider uppercase bg-dark-800 px-3 py-1 rounded-lg">
                    {referralStatus.usedReferralCode}
                  </span>
                </div>
              )}

              <div className="space-y-4">
                <input
                  type="text"
                  placeholder="PROMO-CODE"
                  value={applyCode}
                  onChange={(e) => setApplyCode(e.target.value.toUpperCase())}
                  className="w-full bg-dark-800 border border-dark-600 rounded-xl py-4 px-4 text-white focus:border-primary-500 outline-none text-center font-black tracking-widest"
                />
                <button
                  onClick={handleApplyCode}
                  disabled={loading || !applyCode}
                  className="w-full bg-accent-success hover:bg-green-600 disabled:opacity-50 text-dark-900 py-4 rounded-xl font-black text-sm uppercase transition-all active:scale-95"
                >
                  Claim Bonus
                </button>
              </div>

              <div className="mt-auto pt-8">
                <div className="bg-dark-800/50 p-4 rounded-xl border border-dark-700">
                  <h4 className="text-xs font-bold text-gray-300 mb-1">
                    Rules:
                  </h4>
                  <ul className="text-[10px] text-gray-500 space-y-1 list-disc pl-3">
                    <li>Friend referral codes only work for new accounts.</li>
                    <li>You can only use one referral code per account.</li>
                    <li>Promotional codes may have an expiration date.</li>
                    <li>Bonuses are credited as Freebets.</li>
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
            <h4 className="font-bold text-white mb-1">Freebets</h4>
            <p className="text-xs text-gray-400">
              Every referral grants a 20 PLN Freebet for both you and your friend.
            </p>
          </div>
          <div className="bg-dark-800 border border-dark-700 p-6 rounded-3xl">
            <div className="w-10 h-10 bg-yellow-500/10 rounded-xl flex items-center justify-center mb-4">
              <Trophy className="w-6 h-6 text-yellow-500" />
            </div>
            <h4 className="font-bold text-white mb-1">Milestones</h4>
            <p className="text-xs text-gray-400">
              Reach referral thresholds (5, 15, 25...) to unlock even bigger
              rewards!
            </p>
          </div>
          <div className="bg-dark-800 border border-dark-700 p-6 rounded-3xl">
            <div className="w-10 h-10 bg-accent-success/10 rounded-xl flex items-center justify-center mb-4">
              <Gift className="w-6 h-6 text-accent-success" />
            </div>
            <h4 className="font-bold text-white mb-1">Promo Codes</h4>
            <p className="text-xs text-gray-400">
              Follow our social media to never miss out on limited promotional
              codes.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ReferralsPage;
