import { useQuery } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { CreditServiceResponse } from '../../types/credit-service.types';

export const useGetCreditService = (id: string) => {
  return useQuery({
    queryKey: ['credit-services', id],
    queryFn: () => http.get<CreditServiceResponse>(`/api/creditservices/${id}`),
  });
};
