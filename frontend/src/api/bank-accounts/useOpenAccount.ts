import { useMutation, useQueryClient } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { BankAccountResponse, CreateBankAccountDto } from '../../types/bank-account.types';

type OpenAccountVariables = { clientId: string } & CreateBankAccountDto;

export const useOpenAccount = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ clientId, ...dto }: OpenAccountVariables) =>
      http.post<BankAccountResponse>(`/api/clients/${clientId}/accounts`, dto),
    onSuccess: (_data, { clientId }) => {
      queryClient.invalidateQueries({ queryKey: ['accounts', clientId] });
    },
  });
};
