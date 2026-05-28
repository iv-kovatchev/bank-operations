import { useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { http } from '../../services/http';
import { useAuth } from '../../context/auth/useAuth';

export const useLogout = () => {
  const navigate = useNavigate();
  const { clearToken } = useAuth();

  const clearAndRedirect = () => {
    clearToken();
    navigate('/login');
  };

  return useMutation({
    mutationFn: () => http.post<void>('/api/auth/logout', {}),
    onSuccess: clearAndRedirect,
    onError: clearAndRedirect,
  });
};
