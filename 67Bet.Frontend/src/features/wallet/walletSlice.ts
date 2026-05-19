import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { walletApi } from '../../api/axios';

interface WalletState {
  balance: number;
  loading: boolean;
  error: string | null;
}

const initialState: WalletState = {
  balance: 0,
  loading: false,
  error: null,
};

export const fetchBalanceAsync = createAsyncThunk(
  'wallet/fetchBalance',
  async (_, { rejectWithValue }) => {
    try {
      // In a real app, the user ID would be taken from the token on the server
      const response = await walletApi.get('/wallet/balance');
      return typeof response.data === 'object' && response.data !== null && 'balance' in response.data ? response.data.balance : response.data;
    } catch (error: any) {
      const message = error.response?.data?.message || error.response?.data || error.message || 'Failed to fetch balance';
      return rejectWithValue(typeof message === 'object' ? JSON.stringify(message) : message);
    }
  }
);

export const createPaymentIntentAsync = createAsyncThunk(
  'wallet/createPaymentIntent',
  async (amount: number, { rejectWithValue }) => {
    try {
      const response = await walletApi.post('/wallet/create-payment-intent', { amount });
      return response.data;
    } catch (error: any) {
      const message = error.response?.data?.message || error.response?.data || error.message || 'Failed to create payment intent';
      return rejectWithValue(typeof message === 'object' ? JSON.stringify(message) : message);
    }
  }
);

export const withdrawAsync = createAsyncThunk(
  'wallet/withdraw',
  async (amount: number, { rejectWithValue }) => {
    try {
      await walletApi.post('/wallet/withdraw', { amount });
      return amount;
    } catch (error: any) {
      const message = error.response?.data?.message || error.response?.data || error.message || 'Failed to withdraw funds';
      return rejectWithValue(typeof message === 'object' ? JSON.stringify(message) : message);
    }
  }
);

const walletSlice = createSlice({
  name: 'wallet',
  initialState,
  reducers: {
    updateBalance: (state, action) => {
      state.balance = action.payload;
    }
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchBalanceAsync.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchBalanceAsync.fulfilled, (state, action) => {
        state.loading = false;
        state.balance = action.payload;
      })
      .addCase(fetchBalanceAsync.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      .addCase(withdrawAsync.fulfilled, (state, action) => {
        state.balance -= action.payload;
      });
  },
});

export const { updateBalance } = walletSlice.actions;
export default walletSlice.reducer;
