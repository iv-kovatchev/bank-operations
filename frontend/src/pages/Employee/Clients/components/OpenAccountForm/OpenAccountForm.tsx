import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Box, Flex, Text, TextField } from '@radix-ui/themes';
import { useOpenAccount } from '../../../../../api/bank-accounts/useOpenAccount';
import Button from '../../../../../components/Button/Button';
import Toast from '../../../../../components/Toast/Toast';
import { openAccountSchema } from '../openAccountForm.schema';
import type { OpenAccountFormData } from '../openAccountForm.schema';

interface OpenAccountFormProps {
  clientId: string;
  onClose: () => void;
}

const generateIban = (): string => {
  const checkDigits = Math.floor(Math.random() * 90 + 10);
  const accountDigits = Array.from({ length: 14 }, () => Math.floor(Math.random() * 10)).join('');
  return `BG${checkDigits}BANK${accountDigits}`;
};

const OpenAccountForm = ({ clientId, onClose }: OpenAccountFormProps) => {
  const { mutate: openAccount, isPending, error } = useOpenAccount();

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<OpenAccountFormData>({
    resolver: zodResolver(openAccountSchema),
    defaultValues: { iban: '', initialBalance: 0 },
  });

  const handleGenerate = () => setValue('iban', generateIban(), { shouldValidate: true });

  const onSubmit = (data: OpenAccountFormData) => {
    openAccount({ clientId, ...data }, { onSuccess: onClose });
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <Flex direction="column" gap="4" mt="3">
        {error && <Toast message={error.message} type="error" />}

        <Flex direction="column" gap="1">
          <Text as="label" size="2" weight="medium">IBAN</Text>
          <Flex gap="2">
            <Box flexGrow="1">
              <TextField.Root
                placeholder="BG00BANK00000000000000"
                {...register('iban')}
              />
            </Box>
            <Button type="button" variant="soft" onClick={handleGenerate}>
              Generate
            </Button>
          </Flex>
          {errors.iban && <Text size="1" color="red">{errors.iban.message}</Text>}
        </Flex>

        <Flex direction="column" gap="1">
          <Text as="label" size="2" weight="medium">Initial Balance (BGN)</Text>
          <TextField.Root
            type="number"
            placeholder="0"
            {...register('initialBalance', { valueAsNumber: true })}
          />
          {errors.initialBalance && (
            <Text size="1" color="red">{errors.initialBalance.message}</Text>
          )}
        </Flex>

        <Flex gap="3" justify="end" mt="2">
          <Button type="button" variant="soft" color="gray" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" loading={isPending} disabled={isPending}>
            Open Account
          </Button>
        </Flex>
      </Flex>
    </form>
  );
};

export default OpenAccountForm;
