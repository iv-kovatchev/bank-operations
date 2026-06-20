import { useState } from 'react';
import { useGetRepaymentPlan } from '../../../../../api/credits/useGetRepaymentPlan';
import { useUnpayInstallment } from '../../../../../api/credits/useUnpayInstallment';

interface PayModalState {
  open: boolean;
  installmentId: string | null;
}

const INITIAL_PAY_MODAL: PayModalState = { open: false, installmentId: null };

export const useRepaymentPlanSection = (creditId: string, clientId: string, role: string) => {
  const { data: plan, isLoading } = useGetRepaymentPlan(creditId);
  const { mutate: unpayInstallment, isPending: isUnpaying } = useUnpayInstallment();

  const [payModal, setPayModal] = useState<PayModalState>(INITIAL_PAY_MODAL);

  const handlePayClick = (installmentId: string) => {
    if (role === 'Client') return;
    setPayModal({ open: true, installmentId });
  };

  const handleClosePayModal = () => setPayModal(INITIAL_PAY_MODAL);

  const handleUnpayClick = (installmentId: string) => {
    if (role === 'Client') return;
    unpayInstallment({ creditId, installmentId, clientId });
  };

  return { plan, isLoading, isUnpaying, payModal, handlePayClick, handleClosePayModal, handleUnpayClick };
};
