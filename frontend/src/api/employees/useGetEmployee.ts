import { useQuery } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { EmployeeResponse } from '../../types/employee.types';

export const useGetEmployee = (id: string) => {
  return useQuery({
    queryKey: ['employees', id],
    queryFn: () => http.get<EmployeeResponse>(`/api/employees/${id}`),
  });
};
