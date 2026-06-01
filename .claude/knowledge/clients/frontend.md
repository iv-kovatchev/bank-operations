# Clients Frontend

## Overview
Full CRUD frontend for Individual and Corporate clients. Employees manage their own clients; Admins see and manage all. Both roles use the same pages — the URL prefix (`/employee/` vs `/admin/`) determines the context.

## Location
- `src/types/client.types.ts` — all client TypeScript types and DTOs
- `src/api/clients/` — 8 React Query hooks (get, create, update, activate, deactivate)
- `src/pages/Employee/Clients/List/ClientsListPage.tsx` — two-table list with search/filter
- `src/pages/Employee/Clients/List/useClientsListPage.ts` — all list page logic
- `src/pages/Employee/Clients/List/ClientsListPage.styles.css` — responsive styles
- `src/pages/Employee/Clients/Detail/ClientDetailPage.tsx` — detail view with edit + confirm modals
- `src/pages/Employee/Clients/Detail/useClientDetailPage.ts` — all detail page logic
- `src/pages/Employee/Clients/components/IndividualClientForm/IndividualClientForm.tsx` — form component
- `src/pages/Employee/Clients/components/IndividualClientForm/useIndividualClientForm.ts` — form hook
- `src/pages/Employee/Clients/components/CorporateClientForm/CorporateClientForm.tsx` — form component
- `src/pages/Employee/Clients/components/CorporateClientForm/useCorporateClientForm.ts` — form hook
- `src/pages/Employee/Clients/components/clientForm.schema.ts` — Zod schemas for both forms
- `src/components/FormModal/FormModal.tsx` — generic Dialog wrapper
- `src/components/ConfirmModal/ConfirmModal.tsx` — reusable confirm dialog
- `src/components/Button/Button.tsx` — Radix Button wrapper (uppercase, letter-spacing)
- `src/components/Badge/Badge.tsx` — Radix Badge wrapper (outline variant)
- `src/routes/index.tsx` — `/employee/clients`, `/employee/clients/:id`, `/admin/clients`, `/admin/clients/:id`

## How it works

### Types (`client.types.ts`)
- `ClientType` enum: `Individual | Corporate`
- `ClientResponse` — flat list DTO with optional subtype fields (used in GET /api/clients)
- `IndividualClientResponse` / `CorporateClientResponse` — detail DTOs with `type: ClientType`
- `ClientDetailResponse = IndividualClientResponse | CorporateClientResponse` — union for GET /api/clients/:id
- Both detail interfaces have `type: ClientType` (not a literal type), so TypeScript **cannot auto-narrow** the union. Use explicit type assertions after checking `client.type === ClientType.Individual`.

### API hooks
All hooks are in `src/api/clients/`. Pattern: `useQuery` for reads, `useMutation` with `queryClient.invalidateQueries({ queryKey: ['clients'] })` on success.
- `useGetClients` — `GET /api/clients`, returns `ClientResponse[]`
- `useGetClient(id)` — `GET /api/clients/:id`, returns `ClientDetailResponse`
- `useCreateIndividualClient` — `POST /api/clients/individual`
- `useCreateCorporateClient` — `POST /api/clients/corporate`
- `useUpdateIndividualClient` — `PUT /api/clients/individual/:id`; also invalidates `['clients', id]`
- `useUpdateCorporateClient` — `PUT /api/clients/corporate/:id`; also invalidates `['clients', id]`
- `useDeactivateClient` — `PATCH /api/clients/:id/deactivate`
- `useActivateClient` — `PATCH /api/clients/:id/activate`

### http.ts additions
- `patch<T>(url, body?)` method added — used by activate/deactivate hooks
- `handleResponse` handles 204 No Content: returns `undefined as T` instead of calling `response.json()`

### List page
- Two tables: Individual Clients (5 columns) and Corporate Clients (6 columns)
- Search/filter per table: `individualFilterBy` (`name|egn|email`) + `individualSearch`; same for corporate
- Filtering is derived state in `useClientsListPage` — no extra API calls
- Activate/Deactivate buttons visible only to `role === 'Admin'`; clicking opens a `ConfirmModal` (not the mutation directly)
- `confirmModal` state: `{ open, clientId, action: 'activate'|'deactivate'|null }`; reset in `onSuccess` so spinner stays until server responds
- `basePath` computed from `window.location.pathname.includes('/admin/')` — details navigate to `/admin/clients/:id` or `/employee/clients/:id` accordingly

### Detail page
- Type narrowing: `isIndividual = client.type === ClientType.Individual` → cast to `individual` / `corporate` locals
- Edit: `isEditModalOpen` boolean + `editModalMode` (`'edit-individual'|'edit-corporate'`) derived from `client.type`
- Edit modal: `<FormModal open={isEditModalOpen && editModalMode === 'edit-individual'}>` — two separate `FormModal` instances, one per type
- Same `confirmModal` pattern as list page; deactivate additionally navigates back to list on success
- Back button uses `window.location.pathname.includes('/admin/')` to navigate to the right list URL

