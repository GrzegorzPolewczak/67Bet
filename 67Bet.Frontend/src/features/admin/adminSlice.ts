import { createSlice, createAsyncThunk, type PayloadAction } from '@reduxjs/toolkit';
import { customBetApi } from '../../api/axios';
import { referralApi } from '../../api/referral';

export interface CustomBetRequest {
  id: string;
  userId: string;
  description: string;
  status: 'Pending' | 'Accepted' | 'Rejected';
  createdAt: string;
  finalOdds?: number;
  adminNote?: string;
}

export interface PromoCode {
  code: string;
  rewardAmount: number;
  isActive: boolean;
}

interface AdminState {
  pendingRequests: CustomBetRequest[];
  promoCodes: PromoCode[];
  loading: boolean;
  error: string | null;
  stats: {
    totalUsers: number;
    activeBets: number;
    revenue: number;
  };
}

const initialState: AdminState = {
  pendingRequests: [],
  promoCodes: [],
  loading: false,
  error: null,
  stats: {
    totalUsers: 1245,
    activeBets: 843,
    revenue: 15420.50,
  }
};

export const fetchPromoCodesAsync = createAsyncThunk(
  'admin/fetchPromoCodes',
  async (_, { rejectWithValue }) => {
    try {
      const response = await referralApi.getAllPromos();
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data || error.message);
    }
  }
);

export const createPromoCodeAsync = createAsyncThunk(
  'admin/createPromoCode',
  async ({ code, reward }: { code: string; reward: number }, { rejectWithValue }) => {
    try {
      await referralApi.createPromo(code, reward);
      return { code, rewardAmount: reward, isActive: true };
    } catch (error: any) {
      return rejectWithValue(error.response?.data || error.message);
    }
  }
);

export const togglePromoCodeStatusAsync = createAsyncThunk(
  'admin/togglePromoCodeStatus',
  async ({ code, isActive }: { code: string; isActive: boolean }, { rejectWithValue }) => {
    try {
      if (isActive) {
        await referralApi.deactivatePromo(code);
      } else {
        await referralApi.activatePromo(code);
      }
      return { code, isActive: !isActive };
    } catch (error: any) {
      return rejectWithValue(error.response?.data || error.message);
    }
  }
);

export const fetchPendingRequestsAsync = createAsyncThunk(
  'admin/fetchPendingRequests',
  async (_, { rejectWithValue }) => {
    try {
      const response = await customBetApi.get('/CustomBet/pending');
      return response.data;
    } catch (error: any) {
      const message = error.response?.data?.message || error.response?.data || error.message || 'Failed to fetch pending requests';
      return rejectWithValue(typeof message === 'object' ? JSON.stringify(message) : message);
    }
  }
);

export const acceptRequestAsync = createAsyncThunk(
  'admin/acceptRequest',
  async ({ id, odds, note }: { id: string; odds: number; note: string }, { rejectWithValue }) => {
    try {
      await customBetApi.post(`/CustomBet/requests/${id}/accept`, { finalOdds: odds, adminNote: note });
      return id;
    } catch (error: any) {
      const message = error.response?.data?.message || error.response?.data || error.message || 'Failed to accept request';
      return rejectWithValue(typeof message === 'object' ? JSON.stringify(message) : message);
    }
  }
);

export const rejectRequestAsync = createAsyncThunk(
  'admin/rejectRequest',
  async ({ id, reason }: { id: string; reason: string }, { rejectWithValue }) => {
    try {
      await customBetApi.post(`/CustomBet/requests/${id}/reject`, { reason });
      return id;
    } catch (error: any) {
      const message = error.response?.data?.message || error.response?.data || error.message || 'Failed to reject request';
      return rejectWithValue(typeof message === 'object' ? JSON.stringify(message) : message);
    }
  }
);

export const submitCustomBetAsync = createAsyncThunk(
  'admin/submitCustomBet',
  async (description: string, { rejectWithValue }) => {
    try {
      const response = await customBetApi.post('/CustomBet/requests', { description });
      return response.data;
    } catch (error: any) {
      const message = error.response?.data?.message || error.response?.data || error.message || 'Failed to submit custom bet';
      return rejectWithValue(typeof message === 'object' ? JSON.stringify(message) : message);
    }
  }
);

const adminSlice = createSlice({
  name: 'admin',
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchPendingRequestsAsync.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchPendingRequestsAsync.fulfilled, (state, action) => {
        state.loading = false;
        state.pendingRequests = action.payload;
      })
      .addCase(fetchPendingRequestsAsync.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      .addCase(acceptRequestAsync.fulfilled, (state, action) => {
        state.pendingRequests = state.pendingRequests.filter(r => r.id !== action.payload);
      })
      .addCase(rejectRequestAsync.fulfilled, (state, action) => {
        state.pendingRequests = state.pendingRequests.filter(r => r.id !== action.payload);
      })
      .addCase(submitCustomBetAsync.fulfilled, (state, action) => {
        // Add the newly created request to the pending list so it shows up in the dashboard
        state.pendingRequests.unshift(action.payload);
      })
      .addCase(fetchPromoCodesAsync.fulfilled, (state, action) => {
        state.promoCodes = action.payload;
      })
      .addCase(createPromoCodeAsync.fulfilled, (state, action) => {
        state.promoCodes.unshift(action.payload);
      })
      .addCase(togglePromoCodeStatusAsync.fulfilled, (state, action) => {
        const promo = state.promoCodes.find(p => p.code === action.payload.code);
        if (promo) {
          promo.isActive = action.payload.isActive;
        }
      });
  },
});

export default adminSlice.reducer;