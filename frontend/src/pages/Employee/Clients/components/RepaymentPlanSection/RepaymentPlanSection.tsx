import { Box, Flex, Table, Text } from '@radix-ui/themes';
import Badge from '../../../../../components/Badge/Badge';
import Button from '../../../../../components/Button/Button';
import FormModal from '../../../../../components/FormModal/FormModal';
import LoadingSpinner from '../../../../../components/LoadingSpinner/LoadingSpinner';
import PayInstallmentForm from '../PayInstallmentForm/PayInstallmentForm';
import { useRepaymentPlanSection } from './useRepaymentPlanSection';

interface RepaymentPlanSectionProps {
  creditId: string;
  clientId: string;
  role: string;
}

const RepaymentPlanSection = ({ creditId, clientId, role }: RepaymentPlanSectionProps) => {
  const { plan, isLoading, isUnpaying, payModal, handlePayClick, handleClosePayModal, handleUnpayClick } =
    useRepaymentPlanSection(creditId, clientId, role);

  if (isLoading) return <LoadingSpinner />;

  if (!plan) {
    return (
      <Flex justify="center" py="4">
        <Text color="gray">No repayment plan</Text>
      </Flex>
    );
  }

  const remainingAmount = plan.installments
    .filter((i) => !i.isPaid)
    .reduce((sum, i) => sum + i.totalAmount, 0);

  return (
    <>
      <Flex direction="column" gap="1" mb="4">
        <Flex justify="between">
          <Text size="2" color="gray">Monthly Installment</Text>
          <Text size="3">{plan.monthlyInstallment.toFixed(2)} EUR</Text>
        </Flex>
        <Flex justify="between">
          <Text size="2" color="gray">Generated At</Text>
          <Text size="3">{new Date(plan.generatedAt).toLocaleDateString()}</Text>
        </Flex>
        <Flex justify="between">
          <Text size="2" color="gray">Remaining Amount</Text>
          <Text size="3">{remainingAmount.toFixed(2)} EUR</Text>
        </Flex>
      </Flex>

      <Box overflowX="auto">
        <Table.Root variant="surface">
          <Table.Header>
            <Table.Row>
              <Table.ColumnHeaderCell>#</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Due Date</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Principal (EUR)</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Interest (EUR)</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Total (EUR)</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Remaining Balance (EUR)</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Status</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Actions</Table.ColumnHeaderCell>
            </Table.Row>
          </Table.Header>
          <Table.Body>
            {plan.installments.length === 0 ? (
              <Table.Row>
                <Table.Cell colSpan={8}>
                  <Flex justify="center" py="4">
                    <Text color="gray">No installments</Text>
                  </Flex>
                </Table.Cell>
              </Table.Row>
            ) : (
              plan.installments.map((installment) => (
                <Table.Row key={installment.id}>
                  <Table.Cell>{installment.installmentNumber}</Table.Cell>
                  <Table.Cell>{new Date(installment.dueDate).toLocaleDateString()}</Table.Cell>
                  <Table.Cell>{installment.principalPart.toFixed(2)}</Table.Cell>
                  <Table.Cell>{installment.interestPart.toFixed(2)}</Table.Cell>
                  <Table.Cell>{installment.totalAmount.toFixed(2)}</Table.Cell>
                  <Table.Cell>{installment.remainingBalance.toFixed(2)}</Table.Cell>
                  <Table.Cell>
                    <Badge color={installment.isPaid ? 'green' : 'gray'}>
                      {installment.isPaid ? 'Paid' : 'Pending'}
                    </Badge>
                  </Table.Cell>
                  <Table.Cell>
                    {!installment.isPaid && role !== 'Client' && (
                      <Button
                        variant="soft"
                        color="green"
                        size="1"
                        onClick={() => handlePayClick(installment.id)}
                      >
                        Pay
                      </Button>
                    )}
                    {installment.isPaid && role !== 'Client' && (
                      <Button
                        variant="soft"
                        color="amber"
                        size="1"
                        disabled={isUnpaying}
                        onClick={() => handleUnpayClick(installment.id)}
                      >
                        Unpay
                      </Button>
                    )}
                  </Table.Cell>
                </Table.Row>
              ))
            )}
          </Table.Body>
        </Table.Root>
      </Box>

      <FormModal open={payModal.open} title="Pay Installment" onClose={handleClosePayModal}>
        {payModal.installmentId && (
          <PayInstallmentForm
            creditId={creditId}
            installmentId={payModal.installmentId}
            clientId={clientId}
            onClose={handleClosePayModal}
          />
        )}
      </FormModal>
    </>
  );
};

export default RepaymentPlanSection;
