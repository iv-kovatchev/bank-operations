import { Box, Card, Flex, Grid, Heading, Text } from '@radix-ui/themes';
import { ArrowLeftIcon } from '@radix-ui/react-icons';
import { useClientDetailPage } from './useClientDetailPage';
import ConfirmModal from '../../../../components/ConfirmModal/ConfirmModal';
import FormModal from '../../../../components/FormModal/FormModal';
import LoadingSpinner from '../../../../components/LoadingSpinner/LoadingSpinner';
import Button from '../../../../components/Button/Button';
import Badge from '../../../../components/Badge/Badge';
import IndividualClientForm from '../components/IndividualClientForm/IndividualClientForm';
import CorporateClientForm from '../components/CorporateClientForm/CorporateClientForm';
import AccountsSection from '../components/AccountsSection/AccountsSection';
import { ClientType } from '../../../../types/client.types';
import type { IndividualClientResponse, CorporateClientResponse } from '../../../../types/client.types';

const ClientDetailPage = () => {
  const {
    client,
    isLoading,
    role,
    isEditModalOpen,
    editModalMode,
    handleBack,
    handleOpenEdit,
    handleCloseEdit,
    confirmModal,
    isPendingActivate,
    isPendingDeactivate,
    handleDeactivateClick,
    handleActivateClick,
    handleConfirm,
    handleCancelConfirm,
  } = useClientDetailPage();

  if (isLoading) return <LoadingSpinner />;

  if (!client) return (
    <Box p="6">
      <Text>Client not found</Text>
    </Box>
  );

  const isIndividual = client.type === ClientType.Individual;
  const individual = isIndividual ? client as IndividualClientResponse : null;
  const corporate = !isIndividual ? client as CorporateClientResponse : null;

  const clientName = isIndividual
    ? `${individual!.firstName} ${individual!.lastName}`
    : corporate!.companyName;

  return (
    <Box p="6">
      <Flex justify="between" align="center" mb="5">
        <Flex align="center" gap="3">
          <Button variant="ghost" onClick={handleBack}>
            <Flex align="center" gap="1"><ArrowLeftIcon />Back</Flex>
          </Button>
          <Heading size="6">{clientName}</Heading>
        </Flex>
        <Flex gap="3">
          <Button variant="soft" onClick={handleOpenEdit}>Edit</Button>
          {role === 'Admin' && client.isActive && (
            <Button variant="soft" color="red" onClick={handleDeactivateClick}>
              Deactivate
            </Button>
          )}
          {role === 'Admin' && !client.isActive && (
            <Button variant="soft" color="green" onClick={handleActivateClick}>
              Activate
            </Button>
          )}
        </Flex>
      </Flex>

      <Card mb="5">
        <Grid columns="2" gap="5" p="2">
          {isIndividual && individual ? (
            <>
              <Flex direction="column" gap="1">
                <Text size="2" color="gray">First Name</Text>
                <Text size="3">{individual.firstName}</Text>
              </Flex>
              <Flex direction="column" gap="1">
                <Text size="2" color="gray">Last Name</Text>
                <Text size="3">{individual.lastName}</Text>
              </Flex>
              <Flex direction="column" gap="1">
                <Text size="2" color="gray">EGN</Text>
                <Text size="3">{individual.egn}</Text>
              </Flex>
            </>
          ) : corporate ? (
            <>
              <Flex direction="column" gap="1">
                <Text size="2" color="gray">Company Name</Text>
                <Text size="3">{corporate.companyName}</Text>
              </Flex>
              <Flex direction="column" gap="1">
                <Text size="2" color="gray">EIK</Text>
                <Text size="3">{corporate.eik}</Text>
              </Flex>
              <Flex direction="column" gap="1">
                <Text size="2" color="gray">Representative First Name</Text>
                <Text size="3">{corporate.representativeFirstName}</Text>
              </Flex>
              <Flex direction="column" gap="1">
                <Text size="2" color="gray">Representative Last Name</Text>
                <Text size="3">{corporate.representativeLastName}</Text>
              </Flex>
            </>
          ) : null}

          <Flex direction="column" gap="1">
            <Text size="2" color="gray">Email</Text>
            <Text size="3">{client.email}</Text>
          </Flex>
          <Flex direction="column" gap="1">
            <Text size="2" color="gray">Status</Text>
            <Badge color={client.isActive ? 'green' : 'red'}>{client.isActive ? 'Active' : 'Inactive'}</Badge>
          </Flex>
          <Flex direction="column" gap="1">
            <Text size="2" color="gray">Created At</Text>
            <Text size="3">{new Date(client.createdAt).toLocaleDateString('bg-BG')}</Text>
          </Flex>
        </Grid>
      </Card>

      <AccountsSection clientId={client.id} role={role ?? ''} />

      <Heading size="4" mb="2" mt="5">Credits</Heading>
      <Text color="gray">Coming soon...</Text>

      <FormModal
        open={isEditModalOpen && editModalMode === 'edit-individual'}
        title="Edit Individual Client"
        onClose={handleCloseEdit}
      >
        <IndividualClientForm mode="edit-individual" initialData={individual ?? undefined} onClose={handleCloseEdit} />
      </FormModal>
      <FormModal
        open={isEditModalOpen && editModalMode === 'edit-corporate'}
        title="Edit Corporate Client"
        onClose={handleCloseEdit}
      >
        <CorporateClientForm mode="edit-corporate" initialData={corporate ?? undefined} onClose={handleCloseEdit} />
      </FormModal>

      <ConfirmModal
        open={confirmModal.open}
        title={confirmModal.action === 'activate' ? 'Activate Client' : 'Deactivate Client'}
        description={`Are you sure you want to ${confirmModal.action === 'activate' ? 'activate' : 'deactivate'} this client?`}
        confirmLabel={confirmModal.action === 'activate' ? 'Activate' : 'Deactivate'}
        confirmColor={confirmModal.action === 'activate' ? 'green' : 'red'}
        isLoading={isPendingActivate || isPendingDeactivate}
        onConfirm={handleConfirm}
        onCancel={handleCancelConfirm}
      />
    </Box>
  );
};

export default ClientDetailPage;
