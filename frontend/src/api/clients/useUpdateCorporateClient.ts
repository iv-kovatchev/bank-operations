import { useMutation, useQueryClient } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { UpdateCorporateClientDto, CorporateClientResponse } from '../../types/client.types';

type UpdateCorporateClientVars = { id: string } & UpdateCorporateClientDto;

export const useUpdateCorporateClient = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, ...data }: UpdateCorporateClientVars) =>
      http.put<CorporateClientResponse>(`/api/clients/corporate/${id}`, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['clients'] });
      queryClient.invalidateQueries({ queryKey: ['clients', id] });
    },
  });
};
