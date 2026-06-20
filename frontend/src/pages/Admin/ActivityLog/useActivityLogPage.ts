import { useMemo, useState } from 'react';
import { useGetActivityLogs } from '../../../api/activity-logs/useGetActivityLogs';

const ALL = 'All';

export const useActivityLogPage = () => {
  const { data: logs = [], isLoading } = useGetActivityLogs();

  const [userFilter, setUserFilter] = useState(ALL);
  const [actionFilter, setActionFilter] = useState(ALL);
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');

  const userOptions = useMemo(
    () => [ALL, ...Array.from(new Set(logs.map(l => l.userName)))],
    [logs]
  );

  const actionOptions = useMemo(
    () => [ALL, ...Array.from(new Set(logs.map(l => l.action)))],
    [logs]
  );

  const filteredLogs = logs.filter(log => {
    if (userFilter !== ALL && log.userName !== userFilter) return false;
    if (actionFilter !== ALL && log.action !== actionFilter) return false;

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
