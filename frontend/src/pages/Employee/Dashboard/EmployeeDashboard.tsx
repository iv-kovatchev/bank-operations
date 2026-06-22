import { Box, Card, Flex, Grid, Heading, Text } from '@radix-ui/themes';
import { PersonIcon } from '@radix-ui/react-icons';
import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Legend,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';
import LoadingSpinner from '../../../components/LoadingSpinner/LoadingSpinner';
import { useEmployeeDashboard } from './useEmployeeDashboard';

const ACTIVE_COLOR = '#56B370';
const INACTIVE_COLOR = '#6c6d78';

const EmployeeDashboard = () => {
  const { stats, isLoading } = useEmployeeDashboard();

  if (isLoading || !stats) return <LoadingSpinner />;

  const clientsData = [
    { name: 'Active', value: stats.activeClients },
    { name: 'Inactive', value: stats.totalClients - stats.activeClients },
  ];

  const accountsData = [
    { name: 'Accounts', active: stats.activeBankAccounts, total: stats.totalBankAccounts },
  ];

  return (
    <Box p="6">
      <Heading size="6" mb="5">My Dashboard</Heading>

      <Grid columns="4" gap="4" mb="5">
        <Card>
          <Flex direction="column" gap="1" p="2">
            <Flex align="center" gap="2">
              <PersonIcon />
              <Text size="2" color="gray">My Clients</Text>
            </Flex>
            <Heading size="5">{stats.totalClients}</Heading>
          </Flex>
        </Card>
        <Card>
          <Flex direction="column" gap="1" p="2">
            <Text size="2" color="gray">Active Accounts</Text>
            <Heading size="5">{stats.activeBankAccounts}</Heading>
          </Flex>
        </Card>
        <Card>
          <Flex direction="column" gap="1" p="2">
            <Text size="2" color="gray">Active Credits</Text>
            <Heading size="5">{stats.activeCredits}</Heading>
          </Flex>
        </Card>
        <Card>
          <Flex direction="column" gap="1" p="2">
            <Text size="2" color="gray">Total Credit Amount</Text>
            <Heading size="5">{stats.totalCreditAmount.toFixed(2)} EUR</Heading>
          </Flex>
        </Card>
      </Grid>

      <Grid columns="2" gap="4">
        <Card>
          <Flex direction="column" gap="3" p="2">
            <Heading size="4">My Clients</Heading>
            <ResponsiveContainer width="100%" height={300}>
              <PieChart>
                <Pie data={clientsData} dataKey="value" nameKey="name" cx="50%" cy="50%" outerRadius={100} label>
                  {clientsData.map((entry) => (
                    <Cell key={entry.name} fill={entry.name === 'Active' ? ACTIVE_COLOR : INACTIVE_COLOR} />
                  ))}
                </Pie>
                <Tooltip />
                <Legend />
              </PieChart>
            </ResponsiveContainer>
          </Flex>
        </Card>

        <Card>
          <Flex direction="column" gap="3" p="2">
            <Heading size="4">Accounts Overview</Heading>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={accountsData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="name" />
                <YAxis />
                <Tooltip />
                <Legend />
                <Bar dataKey="active" name="Active" fill={ACTIVE_COLOR} />
                <Bar dataKey="total" name="Total" fill={INACTIVE_COLOR} />
              </BarChart>
            </ResponsiveContainer>
          </Flex>
        </Card>
      </Grid>
    </Box>
  );
};

export default EmployeeDashboard;
