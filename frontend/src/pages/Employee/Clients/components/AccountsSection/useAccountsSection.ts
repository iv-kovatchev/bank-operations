import { useState } from 'react';
import { useGetClientAccounts } from '../../../../../api/bank-accounts/useGetClientAccounts';
import { useCloseAccount } from '../../../../../api/bank-accounts/useCloseAccount';
import { useDeleteAccount } from '../../../../../api/bank-accounts/useDeleteAccount';
import { useDepositToAccount } from '../../../../../api/bank-accounts/useDepositToAccount';
import { useWithdrawFromAccount } from '../../../../../api/bank-accounts/useWithdrawFromAccount';

interface ConfirmState {
  open: boolean;
  accountId: string | null;
}

interface TransactionState {
  open: boolean;
  accountId: string | null;
  mode: 'deposit' | 'withdraw' | null;
}

const INITIAL_CONFIRM: ConfirmState = { open: false, accountId: null };
const INITIAL_TRANSACTION: TransactionState = { open: false, accountId: null, mode: null };

export const useAccountsSection = (clientId: string) => {
  const { data: accounts, isLoading } = useGetClientAccounts(clientId);
  const { mutate: closeAccount, isPending: isClosing } = useCloseAccount();
  const { mutate: deleteAccount, isPending: isDeleting } = useDeleteAccount();
  const { isPending: isDepositing } = useDepositToAccount();
  const { isPending: isWithdrawing } = useWithdrawFromAccount();

  const [isOpenAccountModalOpen, setIsOpenAccountModalOpen] = useState(false);
  const [confirmClose, setConfirmClose] = useState<ConfirmState>(INITIAL_CONFIRM);
  const [confirmDelete, setConfirmDelete] = useState<ConfirmState>(INITIAL_CONFIRM);
  const [transactionModal, setTransactionModal] = useState<TransactionState>(INITIAL_TRANSACTION);

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

  const handleDepositClick = (id: string) =>
    setTransactionModal({ open: true, accountId: id, mode: 'deposit' });

  const handleWithdrawClick = (id: string) =>
    setTransactionModal({ open: true, accountId: id, mode: 'withdraw' });

  const handleCloseTransaction = () => setTransactionModal(INITIAL_TRANSACTION);

  return {
    accounts,
    isLoading,
    isClosing,
    isDeleting,
    isDepositing,
    isWithdrawing,
    isOpenAccountModalOpen,
    confirmClose,
    confirmDelete,
    transactionModal,
    handleOpenAccountModal,
    handleCloseAccountModal,
    handleCloseAccountClick,
    handleCancelClose,
    handleConfirmClose,
    handleDeleteClick,
    handleCancelDelete,
    handleConfirmDelete,
    handleDepositClick,
    handleWithdrawClick,
    handleCloseTransaction,
  };
};
