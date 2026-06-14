import { useMutation, useQueryClient } from '@tanstack/react-query';
import { http } from '../../services/http';

export const useDeleteCreditService = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => http.del<void>(`/api/creditservices/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['credit-services'] });
    },
  });
};
