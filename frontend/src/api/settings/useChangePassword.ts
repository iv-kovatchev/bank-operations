import { useMutation } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { ChangePasswordDto } from '../../types/settings.types';

export const useChangePassword = () => {
  return useMutation({
    mutationFn: (data: ChangePasswordDto) =>
      http.patch<void>('/api/settings/password', data),
  });
};
