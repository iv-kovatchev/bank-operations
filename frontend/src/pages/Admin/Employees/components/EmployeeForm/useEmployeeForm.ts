import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useCreateEmployee } from '../../../../../api/employees/useCreateEmployee';
import { employeeFormSchema, type EmployeeFormData } from './employeeForm.schema';

interface UseEmployeeFormProps {
  onClose: () => void;
}

export const useEmployeeForm = ({ onClose }: UseEmployeeFormProps) => {
  const { mutate: create, isPending, error } = useCreateEmployee();

  const { register, handleSubmit, formState: { errors } } = useForm<EmployeeFormData>({
    resolver: zodResolver(employeeFormSchema),
    defaultValues: { firstName: '', lastName: '', email: '' },
  });

  const onSubmit = (data: EmployeeFormData) => {
    create(data, { onSuccess: onClose });
  };

  return { register, handleSubmit, errors, isPending, error, onSubmit };
};
