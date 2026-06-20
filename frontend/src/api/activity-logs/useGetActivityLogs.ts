import { useQuery } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { ActivityLogResponse } from '../../types/activity-log.types';

export const useGetActivityLogs = () => {
  return useQuery({
    queryKey: ['activity-logs'],
    queryFn: () => http.get<ActivityLogResponse[]>('/api/activity-logs'),
  });
};
