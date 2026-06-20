import { Controller } from 'react-hook-form';
import { Flex, Select, Text, TextField } from '@radix-ui/themes';
import Button from '../../../../../components/Button/Button';
import Toast from '../../../../../components/Toast/Toast';
import { CreditType, type CreditServiceResponse } from '../../../../../types/credit-service.types';
import { useCreditServiceForm } from './useCreditServiceForm';

interface CreditServiceFormProps {
  mode: 'create' | 'edit';
  initialData?: CreditServiceResponse;
  onClose: () => void;
}

const CreditServiceForm = ({ mode, initialData, onClose }: CreditServiceFormProps) => {
  const { register, handleSubmit, errors, control, isEdit, isPending, error, onSubmit } =
    useCreditServiceForm({ mode, initialData, onClose });

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <Flex direction="column" gap="4" mt="3">
        {error && <Toast message={error.message} type="error" />}

        <Flex direction="column" gap="1">
          <Text as="label" size="2" weight="medium">Name</Text>
          <TextField.Root placeholder="Consumer Standard" {...register('name')} />
          {errors.name && <Text size="1" color="red">{errors.name.message}</Text>}
        </Flex>

        <Flex direction="column" gap="1">
          <Text as="label" size="2" weight="medium">Type</Text>
          <Controller
            name="type"
            control={control}
            render={({ field }) => (
              <Select.Root key={field.value} value={field.value} onValueChange={field.onChange}>
                <Select.Trigger placeholder="Select type" />
                <Select.Content>
                  <Select.Item value={CreditType.Consumer}>Consumer</Select.Item>
                  <Select.Item value={CreditType.Mortgage}>Mortgage</Select.Item>
                </Select.Content>
              </Select.Root>
            )}
          />
          {errors.type && <Text size="1" color="red">{errors.type.message}</Text>}
        </Flex>

        <Flex direction="column" gap="1">
          <Text as="label" size="2" weight="medium">Interest Rate (%)</Text>
          <TextField.Root type="number" step="0.01" placeholder="5.5" {...register('interestRate')} />
          {errors.interestRate && <Text size="1" color="red">{errors.interestRate.message}</Text>}
        </Flex>

        <Flex direction="column" gap="1">
          <Text as="label" size="2" weight="medium">Max Amount (EUR)</Text>
          <TextField.Root type="number" placeholder="10000" {...register('maxAmount')} />
          {errors.maxAmount && <Text size="1" color="red">{errors.maxAmount.message}</Text>}
        </Flex>

        <Flex direction="column" gap="1">
          <Text as="label" size="2" weight="medium">Max Term (months)</Text>
          <TextField.Root type="number" placeholder="36" {...register('maxTermMonths')} />
          {errors.maxTermMonths && <Text size="1" color="red">{errors.maxTermMonths.message}</Text>}
        </Flex>

        <Flex gap="3" justify="end" mt="2">
          <Button type="button" variant="soft" color="gray" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" loading={isPending} disabled={isPending}>
            {isEdit ? 'Save' : 'Create'}
          </Button>
        </Flex>
      </Flex>
    </form>
  );
};

export default CreditServiceForm;
