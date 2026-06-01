import { useMutation, useQueryClient } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { CreateCorporateClientDto, CorporateClientResponse } from '../../types/client.types';

export const useCreateCorporateClient = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateCorporateClientDto) =>
      http.post<CorporateClientResponse>('/api/clients/corporate', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['clients'] });
    },
  });
};
