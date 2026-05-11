import React from 'react';
import { Outlet } from 'react-router-dom';
import Navbar from './Navbar';
import Sidebar from './Sidebar';
import BetSlip from '../../features/betslip/BetSlip';
import { useSelector } from 'react-redux';
import type { RootState } from '../../app/store';

const MainLayout: React.FC = () => {
  const isBetslipOpen = useSelector((state: RootState) => state.betslip.isOpen);

  return (
    <div className="flex flex-col h-screen overflow-hidden">
      <Navbar />
      <div className="flex flex-1 overflow-hidden">
        <Sidebar />
        <main className="flex-1 overflow-y-auto bg-dark-900 p-4 relative">
          <Outlet />
        </main>
        <aside 
          className={`transition-all duration-300 ease-in-out border-l border-dark-700 bg-dark-800 ${
            isBetslipOpen ? 'w-80' : 'w-0 overflow-hidden border-none'
          }`}
        >
          <BetSlip />
        </aside>
      </div>
    </div>
  );
};

export default MainLayout;
