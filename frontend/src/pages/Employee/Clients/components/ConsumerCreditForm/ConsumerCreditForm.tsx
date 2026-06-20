import { Controller } from 'react-hook-form';
import { Flex, Select, Text, TextField } from '@radix-ui/themes';
import Button from '../../../../../components/Button/Button';
import Toast from '../../../../../components/Toast/Toast';
import { useGetCreditServices } from '../../../../../api/credit-services/useGetCreditServices';
import { CreditType } from '../../../../../types/credit-service.types';
import { CreditPurpose, type CreditResponse } from '../../../../../types/credit.types';
import { useConsumerCreditForm } from './useConsumerCreditForm';

interface ConsumerCreditFormProps {
  mode: 'create' | 'edit';
  initialData?: CreditResponse;
  clientId: string;
  onClose: () => void;
}

const PURPOSE_LABELS: Record<CreditPurpose, string> = {
  [CreditPurpose.CarPurchase]: 'Car Purchase',
  [CreditPurpose.HomeRenovation]: 'Home Renovation',
  [CreditPurpose.Education]: 'Education',
  [CreditPurpose.Other]: 'Other',
};

const ConsumerCreditForm = ({ mode, initialData, clientId, onClose }: ConsumerCreditFormProps) => {
  const { register, handleSubmit, errors, control, isEdit, isPending, error, onSubmit } =
    useConsumerCreditForm({ mode, initialData, clientId, onClose });

  const { data: creditServices } = useGetCreditServices();
  const consumerCreditServices = (creditServices ?? []).filter((cs) => cs.type === CreditType.Consumer);

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <Flex direction="column" gap="4" mt="3">
        {error && <Toast message={error.message} type="error" />}

        <Flex direction="column" gap="1">
          <Text as="label" size="2" weight="medium">Credit Service</Text>
          <Controller
            name="creditServiceId"
            control={control}
            render={({ field }) => (
              <Select.Root key={field.value} value={field.value} onValueChange={field.onChange}>
                <Select.Trigger placeholder="Select credit service" />
                <Select.Content>
                  {consumerCreditServices.map((cs) => (
                    <Select.Item key={cs.id} value={cs.id}>{cs.name}</Select.Item>
                  ))}
                </Select.Content>
              </Select.Root>
            )}
          />
          {errors.creditServiceId && <Text size="1" color="red">{errors.creditServiceId.message}</Text>}
        </Flex>

        <Flex direction="column" gap="1">
          <Text as="label" size="2" weight="medium">Amount (BGN)</Text>
          <TextField.Root type="number" step="0.01" placeholder="5000" {...register('amount')} />
          {errors.amount && <Text size="1" color="red">{errors.amount.message}</Text>}
        </Flex>

        <Flex direction="column" gap="1">
          <Text as="label" size="2" weight="medium">Term (months)</Text>
          <TextField.Root type="number" placeholder="24" {...register('termMonths')} />
          {errors.termMonths && <Text size="1" color="red">{errors.termMonths.message}</Text>}
        </Flex>

        <Flex direction="column" gap="1">
          <Text as="label" size="2" weight="medium">Purpose</Text>
          <Controller
            name="purpose"
            control={control}
            render={({ field }) => (
              <Select.Root key={field.value} value={field.value} onValueChange={field.onChange}>
                <Select.Trigger placeholder="Select purpose" />
                <Select.Content>
                  {Object.values(CreditPurpose).map((purpose) => (
                    <Select.Item key={purpose} value={purpose}>{PURPOSE_LABELS[purpose]}</Select.Item>
                  ))}
                </Select.Content>
              </Select.Root>
            )}
          />
          {errors.purpose && <Text size="1" color="red">{errors.purpose.message}</Text>}
        </Flex>

        <Flex gap="3" justify="end" mt="2">
          <Button type="button" variant="soft" color="gray" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" loading={isPending} disabled={isPending}>
            {isEdit ? 'Save' : 'Grant'}
          </Button>
        </Flex>
      </Flex>
    </form>
  );
};

export default ConsumerCreditForm;
