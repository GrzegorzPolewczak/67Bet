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

const MOCK_EVENTS = [
  {
    id: "evt-1",
    name: "Real Madrid vs Bayern Munich",
    league: "Champions League",
    time: "20:45",
    markets: [
      {
        id: "m-1",
        name: "Match Winner",
        outcomes: [
          { id: "o-1", name: "1", odd: 2.10 },
          { id: "o-2", name: "X", odd: 3.50 },
          { id: "o-3", name: "2", odd: 3.20 }
        ]
      }
    ]
  },
  {
    id: "evt-2",
    name: "FC Barcelona vs Juventus",
    league: "Champions League",
    time: "21:00",
    markets: [
      {
        id: "m-2",
        name: "Match Winner",
        outcomes: [
          { id: "o-4", name: "1", odd: 1.85 },
          { id: "o-5", name: "X", odd: 3.80 },
          { id: "o-6", name: "2", odd: 4.10 }
        ]
      }
    ]
  }
];

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
        state.events = MOCK_EVENTS;
      });
  },
});

export default bettingSlice.reducer;
