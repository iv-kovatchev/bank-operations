import { useGetStats } from '../../../api/stats/useGetStats';

export const useEmployeeDashboard = () => {
  const { data: stats, isLoading } = useGetStats();

  return { stats, isLoading };
};
