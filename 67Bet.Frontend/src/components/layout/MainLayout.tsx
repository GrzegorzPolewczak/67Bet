import React from "react";
import { Outlet, Link } from "react-router-dom";
import Navbar from "./Navbar";
import Sidebar from "./Sidebar";
import BetSlip from "../../features/betslip/BetSlip";
import { useSelector } from "react-redux";
import type { RootState } from "../../app/store";
import { processDailyLogin } from "../../api/gamification";

const MainLayout: React.FC = () => {
  const isBetslipOpen = useSelector((state: RootState) => state.betslip.isOpen);
  const { user, isAuthenticated } = useSelector(
    (state: RootState) => state.auth,
  );

  React.useEffect(() => {
    if (isAuthenticated) {
      processDailyLogin().catch(err => console.error("Daily login failed", err));
    }
  }, [isAuthenticated]);

  return (
    <div className="flex flex-col h-screen overflow-hidden">
      {isAuthenticated && user && !user.isKycVerified && (
        <div className="bg-yellow-500 text-yellow-900 px-4 py-2 text-center font-semibold">
          Your account is not fully verified yet. You must complete KYC
          verification to unlock all features.{" "}
          <Link
            to="/kyc-verify"
            className="underline ml-2 hover:text-yellow-800"
          >
            Verify Now
          </Link>
        </div>
      )}
      <Navbar />
      <div className="flex flex-1 overflow-hidden">
        <Sidebar />
        <main className="flex-1 overflow-y-auto bg-dark-900 p-4 relative">
          <Outlet />
        </main>
        <aside
          className={`transition-all duration-300 ease-in-out border-l border-dark-700 bg-dark-800 ${
            isBetslipOpen ? "w-80" : "w-0 overflow-hidden border-none"
          }`}
        >
          <BetSlip />
        </aside>
      </div>
    </div>
  );
};

export default MainLayout;
