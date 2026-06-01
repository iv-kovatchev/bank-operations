import { Box, Flex, Heading, Select, Table, Text, TextField } from '@radix-ui/themes';
import { useClientsListPage } from './useClientsListPage';
import LoadingSpinner from '../../../../components/LoadingSpinner/LoadingSpinner';
import ConfirmModal from '../../../../components/ConfirmModal/ConfirmModal';
import FormModal from '../../../../components/FormModal/FormModal';
import Button from '../../../../components/Button/Button';
import Badge from '../../../../components/Badge/Badge';
import IndividualClientForm from '../components/IndividualClientForm/IndividualClientForm';
import CorporateClientForm from '../components/CorporateClientForm/CorporateClientForm';
import './ClientsListPage.styles.css';

const ClientsListPage = () => {
  const {
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
    handleOpenCreateIndividual,
    handleOpenCreateCorporate,
    handleCloseModal,
    confirmModal,
    isPendingActivate,
    isPendingDeactivate,
    handleDeactivateClick,
    handleActivateClick,
    handleConfirm,
    handleCancelConfirm,
    handleViewDetails,
  } = useClientsListPage();

  if (isLoading) return <LoadingSpinner />;

  return (
    <Box p="6">
      <Flex justify="between" align="center" mb="5" className="clients-header">
        <Heading size="6">Clients</Heading>
        <Flex gap="3" className="clients-header-buttons">
          <Button onClick={handleOpenCreateIndividual}>New Individual Client</Button>
          <Button onClick={handleOpenCreateCorporate}>New Corporate Client</Button>
        </Flex>
      </Flex>

      <Box mb="6">
        <Flex className="table-section-header">
          <Heading size="4">Individual Clients</Heading>
          <Flex className="search-row">
            <Select.Root value={individualFilterBy} onValueChange={(v) => setIndividualFilterBy(v as 'name' | 'egn' | 'email')}>
              <Select.Trigger />
              <Select.Content>
                <Select.Item value="name">Name</Select.Item>
                <Select.Item value="egn">EGN</Select.Item>
                <Select.Item value="email">Email</Select.Item>
              </Select.Content>
            </Select.Root>
            <TextField.Root
              className="search-input"
              placeholder="Search..."
              value={individualSearch}
              onChange={e => setIndividualSearch(e.target.value)}
            />
          </Flex>
        </Flex>
        <Box className="clients-table-wrapper">
        <Table.Root variant="surface">
          <Table.Header>
            <Table.Row>
              <Table.ColumnHeaderCell>Name</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>EGN</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Email</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell className="col-status">Status</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell className="col-actions">Actions</Table.ColumnHeaderCell>
            </Table.Row>
          </Table.Header>
          <Table.Body>
            {filteredIndividualClients.map(client => (
              <Table.Row key={client.id} className="clients-table-row">
                <Table.Cell>{client.firstName ?? ''} {client.lastName ?? ''}</Table.Cell>
                <Table.Cell>{client.egn ?? ''}</Table.Cell>
                <Table.Cell>{client.email}</Table.Cell>
                <Table.Cell className="col-status">
                  <Badge color={client.isActive ? 'green' : 'red'}>{client.isActive ? 'Active' : 'Inactive'}</Badge>
                </Table.Cell>
                <Table.Cell className="col-actions">
                  <Flex gap="2">
                    <Button size="1" variant="soft" onClick={() => handleViewDetails(client.id)}>
                      Details
                    </Button>
                    {role === 'Admin' && client.isActive && (
                      <Button size="1" variant="soft" color="red" onClick={(e) => { e.stopPropagation(); handleDeactivateClick(client.id); }}>
                        Deactivate
                      </Button>
                    )}
                    {role === 'Admin' && !client.isActive && (
                      <Button size="1" variant="soft" color="green" onClick={(e) => { e.stopPropagation(); handleActivateClick(client.id); }}>
                        Activate
                      </Button>
                    )}
                  </Flex>
                </Table.Cell>
              </Table.Row>
            ))}
            {filteredIndividualClients.length === 0 && (
              <Table.Row>
                <Table.Cell colSpan={5}>
                  <Flex justify="center" py="4">
                    <Text color="gray">No clients</Text>
                  </Flex>
                </Table.Cell>
              </Table.Row>
            )}
          </Table.Body>
        </Table.Root>
        </Box>
      </Box>

      <Box mb="6">
        <Flex className="table-section-header">
          <Heading size="4">Corporate Clients</Heading>
          <Flex className="search-row">
            <Select.Root value={corporateFilterBy} onValueChange={(v) => setCorporateFilterBy(v as 'company' | 'eik' | 'email')}>
              <Select.Trigger />
              <Select.Content>
                <Select.Item value="company">Company</Select.Item>
                <Select.Item value="eik">EIK</Select.Item>
                <Select.Item value="email">Email</Select.Item>
              </Select.Content>
            </Select.Root>
            <TextField.Root
              className="search-input"
              placeholder="Search..."
              value={corporateSearch}
              onChange={e => setCorporateSearch(e.target.value)}
            />
          </Flex>
        </Flex>
        <Box className="clients-table-wrapper">
        <Table.Root variant="surface">
          <Table.Header>
            <Table.Row>
              <Table.ColumnHeaderCell>Company</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>EIK</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Representative</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Email</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell className="col-status">Status</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell className="col-actions">Actions</Table.ColumnHeaderCell>
            </Table.Row>
          </Table.Header>
          <Table.Body>
            {filteredCorporateClients.map(client => (
              <Table.Row key={client.id} className="clients-table-row">
                <Table.Cell>{client.companyName ?? ''}</Table.Cell>
                <Table.Cell>{client.eik ?? ''}</Table.Cell>
                <Table.Cell>{client.representativeFirstName ?? ''} {client.representativeLastName ?? ''}</Table.Cell>
                <Table.Cell>{client.email}</Table.Cell>
                <Table.Cell className="col-status">
                  <Badge color={client.isActive ? 'green' : 'red'}>{client.isActive ? 'Active' : 'Inactive'}</Badge>
                </Table.Cell>
                <Table.Cell className="col-actions">
                  <Flex gap="2">
                    <Button size="1" variant="soft" onClick={() => handleViewDetails(client.id)}>
                      Details
                    </Button>
                    {role === 'Admin' && client.isActive && (
                      <Button size="1" variant="soft" color="red" onClick={(e) => { e.stopPropagation(); handleDeactivateClick(client.id); }}>
                        Deactivate
                      </Button>
                    )}
                    {role === 'Admin' && !client.isActive && (
                      <Button size="1" variant="soft" color="green" onClick={(e) => { e.stopPropagation(); handleActivateClick(client.id); }}>
                        Activate
                      </Button>
                    )}
                  </Flex>
                </Table.Cell>
              </Table.Row>
            ))}
            {filteredCorporateClients.length === 0 && (
              <Table.Row>
                <Table.Cell colSpan={6}>
                  <Flex justify="center" py="4">
                    <Text color="gray">No clients</Text>
                  </Flex>
                </Table.Cell>
              </Table.Row>
            )}
          </Table.Body>
        </Table.Root>
        </Box>
      </Box>

      <FormModal
        open={modalMode === 'create-individual'}
        title="New Individual Client"
        onClose={handleCloseModal}
      >
        <IndividualClientForm mode="create-individual" onClose={handleCloseModal} />
      </FormModal>
      <FormModal
        open={modalMode === 'create-corporate'}
        title="New Corporate Client"
        onClose={handleCloseModal}
      >
        <CorporateClientForm mode="create-corporate" onClose={handleCloseModal} />
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

export default ClientsListPage;
