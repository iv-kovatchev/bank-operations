import { useQuery } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { EmployeeResponse } from '../../types/employee.types';

export const useGetEmployees = () => {
  return useQuery({
    queryKey: ['employees'],
    queryFn: () => http.get<EmployeeResponse[]>('/api/employees'),
  });
};
