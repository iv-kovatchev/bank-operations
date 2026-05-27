import { useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { jwtDecode } from 'jwt-decode';
import { http } from '../../services/http';
import type { AuthResponse, VerifyOtpRequest } from '../../types/auth.types';

type JwtPayload = {
  role: string;
};

const roleDashboards: Record<string, string> = {
  Admin: '/admin/dashboard',
  Employee: '/employee/dashboard',
  Client: '/client/dashboard',
};

export const useVerifyOtp = () => {
  const navigate = useNavigate();

  return useMutation({
    mutationFn: (data: VerifyOtpRequest) => http.post<AuthResponse>('/api/auth/verify-otp', data),
    onSuccess: (response) => {
      if (response.accessToken) {
        localStorage.setItem('accessToken', response.accessToken);
        const role = jwtDecode<JwtPayload>(response.accessToken).role;
        navigate(roleDashboards[role] ?? '/login');
      }
    },
    onError: (error: Error) => {
      console.error('OTP verification failed:', error.message);
    },
  });
};
