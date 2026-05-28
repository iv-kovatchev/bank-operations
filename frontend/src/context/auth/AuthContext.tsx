import { useEffect, useState, type ReactNode } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { jwtDecode } from 'jwt-decode';
import { AuthContext } from './authContextDef';

const BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:8080';

type JwtPayload = {
  role: string;
  exp: number;
};

const getStoredToken = (): string | null => {
  const token = localStorage.getItem('accessToken');
  if (!token) return null;
  try {
    const { exp } = jwtDecode<JwtPayload>(token);
    return exp * 1000 > Date.now() ? token : null;
  } catch {
    return null;
  }
};

const getRoleFromToken = (token: string | null): string | null => {
  if (!token) return null;
  try {
    return jwtDecode<JwtPayload>(token).role;
  } catch {
    return null;
  }
};

const fetchRefreshedToken = async (): Promise<string> => {
  const response = await fetch(`${BASE_URL}/api/auth/refresh`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
  });
  if (!response.ok) throw new Error('Refresh failed');
  const { accessToken } = await response.json() as { accessToken: string };
  localStorage.setItem('accessToken', accessToken);
  return accessToken;
};

export const AuthContextProvider = ({ children }: { children: ReactNode }) => {
  const [manualToken, setManualToken] = useState<string | null>(getStoredToken);
  const queryClient = useQueryClient();

  const isAuthenticated = manualToken !== null;

  const setToken = (token: string) => {
    localStorage.setItem('accessToken', token);
    setManualToken(token);
  };

  const clearToken = () => {
    localStorage.clear();
    setManualToken(null);
  };

  const { data: refreshedToken, error: refreshError } = useQuery({
    queryKey: ['token-refresh'],
    queryFn: fetchRefreshedToken,
    refetchInterval: 14 * 60 * 1000,
    refetchOnWindowFocus: true,
    enabled: isAuthenticated,
    staleTime: Infinity,
    retry: false,
  });

  useEffect(() => {
    const handleVisibilityChange = () => {
      if (document.visibilityState !== 'visible') return;
      const token = localStorage.getItem('accessToken');
      if (!token) return;
      try {
        const { exp } = jwtDecode<JwtPayload>(token);
        const expiresInMs = exp * 1000 - Date.now();
        if (expiresInMs < 2 * 60 * 1000) {
          queryClient.invalidateQueries({ queryKey: ['token-refresh'] });
        }
      } catch {
        // malformed token — let the next API call handle it
      }
    };

    document.addEventListener('visibilitychange', handleVisibilityChange);
    return () => document.removeEventListener('visibilitychange', handleVisibilityChange);
  }, [queryClient]);

  useEffect(() => {
    if (refreshError) {
      localStorage.clear();
      window.location.href = '/login';
    }
  }, [refreshError]);

  const accessToken = isAuthenticated ? (refreshedToken ?? manualToken) : null;
  const role = getRoleFromToken(accessToken);

  return (
    <AuthContext.Provider value={{ accessToken, role, isAuthenticated, setToken, clearToken }}>
      {children}
    </AuthContext.Provider>
  );
};
