import { useMutation, useQueryClient } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { CreditResponse, UpdateMortgageCreditRequest } from '../../types/credit.types';

type UpdateMortgageCreditVariables = { id: string; clientId: string } & UpdateMortgageCreditRequest;

export const useUpdateMortgageCredit = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, creditServiceId, amount, termMonths, propertyAddress, propertyType }: UpdateMortgageCreditVariables) => {
      const dto: UpdateMortgageCreditRequest = { creditServiceId, amount, termMonths, propertyAddress, propertyType };
      return http.put<CreditResponse>(`/api/credits/${id}/mortgage`, dto);
    },
    onSuccess: (_data, { id, clientId }) => {
      queryClient.invalidateQueries({ queryKey: ['credits', clientId] });
      queryClient.invalidateQueries({ queryKey: ['credits', 'detail', id] });
    },
  });
};
