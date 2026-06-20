import { useQuery } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { CreditServiceResponse } from '../../types/credit-service.types';

interface UseGetCreditServicesOptions {
  enabled?: boolean;
}

export const useGetCreditServices = (options?: UseGetCreditServicesOptions) => {
  return useQuery({
    queryKey: ['credit-services'],
    queryFn: () => http.get<CreditServiceResponse[]>('/api/creditservices'),
    enabled: options?.enabled ?? true,
  });
};
