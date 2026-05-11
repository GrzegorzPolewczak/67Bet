import { createSlice, type PayloadAction } from '@reduxjs/toolkit';

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
}

const initialState: BetslipState = {
  selections: [],
  stake: 0,
  isOpen: false,
};

const betslipSlice = createSlice({
  name: 'betslip',
  initialState,
  reducers: {
    toggleBetslip: (state) => {
      state.isOpen = !state.isOpen;
    },
    addSelection: (state, action: PayloadAction<BetSelection>) => {
      const exists = state.selections.find(
        (s) => s.eventId === action.payload.eventId
      );
      if (exists) {
        // Replace selection for the same event (typical for sports betting)
        state.selections = state.selections.map((s) =>
          s.eventId === action.payload.eventId ? action.payload : s
        );
      } else {
        state.selections.push(action.payload);
      }
      state.isOpen = true;
    },
    removeSelection: (state, action: PayloadAction<string>) => {
      state.selections = state.selections.filter(
        (s) => s.outcomeId !== action.payload
      );
    },
    clearBetslip: (state) => {
      state.selections = [];
      state.stake = 0;
    },
    setStake: (state, action: PayloadAction<number>) => {
      state.stake = action.payload;
    },
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
