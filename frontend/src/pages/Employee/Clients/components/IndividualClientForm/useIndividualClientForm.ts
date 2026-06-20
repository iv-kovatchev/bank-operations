import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useCreateIndividualClient } from '../../../../../api/clients/useCreateIndividualClient';
import { useUpdateIndividualClient } from '../../../../../api/clients/useUpdateIndividualClient';
import { individualClientSchema, type IndividualClientFormData } from './individualClientForm.schema';
import type { IndividualClientResponse } from '../../../../../types/client.types';

interface UseIndividualClientFormProps {
  mode: 'create-individual' | 'edit-individual';
  initialData?: IndividualClientResponse;
  onClose: () => void;
}

export const useIndividualClientForm = ({ mode, initialData, onClose }: UseIndividualClientFormProps) => {
  const isEdit = mode === 'edit-individual';

  const { mutate: create, isPending: isCreating, error: createError } = useCreateIndividualClient();
  const { mutate: update, isPending: isUpdating, error: updateError } = useUpdateIndividualClient();

  const isPending = isCreating || isUpdating;
  const error = createError ?? updateError;

  const { register, handleSubmit, formState: { errors }, reset } = useForm<IndividualClientFormData>({
    resolver: zodResolver(individualClientSchema),
    defaultValues: { firstName: '', lastName: '', egn: '', email: '' },
  });

  useEffect(() => {
    if (initialData) {
      reset({
        firstName: initialData.firstName,
        lastName: initialData.lastName,
        egn: initialData.egn,
        email: initialData.email,
      });
    } else {
      reset({ firstName: '', lastName: '', egn: '', email: '' });
    }
  }, [initialData, reset]);

  const onSubmit = (data: IndividualClientFormData) => {
    if (isEdit && initialData) {
      update(
        { id: initialData.id, firstName: data.firstName, lastName: data.lastName, email: data.email },
        { onSuccess: onClose },
      );
    } else {
      create(data, { onSuccess: onClose });
    }
  };

  return { register, handleSubmit, errors, isEdit, isPending, error, onSubmit };
};
