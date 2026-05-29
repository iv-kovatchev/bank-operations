import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useLogin } from '../../api/auth/useLogin';
import { loginSchema, type LoginFormData } from './Login.schema';

export const useLoginPage = () => {
  const { mutate: login, isPending, error } = useLogin();

  const form = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
    mode: 'onChange',
  });

  const onSubmit = (data: LoginFormData) => login(data);

  return { form, onSubmit, isPending, error };
};
