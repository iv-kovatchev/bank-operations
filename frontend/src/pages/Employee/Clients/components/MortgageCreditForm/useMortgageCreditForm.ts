import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useGrantMortgageCredit } from '../../../../../api/credits/useGrantMortgageCredit';
import { useUpdateMortgageCredit } from '../../../../../api/credits/useUpdateMortgageCredit';
import { mortgageCreditSchema, type MortgageCreditFormInput, type MortgageCreditFormOutput } from './mortgageCreditForm.schema';
import { PropertyType, type CreditResponse } from '../../../../../types/credit.types';

interface UseMortgageCreditFormProps {
  mode: 'create' | 'edit';
  initialData?: CreditResponse;
  clientId: string;
  onClose: () => void;
}

export const useMortgageCreditForm = ({ mode, initialData, clientId, onClose }: UseMortgageCreditFormProps) => {
  const isEdit = mode === 'edit';

  const { mutate: grant, isPending: isGranting, error: grantError } = useGrantMortgageCredit();
  const { mutate: update, isPending: isUpdating, error: updateError } = useUpdateMortgageCredit();

  const isPending = isGranting || isUpdating;
  const error = grantError ?? updateError;

  const {
    register,
    handleSubmit,
    formState: { errors },
    control,
    reset,
  } = useForm<MortgageCreditFormInput, unknown, MortgageCreditFormOutput>({
    resolver: zodResolver(mortgageCreditSchema),
    defaultValues: {
      creditServiceId: '',
      amount: 0,
      termMonths: 0,
      propertyAddress: '',
      propertyType: '' as PropertyType,
    },
  });

  useEffect(() => {
    if (initialData) {
      reset({
        creditServiceId: initialData.creditServiceId,
        amount: initialData.amount,
        termMonths: initialData.termMonths,
        propertyAddress: initialData.propertyAddress ?? '',
        propertyType: (initialData.propertyType ?? '') as PropertyType,
      });
    } else {
      reset({ creditServiceId: '', amount: 0, termMonths: 0, propertyAddress: '', propertyType: '' as PropertyType });
    }
  }, [initialData, reset]);

  const onSubmit = (data: MortgageCreditFormOutput) => {
    if (isEdit && initialData) {
      update({ id: initialData.id, clientId, ...data }, { onSuccess: onClose });
    } else {
      grant({ clientId, ...data }, { onSuccess: onClose });
    }
  };

  return { register, handleSubmit, errors, control, isEdit, isPending, error, onSubmit };
};
