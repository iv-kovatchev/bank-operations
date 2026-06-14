import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useDepositToAccount } from '../../../../../api/bank-accounts/useDepositToAccount';
import { useWithdrawFromAccount } from '../../../../../api/bank-accounts/useWithdrawFromAccount';

const transactionSchema = z.object({
  amount: z.preprocess(
    (val) => (val === '' || val === null || val === undefined ? undefined : Number(val)),
    z.number({ error: 'Amount is required' })
      .min(0.01, 'Amount must be greater than 0')
  ),
});

type TransactionFormInput = z.input<typeof transactionSchema>;
type TransactionFormOutput = z.output<typeof transactionSchema>;

interface UseTransactionFormParams {
  mode: 'deposit' | 'withdraw';
  accountId: string;
  clientId: string;
  onClose: () => void;
}

export const useTransactionForm = ({ mode, accountId, clientId, onClose }: UseTransactionFormParams) => {
  const deposit = useDepositToAccount();
  const withdraw = useWithdrawFromAccount();
  const { mutate, isPending, error } = mode === 'deposit' ? deposit : withdraw;

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<TransactionFormInput, unknown, TransactionFormOutput>({
    resolver: zodResolver(transactionSchema),
    defaultValues: { amount: 0 },
  });

  const onSubmit = (data: TransactionFormOutput) => {
    mutate({ id: accountId, clientId, amount: data.amount }, { onSuccess: onClose });
  };

  return { register, handleSubmit, errors, isPending, error, onSubmit };
};
