import { Box, Flex, Table, Text } from '@radix-ui/themes';
import Badge from '../../../../../components/Badge/Badge';
import LoadingSpinner from '../../../../../components/LoadingSpinner/LoadingSpinner';
import { useRepaymentPlanSection } from './useRepaymentPlanSection';

interface RepaymentPlanSectionProps {
  creditId: string;
}

const RepaymentPlanSection = ({ creditId }: RepaymentPlanSectionProps) => {
  const { plan, isLoading } = useRepaymentPlanSection(creditId);

  if (isLoading) return <LoadingSpinner />;

  if (!plan) {
    return (
      <Flex justify="center" py="4">
        <Text color="gray">No repayment plan</Text>
      </Flex>
    );
  }

  return (
    <>
      <Flex direction="column" gap="1" mb="4">
        <Flex justify="between">
          <Text size="2" color="gray">Monthly Installment</Text>
          <Text size="3">{plan.monthlyInstallment.toFixed(2)} BGN</Text>
        </Flex>
        <Flex justify="between">
          <Text size="2" color="gray">Generated At</Text>
          <Text size="3">{new Date(plan.generatedAt).toLocaleDateString()}</Text>
        </Flex>
      </Flex>

      <Box overflowX="auto">
        <Table.Root variant="surface">
          <Table.Header>
            <Table.Row>
              <Table.ColumnHeaderCell>#</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Due Date</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Principal (BGN)</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Interest (BGN)</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Total (BGN)</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Remaining Balance (BGN)</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Status</Table.ColumnHeaderCell>
            </Table.Row>
          </Table.Header>
          <Table.Body>
            {plan.installments.length === 0 ? (
              <Table.Row>
                <Table.Cell colSpan={7}>
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
                </Table.Row>
              ))
            )}
          </Table.Body>
        </Table.Root>
      </Box>
    </>
  );
};

export default RepaymentPlanSection;
