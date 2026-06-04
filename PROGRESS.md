# Bank Operations System — [PROGRESS.md](http://PROGRESS.md)

> Update this file after every merge into `develop`. Cursor updates it automatically after each completed task.

---

## ✅ Completed

### Architecture & Planning

- Finalized database schema (TPT inheritance for Clients and Credits)
- Defined roles: Admin, Employee, Client
- Defined all functional requirements
- Decided on single-project backend structure (no layered solution)
- Decided on generic interfaces only — no generic implementations
- Created ER diagram (draw.io) and DB schema (dbdiagram.io)

### Documentation

- `PROJECT.md` created
- `PROGRESS.md` created
- `CONVENTIONS.md` created
- `README.md` created
- `.gitignore` created
- `.cursor/rules` created

### Phase 1 — Foundation

- [x] `feature/backend-setup` — ASP.NET Core Web API (.NET 10) project structure + `/health` endpoint — `2026-05-13`
  - Folder structure: Controllers, Data, DTOs, Entities, Enums, Exceptions, Middleware, Repositories, Services
  - `GlobalExceptionMiddleware` — maps NotFoundException→404, ConflictException→409, ValidationException→400
  - Custom exceptions: `NotFoundException`, `ConflictException`, `ValidationException`
  - `GET /api/health` → `{ "status": "healthy" }`
  - Swagger UI added (Swashbuckle.AspNetCore) — available at `/swagger` in Development only

- [x] `feature/ci-cd` — GitHub Actions pipeline → Azure App Service deploy — `2026-05-14`
  - Workflow file: `.github/workflows/develop_bank-operations-api.yml`
  - Trigger: push to `develop` branch with path filter `backend/**` (+ manual `workflow_dispatch`)
  - Build job: checkout → setup .NET 10 → `dotnet build --configuration Release` → `dotnet publish` → upload artifact
  - Deploy job: download artifact → Azure OIDC login (Federated Identity, no client secret) → `azure/webapps-deploy@v3` to `bank-operations-api` Production slot
  - Azure credentials stored as GitHub secrets (`AZUREAPPSERVICE_CLIENTID_*`, `AZUREAPPSERVICE_TENANTID_*`, `AZUREAPPSERVICE_SUBSCRIPTIONID_*`)

- [x] `feature/database-models` — EF Core entities, DbContext, migrations, Azure SQL — `2026-05-27`
  - All entities created: `Client`, `IndividualClient`, `CorporateClient`, `BankAccount`, `Credit`, `ConsumerCredit`, `MortgageCredit`, `RepaymentPlan`, `RepaymentInstallment`, `ActivityLog`, `OtpCode`, `RefreshToken`
  - `ApplicationDbContext` configured with all EF Fluent API configurations in `Data/Configurations/`
  - TPT inheritance configured for Clients and Credits
  - `decimal(18,2)` precision enforced on all monetary columns
  - Initial migration created and applied; migrate job added to CI/CD pipeline

- [x] `feature/auth` — ASP.NET Identity + JWT + Refresh Token + 2FA (OTP via email) — `2026-05-28`
  - `DataSeeder` — roles (Admin, Employee, Client) + initial admin account seeded on startup
  - `AuthController` — `POST /api/auth/login`, `POST /api/auth/verify-otp`, `POST /api/auth/refresh`, `POST /api/auth/logout`; decorated with `[AllowAnonymous]`; rate-limited (5 req/min)
  - Two-step login: Step 1 validates password → generates + emails OTP; Step 2 validates OTP → issues access token + refresh token
  - `TokenService` — generates JWT access token (15 min) + cryptographic refresh token; stores/revokes refresh tokens via `RefreshTokenRepository`
  - `OtpService` — generates 6-digit OTP via `RandomNumberGenerator`, stores with 5-min expiry, invalidates all previous OTPs for user on new request
  - `EmailService` — sends OTP via SMTP (`System.Net.Mail`); credentials injected via env vars `EMAIL_ADDRESS` / `EMAIL_PASSWORD`
  - `AuthResultDto` — `RefreshToken` field marked `[JsonIgnore]`; refresh token delivered only via HttpOnly cookie, never in response body
  - `NotFoundException` — added string overload for email-based lookups (`NotFoundException("User", email)`)
  - `UnauthorizedException` — new custom exception → 401; added to `GlobalExceptionMiddleware`
  - DI registrations extracted to `Config/ServiceExtensions.cs` and `Config/RepositoryExtensions.cs`

