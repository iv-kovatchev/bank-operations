import { createContext } from 'react';

export type AuthContextValue = {
  accessToken: string | null;
  role: string | null;
  isAuthenticated: boolean;
  setToken: (token: string) => void;
  clearToken: () => void;
};

export const AuthContext = createContext<AuthContextValue | null>(null);
