import { useQuery } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { BankAccountResponse } from '../../types/bank-account.types';

export const useGetClientAccounts = (clientId: string) =>
  useQuery({
    queryKey: ['accounts', clientId],
    queryFn: () => http.get<BankAccountResponse[]>(`/api/clients/${clientId}/accounts`),
    enabled: !!clientId,
  });
