import { configureStore } from '@reduxjs/toolkit';
import authReducer from '../features/auth/authSlice';
import betslipReducer from '../features/betslip/betslipSlice';

export const store = configureStore({
  reducer: {
    auth: authReducer,
    betslip: betslipReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
