import { useMutation, useQueryClient } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { UpdateCreditServiceDto, CreditServiceResponse } from '../../types/credit-service.types';

type UpdateCreditServiceVars = { id: string } & UpdateCreditServiceDto;

export const useUpdateCreditService = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, ...data }: UpdateCreditServiceVars) =>
      http.put<CreditServiceResponse>(`/api/creditservices/${id}`, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['credit-services'] });
      queryClient.invalidateQueries({ queryKey: ['credit-services', id] });
    },
  });
};
