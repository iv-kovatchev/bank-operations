import { useQuery } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { CreditResponse } from '../../types/credit.types';

export const useGetClientCredits = (clientId: string) =>
  useQuery({
    queryKey: ['credits', clientId],
    queryFn: () => http.get<CreditResponse[]>(`/api/clients/${clientId}/credits`),
    enabled: !!clientId,
  });
