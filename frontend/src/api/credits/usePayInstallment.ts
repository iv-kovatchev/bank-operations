import { useMutation, useQueryClient } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { PayInstallmentRequest, RepaymentInstallmentResponse } from '../../types/credit.types';

type PayInstallmentVariables = { creditId: string; installmentId: string; clientId: string; bankAccountId: string };

export const usePayInstallment = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ creditId, installmentId, bankAccountId }: PayInstallmentVariables) => {
      const dto: PayInstallmentRequest = { bankAccountId };
      return http.patch<RepaymentInstallmentResponse>(`/api/credits/${creditId}/installments/${installmentId}/pay`, dto);
    },
    onSuccess: (_data, { creditId, clientId }) => {
      queryClient.invalidateQueries({ queryKey: ['repayment-plan', creditId] });
      queryClient.invalidateQueries({ queryKey: ['credits', clientId] });
      queryClient.invalidateQueries({ queryKey: ['accounts', clientId] });
    },
  });
};
