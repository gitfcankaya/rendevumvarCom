import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { mockAuthService } from '../../services/mockAuthService';

interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  tenantId?: string;
}

interface AuthState {
  user: User | null;
  token: string | null;
  isLoading: boolean;
  error: string | null;
  isAuthenticated: boolean;
}

const initialState: AuthState = {
  user: null,
  token: localStorage.getItem('token'),
  isLoading: false,
  error: null,
  isAuthenticated: !!localStorage.getItem('token'),
};

export const login = createAsyncThunk(
  'auth/login',
  async (credentials: { email: string; password: string }, { rejectWithValue }) => {
    try {
      // First try mock service
      try {
        const mockResult = await mockAuthService.login(credentials);
        localStorage.setItem('token', mockResult.token);
        return {
          accessToken: mockResult.token,
          user: mockResult.user
        };
      } catch (mockError) {
        // If mock service fails, try API
        const response = await fetch('http://localhost:5275/api/auth/login', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(credentials),
        });

        if (!response.ok) {
          const errorData = await response.json().catch(() => ({}));
          throw new Error(errorData.message || 'Login failed');
        }

        const data = await response.json();
        localStorage.setItem('token', data.accessToken);
        return data;
      }
    } catch (error: any) {
      return rejectWithValue(error.message || 'Login failed');
    }
  }
);

export const register = createAsyncThunk(
  'auth/register',
  async (userData: {
    email: string;
    password: string;
    firstName: string;
    lastName: string;
    phone?: string;
    role?: string;
  }, { rejectWithValue }) => {
    try {
      // First try mock service
      try {
        const mockResult = await mockAuthService.register({
          email: userData.email,
          password: userData.password,
          firstName: userData.firstName,
          lastName: userData.lastName,
          role: userData.role || "Customer"
        });
        localStorage.setItem('token', mockResult.token);
        return {
          accessToken: mockResult.token,
          user: mockResult.user
        };
      } catch (mockError) {
        // If mock service fails, try API
        const requestData = {
          email: userData.email,
          password: userData.password,
          confirmPassword: userData.password, // Auto-confirm for now
          firstName: userData.firstName,
          lastName: userData.lastName,
          phone: userData.phone || "",
          role: userData.role || "Customer" // Default role
        };

        const response = await fetch('http://localhost:5275/api/auth/register', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(requestData),
        });

        if (!response.ok) {
          const errorData = await response.json().catch(() => ({}));
          throw new Error(errorData.message || 'Registration failed');
        }

        const data = await response.json();
        localStorage.setItem('token', data.accessToken);
        return data;
      }
    } catch (error: any) {
      return rejectWithValue(error.message || 'Registration failed');
    }
  }
);

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    logout: (state) => {
      localStorage.removeItem('token');
      state.user = null;
      state.token = null;
      state.isAuthenticated = false;
      state.error = null;
    },
    clearError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      // Login
      .addCase(login.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(login.fulfilled, (state, action) => {
        state.isLoading = false;
        state.user = action.payload.user;
        state.token = action.payload.accessToken;
        state.isAuthenticated = true;
        state.error = null;
      })
      .addCase(login.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
        state.isAuthenticated = false;
      })
      // Register
      .addCase(register.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(register.fulfilled, (state, action) => {
        state.isLoading = false;
        state.user = action.payload.user;
        state.token = action.payload.accessToken;
        state.isAuthenticated = true;
        state.error = null;
      })
      .addCase(register.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
        state.isAuthenticated = false;
      });
  },
});

export const { logout, clearError } = authSlice.actions;
export default authSlice.reducer;