import { useMutation, useQueryClient } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { BankAccountResponse, TransactionDto } from '../../types/bank-account.types';

type WithdrawVariables = { id: string; clientId: string; amount: number };

export const useWithdrawFromAccount = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, amount }: WithdrawVariables) => {
      const dto: TransactionDto = { amount };
      return http.patch<BankAccountResponse>(`/api/accounts/${id}/withdraw`, dto);
    },
    onSuccess: (_data, { clientId }) => {
      queryClient.invalidateQueries({ queryKey: ['accounts', clientId] });
    },
  });
};
