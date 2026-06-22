import { useGetStats } from '../../../api/stats/useGetStats';

export const useAdminDashboard = () => {
  const { data: stats, isLoading } = useGetStats();

  return { stats, isLoading };
};
