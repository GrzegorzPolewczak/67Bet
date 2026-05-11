import { NavLink } from 'react-router-dom';
import { useSelector } from 'react-redux';
import type { RootState } from '../../app/store';
import { 
  Trophy, 
  Gamepad2, 
  Activity, 
  Zap, 
  Flame, 
  History, 
  Settings,
  Star,
  ShieldCheck
} from 'lucide-react';

const sports = [
  { icon: Flame, name: 'Popular', color: 'text-orange-500' },
  { icon: Zap, name: 'Live', color: 'text-accent-success' },
  { icon: Trophy, name: 'Football' },
  { icon: Activity, name: 'Basketball' },
  { icon: Gamepad2, name: 'Esports' },
  { icon: Star, name: 'MMA' },
];

const Sidebar: React.FC = () => {
  const { user } = useSelector((state: RootState) => state.auth);

  return (
    <aside className="w-64 bg-dark-800 border-r border-dark-700 hidden lg:flex flex-col">
      <div className="p-4 flex flex-col gap-6">
        <div>
          <h3 className="text-[10px] font-bold text-gray-500 uppercase tracking-widest px-4 mb-2">Sports</h3>
          <ul className="space-y-1">
            {sports.map((sport) => (
              <li key={sport.name}>
                <NavLink 
                  to={sport.name === 'Popular' ? '/' : `/sport/${sport.name}`}
                  className={({ isActive }) => `
                    w-full flex items-center gap-3 px-4 py-2.5 rounded-xl transition-all text-sm font-medium group
                    ${isActive 
                      ? 'bg-primary-600/10 text-primary-500' 
                      : 'text-gray-300 hover:bg-dark-700 hover:text-white'}
                  `}
                >
                  <sport.icon className={`w-5 h-5 ${sport.color || 'text-gray-400 group-hover:text-primary-500'}`} />
                  {sport.name}
                </NavLink>
              </li>
            ))}
          </ul>
        </div>

        <div className="pt-6 border-t border-dark-700">
          <h3 className="text-[10px] font-bold text-gray-500 uppercase tracking-widest px-4 mb-2">My Account</h3>
          <ul className="space-y-1">
            <li>
              <NavLink 
                to="/history"
                className={({ isActive }) => `
                  w-full flex items-center gap-3 px-4 py-2.5 rounded-xl transition-all text-sm font-medium group
                  ${isActive 
                    ? 'bg-primary-600/10 text-primary-500' 
                    : 'text-gray-300 hover:bg-dark-700 hover:text-white'}
                `}
              >
                <History className="w-5 h-5 text-gray-400 group-hover:text-primary-500" />
                Bet History
              </NavLink>
            </li>
            <li>
              <NavLink 
                to="/settings"
                className={({ isActive }) => `
                  w-full flex items-center gap-3 px-4 py-2.5 rounded-xl transition-all text-sm font-medium group
                  ${isActive 
                    ? 'bg-primary-600/10 text-primary-500' 
                    : 'text-gray-300 hover:bg-dark-700 hover:text-white'}
                `}
              >
                <Settings className="w-5 h-5 text-gray-400 group-hover:text-primary-500" />
                Settings
              </NavLink>
            </li>
            
            {user?.role === 'Admin' && (
              <li>
                <NavLink 
                  to="/admin/dashboard"
                  className={({ isActive }) => `
                    w-full flex items-center gap-3 px-4 py-2.5 rounded-xl transition-all text-sm font-medium group mt-4 border border-primary-500/30
                    ${isActive 
                      ? 'bg-primary-600/20 text-primary-500' 
                      : 'text-primary-400 hover:bg-primary-600/10 hover:text-primary-500'}
                  `}
                >
                  <ShieldCheck className="w-5 h-5" />
                  Admin Panel
                </NavLink>
              </li>
            )}
          </ul>
        </div>
      </div>

      <div className="mt-auto p-4">
        <div className="bg-gradient-to-br from-primary-600 to-primary-800 rounded-2xl p-4 relative overflow-hidden group cursor-pointer">
          <div className="relative z-10">
            <h4 className="text-sm font-bold text-white mb-1">VIP Program</h4>
            <p className="text-[10px] text-primary-100">Unlock exclusive bonuses and higher limits.</p>
          </div>
          <Zap className="absolute -right-2 -bottom-2 w-16 h-16 text-primary-400/20 group-hover:scale-110 transition-transform" />
        </div>
      </div>
    </aside>
  );
};

export default Sidebar;
