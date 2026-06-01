import { useMutation, useQueryClient } from '@tanstack/react-query';
import { http } from '../../services/http';

export const useActivateClient = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => http.patch<void>(`/api/clients/${id}/activate`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['clients'] });
    },
  });
};
