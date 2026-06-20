import { useMutation, useQueryClient } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { CreditResponse, CreateConsumerCreditRequest } from '../../types/credit.types';

type GrantConsumerCreditVariables = { clientId: string } & CreateConsumerCreditRequest;

export const useGrantConsumerCredit = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ clientId, ...dto }: GrantConsumerCreditVariables) =>
      http.post<CreditResponse>(`/api/clients/${clientId}/credits/consumer`, dto),
    onSuccess: (_data, { clientId }) => {
      queryClient.invalidateQueries({ queryKey: ['credits', clientId] });
    },
  });
};
