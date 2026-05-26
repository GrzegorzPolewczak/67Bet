import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import { bettingApi } from "../../api/axios";

interface BetDto {
  outcomeId: string;
  fixedPrice: number;
  status: string;
}

interface TicketDto {
  id: string;
  stake: number;
  totalOdds: number;
  potentialWinning: number;
  status: string;
  bets: BetDto[];
}

interface HistoryState {
  tickets: TicketDto[];
  loading: boolean;
  error: string | null;
}

const initialState: HistoryState = {
  tickets: [],
  loading: false,
  error: null,
};

export const fetchHistoryAsync = createAsyncThunk(
  "history/fetchHistory",
  async (_, { rejectWithValue }) => {
    try {
      const response = await bettingApi.get("/tickets/my");
      return response.data;
    } catch (error: any) {
      const message =
        error.response?.data?.message ||
        error.response?.data ||
        error.message ||
        "Failed to fetch history";
      return rejectWithValue(
        typeof message === "object" ? JSON.stringify(message) : message,
      );
    }
  },
);

const historySlice = createSlice({
  name: "history",
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchHistoryAsync.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchHistoryAsync.fulfilled, (state, action) => {
        state.loading = false;
        state.tickets = action.payload;
      })
      .addCase(fetchHistoryAsync.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      });
  },
});

export default historySlice.reducer;
