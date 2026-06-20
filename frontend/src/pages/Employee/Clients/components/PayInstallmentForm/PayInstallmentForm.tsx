import { Controller } from 'react-hook-form';
import { Flex, Select, Text } from '@radix-ui/themes';
import Button from '../../../../../components/Button/Button';
import Toast from '../../../../../components/Toast/Toast';
import { usePayInstallmentForm } from './usePayInstallmentForm';

interface PayInstallmentFormProps {
  creditId: string;
  installmentId: string;
  clientId: string;
  onClose: () => void;
}

const PayInstallmentForm = ({ creditId, installmentId, clientId, onClose }: PayInstallmentFormProps) => {
  const { handleSubmit, errors, control, activeAccounts, isPending, error, onSubmit } =
    usePayInstallmentForm({ creditId, installmentId, clientId, onClose });

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <Flex direction="column" gap="4" mt="3">
        {error && <Toast message={error.message} type="error" />}

        <Flex direction="column" gap="1">
          <Text as="label" size="2" weight="medium">Bank Account</Text>
          <Controller
            name="bankAccountId"
            control={control}
            render={({ field }) => (
              <Select.Root key={field.value} value={field.value} onValueChange={field.onChange}>
                <Select.Trigger placeholder="Select bank account" />
                <Select.Content>
                  {activeAccounts.map((account) => (
                    <Select.Item key={account.id} value={account.id}>
                      {account.iban} — {account.balance.toFixed(2)} EUR
                    </Select.Item>
                  ))}
                </Select.Content>
              </Select.Root>
            )}
          />
          {errors.bankAccountId && <Text size="1" color="red">{errors.bankAccountId.message}</Text>}
        </Flex>

        <Flex gap="3" justify="end" mt="2">
          <Button type="button" variant="soft" color="gray" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" loading={isPending} disabled={isPending}>
            Pay
          </Button>
        </Flex>
      </Flex>
    </form>
  );
};

export default PayInstallmentForm;
