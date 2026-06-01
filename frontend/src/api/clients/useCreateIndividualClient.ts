import { useMutation, useQueryClient } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { CreateIndividualClientDto, IndividualClientResponse } from '../../types/client.types';

export const useCreateIndividualClient = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateIndividualClientDto) =>
      http.post<IndividualClientResponse>('/api/clients/individual', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['clients'] });
    },
  });
};
