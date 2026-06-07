import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import { getMyProgress, getMyAchievements } from "../../api/gamification";
import type { UserProgress, Achievement } from "../../api/gamification";

interface GamificationState {
  progress: UserProgress | null;
  achievements: Achievement[];
  loading: boolean;
  error: string | null;
}

const initialState: GamificationState = {
  progress: null,
  achievements: [],
  loading: false,
  error: null,
};

export const fetchGamificationProgress = createAsyncThunk(
  "gamification/fetchProgress",
  async () => {
    return await getMyProgress();
  },
);

export const fetchAchievements = createAsyncThunk(
  "gamification/fetchAchievements",
  async () => {
    return await getMyAchievements();
  },
);

const gamificationSlice = createSlice({
  name: "gamification",
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchGamificationProgress.pending, (state) => {
        state.loading = true;
      })
      .addCase(fetchGamificationProgress.fulfilled, (state, action) => {
        state.loading = false;
        state.progress = action.payload;
      })
      .addCase(fetchGamificationProgress.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || "Failed to fetch progress";
      })
      .addCase(fetchAchievements.fulfilled, (state, action) => {
        state.achievements = action.payload;
      });
  },
});

export default gamificationSlice.reducer;
