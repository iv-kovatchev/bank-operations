import { Controller } from 'react-hook-form';
import { Flex, Select, Text, TextField } from '@radix-ui/themes';
import Button from '../../../../../components/Button/Button';
import Toast from '../../../../../components/Toast/Toast';
import { useGetCreditServices } from '../../../../../api/credit-services/useGetCreditServices';
import { CreditType } from '../../../../../types/credit-service.types';
import { PropertyType, type CreditResponse } from '../../../../../types/credit.types';
import { useMortgageCreditForm } from './useMortgageCreditForm';

interface MortgageCreditFormProps {
  mode: 'create' | 'edit';
  initialData?: CreditResponse;
  clientId: string;
  onClose: () => void;
}

const PROPERTY_TYPE_LABELS: Record<PropertyType, string> = {
  [PropertyType.Apartment]: 'Apartment',
  [PropertyType.House]: 'House',
  [PropertyType.Commercial]: 'Commercial',
};

const MortgageCreditForm = ({ mode, initialData, clientId, onClose }: MortgageCreditFormProps) => {
  const { register, handleSubmit, errors, control, isEdit, isPending, error, onSubmit } =
    useMortgageCreditForm({ mode, initialData, clientId, onClose });

  const { data: creditServices } = useGetCreditServices();
  const mortgageCreditServices = (creditServices ?? []).filter((cs) => cs.type === CreditType.Mortgage);

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
                  {mortgageCreditServices.map((cs) => (
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
          <TextField.Root type="number" step="0.01" placeholder="100000" {...register('amount')} />
          {errors.amount && <Text size="1" color="red">{errors.amount.message}</Text>}
        </Flex>

        <Flex direction="column" gap="1">
          <Text as="label" size="2" weight="medium">Term (months)</Text>
          <TextField.Root type="number" placeholder="240" {...register('termMonths')} />
          {errors.termMonths && <Text size="1" color="red">{errors.termMonths.message}</Text>}
        </Flex>

        <Flex direction="column" gap="1">
          <Text as="label" size="2" weight="medium">Property Address</Text>
          <TextField.Root placeholder="123 Main St, Sofia" {...register('propertyAddress')} />
          {errors.propertyAddress && <Text size="1" color="red">{errors.propertyAddress.message}</Text>}
        </Flex>

        <Flex direction="column" gap="1">
          <Text as="label" size="2" weight="medium">Property Type</Text>
          <Controller
            name="propertyType"
            control={control}
            render={({ field }) => (
              <Select.Root key={field.value} value={field.value} onValueChange={field.onChange}>
                <Select.Trigger placeholder="Select property type" />
                <Select.Content>
                  {Object.values(PropertyType).map((propertyType) => (
                    <Select.Item key={propertyType} value={propertyType}>{PROPERTY_TYPE_LABELS[propertyType]}</Select.Item>
                  ))}
                </Select.Content>
              </Select.Root>
            )}
          />
          {errors.propertyType && <Text size="1" color="red">{errors.propertyType.message}</Text>}
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

export default MortgageCreditForm;
