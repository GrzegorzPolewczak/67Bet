import React, { useEffect } from "react";
import { useSelector, useDispatch } from "react-redux";
import type { RootState, AppDispatch } from "../../app/store";
import {
  Trophy,
  ChevronLeft,
  Star,
  Award,
  Wallet,
  Flame,
  Check,
  Zap,
  Info,
  CheckSquare,
} from "lucide-react";
import { Link } from "react-router-dom";
import {
  fetchGamificationProgress,
  fetchAchievements,
} from "../../features/gamification/gamificationSlice";

const TrophyRoomPage: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const { progress, achievements } = useSelector(
    (state: RootState) => state.gamification,
  );

  useEffect(() => {
    dispatch(fetchGamificationProgress());
    dispatch(fetchAchievements());
  }, [dispatch]);

  const currentCount = progress?.experiencePoints || 0;
  const currentLevel = progress?.currentLevel || 1;
  const nextLevelXp = progress?.nextLevelXp || 250;
  const progressPercentage = progress?.progressPercentage || 0;

  // XP needed for current level to calculate current level relative progress
  const xpForCurrentLevel = (level: number) => {
    if (level <= 1) return 0;
    return Math.floor(100 * Math.pow(level, 1.5));
  };
  const currentLevelMinXp = xpForCurrentLevel(currentLevel);
  const relativeXpInLevel = currentCount - currentLevelMinXp;
  const relativeXpNeededForNext = nextLevelXp - currentLevelMinXp;

  const categories = [
    {
      type: "TotalBets",
      title: "Matches Placed",
      description:
        "Place slips on sports events to gain experience and climb tiers.",
      icon: Trophy,
      color: "from-orange-500 to-amber-600",
      textColor: "text-orange-500",
      bgLight: "bg-orange-500/10 border-orange-500/20",
      unit: "bets",
    },
    {
      type: "HighOdds",
      title: "Sniper (Highest Win Odds)",
      description:
        "Hit winning slips with high multipliers to prove your precision.",
      icon: Zap,
      color: "from-red-500 to-pink-600",
      textColor: "text-red-500",
      bgLight: "bg-red-500/10 border-red-500/20",
      unit: "odds",
    },
    {
      type: "TotalWinnings",
      title: "High Roller",
      description: "Accumulate total payouts from winning slips.",
      icon: Wallet,
      color: "from-green-500 to-emerald-600",
      textColor: "text-green-500",
      bgLight: "bg-green-500/10 border-green-500/20",
      unit: "PLN",
    },
    {
      type: "LoginStreak",
      title: "Daily Bettor",
      description:
        "Log in consecutively to build your streak and show consistency.",
      icon: Flame,
      color: "from-amber-500 to-yellow-600",
      textColor: "text-amber-500",
      bgLight: "bg-amber-500/10 border-amber-500/20",
      unit: "days",
    },
  ];

  const getTierColor = (index: number, isUnlocked: boolean) => {
    if (!isUnlocked) return "border-dark-600 bg-dark-900 text-gray-700";
    switch (index) {
      case 0: // Bronze
        return "border-orange-500 bg-orange-500/20 text-orange-500 shadow-[0_0_10px_rgba(249,115,22,0.15)]";
      case 1: // Silver
        return "border-slate-300 bg-slate-300/20 text-slate-300 shadow-[0_0_10px_rgba(203,213,225,0.15)]";
      case 2: // Gold
        return "border-yellow-400 bg-yellow-400/20 text-yellow-400 shadow-[0_0_10px_rgba(250,204,21,0.15)]";
      case 3: // Diamond
        return "border-cyan-400 bg-cyan-400/20 text-cyan-400 shadow-[0_0_10px_rgba(34,211,238,0.15)]";
      default:
        return "border-primary-500 bg-primary-500/20 text-primary-500";
    }
  };

  const getTierName = (index: number) => {
    switch (index) {
      case 0:
        return "Bronze";
      case 1:
        return "Silver";
      case 2:
        return "Gold";
      case 3:
        return "Diamond";
      default:
        return "";
    }
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
        <h1 className="text-3xl font-black text-white flex items-center gap-3">
          <Award className="w-8 h-8 text-primary-500" /> Trophy Room
        </h1>
        <p className="text-gray-400 text-sm">
          Track your progression level, XP stats, and unlock stages of
          achievements.
        </p>
      </div>

      {/* Progress & Info Grid */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
        {/* Big Progression Card */}
        <section className="md:col-span-2 bg-dark-800 border border-dark-700 rounded-3xl p-6 flex flex-col justify-between relative overflow-hidden">
          <div className="absolute top-0 right-0 w-32 h-32 bg-primary-500/10 rounded-full blur-3xl -z-10" />

          <div>
            <div className="flex justify-between items-start mb-6">
              <div>
                <p className="text-[10px] text-gray-500 uppercase font-black tracking-wider mb-1">
                  Current Status
                </p>
                <h2 className="text-4xl font-black text-white flex items-baseline gap-2">
                  Level{" "}
                  <span className="text-primary-500 text-5xl">
                    {currentLevel}
                  </span>
                </h2>
              </div>
              <div className="text-right">
                <p className="text-[10px] text-gray-500 uppercase font-black tracking-wider mb-1">
                  Total Experience
                </p>
                <p className="text-xl font-black text-white">
                  {currentCount.toLocaleString()} XP
                </p>
              </div>
            </div>

            <div className="space-y-2 mb-4">
              <div className="flex justify-between items-end text-xs">
                <span className="text-gray-400 font-bold">
                  Progress to Level {currentLevel + 1}
                </span>
                <span className="font-mono text-white font-black">
                  {Math.max(0, relativeXpInLevel).toLocaleString()} /{" "}
                  {relativeXpNeededForNext.toLocaleString()} XP
                </span>
              </div>
              <div className="w-full bg-dark-900 h-4 rounded-full overflow-hidden border border-dark-700 p-0.5">
                <div
                  className="h-full bg-gradient-to-r from-primary-600 to-primary-400 rounded-full transition-all duration-1000 shadow-[0_0_15px_rgba(59,130,246,0.5)]"
                  style={{ width: `${Math.min(progressPercentage, 100)}%` }}
                />
              </div>
            </div>
          </div>

          <div className="border-t border-dark-700/50 pt-4 mt-6 flex justify-between items-center text-xs">
            <span className="text-gray-500 font-bold">
              Remaining to Level Up:
            </span>
            <span className="text-primary-400 font-black tracking-wider font-mono bg-primary-500/10 px-3 py-1 rounded-lg border border-primary-500/20">
              {(nextLevelXp - currentCount).toLocaleString()} XP
            </span>
          </div>
        </section>

        {/* XP Rules / How to get XP */}
        <section className="bg-dark-800 border border-dark-700 rounded-3xl p-6">
          <h3 className="text-sm font-black text-white uppercase tracking-wider mb-4 flex items-center gap-2">
            <Info className="w-4 h-4 text-primary-500" /> How to earn XP?
          </h3>
          <div className="space-y-4">
            <div className="bg-dark-900 p-3 rounded-xl border border-dark-700">
              <h4 className="text-xs font-bold text-gray-300 flex items-center gap-1.5 mb-1">
                <CheckSquare className="w-3.5 h-3.5 text-orange-500" /> Placed
                Bets
              </h4>
              <p className="text-[10px] text-gray-500 leading-relaxed">
                Earn <strong>1 XP</strong> for every <strong>1 PLN</strong>{" "}
                staked on any sports slip.
              </p>
            </div>
            <div className="bg-dark-900 p-3 rounded-xl border border-dark-700">
              <h4 className="text-xs font-bold text-gray-300 flex items-center gap-1.5 mb-1">
                <CheckSquare className="w-3.5 h-3.5 text-accent-success" />{" "}
                Winning Bets
              </h4>
              <p className="text-[10px] text-gray-500 leading-relaxed">
                Earn bonus XP on winning slips based on multipliers: <br />
                <span className="font-mono text-gray-400">
                  XP = Stake * (Odds - 1) * 0.5
                </span>
              </p>
            </div>
            <div className="bg-dark-900 p-3 rounded-xl border border-dark-700">
              <h4 className="text-xs font-bold text-gray-300 flex items-center gap-1.5 mb-1">
                <CheckSquare className="w-3.5 h-3.5 text-amber-500" /> Daily
                Login
              </h4>
              <p className="text-[10px] text-gray-500 leading-relaxed">
                Claim a <strong>+20 XP boost</strong> by logging in and checking
                your dashboard daily.
              </p>
            </div>
          </div>
        </section>
      </div>

      {/* Grouped Achievements Section */}
      <div>
        <h2 className="text-xl font-black text-white mb-6 flex items-center gap-2">
          <Trophy className="w-5 h-5 text-primary-500" /> Trophies & Milestones
        </h2>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {categories.map((cat) => {
            const catAchievements = achievements
              .filter((a) => a.type === cat.type)
              .sort((a, b) => a.threshold - b.threshold);

            const unlockedCount = catAchievements.filter(
              (a) => a.isUnlocked,
            ).length;
            const nextAchievement = catAchievements.find((a) => !a.isUnlocked);
            const currentVal = catAchievements[0]?.currentProgress ?? 0;

            let threshold =
              nextAchievement?.threshold ??
              (catAchievements[catAchievements.length - 1]?.threshold || 100);
            let percent = nextAchievement
              ? (Number(currentVal) / Number(threshold)) * 100
              : 100;
            percent = Math.min(100, Math.max(0, percent));

            const IconComp = cat.icon;

            return (
              <div
                key={cat.type}
                className="bg-dark-800 border border-dark-700 rounded-3xl p-6 flex flex-col justify-between gap-6 hover:border-dark-600 transition-colors"
              >
                <div>
                  {/* Stages dots on top */}
                  <div className="flex items-center justify-between border-b border-dark-700/50 pb-4 mb-4">
                    <div className="flex gap-2">
                      {[0, 1, 2, 3].map((idx) => {
                        const ach = catAchievements[idx];
                        const isUnlocked = ach?.isUnlocked ?? false;
                        return (
                          <div
                            key={idx}
                            className={`w-7 h-7 rounded-full border-2 flex items-center justify-center text-[10px] font-black transition-all ${getTierColor(idx, isUnlocked)}`}
                            title={ach ? `${ach.name}: ${ach.description}` : ""}
                          >
                            {isUnlocked ? (
                              <Check className="w-3.5 h-3.5 stroke-[3px]" />
                            ) : (
                              getTierName(idx)[0]
                            )}
                          </div>
                        );
                      })}
                    </div>
                    <span className="text-[10px] text-gray-500 font-bold uppercase tracking-wider">
                      {unlockedCount} / 4 Unlocked
                    </span>
                  </div>

                  {/* Header info */}
                  <div className="flex items-start gap-4">
                    <div className={`p-3 rounded-2xl ${cat.bgLight} shrink-0`}>
                      <IconComp className={`w-6 h-6 ${cat.textColor}`} />
                    </div>
                    <div>
                      <h3 className="font-black text-white text-lg leading-tight mb-1">
                        {cat.title}
                      </h3>
                      <p className="text-xs text-gray-400 leading-normal">
                        {cat.description}
                      </p>
                    </div>
                  </div>
                </div>

                {/* Progress bar towards next tier */}
                <div className="space-y-2">
                  <div className="flex justify-between items-baseline text-xs">
                    {nextAchievement ? (
                      <>
                        <span className="text-gray-500 font-bold">
                          Next Rank:{" "}
                          <span className={cat.textColor}>
                            {nextAchievement.name
                              .split("(")[1]
                              ?.replace(")", "") || "Next"}
                          </span>
                        </span>
                        <span className="font-mono text-white font-bold">
                          {Number(currentVal).toLocaleString()} /{" "}
                          {Number(threshold).toLocaleString()} {cat.unit}
                        </span>
                      </>
                    ) : (
                      <>
                        <span className="text-accent-success font-black uppercase tracking-wider flex items-center gap-1">
                          <Check className="w-3.5 h-3.5 stroke-[3.5px]" />{" "}
                          Diamond Rank Maxed!
                        </span>
                        <span className="font-mono text-gray-500 font-bold">
                          {Number(currentVal).toLocaleString()} {cat.unit}
                        </span>
                      </>
                    )}
                  </div>
                  <div className="w-full bg-dark-900 h-2.5 rounded-full overflow-hidden border border-dark-700 p-0.5">
                    <div
                      className={`h-full rounded-full bg-gradient-to-r ${cat.color} transition-all duration-1000`}
                      style={{ width: `${percent}%` }}
                    />
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
};

export default TrophyRoomPage;
