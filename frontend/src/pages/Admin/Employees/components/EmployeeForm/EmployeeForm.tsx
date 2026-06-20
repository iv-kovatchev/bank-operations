import { Flex, Text, TextField } from '@radix-ui/themes';
import Button from '../../../../../components/Button/Button';
import Toast from '../../../../../components/Toast/Toast';
import { useEmployeeForm } from './useEmployeeForm';

interface EmployeeFormProps {
  onClose: () => void;
}

const EmployeeForm = ({ onClose }: EmployeeFormProps) => {
  const { register, handleSubmit, errors, isPending, error, onSubmit } = useEmployeeForm({ onClose });

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <Flex direction="column" gap="4" mt="3">
        {error && <Toast message={error.message} type="error" />}

        <Flex direction="column" gap="1">
          <Text as="label" size="2" weight="medium">First Name</Text>
          <TextField.Root placeholder="Ivan" {...register('firstName')} />
          {errors.firstName && <Text size="1" color="red">{errors.firstName.message}</Text>}
        </Flex>

        <Flex direction="column" gap="1">
          <Text as="label" size="2" weight="medium">Last Name</Text>
          <TextField.Root placeholder="Ivanov" {...register('lastName')} />
          {errors.lastName && <Text size="1" color="red">{errors.lastName.message}</Text>}
        </Flex>

        <Flex direction="column" gap="1">
          <Text as="label" size="2" weight="medium">Email</Text>
          <TextField.Root type="email" placeholder="ivan@example.com" {...register('email')} />
          {errors.email && <Text size="1" color="red">{errors.email.message}</Text>}
        </Flex>

        <Flex gap="3" justify="end" mt="2">
          <Button type="button" variant="soft" color="gray" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" loading={isPending} disabled={isPending}>
            Create
          </Button>
        </Flex>
      </Flex>
    </form>
  );
};

export default EmployeeForm;