### Phase 2 — Frontend Foundation

- [x] `feature/frontend-setup` — React + TypeScript + Radix UI Themes + React Query + React Router + React Hook Form + Zod — `2026-05-28`
  - Vite scaffold + full `src/` folder structure: `api/`, `components/`, `hooks/`, `pages/`, `services/`, `theme/`, `types/`
  - `src/theme/palette.css` — custom green + gray dark/light palette with P3 wide-gamut support; imported first in `main.tsx`
  - `main.tsx` — providers: `QueryClientProvider` → `Theme` (dark, green, gray) → `BrowserRouter`
  - `src/services/http.ts` — generic fetch wrapper (`get`, `post`, `put`, `del`) with JWT from localStorage, 401 auto-redirect, error parsing matching backend `{ "error": "..." }` format; `credentials: 'include'` on all requests
  - `src/api/auth/` — auth hooks: `useLogin.ts`, `useVerifyOtp.ts`, `useLogout.ts`; each calls `http` directly
  - `src/types/auth.types.ts` — `LoginRequest`, `VerifyOtpRequest`, `AuthResponse`
  - `useLogin` — on OTP required: stores email in `sessionStorage`, navigates to `/verify-otp`
  - `useVerifyOtp` — decodes JWT with `jwt-decode`, redirects to role-based dashboard (`/admin/dashboard`, `/employee/dashboard`, `/client/dashboard`)
  - `useLogout` — clears `localStorage` and redirects to `/login` on both success and error
  - Reusable components: `PageLayout`, `Sidebar`, `PageHeader`, `LoadingSpinner`, `Toast` — each in own folder with `.types.ts` and `.styles.css`
  - Backend: `TokenService.cs` role claim changed from `ClaimTypes.Role` to plain `"role"` for simple JWT decoding on the frontend

- [x] `feature/frontend-auth` — Login page + protected routes + auto token refresh — `2026-05-29`
  - `src/context/auth/` — `authContextDef.ts` (context object + type), `AuthContext.tsx` (provider only), `useAuth.ts`; split into def + provider to satisfy Vite Fast Refresh
  - `src/context/theme/` — `themeContextDef.ts`, `ThemeContext.tsx`, `useTheme.ts`; same pattern; reads/writes `localStorage('theme')`, defaults to `'dark'`
  - Auto token refresh — `useQuery(['token-refresh'])` inside `AuthContextProvider`; calls `POST /api/auth/refresh` directly (not via `http.ts`) every 14 min when authenticated; `localStorage` updated in `queryFn`; on error: hard redirect to `/login`
  - Route guards — `PublicRoutes`, `AdminRoutes`, `EmployeeRoutes`, `ClientRoutes` using `<Outlet />`; each checks `useAuth()` and redirects appropriately
  - Route layouts — `PublicLayout` (Header with `isAuthenticated={false}` + Outlet) and `AuthenticatedLayout` (`PageLayout` + Outlet); both defined once in `src/routes/index.tsx` — no per-page import needed
  - `Header` component — logo + "Bank Operations" text; theme toggle (`MoonIcon`/`SunIcon`); when authenticated: display name (left of avatar) + Radix `Avatar` + `DropdownMenu` (role, Settings, Sign out); business logic in `useHeader.ts`
  - `PageLayout` — sticky `Header` (56px) + column flex; reads `isAuthenticated` from `useAuth()` internally
  - Pages — `LoginPage`, `VerifyOtpPage` (centered card with layered box shadow in light, none in dark); `AdminDashboard`, `EmployeeDashboard`, `ClientDashboard` (placeholders); `NotFound` (404)
  - `src/index.css` — replaced Vite scaffold template with clean global reset (`box-sizing`, zero margin, `#root { height: 100vh }`)
  - `src/vite-env.d.ts` — added for IDE `import.meta.env` support

---

## 🔄 In Progress

- Nothing in progress yet

---

## 📋 Backlog (in order)

### Phase 1 — Foundation

