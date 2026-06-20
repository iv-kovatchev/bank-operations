import { useState } from 'react';
import { useGetEmployees } from '../../../api/employees/useGetEmployees';
import { useDeactivateEmployee } from '../../../api/employees/useDeactivateEmployee';
import { useActivateEmployee } from '../../../api/employees/useActivateEmployee';

type ModalMode = null | 'create';

type ConfirmModal = {
  open: boolean;
  employeeId: string | null;
  action: 'activate' | 'deactivate' | null;
};

const INITIAL_CONFIRM: ConfirmModal = { open: false, employeeId: null, action: null };

export const useEmployeesListPage = () => {
  const { data: employees = [], isLoading } = useGetEmployees();
  const { mutate: deactivate, isPending: isPendingDeactivate } = useDeactivateEmployee();
  const { mutate: activate, isPending: isPendingActivate } = useActivateEmployee();

  const [search, setSearch] = useState('');
  const [filterBy, setFilterBy] = useState<'name' | 'email'>('name');

  const filteredEmployees = employees.filter(e => {
    if (!search) return true;
    const term = search.toLowerCase();
    if (filterBy === 'name') return `${e.firstName} ${e.lastName}`.toLowerCase().includes(term);
    return e.email.toLowerCase().includes(term);
  });

  const [modalMode, setModalMode] = useState<ModalMode>(null);
  const [confirmModal, setConfirmModal] = useState<ConfirmModal>(INITIAL_CONFIRM);

  const handleOpenCreate = () => setModalMode('create');
  const handleCloseModal = () => setModalMode(null);

  const handleDeactivateClick = (id: string) =>
    setConfirmModal({ open: true, employeeId: id, action: 'deactivate' });

  const handleActivateClick = (id: string) =>
    setConfirmModal({ open: true, employeeId: id, action: 'activate' });

  const handleCancelConfirm = () => setConfirmModal(INITIAL_CONFIRM);

  const handleConfirm = () => {
    if (!confirmModal.employeeId) return;
    const { employeeId, action } = confirmModal;

    if (action === 'deactivate') {
      deactivate(employeeId, { onSuccess: () => setConfirmModal(INITIAL_CONFIRM) });
    } else if (action === 'activate') {
      activate(employeeId, { onSuccess: () => setConfirmModal(INITIAL_CONFIRM) });
    }
  };

  return {
    filteredEmployees,
    search,
    setSearch,
    filterBy,
    setFilterBy,
    isLoading,
    modalMode,
    confirmModal,
    isPendingActivate,
    isPendingDeactivate,
    handleOpenCreate,
    handleCloseModal,
    handleDeactivateClick,
    handleActivateClick,
    handleConfirm,
    handleCancelConfirm,
  };
};
