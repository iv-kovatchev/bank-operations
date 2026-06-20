import { useMutation, useQueryClient } from '@tanstack/react-query';
import { http } from '../../services/http';
import type { CreateEmployeeDto, EmployeeResponse } from '../../types/employee.types';

export const useCreateEmployee = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateEmployeeDto) =>
      http.post<EmployeeResponse>('/api/employees', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['employees'] });
    },
  });
};
