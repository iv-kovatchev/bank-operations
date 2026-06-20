# Activity Log Frontend

## Overview
Admin-only read-only page listing all activity log entries with client-side filters (user, action, date range). No create/edit/delete — logs are write-only from the backend's perspective and read-only here.

## Location
- `src/types/activity-log.types.ts` — `ActivityLogResponse`
- `src/api/activity-logs/useGetActivityLogs.ts` — single React Query hook
- `src/pages/Admin/ActivityLog/ActivityLogPage.tsx` — render-only component
- `src/pages/Admin/ActivityLog/useActivityLogPage.ts` — all state/filter logic
- `src/pages/Admin/ActivityLog/ActivityLogPage.styles.css`
- Route `/admin/activity-log` under `AdminRoutes` in `src/routes/index.tsx`; Sidebar "Activity Log" item (`ClockIcon`) in `ADMIN_ITEMS`

## How it works

### Types
```ts
export interface ActivityLogResponse {
  id: string;
  userId: string;
  userName: string;
  action: string;
  entityType: string;
  entityId: string;
  timestamp: string;
  details: string | null;
}
```

### Hook — read-only, no mutations
```ts
export const useGetActivityLogs = () => {
  return useQuery({
    queryKey: ['activity-logs'],
    queryFn: () => http.get<ActivityLogResponse[]>('/api/activity-logs'),
  });
};
```
There is no `useCreateActivityLog`/etc. — the only way activity logs are created is server-side via `IActivityLogService.LogAsync`, never from the UI.

### Client-side filter pattern (same as useClientsListPage/useEmployeesListPage)
All logs are fetched once; filtering happens entirely in `useActivityLogPage` against the already-loaded array — no extra API calls per filter change:
```ts
const ALL_USERS = 'All Users';
const ALL_ACTIONS = 'All Actions';

const [userFilter, setUserFilter] = useState(ALL_USERS);
const [actionFilter, setActionFilter] = useState(ALL_ACTIONS);
const [dateFrom, setDateFrom] = useState('');
const [dateTo, setDateTo] = useState('');

const userOptions = useMemo(
  () => [ALL_USERS, ...Array.from(new Set(logs.map(l => l.userName)))],
  [logs]
);
// actionOptions mirrors this for l.action

const filteredLogs = logs.filter(log => {
  if (userFilter !== ALL_USERS && log.userName !== userFilter) return false;
  if (actionFilter !== ALL_ACTIONS && log.action !== actionFilter) return false;

  const timestamp = new Date(log.timestamp);
  if (dateFrom && timestamp < new Date(dateFrom)) return false;
  if (dateTo && timestamp > new Date(`${dateTo}T23:59:59.999`)) return false;

  return true;
});
```
`ALL_USERS`/`ALL_ACTIONS` are used directly as both the Select's sentinel value and its display label — no separate value/label mapping needed, since these strings can't collide with a real `userName` or `action` value. `dateTo` is compared against end-of-day (`T23:59:59.999`) so a log timestamped any time on the selected end date is included, not just exactly midnight.

### Table
Columns: Timestamp, User, Action, Entity Type, Entity Id, Details. `Timestamp` is rendered via `new Date(log.timestamp).toLocaleString()` — never the raw ISO string. Empty state: `"No activity logs"` centered text when `filteredLogs.length === 0`, same pattern as every other list page in the project.

### Date-range inputs — icon-only via webkit CSS, with text labels
Each date input is wrapped with a "From"/"To" `Text` label:
```tsx
<Flex align="center" gap="1">
  <Text size="2" color="gray">From</Text>
  <TextField.Root className="activity-log-date-input" type="date" value={dateFrom} onChange={...} />
</Flex>
```
The native `dd/mm/yyyy` text inside the date input is hidden via webkit pseudo-elements, leaving only the calendar icon clickable:
```css
.activity-log-date-input {
  width: auto;
  min-width: 32px;
}

.activity-log-date-input input[type="date"]::-webkit-datetime-edit,
.activity-log-date-input input[type="date"]::-webkit-datetime-edit-fields-wrapper,
.activity-log-date-input input[type="date"]::-webkit-datetime-edit-text,
.activity-log-date-input input[type="date"]::-webkit-datetime-edit-month-field,
.activity-log-date-input input[type="date"]::-webkit-datetime-edit-day-field,
.activity-log-date-input input[type="date"]::-webkit-datetime-edit-year-field {
  display: none;
}

.activity-log-date-input input[type="date"]::-webkit-calendar-picker-indicator {
  margin: 0;
  cursor: pointer;
}
```
This only hides the **visual rendering** of the field text via CSS pseudo-elements — the underlying `<input type="date">` element, its `value`, and its `change` event are completely unaffected. Clicking the (still visible) calendar icon opens the native picker, and selecting a date still fires `onChange` and updates `dateFrom`/`dateTo` state normally.

### Visual grouping
The User/Action `Select` filters and the From/To date inputs are visually separated into two groups: the date-range `Flex` has `ml="4"` plus a `border-left` divider (`.activity-log-date-range` class) so it reads as a distinct "date range" filter group, not just two more controls in the same row. On mobile (`max-width: 768px`) the divider and margin collapse so the group stacks cleanly.

## Key details
- **Webkit-only CSS** — `::-webkit-datetime-edit*`/`::-webkit-calendar-picker-indicator` are non-standard pseudo-elements supported by Chrome/Edge/Safari (this project's target browser). Firefox does not support them, so in Firefox the date input would show its normal text — acceptable since the project targets Chromium-based browsers.
- Hiding the text via `display: none` on pseudo-elements does **not** disable or break the input — this is purely cosmetic CSS, not a functional change. Don't confuse this with `pointer-events: none` or `visibility: hidden` on the whole input, which would actually break interaction.
- `userOptions`/`actionOptions` are derived via `useMemo` keyed on `logs` — recomputed only when the underlying log list changes, not on every filter keystroke.

## Dependencies
- `@tanstack/react-query` — `useGetActivityLogs`
- `@radix-ui/themes` — `Select`, `Table`, `TextField`, `Flex`, `Text`, `Box`, `Heading`
- `@radix-ui/react-icons` — `ClockIcon` (Sidebar)

## How to extend

### Add a new filter (e.g. by EntityType)
1. Add `entityTypeFilter` state + `ALL_ENTITY_TYPES` sentinel in `useActivityLogPage.ts`, following the same `useMemo` distinct-values pattern as `userOptions`/`actionOptions`
2. Add the check to the `filteredLogs.filter(...)` predicate
3. Add a new `Select.Root` in `ActivityLogPage.tsx`, in the same `.activity-log-filters` row

### Add server-side filtering/pagination (if log volume grows)
Would require a new query-params-aware `GET /api/activity-logs?user=...&action=...&from=...&to=...` backend endpoint and switching `useGetActivityLogs` to accept filter params in its `queryKey`/`queryFn` — a larger change than the current client-side approach, only worth it once the full-table load becomes a real performance problem.
