# New Frontend Feature Command

Follow these steps exactly when creating a new frontend feature. Read @frontend/CLAUDE.md before starting.

## Steps

### 1. Types
- Create TypeScript interface in `src/types/[Feature].ts`
- Mirror the backend `[Feature]ResponseDto` exactly
- Add request types for create/update if needed

### 2. Service
- Create `src/services/[feature]Service.ts`
- Use axios instance from `src/services/axiosInstance.ts` — never import axios directly
- All functions must be async and typed
- Example:
```typescript
export const getClients = async (): Promise<ClientResponse[]> => {
  const { data } = await api.get('/clients');
  return data;
};
```

### 3. React Query hooks (if needed)
- Create `src/hooks/use[Feature].ts`
- Use `useQuery` for fetching, `useMutation` for create/update/delete
- Example:
```typescript
export const useClients = () => {
  return useQuery({
    queryKey: ['clients'],
    queryFn: getClients,
  });
};
```

### 4. Pages
- Create `src/pages/[Feature]/` folder
- `[Feature]ListPage.tsx` — list view with table
- `[Feature]DetailPage.tsx` — detail/edit view (if needed)
- Use Chakra UI components only — no custom CSS unless absolutely necessary

### 5. Components (if needed)
- Create reusable components in `src/components/[Feature]/`
- Forms: always use React Hook Form + Zod validation
- Example schema:
```typescript
const schema = z.object({
  firstName: z.string().min(1, 'Required').max(100),
  egn: z.string().length(10, 'EGN must be 10 digits'),
});
```

### 6. Routing
- Add route in `src/App.tsx` or router config
- Wrap with `<ProtectedRoute>` if authentication required
- Wrap with `<AdminRoute>` if admin only

### 7. After completion
- Update `PROGRESS.md` — mark feature as done
- Suggest next step