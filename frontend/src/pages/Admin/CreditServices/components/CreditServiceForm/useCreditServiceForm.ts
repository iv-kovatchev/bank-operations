import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useCreateCreditService } from '../../../../../api/credit-services/useCreateCreditService';
import { useUpdateCreditService } from '../../../../../api/credit-services/useUpdateCreditService';
import { creditServiceSchema, type CreditServiceFormInput, type CreditServiceFormOutput } from './creditServiceForm.schema';
import { CreditType, type CreditServiceResponse } from '../../../../../types/credit-service.types';

interface UseCreditServiceFormProps {
  mode: 'create' | 'edit';
  initialData?: CreditServiceResponse;
  onClose: () => void;
}

export const useCreditServiceForm = ({ mode, initialData, onClose }: UseCreditServiceFormProps) => {
  const isEdit = mode === 'edit';

  const { mutate: create, isPending: isCreating, error: createError } = useCreateCreditService();
  const { mutate: update, isPending: isUpdating, error: updateError } = useUpdateCreditService();

  const isPending = isCreating || isUpdating;
  const error = createError ?? updateError;

  const {
    register,
    handleSubmit,
    formState: { errors },
    control,
    reset,
  } = useForm<CreditServiceFormInput, unknown, CreditServiceFormOutput>({
    resolver: zodResolver(creditServiceSchema),
    defaultValues: {
      name: '',
      type: '' as CreditType,
      interestRate: 0,
      maxAmount: 0,
      maxTermMonths: 0,
    },
  });

  useEffect(() => {
    console.log('initialData changed:', initialData);
    if (initialData) {
      reset({
        name: initialData.name,
        type: initialData.type,
        interestRate: initialData.interestRate,
        maxAmount: initialData.maxAmount,
        maxTermMonths: initialData.maxTermMonths,
      });
    } else {
      reset({ name: '', type: '' as CreditType, interestRate: 0, maxAmount: 0, maxTermMonths: 0 });
    }
  }, [initialData, reset]);

  const onSubmit = (data: CreditServiceFormOutput) => {
    if (isEdit && initialData) {
      update({ id: initialData.id, ...data }, { onSuccess: onClose });
    } else {
      create(data, { onSuccess: onClose });
    }
  };

  return { register, handleSubmit, errors, control, isEdit, isPending, error, onSubmit };
};
