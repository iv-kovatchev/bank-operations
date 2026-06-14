import { useMutation, useQueryClient } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { BankAccountResponse, TransactionDto } from '../../types/bank-account.types';

type DepositVariables = { id: string; clientId: string; amount: number };

export const useDepositToAccount = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, amount }: DepositVariables) => {
      const dto: TransactionDto = { amount };
      return http.patch<BankAccountResponse>(`/api/accounts/${id}/deposit`, dto);
    },
    onSuccess: (_data, { clientId }) => {
      queryClient.invalidateQueries({ queryKey: ['accounts', clientId] });
    },
  });
};
