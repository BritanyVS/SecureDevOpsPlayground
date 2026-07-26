import axiosClient from './axiosClient';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  firstName?: string;
  lastName?: string;
}

export interface UserResponse {
  id: string;
  username: string;
  email: string;
  role: string;
  firstName?: string;
  lastName?: string;
  isActive: boolean;
  createdAt: string;
}

export interface ApiError {
  errors: string[];
}

export const authApi = {
  login: async (data: LoginRequest): Promise<LoginResponse> => {
    const response = await axiosClient.post<LoginResponse>('/auth/login', data);
    return response.data;
  },

  register: async (data: RegisterRequest): Promise<UserResponse> => {
    const response = await axiosClient.post<UserResponse>('/auth/register', data);
    return response.data;
  },

  getProfile: async (token?: string): Promise<{ userId: string; username: string; email: string; role: string }> => {
    const config = token ? { headers: { Authorization: `Bearer ${token}` } } : {};
    const response = await axiosClient.get('/user/profile', config);
    return response.data;
  },
};
