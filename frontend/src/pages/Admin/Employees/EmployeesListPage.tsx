import { Box, Flex, Heading, Select, Table, Text, TextField } from '@radix-ui/themes';
import { useEmployeesListPage } from './useEmployeesListPage';
import LoadingSpinner from '../../../components/LoadingSpinner/LoadingSpinner';
import ConfirmModal from '../../../components/ConfirmModal/ConfirmModal';
import FormModal from '../../../components/FormModal/FormModal';
import Button from '../../../components/Button/Button';
import Badge from '../../../components/Badge/Badge';
import EmployeeForm from './components/EmployeeForm/EmployeeForm';
import './EmployeesListPage.styles.css';

const EmployeesListPage = () => {
  const {
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
  } = useEmployeesListPage();

  if (isLoading) return <LoadingSpinner />;

  return (
    <Box p="6">
      <Flex justify="between" align="center" mb="5" className="employees-header">
        <Heading size="6">Employees</Heading>
        <Button onClick={handleOpenCreate}>+ Add Employee</Button>
      </Flex>

      <Box mb="6">
        <Flex className="table-section-header">
          <Heading size="4">All Employees</Heading>
          <Flex className="search-row">
            <Select.Root value={filterBy} onValueChange={(v) => setFilterBy(v as 'name' | 'email')}>
              <Select.Trigger />
              <Select.Content>
                <Select.Item value="name">Name</Select.Item>
                <Select.Item value="email">Email</Select.Item>
              </Select.Content>
            </Select.Root>
            <TextField.Root
              className="search-input"
              placeholder="Search..."
              value={search}
              onChange={e => setSearch(e.target.value)}
            />
          </Flex>
        </Flex>
        <Box className="employees-table-wrapper">
          <Table.Root variant="surface">
            <Table.Header>
              <Table.Row>
                <Table.ColumnHeaderCell>Name</Table.ColumnHeaderCell>
                <Table.ColumnHeaderCell>Email</Table.ColumnHeaderCell>
                <Table.ColumnHeaderCell className="col-status">Status</Table.ColumnHeaderCell>
                <Table.ColumnHeaderCell className="col-actions">Actions</Table.ColumnHeaderCell>
              </Table.Row>
            </Table.Header>
            <Table.Body>
              {filteredEmployees.map(employee => (
                <Table.Row key={employee.id} className="employees-table-row">
                  <Table.Cell>{employee.firstName} {employee.lastName}</Table.Cell>
                  <Table.Cell>{employee.email}</Table.Cell>
                  <Table.Cell className="col-status">
                    <Badge color={employee.isActive ? 'green' : 'red'}>{employee.isActive ? 'Active' : 'Inactive'}</Badge>
                  </Table.Cell>
                  <Table.Cell className="col-actions">
                    <Flex gap="2">
                      {employee.isActive && (
                        <Button size="1" variant="soft" color="red" onClick={() => handleDeactivateClick(employee.id)}>
                          Deactivate
                        </Button>
                      )}
                      {!employee.isActive && (
                        <Button size="1" variant="soft" color="green" onClick={() => handleActivateClick(employee.id)}>
                          Activate
                        </Button>
                      )}
                    </Flex>
                  </Table.Cell>
                </Table.Row>
              ))}
              {filteredEmployees.length === 0 && (
                <Table.Row>
                  <Table.Cell colSpan={4}>
                    <Flex justify="center" py="4">
                      <Text color="gray">No employees</Text>
                    </Flex>
                  </Table.Cell>
                </Table.Row>
              )}
            </Table.Body>
          </Table.Root>
        </Box>
      </Box>

      <FormModal
        open={modalMode === 'create'}
        title="New Employee"
        onClose={handleCloseModal}
      >
        <EmployeeForm onClose={handleCloseModal} />
      </FormModal>

      <ConfirmModal
        open={confirmModal.open}
        title={confirmModal.action === 'activate' ? 'Activate Employee' : 'Deactivate Employee'}
        description={`Are you sure you want to ${confirmModal.action === 'activate' ? 'activate' : 'deactivate'} this employee?`}
        confirmLabel={confirmModal.action === 'activate' ? 'Activate' : 'Deactivate'}
        confirmColor={confirmModal.action === 'activate' ? 'green' : 'red'}
        isLoading={isPendingActivate || isPendingDeactivate}
        onConfirm={handleConfirm}
        onCancel={handleCancelConfirm}
      />
    </Box>
  );
};

export default EmployeesListPage;
