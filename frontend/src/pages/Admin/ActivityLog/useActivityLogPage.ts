import { useMemo, useState } from 'react';
import { useGetActivityLogs } from '../../../api/activity-logs/useGetActivityLogs';

const ALL_USERS = 'All Users';
const ALL_ACTIONS = 'All Actions';

export const useActivityLogPage = () => {
  const { data: logs = [], isLoading } = useGetActivityLogs();

  const [userFilter, setUserFilter] = useState(ALL_USERS);
  const [actionFilter, setActionFilter] = useState(ALL_ACTIONS);
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');

  const userOptions = useMemo(
    () => [ALL_USERS, ...Array.from(new Set(logs.map(l => l.userName)))],
    [logs]
  );

  const actionOptions = useMemo(
    () => [ALL_ACTIONS, ...Array.from(new Set(logs.map(l => l.action)))],
    [logs]
  );

  const filteredLogs = logs.filter(log => {
    if (userFilter !== ALL_USERS && log.userName !== userFilter) return false;
    if (actionFilter !== ALL_ACTIONS && log.action !== actionFilter) return false;

    const timestamp = new Date(log.timestamp);
    if (dateFrom && timestamp < new Date(dateFrom)) return false;
    if (dateTo && timestamp > new Date(`${dateTo}T23:59:59.999`)) return false;

    return true;
  });

  return {
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
  };
};
