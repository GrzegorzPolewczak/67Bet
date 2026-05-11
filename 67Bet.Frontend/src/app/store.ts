import { configureStore } from '@reduxjs/toolkit';
import authReducer from '../features/auth/authSlice';
import betslipReducer from '../features/betslip/betslipSlice';
import bettingReducer from '../features/betting/bettingSlice';
import walletReducer from '../features/wallet/walletSlice';
import historyReducer from '../features/user/historySlice';
import adminReducer from '../features/admin/adminSlice';

export const store = configureStore({
  reducer: {
    auth: authReducer,
    betslip: betslipReducer,
    betting: bettingReducer,
    wallet: walletReducer,
    history: historyReducer,
    admin: adminReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
