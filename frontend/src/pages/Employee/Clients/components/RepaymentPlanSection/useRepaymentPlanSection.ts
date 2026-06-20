import { useGetRepaymentPlan } from '../../../../../api/credits/useGetRepaymentPlan';

export const useRepaymentPlanSection = (creditId: string) => {
  const { data: plan, isLoading } = useGetRepaymentPlan(creditId);

  return { plan, isLoading };
};
