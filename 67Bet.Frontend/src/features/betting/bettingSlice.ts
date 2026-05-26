import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import { oddsApi } from "../../api/axios";

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
  sportKey: string;
  time: string;
  rawTime: string;
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
          { id: "a0000000-0000-0000-0000-000000000001", name: "1", odd: 2.1 },
          { id: "a0000000-0000-0000-0000-000000000002", name: "X", odd: 3.5 },
          { id: "a0000000-0000-0000-0000-000000000003", name: "2", odd: 3.2 },
        ],
      },
    ],
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
          { id: "b0000000-0000-0000-0000-000000000002", name: "X", odd: 3.8 },
          { id: "b0000000-0000-0000-0000-000000000003", name: "2", odd: 4.1 },
        ],
      },
    ],
  },
];

export const fetchEventsAsync = createAsyncThunk(
  "betting/fetchEvents",
  async (_, { rejectWithValue }) => {
    try {
      const response = await oddsApi.get("/externalodds/events");

      // Map External API DTO to Frontend Model
      const mappedEvents = response.data.map((event: any) => ({
        id: event.id,
        name: `${event.home_team} vs ${event.away_team}`,
        league: event.sport_title || event.sport_key,
        sportKey: event.sport_key || "",
        rawTime: event.commence_time,
        time: new Date(event.commence_time).toLocaleString("pl-PL", {
          day: "2-digit",
          month: "2-digit",
          hour: "2-digit",
          minute: "2-digit",
        }),
        markets:
          event.bookmakers?.[0]?.markets?.map((market: any) => ({
            id: market.key,
            name: market.key === "h2h" ? "Match Winner" : market.key,
            outcomes: market.outcomes?.map((outcome: any) => ({
              id: `${event.id}_${market.key}_${outcome.name}`,
              name: outcome.name,
              odd: outcome.price,
            })),
          })) || [],
      }));

      return mappedEvents;
    } catch (error: any) {
      const message =
        error.response?.data?.message ||
        error.response?.data ||
        error.message ||
        "Failed to fetch events";
      return rejectWithValue(
        typeof message === "object" ? JSON.stringify(message) : message,
      );
    }
  },
);

const bettingSlice = createSlice({
  name: "betting",
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
