import React, { useEffect, useMemo, useState } from "react";
import { useSelector } from "react-redux";
import {
  AlertTriangle,
  Ban,
  Clock,
  Gauge,
  RefreshCw,
  ShieldAlert,
} from "lucide-react";
import type { RootState } from "../../app/store";
import { bettingApi } from "../../api/axios";

type LimitType = 1 | 2 | 3;

interface LimitDto {
  id: string;
  type: LimitType;
  amount: number;
  pendingAmount?: number | null;
  pendingActivationUtc?: string | null;
}

interface UsageDto {
  dailyStakeUsed: number;
  dailyDepositUsed: number;
  weeklyNetLoss: number;
  dailyStakeRemaining?: number | null;
  dailyDepositRemaining?: number | null;
  weeklyLossRemaining?: number | null;
}

interface SelfExclusionDto {
  id: string;
  startsAtUtc: string;
  endsAtUtc: string;
  reason: string;
  isActive: boolean;
}

interface DashboardDto {
  limits: LimitDto[];
  usage: UsageDto;
  activeSelfExclusion?: SelfExclusionDto | null;
  selfExclusionHistory: SelfExclusionDto[];
}

const limitLabels: Record<LimitType, string> = {
  1: "Single stake",
  2: "Daily stake",
  3: "Weekly loss",
};

const formatMoney = (value?: number | null) =>
  value == null ? "Not set" : `${Number(value).toFixed(2)} PLN`;

const formatDate = (value?: string | null) =>
  value ? new Date(value).toLocaleString() : "-";

