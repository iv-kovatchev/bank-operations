import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useGetClients } from '../../../../api/clients/useGetClients';
import { useDeactivateClient } from '../../../../api/clients/useDeactivateClient';
import { useActivateClient } from '../../../../api/clients/useActivateClient';
import { useAuth } from '../../../../context/auth/useAuth';
import { ClientType } from '../../../../types/client.types';

type ModalMode = null | 'create-individual' | 'create-corporate';

type ConfirmModal = {
  open: boolean;
  clientId: string | null;
  action: 'activate' | 'deactivate' | null;
};

const INITIAL_CONFIRM: ConfirmModal = { open: false, clientId: null, action: null };

export const useClientsListPage = () => {
  const navigate = useNavigate();
  const { role } = useAuth();
  const { data: clients = [], isLoading } = useGetClients();
  const { mutate: deactivate, isPending: isPendingDeactivate } = useDeactivateClient();
  const { mutate: activate, isPending: isPendingActivate } = useActivateClient();

  const individualClients = clients.filter(c => c.type === ClientType.Individual);
  const corporateClients = clients.filter(c => c.type === ClientType.Corporate);

  const [individualSearch, setIndividualSearch] = useState('');
  const [individualFilterBy, setIndividualFilterBy] = useState<'name' | 'egn' | 'email'>('name');
  const [corporateSearch, setCorporateSearch] = useState('');
  const [corporateFilterBy, setCorporateFilterBy] = useState<'company' | 'eik' | 'email'>('company');

  const filteredIndividualClients = individualClients.filter(c => {
    if (!individualSearch) return true;
    const term = individualSearch.toLowerCase();
    if (individualFilterBy === 'name') return `${c.firstName ?? ''} ${c.lastName ?? ''}`.toLowerCase().includes(term);
    if (individualFilterBy === 'egn') return (c.egn ?? '').includes(individualSearch);
    return c.email.toLowerCase().includes(term);
  });

  const filteredCorporateClients = corporateClients.filter(c => {
    if (!corporateSearch) return true;
    const term = corporateSearch.toLowerCase();
    if (corporateFilterBy === 'company') return (c.companyName ?? '').toLowerCase().includes(term);
    if (corporateFilterBy === 'eik') return (c.eik ?? '').includes(corporateSearch);
    return c.email.toLowerCase().includes(term);
  });

  const [modalMode, setModalMode] = useState<ModalMode>(null);
  const [confirmModal, setConfirmModal] = useState<ConfirmModal>(INITIAL_CONFIRM);

  const handleOpenCreateIndividual = () => setModalMode('create-individual');
  const handleOpenCreateCorporate = () => setModalMode('create-corporate');
  const handleCloseModal = () => setModalMode(null);

  const handleDeactivateClick = (id: string) =>
    setConfirmModal({ open: true, clientId: id, action: 'deactivate' });

  const handleActivateClick = (id: string) =>
    setConfirmModal({ open: true, clientId: id, action: 'activate' });

  const handleCancelConfirm = () => setConfirmModal(INITIAL_CONFIRM);

  const handleConfirm = () => {
    if (!confirmModal.clientId) return;
    const { clientId, action } = confirmModal;

    if (action === 'deactivate') {
      deactivate(clientId, { onSuccess: () => setConfirmModal(INITIAL_CONFIRM) });
    } else if (action === 'activate') {
      activate(clientId, { onSuccess: () => setConfirmModal(INITIAL_CONFIRM) });
    }
  };

  const basePath = window.location.pathname.includes('/admin/')
    ? '/admin/clients'
    : '/employee/clients';
  const handleViewDetails = (id: string) => navigate(`${basePath}/${id}`);

  return {
    filteredIndividualClients,
    filteredCorporateClients,
    individualSearch,
    setIndividualSearch,
    individualFilterBy,
    setIndividualFilterBy,
    corporateSearch,
    setCorporateSearch,
    corporateFilterBy,
    setCorporateFilterBy,
    isLoading,
    role,
    modalMode,
    confirmModal,
    isPendingActivate,
    isPendingDeactivate,
    handleOpenCreateIndividual,
    handleOpenCreateCorporate,
    handleCloseModal,
    handleDeactivateClick,
    handleActivateClick,
    handleConfirm,
    handleCancelConfirm,
    handleViewDetails,
  };
};
