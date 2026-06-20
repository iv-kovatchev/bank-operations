import { useMutation, useQueryClient } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { ProfileResponse, UpdateProfileDto } from '../../types/settings.types';

export const useUpdateProfile = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: UpdateProfileDto) =>
      http.put<ProfileResponse>('/api/settings/profile', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['profile'] });
    },
  });
};
