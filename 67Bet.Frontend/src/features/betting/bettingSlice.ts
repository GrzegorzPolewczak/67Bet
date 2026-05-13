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
    id: "00000000-0000-0000-0000-000000000001",
    name: "Real Madrid vs Bayern Munich",
    league: "Champions League",
    time: "20:45",
    markets: [
      {
        id: "11111111-1111-1111-1111-111111111111",
        name: "Match Winner",
        outcomes: [
          { id: "a0000000-0000-0000-0000-000000000001", name: "1", odd: 2.10 },
          { id: "a0000000-0000-0000-0000-000000000002", name: "X", odd: 3.50 },
          { id: "a0000000-0000-0000-0000-000000000003", name: "2", odd: 3.20 }
        ]
      }
    ]
  },
  {
    id: "00000000-0000-0000-0000-000000000002",
    name: "FC Barcelona vs Juventus",
    league: "Champions League",
    time: "21:00",
    markets: [
      {
        id: "22222222-2222-2222-2222-222222222222",
        name: "Match Winner",
        outcomes: [
          { id: "b0000000-0000-0000-0000-000000000001", name: "1", odd: 1.85 },
          { id: "b0000000-0000-0000-0000-000000000002", name: "X", odd: 3.80 },
          { id: "b0000000-0000-0000-0000-000000000003", name: "2", odd: 4.10 }
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
