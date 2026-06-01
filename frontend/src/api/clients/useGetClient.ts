import { useQuery } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { ClientDetailResponse } from '../../types/client.types';

export const useGetClient = (id: string) => {
  return useQuery({
    queryKey: ['clients', id],
    queryFn: () => http.get<ClientDetailResponse>(`/api/clients/${id}`),
  });
};
