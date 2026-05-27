import { useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { http } from '../../services/http';

export const useLogout = () => {
  const navigate = useNavigate();

  const clearAndRedirect = () => {
    localStorage.clear();
    navigate('/login');
  };

  return useMutation({
    mutationFn: () => http.post<void>('/api/auth/logout', {}),
    onSuccess: clearAndRedirect,
    onError: clearAndRedirect,
  });
};
