import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useCreateCorporateClient } from '../../../../../api/clients/useCreateCorporateClient';
import { useUpdateCorporateClient } from '../../../../../api/clients/useUpdateCorporateClient';
import { corporateClientSchema, type CorporateClientFormData } from '../clientForm.schema';
import type { CorporateClientResponse } from '../../../../../types/client.types';

interface UseCorporateClientFormProps {
  mode: 'create-corporate' | 'edit-corporate';
  initialData?: CorporateClientResponse;
  onClose: () => void;
}

export const useCorporateClientForm = ({ mode, initialData, onClose }: UseCorporateClientFormProps) => {
  const isEdit = mode === 'edit-corporate';

  const { mutate: create, isPending: isCreating, error: createError } = useCreateCorporateClient();
  const { mutate: update, isPending: isUpdating, error: updateError } = useUpdateCorporateClient();

  const isPending = isCreating || isUpdating;
  const error = createError ?? updateError;

  const { register, handleSubmit, formState: { errors }, reset } = useForm<CorporateClientFormData>({
    resolver: zodResolver(corporateClientSchema),
    defaultValues: { companyName: '', eik: '', representativeFirstName: '', representativeLastName: '', email: '' },
  });

  useEffect(() => {
    if (initialData) {
      reset({
        companyName: initialData.companyName,
        eik: initialData.eik,
        representativeFirstName: initialData.representativeFirstName,
        representativeLastName: initialData.representativeLastName,
        email: initialData.email,
      });
    } else {
      reset({ companyName: '', eik: '', representativeFirstName: '', representativeLastName: '', email: '' });
    }
  }, [initialData, reset]);

  const onSubmit = (data: CorporateClientFormData) => {
    if (isEdit && initialData) {
      update(
        {
          id: initialData.id,
          companyName: data.companyName,
          representativeFirstName: data.representativeFirstName,
          representativeLastName: data.representativeLastName,
          email: data.email,
        },
        { onSuccess: onClose },
      );
    } else {
      create(data, { onSuccess: onClose });
    }
  };

  return { register, handleSubmit, errors, isEdit, isPending, error, onSubmit };
};
