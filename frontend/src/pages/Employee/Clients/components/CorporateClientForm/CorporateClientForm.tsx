import { Flex, Text, TextField } from '@radix-ui/themes';
import Button from '../../../../../components/Button/Button';
import Toast from '../../../../../components/Toast/Toast';
import { useCorporateClientForm } from './useCorporateClientForm';
import type { CorporateClientResponse } from '../../../../../types/client.types';

interface CorporateClientFormProps {
  mode: 'create-corporate' | 'edit-corporate';
  initialData?: CorporateClientResponse;
  onClose: () => void;
}

const CorporateClientForm = ({ mode, initialData, onClose }: CorporateClientFormProps) => {
  const { register, handleSubmit, errors, isEdit, isPending, error, onSubmit } =
    useCorporateClientForm({ mode, initialData, onClose });

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <Flex direction="column" gap="4" mt="3">
        {error && <Toast message={error.message} type="error" />}

        <Flex direction="column" gap="1">
          <Text as="label" size="2" weight="medium">Company Name</Text>
          <TextField.Root placeholder="Company Ltd" {...register('companyName')} />
          {errors.companyName && <Text size="1" color="red">{errors.companyName.message}</Text>}
        </Flex>

        <Flex direction="column" gap="1">
          <Text as="label" size="2" weight="medium">EIK</Text>
          <TextField.Root placeholder="000000000" disabled={isEdit} {...register('eik')} />
          {errors.eik && <Text size="1" color="red">{errors.eik.message}</Text>}
        </Flex>

        <Flex direction="column" gap="1">
          <Text as="label" size="2" weight="medium">Representative First Name</Text>
          <TextField.Root placeholder="Ivan" {...register('representativeFirstName')} />
          {errors.representativeFirstName && <Text size="1" color="red">{errors.representativeFirstName.message}</Text>}
        </Flex>

        <Flex direction="column" gap="1">
          <Text as="label" size="2" weight="medium">Representative Last Name</Text>
          <TextField.Root placeholder="Ivanov" {...register('representativeLastName')} />
          {errors.representativeLastName && <Text size="1" color="red">{errors.representativeLastName.message}</Text>}
        </Flex>

        <Flex direction="column" gap="1">
          <Text as="label" size="2" weight="medium">Email</Text>
          <TextField.Root type="email" placeholder="company@example.com" {...register('email')} />
          {errors.email && <Text size="1" color="red">{errors.email.message}</Text>}
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

export default CorporateClientForm;
