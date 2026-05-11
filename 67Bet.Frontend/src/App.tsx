import { Routes, Route } from 'react-router-dom';
import MainLayout from './components/layout/MainLayout';
import Home from './features/betting/Home';
import SportPage from './features/betting/SportPage';
import CustomBetRequest from './features/betting/CustomBetRequest';
import SettingsPage from './features/user/SettingsPage';
import BetHistoryPage from './features/user/BetHistoryPage';
import AdminDashboard from './features/admin/AdminDashboard';

import LoginPage from './features/auth/LoginPage';
import RegisterPage from './features/auth/RegisterPage';

function App() {
  return (
    <Routes>
      <Route path="/" element={<MainLayout />}>
        <Route index element={<Home />} />
        <Route path="sport/:sportName" element={<SportPage />} />
        <Route path="custom-bet" element={<CustomBetRequest />} />
        <Route path="settings" element={<SettingsPage />} />
        <Route path="history" element={<BetHistoryPage />} />
        <Route path="admin/dashboard" element={<AdminDashboard />} />
      </Route>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
    </Routes>
  );
}

export default App;
