import { Box, Flex, Heading, Select, Table, Text, TextField } from '@radix-ui/themes';
import { useActivityLogPage } from './useActivityLogPage';
import LoadingSpinner from '../../../components/LoadingSpinner/LoadingSpinner';
import './ActivityLogPage.styles.css';

const ActivityLogPage = () => {
  const {
    filteredLogs,
    isLoading,
    userFilter,
    setUserFilter,
    actionFilter,
    setActionFilter,
    dateFrom,
    setDateFrom,
    dateTo,
    setDateTo,
    userOptions,
    actionOptions,
  } = useActivityLogPage();

  if (isLoading) return <LoadingSpinner />;

  return (
    <Box p="6">
      <Flex justify="between" align="center" mb="5">
        <Heading size="6">Activity Log</Heading>
      </Flex>

      <Box mb="6">
        <Flex justify="end" mb="3" className="activity-log-filters">
          <Select.Root value={userFilter} onValueChange={setUserFilter}>
            <Select.Trigger />
            <Select.Content>
              {userOptions.map(option => (
                <Select.Item key={option} value={option}>{option}</Select.Item>
              ))}
            </Select.Content>
          </Select.Root>

          <Select.Root value={actionFilter} onValueChange={setActionFilter}>
            <Select.Trigger />
            <Select.Content>
              {actionOptions.map(option => (
                <Select.Item key={option} value={option}>{option}</Select.Item>
              ))}
            </Select.Content>
          </Select.Root>

          <TextField.Root
            className="activity-log-date-input"
            type="date"
            value={dateFrom}
            onChange={e => setDateFrom(e.target.value)}
          />
          <TextField.Root
            className="activity-log-date-input"
            type="date"
            value={dateTo}
            onChange={e => setDateTo(e.target.value)}
          />
        </Flex>

        <Box className="activity-log-table-wrapper">
          <Table.Root variant="surface">
            <Table.Header>
              <Table.Row>
                <Table.ColumnHeaderCell>Timestamp</Table.ColumnHeaderCell>
                <Table.ColumnHeaderCell>User</Table.ColumnHeaderCell>
                <Table.ColumnHeaderCell>Action</Table.ColumnHeaderCell>
                <Table.ColumnHeaderCell>Entity Type</Table.ColumnHeaderCell>
                <Table.ColumnHeaderCell>Entity Id</Table.ColumnHeaderCell>
                <Table.ColumnHeaderCell>Details</Table.ColumnHeaderCell>
              </Table.Row>
            </Table.Header>
            <Table.Body>
              {filteredLogs.map(log => (
                <Table.Row key={log.id}>
                  <Table.Cell>{new Date(log.timestamp).toLocaleString()}</Table.Cell>
                  <Table.Cell>{log.userName}</Table.Cell>
                  <Table.Cell>{log.action}</Table.Cell>
                  <Table.Cell>{log.entityType}</Table.Cell>
                  <Table.Cell>{log.entityId}</Table.Cell>
                  <Table.Cell>{log.details ?? '—'}</Table.Cell>
                </Table.Row>
              ))}
              {filteredLogs.length === 0 && (
                <Table.Row>
                  <Table.Cell colSpan={6}>
                    <Flex justify="center" py="4">
                      <Text color="gray">No activity logs</Text>
                    </Flex>
                  </Table.Cell>
                </Table.Row>
              )}
            </Table.Body>
          </Table.Root>
        </Box>
      </Box>
    </Box>
  );
};

export default ActivityLogPage;