- [x] `feature/backend-setup` — ASP.NET Core Web API (.NET 10) project structure + `/health` endpoint
- [x] `feature/ci-cd` — GitHub Actions pipeline → Azure App Service deploy
- [x] `feature/database-models` — EF Core entities, DbContext, migrations, Azure SQL
- [x] `feature/auth` — ASP.NET Identity + JWT + Refresh Token + 2FA (OTP via email)

### Phase 2 — Frontend Foundation

- [x] `feature/frontend-setup` — React + TypeScript + Radix UI Themes + React Query + React Router + React Hook Form + Zod
- [x] `feature/frontend-auth` — Login page + JWT interceptors + protected routes

### Phase 2.5 — Frontend Deploy

- [x] `feature/frontend-deploy` — Azure Static Web Apps setup + GitHub Actions frontend pipeline — `2026-05-29`
  - Azure Static Web Apps created + GitHub Actions pipeline
  - `staticwebapp.config.json` with navigationFallback
  - CORS configured on backend with `ALLOWED_ORIGINS` env var
  - Refresh token moved from HttpOnly cookie to localStorage (cross-domain fix)
  - Auto-logout fix: `visibilitychange` listener + `refetchOnWindowFocus: true`
  - Email configuration fixed for both local (appsettings.Development.json) and Azure (App Settings)
  - Root redirect: unauthenticated → /login, authenticated → role-based dashboard
  - Login page: disabled button when form invalid, centered title, schema validation
  - Header: username outside dropdown, logo as link to home

### Phase 3 — Core Features (backend + frontend in parallel)

- [x] `feature/clients` (backend) — Clients CRUD (Individual + Corporate) — `2026-05-31`
  (see above for full details)

