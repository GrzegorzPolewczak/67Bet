import { Routes, Route } from 'react-router-dom';
import MainLayout from './components/layout/MainLayout';
import Home from './features/betting/Home';
import SportPage from './features/betting/SportPage';
import MatchDetailsView from './features/betting/MatchDetailsView';
import CustomBetRequest from './features/betting/CustomBetRequest';
import VirtualRacingPage from './features/betting/VirtualRacingPage';
import SettingsPage from './features/user/SettingsPage';
import BetHistoryPage from './features/user/BetHistoryPage';
import AdminDashboard from './features/admin/AdminDashboard';

import LoginPage from './features/auth/LoginPage';
import RegisterPage from './features/auth/RegisterPage';
import DepositPage from './features/wallet/DepositPage';
import DepositSuccessPage from './features/wallet/DepositSuccessPage';
import WithdrawPage from './features/wallet/WithdrawPage';

function App() {
  return (
    <Routes>
      <Route path="/" element={<MainLayout />}>
        <Route index element={<Home />} />
        <Route path="sport/:sportName" element={<SportPage />} />
        <Route path="match/:matchId" element={<MatchDetailsView />} />
        <Route path="custom-bet" element={<CustomBetRequest />} />
        <Route path="virtual-racing" element={<VirtualRacingPage />} />
        <Route path="settings" element={<SettingsPage />} />
        <Route path="history" element={<BetHistoryPage />} />
        <Route path="deposit" element={<DepositPage />} />
        <Route path="deposit-success" element={<DepositSuccessPage />} />
        <Route path="withdraw" element={<WithdrawPage />} />
        <Route path="admin/dashboard" element={<AdminDashboard />} />
      </Route>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
    </Routes>
  );
}

export default App;
