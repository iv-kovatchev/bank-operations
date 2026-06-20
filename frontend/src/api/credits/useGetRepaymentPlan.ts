import { useQuery } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { RepaymentPlanResponse } from '../../types/credit.types';

export const useGetRepaymentPlan = (id: string) =>
  useQuery({
    queryKey: ['repayment-plan', id],
    queryFn: () => http.get<RepaymentPlanResponse>(`/api/credits/${id}/repayment-plan`),
    enabled: !!id,
  });
