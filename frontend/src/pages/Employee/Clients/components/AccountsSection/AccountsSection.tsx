import { useState } from 'react';
import { Flex, Heading, Table, Text } from '@radix-ui/themes';
import { useGetClientAccounts } from '../../../../../api/bank-accounts/useGetClientAccounts';
import { useCloseAccount } from '../../../../../api/bank-accounts/useCloseAccount';
import { useDeleteAccount } from '../../../../../api/bank-accounts/useDeleteAccount';
import { AccountStatus } from '../../../../../types/bank-account.types';
import FormModal from '../../../../../components/FormModal/FormModal';
import ConfirmModal from '../../../../../components/ConfirmModal/ConfirmModal';
import Button from '../../../../../components/Button/Button';
import Badge from '../../../../../components/Badge/Badge';
import LoadingSpinner from '../../../../../components/LoadingSpinner/LoadingSpinner';
import OpenAccountForm from '../OpenAccountForm/OpenAccountForm';

interface AccountsSectionProps {
  clientId: string;
  role: string;
}

interface ConfirmState {
  open: boolean;
  accountId: string | null;
}

const INITIAL_CONFIRM: ConfirmState = { open: false, accountId: null };

const AccountsSection = ({ clientId, role }: AccountsSectionProps) => {
  const { data: accounts, isLoading } = useGetClientAccounts(clientId);
  const { mutate: closeAccount, isPending: isClosing } = useCloseAccount();
  const { mutate: deleteAccount, isPending: isDeleting } = useDeleteAccount();

  const [isOpenAccountModalOpen, setIsOpenAccountModalOpen] = useState(false);
  const [confirmClose, setConfirmClose] = useState<ConfirmState>(INITIAL_CONFIRM);
  const [confirmDelete, setConfirmDelete] = useState<ConfirmState>(INITIAL_CONFIRM);

  const handleOpenAccountModal = () => setIsOpenAccountModalOpen(true);
  const handleCloseAccountModal = () => setIsOpenAccountModalOpen(false);

  const handleCloseAccountClick = (id: string) =>
    setConfirmClose({ open: true, accountId: id });

  const handleCancelClose = () => setConfirmClose(INITIAL_CONFIRM);

  const handleConfirmClose = () => {
    if (!confirmClose.accountId) return;
    closeAccount(
      { id: confirmClose.accountId, clientId },
      { onSuccess: () => setConfirmClose(INITIAL_CONFIRM) },
    );
  };

  const handleDeleteClick = (id: string) =>
    setConfirmDelete({ open: true, accountId: id });

  const handleCancelDelete = () => setConfirmDelete(INITIAL_CONFIRM);

  const handleConfirmDelete = () => {
    if (!confirmDelete.accountId) return;
    deleteAccount(
      { id: confirmDelete.accountId, clientId },
      { onSuccess: () => setConfirmDelete(INITIAL_CONFIRM) },
    );
  };

  return (
    <>
      <Flex justify="between" align="center" mb="3">
        <Heading size="4">Accounts</Heading>
        {role !== 'Client' && (
          <Button variant="soft" onClick={handleOpenAccountModal}>
            Open Account
          </Button>
        )}
      </Flex>

      {isLoading ? (
        <LoadingSpinner />
      ) : (
        <Table.Root variant="surface" mb="5">
          <Table.Header>
            <Table.Row>
              <Table.ColumnHeaderCell>IBAN</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Balance</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Status</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Actions</Table.ColumnHeaderCell>
            </Table.Row>
          </Table.Header>
          <Table.Body>
            {!accounts || accounts.length === 0 ? (
              <Table.Row>
                <Table.Cell colSpan={4}>
                  <Flex justify="center" py="4">
                    <Text color="gray">No accounts</Text>
                  </Flex>
                </Table.Cell>
              </Table.Row>
            ) : (
              accounts.map((account) => (
                <Table.Row key={account.id}>
                  <Table.Cell>{account.iban}</Table.Cell>
                  <Table.Cell>{account.balance.toFixed(2)} BGN</Table.Cell>
                  <Table.Cell>
                    <Badge color={account.status === AccountStatus.Active ? 'green' : 'red'}>
                      {account.status}
                    </Badge>
                  </Table.Cell>
                  <Table.Cell>
                    <Flex gap="2">
                      {(role === 'Admin' || role === 'Employee') && account.status === AccountStatus.Active && (
                        <Button
                          variant="soft"
                          color="red"
                          size="1"
                          onClick={() => handleCloseAccountClick(account.id)}
                        >
                          Close
                        </Button>
                      )}
                      {role === 'Admin' && account.status === AccountStatus.Closed && (
                        <Button
                          variant="soft"
                          color="red"
                          size="1"
                          onClick={() => handleDeleteClick(account.id)}
                        >
                          Delete
                        </Button>
                      )}
                    </Flex>
                  </Table.Cell>
                </Table.Row>
              ))
            )}
          </Table.Body>
        </Table.Root>
      )}

      <FormModal
        open={isOpenAccountModalOpen}
        title="Open Account"
        onClose={handleCloseAccountModal}
      >
        <OpenAccountForm clientId={clientId} onClose={handleCloseAccountModal} />
      </FormModal>

      <ConfirmModal
        open={confirmClose.open}
        title="Close Account"
        description="Are you sure you want to close this account? This action cannot be undone."
        confirmLabel="Close Account"
        confirmColor="red"
        isLoading={isClosing}
        onConfirm={handleConfirmClose}
        onCancel={handleCancelClose}
      />

      <ConfirmModal
        open={confirmDelete.open}
        title="Delete Account"
        description="Are you sure you want to permanently delete this account? This action cannot be undone."
        confirmLabel="Delete"
        confirmColor="red"
        isLoading={isDeleting}
        onConfirm={handleConfirmDelete}
        onCancel={handleCancelDelete}
      />
    </>
  );
};

export default AccountsSection;
