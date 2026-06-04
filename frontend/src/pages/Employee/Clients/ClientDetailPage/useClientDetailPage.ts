import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useGetClient } from '../../../../api/clients/useGetClient';
import { useDeactivateClient } from '../../../../api/clients/useDeactivateClient';
import { useActivateClient } from '../../../../api/clients/useActivateClient';
import { useAuth } from '../../../../context/auth/useAuth';
import { ClientType } from '../../../../types/client.types';

type ClientFormModalMode = 'edit-individual' | 'edit-corporate';

type ConfirmModal = {
  open: boolean;
  clientId: string | null;
  action: 'activate' | 'deactivate' | null;
};

const INITIAL_CONFIRM: ConfirmModal = { open: false, clientId: null, action: null };

export const useClientDetailPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { role } = useAuth();
  const { data: client, isLoading } = useGetClient(id ?? '');
  const { mutate: deactivate, isPending: isPendingDeactivate } = useDeactivateClient();
  const { mutate: activate, isPending: isPendingActivate } = useActivateClient();

  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [confirmModal, setConfirmModal] = useState<ConfirmModal>(INITIAL_CONFIRM);

  const editModalMode: ClientFormModalMode | null = client
    ? client.type === ClientType.Individual
      ? 'edit-individual'
      : 'edit-corporate'
    : null;

  const clientsListPath = window.location.pathname.includes('/admin/')
    ? '/admin/clients'
    : '/employee/clients';

  const handleBack = () => navigate(clientsListPath);
  const handleOpenEdit = () => setIsEditModalOpen(true);
  const handleCloseEdit = () => setIsEditModalOpen(false);

  const handleDeactivateClick = () => {
    if (id) setConfirmModal({ open: true, clientId: id, action: 'deactivate' });
  };

  const handleActivateClick = () => {
    if (id) setConfirmModal({ open: true, clientId: id, action: 'activate' });
  };

  const handleCancelConfirm = () => setConfirmModal(INITIAL_CONFIRM);

  const handleConfirm = () => {
    if (!confirmModal.clientId) return;
    const { clientId, action } = confirmModal;

    if (action === 'deactivate') {
      deactivate(clientId, {
        onSuccess: () => {
          setConfirmModal(INITIAL_CONFIRM);
          navigate(clientsListPath);
        },
      });
    } else if (action === 'activate') {
      activate(clientId, { onSuccess: () => setConfirmModal(INITIAL_CONFIRM) });
    }
  };

  return {
    client,
    isLoading,
    role,
    isEditModalOpen,
    editModalMode,
    confirmModal,
    isPendingActivate,
    isPendingDeactivate,
    handleBack,
    handleOpenEdit,
    handleCloseEdit,
    handleDeactivateClick,
    handleActivateClick,
    handleConfirm,
    handleCancelConfirm,
  };
};
