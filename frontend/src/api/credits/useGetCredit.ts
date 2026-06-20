import { useQuery } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { CreditResponse } from '../../types/credit.types';

export const useGetCredit = (id: string) =>
  useQuery({
    queryKey: ['credits', 'detail', id],
    queryFn: () => http.get<CreditResponse>(`/api/credits/${id}`),
    enabled: !!id,
  });
