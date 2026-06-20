import { useMutation, useQueryClient } from '@tanstack/react-query';
import { http } from '../../services/http';

export const useActivateEmployee = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => http.patch<void>(`/api/employees/${id}/activate`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['employees'] });
    },
  });
};
