import { useMutation, useQueryClient } from '@tanstack/react-query';
import { http } from '../../services/http';

type DeleteAccountVariables = { id: string; clientId: string };

export const useDeleteAccount = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id }: DeleteAccountVariables) =>
      http.del<void>(`/api/accounts/${id}`),
    onSuccess: (_data, { clientId }) => {
      queryClient.invalidateQueries({ queryKey: ['accounts', clientId] });
    },
  });
};
