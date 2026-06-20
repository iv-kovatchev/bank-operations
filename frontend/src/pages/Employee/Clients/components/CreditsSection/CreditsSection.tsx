import { Flex, Heading, Table, Text } from '@radix-ui/themes';
import { CreditStatus } from '../../../../../types/credit.types';
import FormModal from '../../../../../components/FormModal/FormModal';
import Button from '../../../../../components/Button/Button';
import Badge from '../../../../../components/Badge/Badge';
import LoadingSpinner from '../../../../../components/LoadingSpinner/LoadingSpinner';
import ConsumerCreditForm from '../ConsumerCreditForm/ConsumerCreditForm';
import MortgageCreditForm from '../MortgageCreditForm/MortgageCreditForm';
import { useCreditsSection } from './useCreditsSection';

interface CreditsSectionProps {
  clientId: string;
  role: string;
}

const STATUS_COLORS: Record<string, 'green' | 'blue' | 'red' | 'gray'> = {
  [CreditStatus.Active]: 'green',
  [CreditStatus.PaidOff]: 'blue',
  [CreditStatus.Defaulted]: 'red',
};

const CreditsSection = ({ clientId, role }: CreditsSectionProps) => {
  const {
    credits,
    isLoading,
    modalMode,
    selectedCredit,
    repaymentPlanModal,
    handleGrantConsumerClick,
    handleGrantMortgageClick,
    handleEditClick,
    handleCloseModal,
    handleViewRepaymentPlan,
    handleCloseRepaymentPlan,
  } = useCreditsSection(clientId);

  return (
    <>
      <Flex justify="between" align="center" mb="3">
        <Heading size="4">Credits</Heading>
        {role !== 'Client' && (
          <Flex gap="2">
            <Button variant="soft" onClick={handleGrantConsumerClick}>
              Grant Consumer Credit
            </Button>
            <Button variant="soft" onClick={handleGrantMortgageClick}>
              Grant Mortgage Credit
            </Button>
          </Flex>
        )}
      </Flex>

      {isLoading ? (
        <LoadingSpinner />
      ) : (
        <Table.Root variant="surface" mb="5">
          <Table.Header>
            <Table.Row>
              <Table.ColumnHeaderCell>Type</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Amount</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Term</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Status</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Created At</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Actions</Table.ColumnHeaderCell>
            </Table.Row>
          </Table.Header>
          <Table.Body>
            {!credits || credits.length === 0 ? (
              <Table.Row>
                <Table.Cell colSpan={6}>
                  <Flex justify="center" py="4">
                    <Text color="gray">No credits</Text>
                  </Flex>
                </Table.Cell>
              </Table.Row>
            ) : (
              credits.map((credit) => (
                <Table.Row key={credit.id}>
                  <Table.Cell>
                    <Badge color={credit.creditType === 'Consumer' ? 'blue' : 'green'}>
                      {credit.creditType}
                    </Badge>
                  </Table.Cell>
                  <Table.Cell>{credit.amount.toFixed(2)} BGN</Table.Cell>
                  <Table.Cell>{credit.termMonths} months</Table.Cell>
                  <Table.Cell>
                    <Badge color={STATUS_COLORS[credit.status] ?? 'gray'}>
                      {credit.status}
                    </Badge>
                  </Table.Cell>
                  <Table.Cell>{new Date(credit.createdAt).toLocaleDateString()}</Table.Cell>
                  <Table.Cell>
                    <Flex gap="2">
                      {role !== 'Client' && credit.status === CreditStatus.Active && (
                        <Button
                          variant="soft"
                          size="1"
                          onClick={() => handleEditClick(credit)}
                        >
                          Edit
                        </Button>
                      )}
                      <Button
                        variant="soft"
                        color="gray"
                        size="1"
                        onClick={() => handleViewRepaymentPlan(credit.id)}
                      >
                        Repayment Plan
                      </Button>
                    </Flex>
                  </Table.Cell>
                </Table.Row>
              ))
            )}
          </Table.Body>
        </Table.Root>
      )}

      <FormModal
        open={modalMode === 'create-consumer' || modalMode === 'edit-consumer'}
        title={modalMode === 'edit-consumer' ? 'Edit Consumer Credit' : 'Grant Consumer Credit'}
        onClose={handleCloseModal}
      >
        <ConsumerCreditForm
          mode={modalMode === 'edit-consumer' ? 'edit' : 'create'}
          initialData={modalMode === 'edit-consumer' ? (selectedCredit ?? undefined) : undefined}
          clientId={clientId}
          onClose={handleCloseModal}
        />
      </FormModal>

      <FormModal
        open={modalMode === 'create-mortgage' || modalMode === 'edit-mortgage'}
        title={modalMode === 'edit-mortgage' ? 'Edit Mortgage Credit' : 'Grant Mortgage Credit'}
        onClose={handleCloseModal}
      >
        <MortgageCreditForm
          mode={modalMode === 'edit-mortgage' ? 'edit' : 'create'}
          initialData={modalMode === 'edit-mortgage' ? (selectedCredit ?? undefined) : undefined}
          clientId={clientId}
          onClose={handleCloseModal}
        />
      </FormModal>

      <FormModal
        open={repaymentPlanModal.open}
        title="Repayment Plan"
        onClose={handleCloseRepaymentPlan}
      >
        {repaymentPlanModal.creditId && (
          <Text color="gray">Repayment plan view coming soon.</Text>
        )}
      </FormModal>
    </>
  );
};

export default CreditsSection;
