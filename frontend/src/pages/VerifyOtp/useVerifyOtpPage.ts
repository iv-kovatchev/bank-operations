import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useVerifyOtp } from '../../api/auth/useVerifyOtp';
import { verifyOtpSchema, type VerifyOtpFormData } from './VerifyOtp.schema';

export const useVerifyOtpPage = () => {
  const navigate = useNavigate();
  const email = sessionStorage.getItem('otpEmail');

  useEffect(() => {
    if (!email) navigate('/login', { replace: true });
  }, [email, navigate]);

  const { mutate: verifyOtp, isPending, error } = useVerifyOtp();

  const form = useForm<VerifyOtpFormData>({
    resolver: zodResolver(verifyOtpSchema),
  });

  const onSubmit = ({ code }: VerifyOtpFormData) => {
    verifyOtp({ email: email!, code });
  };

  return { form, onSubmit, isPending, error, email };
};
