import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { bettingApi } from '../../api/axios';

interface Outcome {
  id: string;
  name: string;
  odd: number;
}

interface Market {
  id: string;
  name: string;
  outcomes: Outcome[];
}

interface Event {
  id: string;
  name: string;
  league: string;
  time: string;
  markets: Market[];
}

interface BettingState {
  events: Event[];
  loading: boolean;
  error: string | null;
}

const initialState: BettingState = {
  events: [],
  loading: false,
  error: null,
};

export const fetchEventsAsync = createAsyncThunk(
  'betting/fetchEvents',
  async (_, { rejectWithValue }) => {
    try {
      const response = await bettingApi.get('/events');
      
      // Map API DTO (currentPrice) to Frontend Model (odd)
      const mappedEvents = response.data.map((event: any) => ({
        ...event,
        markets: event.markets?.map((market: any) => ({
          ...market,
          outcomes: market.outcomes?.map((outcome: any) => ({
            ...outcome,
            odd: outcome.currentPrice ?? outcome.odd ?? 0
          }))
        }))
      }));
      
      return mappedEvents;
    } catch (error: any) {
      const message = error.response?.data?.message || error.response?.data || error.message || 'Failed to fetch events';
      return rejectWithValue(typeof message === 'object' ? JSON.stringify(message) : message);
    }
  }
);

const bettingSlice = createSlice({
  name: 'betting',
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchEventsAsync.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchEventsAsync.fulfilled, (state, action) => {
        state.loading = false;
        state.events = action.payload;
      })
      .addCase(fetchEventsAsync.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      });
  },
});

export default bettingSlice.reducer;
