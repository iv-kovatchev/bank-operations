import { useQuery } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { ProfileResponse } from '../../types/settings.types';

export const useGetProfile = (enabled: boolean = true) => {
  return useQuery({
    queryKey: ['profile'],
    queryFn: () => http.get<ProfileResponse>('/api/settings/profile'),
    enabled,
  });
};
