import { useMutation, useQueryClient } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { CreateCreditServiceDto, CreditServiceResponse } from '../../types/credit-service.types';

export const useCreateCreditService = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateCreditServiceDto) =>
      http.post<CreditServiceResponse>('/api/creditservices', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['credit-services'] });
    },
  });
};
