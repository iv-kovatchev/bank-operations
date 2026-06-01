import { useMutation, useQueryClient } from '@tanstack/react-query';
import { http } from '../../services/http';

export const useDeactivateClient = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => http.patch<void>(`/api/clients/${id}/deactivate`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['clients'] });
    },
  });
};
