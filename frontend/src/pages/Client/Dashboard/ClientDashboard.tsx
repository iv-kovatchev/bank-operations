import { Box, Card, Flex, Grid, Heading, Text } from '@radix-ui/themes';
import AccountsSection from '../../Employee/Clients/components/AccountsSection/AccountsSection';
import CreditsSection from '../../Employee/Clients/components/CreditsSection/CreditsSection';
import { useClientDashboard } from './useClientDashboard';

const ClientDashboard = () => {
  const { userId, role, totalBalance, activeCreditsCount, totalCreditAmount } = useClientDashboard();

  return (
    <Box p="6">
      <Heading size="6" mb="5">My Dashboard</Heading>

      <Grid columns="3" gap="4" mb="5">
        <Card>
          <Flex direction="column" gap="1" p="2">
            <Text size="2" color="gray">Total Balance</Text>
            <Heading size="5">{totalBalance.toFixed(2)} EUR</Heading>
          </Flex>
        </Card>
        <Card>
          <Flex direction="column" gap="1" p="2">
            <Text size="2" color="gray">Active Credits</Text>
            <Heading size="5">{activeCreditsCount}</Heading>
          </Flex>
        </Card>
        <Card>
          <Flex direction="column" gap="1" p="2">
            <Text size="2" color="gray">Total Credit Amount</Text>
            <Heading size="5">{totalCreditAmount.toFixed(2)} EUR</Heading>
          </Flex>
        </Card>
      </Grid>

      <AccountsSection clientId={userId} role={role ?? ''} />
      <CreditsSection clientId={userId} role={role ?? ''} />
    </Box>
  );
};

export default ClientDashboard;
