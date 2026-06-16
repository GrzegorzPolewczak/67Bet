import {
  createSlice,
  createAsyncThunk,
  type PayloadAction,
} from "@reduxjs/toolkit";
import { identityApi } from "../../api/axios";

interface User {
  id: string;
  email: string;
  username: string;
  role: "User" | "Admin";
  isKycVerified: boolean;
}

interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  loading: boolean;
  error: string | null;
}

const initialState: AuthState = {
  user: null,
  token: localStorage.getItem("token"),
  isAuthenticated: !!localStorage.getItem("token"),
  loading: false,
  error: null,
};

export const getCurrentUser = createAsyncThunk(
  "auth/getCurrentUser",
  async (_, { rejectWithValue }) => {
    try {
      const response = await identityApi.get("/auth/me");
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data || "Failed to fetch user");
    }
  },
);

export const loginAsync = createAsyncThunk(
  "auth/login",
  async (
    credentials: { email: string; password: string },
    { rejectWithValue },
  ) => {
    try {
      const response = await identityApi.post("/auth/login", credentials);
      const token = response.data;

      // Get user info after successful login
      const userResponse = await identityApi.get("/auth/me", {
        headers: { Authorization: `Bearer ${token}` },
      });

      return { user: userResponse.data, token };
    } catch (error: any) {
      const message =
        error.response?.data?.message ||
        error.response?.data ||
        error.message ||
        "Login failed";
      return rejectWithValue(
        typeof message === "object" ? JSON.stringify(message) : message,
      );
    }
  },
);

export const registerAsync = createAsyncThunk(
  "auth/register",
  async (userData: any, { rejectWithValue }) => {
    try {
      const response = await identityApi.post("/auth/register", userData);
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data || "Registration failed");
    }
  },
);

const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    setCredentials: (
      state,
      action: PayloadAction<{ user: User; token: string }>,
    ) => {
      const { user, token } = action.payload;
      state.user = user;
      state.token = token;
      state.isAuthenticated = true;
      localStorage.setItem("token", token);
    },
    logout: (state) => {
      state.user = null;
      state.token = null;
      state.isAuthenticated = false;
      localStorage.removeItem("token");
    },
    clearError: (state) => {
      state.error = null;
    },
    setKycVerified: (state) => {
      if (state.user) {
        state.user.isKycVerified = true;
      }
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(loginAsync.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(loginAsync.fulfilled, (state, action) => {
        state.loading = false;
        state.user = action.payload.user;
        state.token = action.payload.token;
        state.isAuthenticated = true;
        localStorage.setItem("token", action.payload.token);
      })
      .addCase(loginAsync.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      .addCase(getCurrentUser.pending, (state) => {
        state.loading = true;
      })
      .addCase(getCurrentUser.fulfilled, (state, action) => {
        state.loading = false;
        state.user = action.payload;
        state.isAuthenticated = true;
      })
      .addCase(getCurrentUser.rejected, (state, action) => {
        state.loading = false;
        state.user = null;
        state.token = null;
        state.isAuthenticated = false;
        localStorage.removeItem("token");
      });
  },
});

export const { setCredentials, logout, clearError, setKycVerified } =
  authSlice.actions;
export default authSlice.reducer;