- [x] `feature/frontend-clients` — Clients CRUD frontend (Individual + Corporate) — `2026-06-02`
  - `src/types/client.types.ts` — `ClientType` enum, `ClientResponse`, `IndividualClientResponse`, `CorporateClientResponse`, `ClientDetailResponse` (union), all Create/Update DTOs
  - 8 API hooks in `src/api/clients/`: `useGetClients`, `useGetClient`, `useCreateIndividualClient`, `useCreateCorporateClient`, `useUpdateIndividualClient`, `useUpdateCorporateClient`, `useDeactivateClient`, `useActivateClient`; all invalidate `['clients']` on success
  - `http.ts` — added `patch` method; `handleResponse` handles 204 No Content
  - `ClientsListPage` — two tables (Individual + Corporate), search/filter per table (name/egn/email, company/eik/email), empty state, responsive (Status+Actions hidden on mobile), Admin-only Activate/Deactivate with `ConfirmModal`
  - `ClientDetailPage` — detail card with type-specific fields, edit via `FormModal`, activate/deactivate with `ConfirmModal`, back navigation aware of admin vs employee path
  - `IndividualClientForm` / `CorporateClientForm` — in `src/pages/Employee/Clients/components/`; each in own folder with co-located `use*Form.ts` hook; React Hook Form + Zod; EGN/EIK disabled in edit mode
  - Reusable components: `FormModal` (generic Dialog wrapper), `ConfirmModal`, `Button` (uppercase/letter-spacing wrapper), `Badge` (outline variant with border-color fix)
  - `Sidebar` — role-aware nav (Employee: Dashboard + Clients, Admin: Dashboard + Clients); `useLocation` for active detection; brand + sign-out removed (moved to Header dropdown)
  - Routes: `/employee/clients`, `/employee/clients/:id`, `/admin/clients`, `/admin/clients/:id` — both Employee and Admin can access the same `ClientsListPage` / `ClientDetailPage`
  - Backend additions: `PATCH /api/clients/:id/activate` and `PATCH /api/clients/:id/deactivate` endpoints added to `ClientsController`
  - DTOs: `IndividualClients/` and `CorporateClients/` with validation and TPT inheritance
  - Repository: `ClientRepository` with `ExistsByEmailAsync`, `ExistsByEGNAsync`, `ExistsByEIKAsync`, `GetByIdWithDetailsAsync`
  - Services: `IndividualClientService`, `CorporateClientService` — separate service per subtype
  - Mapper: `ClientMapper` static class in `Mappers/Clients/` with `ToDto` overloads
  - Controller: `ClientsController` with all CRUD endpoints (Create, GetById, GetAll, Update, Delete)
  - `PasswordGenerator` service — generates 12-char random passwords for new clients
  - Swagger JWT auth configured (Swashbuckle downgraded to 6.9.0 for .NET 10 compatibility)
  - `RoleClaimType` fix — set to full Microsoft URI in `TokenValidationParameters` + `Configure<IdentityOptions>`
  - `IService<T>` renamed to `IService`, moved to `Services/IService.cs`; `IRepository<T>` renamed to `IRepository`, moved to `Repositories/IRepository.cs`
  - Unit tests: `ClientsControllerTests` (7), `IndividualClientServiceTests` (6), `CorporateClientServiceTests` (6), `ClientRepositoryTests` (10) — 29 unit tests total
  - Integration tests: `ClientsIntegrationTests` (12) — full HTTP pipeline with InMemory DB, JWT auth, Moq email
  - Total: 41 tests, all passing
  - Test project: `BankOperations.Tests` with xUnit 2.9.3 + Moq 4.20.72 + Shouldly 4.3.0
  - CI pipeline: `test` job added before `build`; runs on push and PR to `develop`; `build`/`migrate`/`deploy` gate on `github.event_name == 'push'`
  - `createdBy` filter: `GetAllClientsAsync(Guid? createdByUserId)` — Employee sees only own clients, Admin sees all
  - Ownership check in `GetClientByIdAsync`: Employee receives `UnauthorizedException` when accessing another employee's client
  - Ownership check in `UpdateAsync`: `isAdmin` flag passed from controller; Employee can only update clients they created
  - Dev OTP bypass: `OtpService` injects `UserManager`; Employee role always receives OTP `000000` in Development
  - `DataSeeder`: seeds `employee1@bank.com` and `employee2@bank.com` (password: `Employee@123`) in all environments
  - `Program.cs`: `IsEnvironment("Testing")` branch uses InMemory DB; production uses SQL Server with retry-on-failure
  - All service and controller tests updated to reflect new method signatures
  - `FormModal` — generic reusable Dialog wrapper in `src/components/FormModal/FormModal.tsx`; accepts `title`, `open`, `onClose`, `children`; used for all create/edit forms
  - `ConfirmModal` — reusable confirmation dialog in `src/components/ConfirmModal/ConfirmModal.tsx`; accepts `title`, `description`, `confirmLabel`, `confirmColor`, `isLoading`, `onConfirm`, `onCancel`
  - `Button` — reusable wrapper around Radix Button with uppercase text and letter-spacing in `src/components/Button/Button.tsx`
  - `Badge` — reusable wrapper around Radix Badge with outline variant and border-color fix for dark theme in `src/components/Badge/Badge.tsx`
  - Sidebar redesigned — icons (`@radix-ui/react-icons`), uppercase labels, drop-shadow instead of border, responsive (hamburger on mobile)
  - Header responsive — ellipsis on long usernames, hamburger toggle for sidebar on mobile
  - Search/filter above each table — Select dropdown (filter by) + TextField (search term), right-aligned, responsive
  - Empty state in tables — "No clients" centered text when filtered results are empty
  - Activate endpoint added to backend — `PATCH /api/clients/:id/activate` (Admin only), symmetric to deactivate
  - 204 No Content fix in `http.ts` — `handleResponse` returns `undefined` instead of calling `response.json()` on empty responses
  - `queryClient.clear()` on logout — clears React Query cache to prevent stale role/data after logout
  - `staticwebapp.config.json` — added `mimeTypes` for `.js`/`.mjs`/`.wasm` to fix MIME type error on Azure Static Web Apps
  - `cursor: pointer` fix — `--cursor-button` CSS variable overridden in `.radix-themes` to apply pointer cursor on all buttons globally
- [x] `feature/frontend-clients` — Clients CRUD frontend (Individual + Corporate) — `2026-06-02`
- [ ] `feature/bank-accounts` + `feature/frontend-accounts` — Bank Accounts CRUD
- [ ] `feature/credits` + `feature/frontend-credits` — Credits (Consumer + Mortgage) + Repayment Plan generation
- [ ] `feature/installments` — Mark installment as paid + credit status check
- [ ] `feature/activity-log` + `feature/frontend-admin` — Activity Log middleware + Employee management + Admin view

