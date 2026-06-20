import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useGetClientAccounts } from '../../../../../api/bank-accounts/useGetClientAccounts';
import { usePayInstallment } from '../../../../../api/credits/usePayInstallment';
import { AccountStatus } from '../../../../../types/bank-account.types';
import { payInstallmentSchema, type PayInstallmentFormInput, type PayInstallmentFormOutput } from './payInstallmentForm.schema';

interface UsePayInstallmentFormProps {
  creditId: string;
  installmentId: string;
  clientId: string;
  onClose: () => void;
}

export const usePayInstallmentForm = ({ creditId, installmentId, clientId, onClose }: UsePayInstallmentFormProps) => {
  const { data: accounts } = useGetClientAccounts(clientId);
  const activeAccounts = (accounts ?? []).filter((account) => account.status === AccountStatus.Active);

  const { mutate: pay, isPending, error } = usePayInstallment();

  const {
    handleSubmit,
    formState: { errors },
    control,
  } = useForm<PayInstallmentFormInput, unknown, PayInstallmentFormOutput>({
    resolver: zodResolver(payInstallmentSchema),
    defaultValues: { bankAccountId: '' },
  });

  const onSubmit = (data: PayInstallmentFormOutput) => {
    pay({ creditId, installmentId, clientId, bankAccountId: data.bankAccountId }, { onSuccess: onClose });
  };

  return { handleSubmit, errors, control, activeAccounts, isPending, error, onSubmit };
};
