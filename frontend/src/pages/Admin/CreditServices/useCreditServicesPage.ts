import { useState } from 'react';
import { useGetCreditServices } from '../../../api/credit-services/useGetCreditServices';
import { useDeleteCreditService } from '../../../api/credit-services/useDeleteCreditService';
import type { CreditServiceResponse } from '../../../types/credit-service.types';

type ModalMode = 'create' | 'edit' | null;

interface ConfirmDeleteState {
  open: boolean;
  creditServiceId: string | null;
}

const INITIAL_CONFIRM_DELETE: ConfirmDeleteState = { open: false, creditServiceId: null };

export const useCreditServicesPage = () => {
  const { data: creditServices, isLoading } = useGetCreditServices();
  const { mutate: deleteCreditService, isPending: isDeleting } = useDeleteCreditService();

  const [modalMode, setModalMode] = useState<ModalMode>(null);
  const [selectedCreditService, setSelectedCreditService] = useState<CreditServiceResponse | undefined>(undefined);
  const [confirmDelete, setConfirmDelete] = useState<ConfirmDeleteState>(INITIAL_CONFIRM_DELETE);

  const handleOpenCreate = () => {
    setSelectedCreditService(undefined);
    setModalMode('create');
  };

  const handleOpenEdit = (creditService: CreditServiceResponse) => {
    setSelectedCreditService(creditService);
    setModalMode('edit');
  };

  const handleCloseModal = () => {
    setModalMode(null);
    setSelectedCreditService(undefined);
  };

  const handleDeleteClick = (creditServiceId: string) => {
    setConfirmDelete({ open: true, creditServiceId });
  };

  const handleCancelDelete = () => {
    setConfirmDelete(INITIAL_CONFIRM_DELETE);
  };

  const handleConfirmDelete = () => {
    if (!confirmDelete.creditServiceId) return;
    deleteCreditService(confirmDelete.creditServiceId, {
      onSuccess: () => setConfirmDelete(INITIAL_CONFIRM_DELETE),
    });
  };

  return {
    creditServices,
    isLoading,
    modalMode,
    selectedCreditService,
    handleOpenCreate,
    handleOpenEdit,
    handleCloseModal,
    confirmDelete,
    isDeleting,
    handleDeleteClick,
    handleCancelDelete,
    handleConfirmDelete,
  };
};
