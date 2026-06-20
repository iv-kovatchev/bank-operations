import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useGrantConsumerCredit } from '../../../../../api/credits/useGrantConsumerCredit';
import { useUpdateConsumerCredit } from '../../../../../api/credits/useUpdateConsumerCredit';
import { consumerCreditSchema, type ConsumerCreditFormInput, type ConsumerCreditFormOutput } from './consumerCreditForm.schema';
import { CreditPurpose, type CreditResponse } from '../../../../../types/credit.types';

interface UseConsumerCreditFormProps {
  mode: 'create' | 'edit';
  initialData?: CreditResponse;
  clientId: string;
  onClose: () => void;
}

export const useConsumerCreditForm = ({ mode, initialData, clientId, onClose }: UseConsumerCreditFormProps) => {
  const isEdit = mode === 'edit';

  const { mutate: grant, isPending: isGranting, error: grantError } = useGrantConsumerCredit();
  const { mutate: update, isPending: isUpdating, error: updateError } = useUpdateConsumerCredit();

  const isPending = isGranting || isUpdating;
  const error = grantError ?? updateError;

  const {
    register,
    handleSubmit,
    formState: { errors },
    control,
    reset,
  } = useForm<ConsumerCreditFormInput, unknown, ConsumerCreditFormOutput>({
    resolver: zodResolver(consumerCreditSchema),
    defaultValues: {
      creditServiceId: '',
      amount: 0,
      termMonths: 0,
      purpose: '' as CreditPurpose,
    },
  });

  useEffect(() => {
    if (initialData) {
      reset({
        creditServiceId: initialData.creditServiceId,
        amount: initialData.amount,
        termMonths: initialData.termMonths,
        purpose: (initialData.purpose ?? '') as CreditPurpose,
      });
    } else {
      reset({ creditServiceId: '', amount: 0, termMonths: 0, purpose: '' as CreditPurpose });
    }
  }, [initialData, reset]);

  const onSubmit = (data: ConsumerCreditFormOutput) => {
    if (isEdit && initialData) {
      update({ id: initialData.id, clientId, ...data }, { onSuccess: onClose });
    } else {
      grant({ clientId, ...data }, { onSuccess: onClose });
    }
  };

  return { register, handleSubmit, errors, control, isEdit, isPending, error, onSubmit };
};