### Form components
Each form has its own folder under `src/pages/Employee/Clients/components/`:
```
IndividualClientForm/
├── IndividualClientForm.tsx   ← renders only
└── useIndividualClientForm.ts ← useForm, mutations, reset, submit
```
- `useEffect` + `reset` pre-fills form when `initialData` changes (covers modal re-open with different data)
- EGN / EIK fields are `disabled={isEdit}` — cannot be changed after creation
- In edit mode, only `firstName`, `lastName`, `email` (Individual) or `companyName`, `representativeFirstName`, `representativeLastName`, `email` (Corporate) are sent to the update endpoint

### FormModal
Generic wrapper: `open`, `title`, `onClose`, `children`. `aria-describedby={undefined}` suppresses the Radix missing-description warning.

### Sidebar
Self-contained — reads `role` from `useAuth()`, defines `EMPLOYEE_ITEMS` and `ADMIN_ITEMS` arrays internally. Uses `useLocation()` + `pathname.startsWith(item.path)` for active detection (handles sub-routes like `/employee/clients/123`).

## Key details

### Route reuse: same pages for Employee and Admin
`ClientsListPage` and `ClientDetailPage` are mounted under both `/employee/clients` and `/admin/clients`. The pages detect their context from `window.location.pathname.includes('/admin/')` to build navigation paths. This means:
- Employee navigates from `/employee/clients` → detail → back to `/employee/clients`
- Admin navigates from `/admin/clients` → detail → back to `/admin/clients`

### TypeScript union narrowing
`ClientDetailResponse` is `IndividualClientResponse | CorporateClientResponse`. Both types declare `type: ClientType` (not a literal). TypeScript does **not** automatically narrow based on this. Always use:
```tsx
const isIndividual = client.type === ClientType.Individual;
const individual = isIndividual ? client as IndividualClientResponse : null;
const corporate = !isIndividual ? client as CorporateClientResponse : null;
```

### ConfirmModal pattern
Never call `deactivate(id)` or `activate(id)` directly from button clicks. Always go through `confirmModal` state:
1. Button click → `setConfirmModal({ open: true, clientId: id, action: 'activate' })`
2. User confirms → mutation called with `onSuccess: () => setConfirmModal(INITIAL_CONFIRM)`
3. Modal stays open with spinner until server responds, then closes

### Activate/Deactivate backend endpoints
These are `PATCH` (not `DELETE`). Added to `ClientsController`:
- `PATCH /api/clients/{id}/deactivate`
- `PATCH /api/clients/{id}/activate`

### Search filtering
Search is client-side only — all clients are loaded once and filtered in the hook. No server-side search endpoint. This works for the current scale; would need pagination + server search if client count grows large.

## Code snippets

### Type narrowing in DetailPage
```tsx
const isIndividual = client.type === ClientType.Individual;
const individual = isIndividual ? client as IndividualClientResponse : null;
const corporate = !isIndividual ? client as CorporateClientResponse : null;
```

### ConfirmModal state
```ts
const [confirmModal, setConfirmModal] = useState<ConfirmModal>(INITIAL_CONFIRM);

const handleDeactivateClick = (id: string) =>
  setConfirmModal({ open: true, clientId: id, action: 'deactivate' });

const handleConfirm = () => {
  if (!confirmModal.clientId) return;
  const { clientId, action } = confirmModal;
  if (action === 'deactivate') {
    deactivate(clientId, { onSuccess: () => setConfirmModal(INITIAL_CONFIRM) });
  }
};
```

### Edit modal (two FormModal instances)
```tsx
<FormModal
  open={isEditModalOpen && editModalMode === 'edit-individual'}
  title="Edit Individual Client"
  onClose={handleCloseEdit}
>
  <IndividualClientForm mode="edit-individual" initialData={individual ?? undefined} onClose={handleCloseEdit} />
</FormModal>
```

### Search filtering in hook
```ts
const filteredIndividualClients = individualClients.filter(c => {
  if (!individualSearch) return true;
  const term = individualSearch.toLowerCase();
  if (individualFilterBy === 'name') return `${c.firstName ?? ''} ${c.lastName ?? ''}`.toLowerCase().includes(term);
  if (individualFilterBy === 'egn') return (c.egn ?? '').includes(individualSearch);
  return c.email.toLowerCase().includes(term);
});
```

## Dependencies
- `@tanstack/react-query` — mutations + query invalidation
- `react-hook-form` + `@hookform/resolvers/zod` — form state + validation
- `zod` — schemas in `clientForm.schema.ts`
- `@radix-ui/themes` — Dialog, Table, Select, TextField, Badge, Button, Flex, Grid, Card
- `@radix-ui/react-icons` — `ArrowLeftIcon`, `DashboardIcon`, `PersonIcon`

## How to extend

### Adding a new client type
1. Add interface to `client.types.ts` extending the base fields + add to `ClientDetailResponse` union
2. Add schema to `clientForm.schema.ts`
3. Create `NewTypeClientForm/` folder under `src/pages/Employee/Clients/components/` with component + hook
4. Add API hooks for create/update
5. Add `useCreateNewTypeClient` / `useUpdateNewTypeClient` to the list/detail hooks
6. Add a new `ModalMode` value and a new `<FormModal>` instance in both list and detail pages
7. Add a new column to the table and a new case in the detail card
