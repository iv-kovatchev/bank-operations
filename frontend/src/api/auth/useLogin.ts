import { useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { http } from '../../services/http';
import type { AuthResponse, LoginRequest } from '../../types/auth.types';

export const useLogin = () => {
  const navigate = useNavigate();

  return useMutation({
    mutationFn: (data: LoginRequest) => http.post<AuthResponse>('/api/auth/login', data),
    onSuccess: (response, variables) => {
      if (response.requiresOtp) {
        sessionStorage.setItem('otpEmail', variables.email);
        navigate('/verify-otp');
      }
    },
    onError: (error: Error) => {
      console.error('Login failed:', error.message);
    },
  });
};
