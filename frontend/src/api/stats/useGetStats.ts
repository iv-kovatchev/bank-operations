import { useQuery } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { StatsResponse } from '../../types/stats.types';

export const useGetStats = () => {
  return useQuery({
    queryKey: ['stats'],
    queryFn: () => http.get<StatsResponse>('/api/stats'),
  });
};
