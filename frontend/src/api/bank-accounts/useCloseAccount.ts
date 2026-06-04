import { useMutation, useQueryClient } from '@tanstack/react-query';
import { http } from '../../services/http';

type CloseAccountVariables = { id: string; clientId: string };

export const useCloseAccount = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id }: CloseAccountVariables) =>
      http.patch<void>(`/api/accounts/${id}/close`),
    onSuccess: (_data, { clientId }) => {
      queryClient.invalidateQueries({ queryKey: ['accounts', clientId] });
    },
  });
};
