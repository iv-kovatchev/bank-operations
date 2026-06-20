import { useMutation, useQueryClient } from '@tanstack/react-query';
import { http } from '../../services/http';

export const useDeactivateEmployee = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => http.patch<void>(`/api/employees/${id}/deactivate`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['employees'] });
    },
  });
};
