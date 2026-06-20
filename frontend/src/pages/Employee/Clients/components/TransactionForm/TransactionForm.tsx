import { Flex, Text, TextField } from '@radix-ui/themes';
import Button from '../../../../../components/Button/Button';
import Toast from '../../../../../components/Toast/Toast';
import { useTransactionForm } from './useTransactionForm';

interface TransactionFormProps {
  mode: 'deposit' | 'withdraw';
  accountId: string;
  clientId: string;
  onClose: () => void;
}

const TransactionForm = ({ mode, accountId, clientId, onClose }: TransactionFormProps) => {
  const { register, handleSubmit, errors, isPending, error, onSubmit } = useTransactionForm({
    mode,
    accountId,
    clientId,
    onClose,
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <Flex direction="column" gap="4" mt="3">
        {error && <Toast message={error.message} type="error" />}

        <Flex direction="column" gap="1">
          <Text as="label" size="2" weight="medium">Amount (EUR)</Text>
          <TextField.Root
            type="number"
            step="0.01"
            placeholder="0"
            {...register('amount', { valueAsNumber: true })}
          />
          {errors.amount && <Text size="1" color="red">{errors.amount.message}</Text>}
        </Flex>

        <Flex gap="3" justify="end" mt="2">
          <Button type="button" variant="soft" color="gray" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" loading={isPending} disabled={isPending}>
            {mode === 'deposit' ? 'Deposit' : 'Withdraw'}
          </Button>
        </Flex>
      </Flex>
    </form>
  );
};

export default TransactionForm;
