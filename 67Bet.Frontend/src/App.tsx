import { useEffect } from "react";
import { Routes, Route } from "react-router-dom";
import { useDispatch, useSelector } from "react-redux";
import type { AppDispatch, RootState } from "./app/store";
import { getCurrentUser } from "./features/auth/authSlice";
import MainLayout from "./components/layout/MainLayout";
import Home from "./features/betting/Home";
import SportPage from "./features/betting/SportPage";
import MatchDetailsView from "./features/betting/MatchDetailsView";
import CustomBetRequest from "./features/betting/CustomBetRequest";
import VirtualRacingPage from "./features/betting/VirtualRacingPage";
import PlinkoPage from "./features/betting/PlinkoPage";
import RoulettePage from "./features/betting/RoulettePage";
import SettingsPage from "./features/user/SettingsPage";
import ReferralsPage from "./features/user/ReferralsPage";
import BetHistoryPage from "./features/user/BetHistoryPage";
import ResponsibleGamblingPage from "./features/user/ResponsibleGamblingPage";
import SharedTicketPage from "./features/betting/SharedTicketPage";
import AdminDashboard from "./features/admin/AdminDashboard";
import LoginPage from "./features/auth/LoginPage";
import RegisterPage from "./features/auth/RegisterPage";
import DesktopVerification from "./features/auth/DesktopVerification";
import MobileVerification from "./features/auth/MobileVerification";
import DepositPage from "./features/wallet/DepositPage";
import DepositSuccessPage from "./features/wallet/DepositSuccessPage";
import WithdrawPage from "./features/wallet/WithdrawPage";

function App() {
  const dispatch = useDispatch<AppDispatch>();
  const { token, user } = useSelector((state: RootState) => state.auth);

  useEffect(() => {
    if (token && !user) {
      dispatch(getCurrentUser());
    }
  }, [token, user, dispatch]);

  return (
    <Routes>
      <Route path="/" element={<MainLayout />}>
        <Route index element={<Home />} />
        <Route path="sport/:sportName" element={<SportPage />} />
        <Route path="match/:matchId" element={<MatchDetailsView />} />
        <Route path="custom-bet" element={<CustomBetRequest />} />
        <Route path="virtual-racing" element={<VirtualRacingPage />} />
        <Route path="plinko" element={<PlinkoPage />} />
        <Route path="roulette" element={<RoulettePage />} />
        <Route path="settings" element={<SettingsPage />} />
        <Route path="referrals" element={<ReferralsPage />} />
        <Route
          path="responsible-gambling"
          element={<ResponsibleGamblingPage />}
        />
        <Route path="history" element={<BetHistoryPage />} />
        <Route path="share-ticket/:id" element={<SharedTicketPage />} />
        <Route path="deposit" element={<DepositPage />} />
        <Route path="deposit-success" element={<DepositSuccessPage />} />
        <Route path="withdraw" element={<WithdrawPage />} />
        <Route path="admin/dashboard" element={<AdminDashboard />} />
        <Route path="kyc-verify" element={<DesktopVerification />} />
      </Route>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route path="/mobile/:sessionId" element={<MobileVerification />} />
    </Routes>
  );
}

export default App;
