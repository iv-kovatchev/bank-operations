import { useState } from 'react';
import { useGetClientCredits } from '../../../../../api/credits/useGetClientCredits';
import { useGetCreditServices } from '../../../../../api/credit-services/useGetCreditServices';
import type { CreditResponse } from '../../../../../types/credit.types';

type ModalMode = 'create-consumer' | 'create-mortgage' | 'edit-consumer' | 'edit-mortgage' | null;

interface RepaymentPlanModalState {
  open: boolean;
  creditId: string | null;
}

const INITIAL_REPAYMENT_PLAN_MODAL: RepaymentPlanModalState = { open: false, creditId: null };

export const useCreditsSection = (clientId: string) => {
  const { data: credits, isLoading } = useGetClientCredits(clientId);
  const { data: creditServices } = useGetCreditServices();

  const [modalMode, setModalMode] = useState<ModalMode>(null);
  const [selectedCredit, setSelectedCredit] = useState<CreditResponse | null>(null);
  const [repaymentPlanModal, setRepaymentPlanModal] = useState<RepaymentPlanModalState>(INITIAL_REPAYMENT_PLAN_MODAL);

  const handleGrantConsumerClick = () => setModalMode('create-consumer');
  const handleGrantMortgageClick = () => setModalMode('create-mortgage');

  const handleEditClick = (credit: CreditResponse) => {
    setSelectedCredit(credit);
    setModalMode(credit.creditType === 'Consumer' ? 'edit-consumer' : 'edit-mortgage');
  };

  const handleCloseModal = () => {
    setModalMode(null);
    setSelectedCredit(null);
  };

  const handleViewRepaymentPlan = (creditId: string) =>
    setRepaymentPlanModal({ open: true, creditId });

  const handleCloseRepaymentPlan = () => setRepaymentPlanModal(INITIAL_REPAYMENT_PLAN_MODAL);

  return {
    credits,
    isLoading,
    creditServices,
    modalMode,
    selectedCredit,
    repaymentPlanModal,
    handleGrantConsumerClick,
    handleGrantMortgageClick,
    handleEditClick,
    handleCloseModal,
    handleViewRepaymentPlan,
    handleCloseRepaymentPlan,
  };
};