const ResponsibleGamblingPage: React.FC = () => {
  const { isAuthenticated } = useSelector((state: RootState) => state.auth);
  const [dashboard, setDashboard] = useState<DashboardDto | null>(null);
  const [limitType, setLimitType] = useState<LimitType>(2);
  const [limitAmount, setLimitAmount] = useState(100);
  const [selfExclusionHours, setSelfExclusionHours] = useState(24);
  const [selfExclusionReason, setSelfExclusionReason] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const limitsByType = useMemo(() => {
    const result = new Map<LimitType, LimitDto>();
    dashboard?.limits.forEach((limit) => result.set(limit.type, limit));
    return result;
  }, [dashboard]);

  useEffect(() => {
    if (isAuthenticated) {
      fetchDashboard();
    }
  }, [isAuthenticated]);

  const fetchDashboard = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await bettingApi.get("/responsible-gambling/me");
      setDashboard(response.data);
    } catch (err: any) {
      setError(err.response?.data?.message || err.response?.data || err.message);
    } finally {
      setIsLoading(false);
    }
  };

  const setLimit = async () => {
    setError(null);
    try {
      await bettingApi.post("/responsible-gambling/me/limits", {
        type: limitType,
        amount: limitAmount,
      });
      await fetchDashboard();
    } catch (err: any) {
      setError(err.response?.data?.error || err.response?.data || err.message);
    }
  };

  const startSelfExclusion = async () => {
    setError(null);
    try {
      await bettingApi.post("/responsible-gambling/me/self-exclusion", {
        durationHours: selfExclusionHours,
        reason: selfExclusionReason,
      });
      await fetchDashboard();
    } catch (err: any) {
      setError(err.response?.data?.error || err.response?.data || err.message);
    }
  };

  if (!isAuthenticated) {
    return (
      <div className="max-w-5xl mx-auto px-4 py-12">
        <div className="bg-dark-800 border border-dark-700 rounded-2xl p-8 text-center">
          <ShieldAlert className="w-12 h-12 text-cyan-400 mx-auto mb-4" />
          <h1 className="text-3xl font-black text-white mb-3">Responsible Gambling Center</h1>
          <p className="text-gray-400">Log in to manage personal limits and cooling-off periods.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-[1400px] w-full px-4 xl:px-8 mx-auto space-y-8">
      <div className="bg-dark-800 border border-dark-700 rounded-2xl p-6 flex flex-col md:flex-row md:items-center justify-between gap-5">
        <div className="flex items-center gap-4">
          <div className="w-12 h-12 rounded-2xl bg-cyan-500/15 flex items-center justify-center">
            <ShieldAlert className="w-7 h-7 text-cyan-400" />
          </div>
          <div>
            <h1 className="text-3xl font-black text-white">Responsible Gambling Center</h1>
            <p className="text-gray-400 text-sm mt-1">Limits, cooling-off and activity usage.</p>
          </div>
        </div>
        <button
          onClick={fetchDashboard}
          disabled={isLoading}
          className="h-12 px-5 rounded-xl bg-dark-700 hover:bg-dark-600 disabled:opacity-50 text-white font-bold flex items-center justify-center gap-2"
        >
          <RefreshCw className={`w-5 h-5 ${isLoading ? "animate-spin" : ""}`} />
          Refresh
        </button>
      </div>

      {error && (
        <div className="bg-red-500/10 border border-red-500/50 rounded-2xl p-4 text-red-300 flex items-center gap-3">
          <AlertTriangle className="w-5 h-5" />
          <span className="font-semibold">{error}</span>
        </div>
      )}

      <div className="grid grid-cols-1 xl:grid-cols-3 gap-6">
        <div className="bg-dark-800 border border-dark-700 rounded-2xl p-6">
          <div className="flex items-center gap-3 mb-5">
            <Gauge className="w-6 h-6 text-cyan-400" />
            <h2 className="text-xl font-black text-white">Usage</h2>
          </div>
          <div className="space-y-4">
            <UsageRow label="Daily stake" used={dashboard?.usage.dailyStakeUsed} remaining={dashboard?.usage.dailyStakeRemaining} />
            <UsageRow label="Weekly net loss" used={dashboard?.usage.weeklyNetLoss} remaining={dashboard?.usage.weeklyLossRemaining} />
          </div>
        </div>

        <div className="bg-dark-800 border border-dark-700 rounded-2xl p-6 xl:col-span-2">
          <div className="flex items-center gap-3 mb-5">
            <ShieldAlert className="w-6 h-6 text-cyan-400" />
            <h2 className="text-xl font-black text-white">Active Limits</h2>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {([1, 2, 3] as LimitType[]).map((type) => {
              const limit = limitsByType.get(type);
              return (
                <div key={type} className="bg-dark-900 border border-dark-700 rounded-2xl p-5">
                  <div className="flex justify-between gap-4 mb-2">
                    <h3 className="text-white font-black">{limitLabels[type]}</h3>
                    <span className="text-cyan-400 font-black">{formatMoney(limit?.amount)}</span>
                  </div>
                  {limit?.pendingAmount && (
                    <div className="mt-4 text-xs text-yellow-300 bg-yellow-500/10 border border-yellow-500/30 rounded-xl p-3">
                      Pending {formatMoney(limit.pendingAmount)} from {formatDate(limit.pendingActivationUtc)}
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-3 gap-6">
        <div className="bg-dark-800 border border-dark-700 rounded-2xl p-6">
          <h2 className="text-xl font-black text-white mb-5">Set Limit</h2>
          <label className="block text-xs font-black text-gray-500 uppercase tracking-widest mb-2">Limit type</label>
          <select
            value={limitType}
            onChange={(event) => setLimitType(Number(event.target.value) as LimitType)}
            className="w-full bg-dark-900 border border-dark-600 rounded-xl px-4 py-3 text-white focus:outline-none focus:border-cyan-500"
          >
            {([1, 2, 3] as LimitType[]).map((type) => (
              <option key={type} value={type}>{limitLabels[type]}</option>
            ))}
          </select>
          <label className="block text-xs font-black text-gray-500 uppercase tracking-widest mt-5 mb-2">Amount</label>
          <input
            type="number"
            min={1}
            step={0.01}
            value={limitAmount}
            onChange={(event) => setLimitAmount(Number(event.target.value))}
            className="w-full bg-dark-900 border border-dark-600 rounded-xl px-4 py-3 text-white focus:outline-none focus:border-cyan-500"
          />
          <button onClick={setLimit} className="w-full mt-5 h-12 rounded-xl bg-cyan-600 hover:bg-cyan-500 text-dark-900 font-black">
            Save Limit
          </button>
        </div>

        <div className="bg-dark-800 border border-dark-700 rounded-2xl p-6">
          <div className="flex items-center gap-3 mb-5">
            <Ban className="w-6 h-6 text-red-400" />
            <h2 className="text-xl font-black text-white">Cooling-Off</h2>
          </div>
          <label className="block text-xs font-black text-gray-500 uppercase tracking-widest mb-2">Duration</label>
          <select
            value={selfExclusionHours}
            onChange={(event) => setSelfExclusionHours(Number(event.target.value))}
            className="w-full bg-dark-900 border border-dark-600 rounded-xl px-4 py-3 text-white focus:outline-none focus:border-cyan-500"
          >
            <option value={24}>24 hours</option>
            <option value={168}>7 days</option>
            <option value={720}>30 days</option>
          </select>
          <label className="block text-xs font-black text-gray-500 uppercase tracking-widest mt-5 mb-2">Reason</label>
          <input
            value={selfExclusionReason}
            onChange={(event) => setSelfExclusionReason(event.target.value)}
            className="w-full bg-dark-900 border border-dark-600 rounded-xl px-4 py-3 text-white focus:outline-none focus:border-cyan-500"
          />
          <button
            onClick={startSelfExclusion}
            disabled={Boolean(dashboard?.activeSelfExclusion)}
            className="w-full mt-5 h-12 rounded-xl bg-red-600 hover:bg-red-500 disabled:opacity-50 disabled:cursor-not-allowed text-white font-black"
          >
            Start Cooling-Off
          </button>
        </div>

        <div className="bg-dark-800 border border-dark-700 rounded-2xl p-6">
          <div className="flex items-center gap-3 mb-5">
            <Clock className="w-6 h-6 text-cyan-400" />
            <h2 className="text-xl font-black text-white">Self-Exclusion History</h2>
          </div>
          {dashboard?.selfExclusionHistory.length ? (
            <div className="space-y-3">
              {dashboard.selfExclusionHistory.map((item) => (
                <div key={item.id} className="bg-dark-900 border border-dark-700 rounded-xl p-4">
                  <div className="flex justify-between gap-4">
                    <span className="font-black text-white">{item.isActive ? "Active" : "Expired"}</span>
                    <span className="text-sm text-gray-400">Until {formatDate(item.endsAtUtc)}</span>
                  </div>
                  <p className="text-sm text-gray-500 mt-2">{item.reason}</p>
                </div>
              ))}
            </div>
          ) : (
            <div className="bg-dark-900 border border-dark-700 rounded-xl p-6 text-center text-gray-500 font-bold">
              No self-exclusion periods.
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

const UsageRow: React.FC<{ label: string; used?: number | null; remaining?: number | null }> = ({
  label,
  used,
  remaining,
}) => (
  <div className="bg-dark-900 border border-dark-700 rounded-xl p-4">
    <div className="flex justify-between gap-4">
      <span className="text-gray-400 font-bold">{label}</span>
      <span className="text-white font-black">{formatMoney(used ?? 0)}</span>
    </div>
    <div className="flex justify-between gap-4 mt-2 text-sm">
      <span className="text-gray-500">Remaining</span>
      <span className="text-cyan-400 font-bold">{formatMoney(remaining)}</span>
    </div>
  </div>
);

export default ResponsibleGamblingPage;
