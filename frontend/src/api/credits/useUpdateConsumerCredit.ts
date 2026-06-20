import { useMutation, useQueryClient } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { CreditResponse, UpdateConsumerCreditRequest } from '../../types/credit.types';

type UpdateConsumerCreditVariables = { id: string; clientId: string } & UpdateConsumerCreditRequest;

export const useUpdateConsumerCredit = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, creditServiceId, amount, termMonths, purpose }: UpdateConsumerCreditVariables) => {
      const dto: UpdateConsumerCreditRequest = { creditServiceId, amount, termMonths, purpose };
      return http.put<CreditResponse>(`/api/credits/${id}/consumer`, dto);
    },
    onSuccess: (_data, { id, clientId }) => {
      queryClient.invalidateQueries({ queryKey: ['credits', clientId] });
      queryClient.invalidateQueries({ queryKey: ['credits', 'detail', id] });
    },
  });
};
