import { useMutation, useQueryClient } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { UpdateIndividualClientDto, IndividualClientResponse } from '../../types/client.types';

type UpdateIndividualClientVars = { id: string } & UpdateIndividualClientDto;

export const useUpdateIndividualClient = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, ...data }: UpdateIndividualClientVars) =>
      http.put<IndividualClientResponse>(`/api/clients/individual/${id}`, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['clients'] });
      queryClient.invalidateQueries({ queryKey: ['clients', id] });
    },
  });
};
