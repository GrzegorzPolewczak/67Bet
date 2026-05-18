import { createSlice } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';
import type { LiveMatchState } from '../../api/signalr';

interface LiveTrackerState {
  currentMatch: LiveMatchState | null;
  isConnected: boolean;
}

const initialState: LiveTrackerState = {
  currentMatch: null,
  isConnected: false,
};

const liveTrackerSlice = createSlice({
  name: 'liveTracker',
  initialState,
  reducers: {
    updateMatchState: (state, action: PayloadAction<LiveMatchState>) => {
      state.currentMatch = action.payload;
    },
    clearMatchState: (state) => {
      state.currentMatch = null;
    },
    setConnectionStatus: (state, action: PayloadAction<boolean>) => {
      state.isConnected = action.payload;
    }
  },
});

export const { updateMatchState, clearMatchState, setConnectionStatus } = liveTrackerSlice.actions;
export default liveTrackerSlice.reducer;
