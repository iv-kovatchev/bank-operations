import { useQuery } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { CreditServiceResponse } from '../../types/credit-service.types';

export const useGetCreditServices = () => {
  return useQuery({
    queryKey: ['credit-services'],
    queryFn: () => http.get<CreditServiceResponse[]>('/api/creditservices'),
  });
};
