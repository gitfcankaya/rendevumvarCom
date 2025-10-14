// Mock authentication service for testing
export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  user: {
    id: string;
    email: string;
    name: string;
  };
}

// Mock authentication service
export const mockAuthService = {
  login: async (credentials: { email: string; password: string }): Promise<LoginResponse> => {
    // Simulate API delay
    await new Promise(resolve => setTimeout(resolve, 1000));

    // Mock validation
    if (credentials.email === 'test@example.com' && credentials.password === 'Test123!') {
      return {
        token: 'mock-jwt-token-' + Date.now(),
        user: {
          id: '1',
          email: credentials.email,
          name: 'Test User'
        }
      };
    } else if (credentials.email === 'admin@example.com' && credentials.password === 'Admin123!') {
      return {
        token: 'mock-jwt-token-admin-' + Date.now(),
        user: {
          id: '2',
          email: credentials.email,
          name: 'Admin User'
        }
      };
    } else {
      throw new Error('Invalid credentials');
    }
  },

  register: async (userData: any): Promise<LoginResponse> => {
    // Simulate API delay
    await new Promise(resolve => setTimeout(resolve, 1000));

    return {
      token: 'mock-jwt-token-new-' + Date.now(),
      user: {
        id: Math.random().toString(),
        email: userData.email,
        name: userData.name || 'New User'
      }
    };
  }
};