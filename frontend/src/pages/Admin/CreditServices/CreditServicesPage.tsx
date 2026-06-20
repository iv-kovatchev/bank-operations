import { Box, Flex, Heading, Table, Text } from '@radix-ui/themes';
import LoadingSpinner from '../../../components/LoadingSpinner/LoadingSpinner';
import FormModal from '../../../components/FormModal/FormModal';
import ConfirmModal from '../../../components/ConfirmModal/ConfirmModal';
import Button from '../../../components/Button/Button';
import Badge from '../../../components/Badge/Badge';
import CreditServiceForm from './components/CreditServiceForm/CreditServiceForm';
import { CreditType } from '../../../types/credit-service.types';
import { useCreditServicesPage } from './useCreditServicesPage';

const CreditServicesPage = () => {
  const {
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
  } = useCreditServicesPage();

  if (isLoading) return <LoadingSpinner />;

  return (
    <Box p="6">
      <Flex justify="between" align="center" mb="5">
        <Heading size="6">Credit Services</Heading>
        <Button onClick={handleOpenCreate}>Add Credit Service</Button>
      </Flex>

      <Table.Root variant="surface">
        <Table.Header>
          <Table.Row>
            <Table.ColumnHeaderCell>Name</Table.ColumnHeaderCell>
            <Table.ColumnHeaderCell>Type</Table.ColumnHeaderCell>
            <Table.ColumnHeaderCell>Interest Rate (%)</Table.ColumnHeaderCell>
            <Table.ColumnHeaderCell>Max Amount (EUR)</Table.ColumnHeaderCell>
            <Table.ColumnHeaderCell>Max Term (months)</Table.ColumnHeaderCell>
            <Table.ColumnHeaderCell>Actions</Table.ColumnHeaderCell>
          </Table.Row>
        </Table.Header>
        <Table.Body>
          {!creditServices || creditServices.length === 0 ? (
            <Table.Row>
              <Table.Cell colSpan={6}>
                <Flex justify="center" py="4">
                  <Text color="gray">No credit services</Text>
                </Flex>
              </Table.Cell>
            </Table.Row>
          ) : (
            creditServices.map((creditService) => (
              <Table.Row key={creditService.id}>
                <Table.Cell>{creditService.name}</Table.Cell>
                <Table.Cell>
                  <Badge color={creditService.type === CreditType.Consumer ? 'blue' : 'green'}>
                    {creditService.type}
                  </Badge>
                </Table.Cell>
                <Table.Cell>{creditService.interestRate.toFixed(2)}</Table.Cell>
                <Table.Cell>{creditService.maxAmount.toFixed(2)}</Table.Cell>
                <Table.Cell>{creditService.maxTermMonths}</Table.Cell>
                <Table.Cell>
                  <Flex gap="2">
                    <Button variant="soft" size="1" onClick={() => handleOpenEdit(creditService)}>
                      Edit
                    </Button>
                    <Button variant="soft" color="red" size="1" onClick={() => handleDeleteClick(creditService.id)}>
                      Delete
                    </Button>
                  </Flex>
                </Table.Cell>
              </Table.Row>
            ))
          )}
        </Table.Body>
      </Table.Root>

      <FormModal
        open={modalMode !== null}
        title={modalMode === 'edit' ? 'Edit Credit Service' : 'Add Credit Service'}
        onClose={handleCloseModal}
      >
        <CreditServiceForm
          key={selectedCreditService?.id ?? 'create'}
          mode={modalMode === 'edit' ? 'edit' : 'create'}
          initialData={selectedCreditService}
          onClose={handleCloseModal}
        />
      </FormModal>

      <ConfirmModal
        open={confirmDelete.open}
        title="Delete Credit Service"
        description="Are you sure you want to delete this credit service? This action cannot be undone."
        confirmLabel="Delete"
        confirmColor="red"
        isLoading={isDeleting}
        onConfirm={handleConfirmDelete}
        onCancel={handleCancelDelete}
      />
    </Box>
  );
};

export default CreditServicesPage;
