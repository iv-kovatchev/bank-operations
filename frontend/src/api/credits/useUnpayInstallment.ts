import { useMutation, useQueryClient } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { RepaymentInstallmentResponse } from '../../types/credit.types';

type UnpayInstallmentVariables = { creditId: string; installmentId: string; clientId: string };

export const useUnpayInstallment = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ creditId, installmentId }: UnpayInstallmentVariables) =>
      http.patch<RepaymentInstallmentResponse>(`/api/credits/${creditId}/installments/${installmentId}/unpay`),
    onSuccess: (_data, { creditId, clientId }) => {
      queryClient.invalidateQueries({ queryKey: ['repayment-plan', creditId] });
      queryClient.invalidateQueries({ queryKey: ['credits', clientId] });
    },
  });
};
