import { useQuery } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { ClientResponse } from '../../types/client.types';

export const useGetClients = () => {
  return useQuery({
    queryKey: ['clients'],
    queryFn: () => http.get<ClientResponse[]>('/api/clients'),
  });
};