### Phase 4 — Dashboards

- [ ] `feature/dashboards` — Admin Dashboard + Employee Dashboard + Client Dashboard + Recharts charts

---

## 🐛 Known Issues

- None yet

---

## 📌 Notes & Decisions Made During Development

- `2026-05-13` — Used .NET 10 (not .NET 8) — .NET 10 is installed on the dev machine
- `2026-05-13` — Swagger gated behind `IsDevelopment()` — will not be exposed on Azure
- `2026-05-14` — CI/CD uses Azure OIDC Federated Identity (no client secret stored) — credentials managed via GitHub secrets with scoped permissions per job
- `2026-05-15` — Revised roles: three roles (Admin, Employee, Client); Client role has read-only self-service access
- `2026-05-15` — Client registration is a single-step transaction: creates AspNetUsers account (role=Client) + Client/IndividualClient or CorporateClient record + sends welcome email
- `2026-05-15` — Clients table: removed Status (use IsActive from AspNetUsers) and CreatedAt (use CreatedAt from AspNetUsers); ClientId is now PK and FK → AspNetUsers (1:1)
- `2026-05-15` — RepaymentInstallments: removed TotalAmount (= PrincipalPart + InterestPart, derived) and IsPaid (= PaidAt != null, derived)
- `2026-05-28` — DataSeeder runs on every startup via `app.Services.CreateScope()` in `Program.cs`; all seed operations are idempotent (existence-checked before insert)
- `2026-05-28` — Email service uses `System.Net.Mail.SmtpClient` instead of SendGrid; simpler setup, no third-party SDK dependency
- `2026-05-28` — DI registrations extracted from `Program.cs` into `Config/ServiceExtensions.cs` and `Config/RepositoryExtensions.cs` to keep `Program.cs` clean as the project grows
- `2026-05-28` — Refresh token never appears in the response body (`[JsonIgnore]`); delivered exclusively via HttpOnly cookie set in `AuthController`
- `2026-05-28` — OTP is always invalidated before generating a new one (`InvalidateAllForUserAsync`) — prevents replay of an old code if user requests a second OTP
- `2026-05-28` — Frontend uses `@radix-ui/themes` instead of Chakra UI; no Axios — native `fetch` wrapped in `src/services/http.ts`; stack: Vite + React 19 + TypeScript + Radix UI + React Query + React Router + React Hook Form + Zod
- `2026-05-28` — JWT role claim changed from `ClaimTypes.Role` (long Microsoft URI) to plain `"role"` in `TokenService.cs`; `TokenValidationParameters.RoleClaimType = "role"` must be set in `Program.cs` for `[Authorize(Roles)]` to work
- `2026-05-28` — Frontend coding rules: always arrow functions; never inline `style={{}}`; styling via Radix UI props or `.styles.css` co-located files
- `2026-05-28` — Auth hooks call `http` service directly — no intermediate `authApi` abstraction layer; each hook in its own file under `src/api/auth/`
- `2026-05-28` — Shared TypeScript types live in `src/types/` (e.g. `auth.types.ts`), not co-located with API hook files
- `2026-05-29` — Context files split into `*Def.ts` (context object + type) + `*Context.tsx` (provider component only) to satisfy Vite Fast Refresh rule: files cannot export both components and non-component values
- `2026-05-29` — Auto token refresh implemented via `useQuery` inside `AuthContextProvider` (proactive, every 14 min) instead of reactive 401 retry in `http.ts`; token stored directly in `queryFn` to avoid `setState` in effects
- `2026-05-29` — Route layouts (`PublicLayout`, `AuthenticatedLayout`) defined once in `src/routes/index.tsx` using React Router nested routes with `<Outlet />`; pages never import `PageLayout` directly
- `2026-05-29` — `ThemeContext` manages dark/light appearance; Radix `Theme` component receives `appearance` from `useTheme()` inside `App.tsx`; theme persisted in `localStorage`
- `2026-05-29` — Context folders: `src/context/auth/` and `src/context/theme/`; each contains the def file, provider, and hook

