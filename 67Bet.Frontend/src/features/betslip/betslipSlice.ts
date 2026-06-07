import {
  createSlice,
  createAsyncThunk,
  type PayloadAction,
} from "@reduxjs/toolkit";
import { bettingApi } from "../../api/axios";

export interface BetSelection {
  eventId: string;
  eventName: string;
  marketId: string;
  marketName: string;
  outcomeId: string;
  outcomeName: string;
  odd: number;
}

interface BetslipState {
  selections: BetSelection[];
  stake: number;
  isOpen: boolean;
  loading: boolean;
  error: string | null;
}

const initialState: BetslipState = {
  selections: [],
  stake: 0,
  isOpen: false,
  loading: false,
  error: null,
};

export const placeBetAsync = createAsyncThunk(
  "betslip/placeBet",
  async (_, { getState, rejectWithValue }) => {
    const state = getState() as any;
    const { selections, stake } = state.betslip;

    if (selections.length === 0 || stake <= 0) {
      return rejectWithValue("Invalid bet");
    }

    const outcomeIds = selections.map((s: BetSelection) => s.outcomeId);

    try {
      const response = await bettingApi.post("/tickets", {
        stake,
        outcomeIds,
      });
      return response.data;
    } catch (error: any) {
      const message =
        error.response?.data?.message ||
        error.response?.data ||
        error.message ||
        "Failed to place bet";
      return rejectWithValue(
        typeof message === "object" ? JSON.stringify(message) : message,
      );
    }
  },
);

const betslipSlice = createSlice({
  name: "betslip",
  initialState,
  reducers: {
    toggleBetslip: (state) => {
      state.isOpen = !state.isOpen;
    },
    addSelection: (state, action: PayloadAction<BetSelection>) => {
      const exists = state.selections.find(
        (s) => s.outcomeId === action.payload.outcomeId,
      );
      if (exists) {
        // Already in betslip, update the selection for this event
        state.selections = state.selections.map((s) =>
          s.eventId === action.payload.eventId ? action.payload : s,
        );
      } else {
        // If we want to enforce one selection per event and we HAVE eventId:
        if (action.payload.eventId) {
          state.selections = state.selections.filter(
            (s) => s.eventId !== action.payload.eventId,
          );
        }
        state.selections.push(action.payload);
      }
      state.isOpen = true;
    },
    removeSelection: (state, action: PayloadAction<string>) => {
      state.selections = state.selections.filter(
        (s) => s.outcomeId !== action.payload,
      );
    },
    clearBetslip: (state) => {
      state.selections = [];
      state.stake = 0;
      state.error = null;
    },
    setStake: (state, action: PayloadAction<number>) => {
      state.stake = action.payload;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(placeBetAsync.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(placeBetAsync.fulfilled, (state) => {
        state.loading = false;
        state.selections = [];
        state.stake = 0;
        state.isOpen = false;
      })
      .addCase(placeBetAsync.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      });
  },
});

export const {
  toggleBetslip,
  addSelection,
  removeSelection,
  clearBetslip,
  setStake,
} = betslipSlice.actions;
export default betslipSlice.reducer;
