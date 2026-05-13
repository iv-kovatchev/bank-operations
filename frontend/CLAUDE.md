# CLAUDE.md — Frontend

## Stack
- React 18+ + TypeScript (strict mode)
- Chakra UI — all UI components
- Axios — HTTP client with JWT interceptors
- React Query — server state management
- React Hook Form + Zod — forms and validation
- React Router — routing

## Project Structure
```
src/
├── components/       → Reusable UI components
├── pages/            → Application pages
├── services/         → API calls (axios)
├── store/            → Client state (if needed)
├── types/            → TypeScript interfaces
├── hooks/            → Custom hooks
└── utils/            → Helper functions
```

## Key Rules
- Never use `any` — always type everything
- All API calls go through `services/` — never call axios directly in components
- All forms use React Hook Form + Zod validation
- All UI components use Chakra UI — no custom CSS unless absolutely necessary
- Protected routes — redirect to login if no valid JWT token
- Axios interceptor automatically attaches JWT token to every request
- On 401 response — refresh token, then retry request

## Naming
- Components: `PascalCase.tsx`
- Hooks: `useHookName.ts`
- Services: `clientService.ts`
- Types: `Client.ts`, `Credit.ts`

## Auth Flow
1. Login → receive Access Token (15min) + Refresh Token (HttpOnly cookie)
2. Axios interceptor attaches Access Token to every request
3. On 401 → call refresh endpoint → get new Access Token → retry
4. On refresh fail → redirect to login