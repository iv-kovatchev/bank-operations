import { useMutation, useQueryClient } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { CreditResponse, CreateMortgageCreditRequest } from '../../types/credit.types';

type GrantMortgageCreditVariables = { clientId: string } & CreateMortgageCreditRequest;

export const useGrantMortgageCredit = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ clientId, ...dto }: GrantMortgageCreditVariables) =>
      http.post<CreditResponse>(`/api/clients/${clientId}/credits/mortgage`, dto),
    onSuccess: (_data, { clientId }) => {
      queryClient.invalidateQueries({ queryKey: ['credits', clientId] });
    },
  });
};
