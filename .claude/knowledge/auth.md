# Auth Feature

## Overview
Two-step login with OTP email verification, JWT access tokens, and proactive refresh. Every login requires a valid password AND a 6-digit OTP sent to the user's email (mandatory 2FA). Refresh token is stored in localStorage (not HttpOnly cookie) because frontend and backend are on different domains.

## Location
- `backend/BankOperations/Controllers/AuthController.cs` — four endpoints: login, verify-otp, refresh, logout
- `backend/BankOperations/Services/Auth/AuthService.cs` — login and OTP verification logic
- `backend/BankOperations/Services/Token/TokenService.cs` — JWT + refresh token generation and validation
- `backend/BankOperations/Services/Otp/OtpService.cs` — OTP generation, storage, invalidation
- `backend/BankOperations/Services/Email/EmailService.cs` — SMTP email sending
- `backend/BankOperations/Data/DataSeeder.cs` — seeds roles + admin account on startup
- `backend/BankOperations/Entities/OtpCode.cs` — OTP entity with expiry
- `backend/BankOperations/Entities/RefreshToken.cs` — refresh token entity
- `backend/BankOperations/DTOs/Auth/` — LoginRequestDto, VerifyOtpRequestDto, AuthResultDto, RefreshRequestDto
- `frontend/src/context/auth/AuthContext.tsx` — token state, auto-refresh logic
- `frontend/src/api/auth/useLogin.ts` — login mutation hook
- `frontend/src/api/auth/useVerifyOtp.ts` — OTP verification mutation hook
- `frontend/src/api/auth/useLogout.ts` — logout mutation hook

## How it works

### Step 1 — Login
1. `POST /api/auth/login` with `{ email, password }`
2. `AuthService` validates credentials via `UserManager.CheckPasswordAsync`
3. On success: `OtpService.InvalidateAllForUserAsync` clears old OTPs, then generates a new 6-digit code stored with 5-min expiry
4. `EmailService.SendOtpEmailAsync` sends the code to the user's email
5. Response: `{ requiresOtp: true }` — no tokens issued yet

### Step 2 — Verify OTP
1. `POST /api/auth/verify-otp` with `{ email, otpCode }`
2. `AuthService` loads the latest valid OTP for the user and checks the code + expiry
3. OTP is invalidated immediately after successful check (one-time use)
4. `TokenService.GenerateAccessToken` creates a JWT (15 min, signed with JWT_SECRET)
5. `TokenService.GenerateRefreshToken` creates a cryptographic refresh token (7 days), stored in `RefreshTokens` table
6. Response body: `{ accessToken, refreshToken }` — both returned in JSON (cross-domain requirement)
7. Frontend stores both in `localStorage`

### Token Refresh
1. `POST /api/auth/refresh` with `{ refreshToken }`
2. `TokenService` looks up the token, checks expiry and revocation
3. Old refresh token is revoked, new one issued (token rotation)
4. Response: `{ accessToken, refreshToken }`

### Logout
1. `POST /api/auth/logout` with `{ refreshToken }`
2. `TokenService` revokes the refresh token in the DB
3. Frontend clears `localStorage`

### Frontend Auto-Refresh
- `AuthContext.tsx` runs `useQuery(['token-refresh'])` with `refetchInterval: 14 * 60 * 1000`
- `refetchOnWindowFocus: true` + `refetchOnMount: false`
- `visibilitychange` listener invalidates the query if token expires in < 2 min OR is already expired
- `getStoredToken` returns raw token from localStorage without expiry check — keeps `isAuthenticated: true` during a refresh attempt

## Key details
- **Refresh token in localStorage, not HttpOnly cookie** — frontend (azurestaticapps.net) and backend (azurewebsites.net) are on different domains; browsers block cross-site cookies (SameSite policy)
- **`AuthResultDto.RefreshToken` is NOT `[JsonIgnore]`** — it must appear in the response body so the frontend can store it in localStorage
- **JWT secret config order**: `_configuration["JWT_SECRET"]` first (Azure App Settings flat key), fallback to `_configuration["Jwt:Secret"]` (local appsettings)
- **Email credentials config order**: `_configuration["EMAIL_ADDRESS"]` first (Azure), fallback to `_configuration["Email:FromEmail"]` (local); same pattern for password
- **OTP always invalidated before generating a new one** — prevents replay of an old code if user requests a second OTP
- **Rate limiting**: 5 req/min on all auth endpoints — configured in `Program.cs`
- **DataSeeder is idempotent** — runs on every startup via `app.Services.CreateScope()`; checks existence before insert

## Code snippets

### TokenService — config key order
```csharp
var secret = _configuration["JWT_SECRET"]
    ?? _configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("JWT secret is not configured.");
```

### EmailService — config key order
```csharp
var fromEmail = _configuration["EMAIL_ADDRESS"]
    ?? _configuration["Email:FromEmail"]
    ?? throw new InvalidOperationException("EMAIL_ADDRESS is not configured.");

var password = _configuration["EMAIL_PASSWORD"]
    ?? _configuration["Email:Password"]
    ?? throw new InvalidOperationException("EMAIL_PASSWORD is not configured.");
```

### OTP invalidation before generation
```csharp
await _otpRepository.InvalidateAllForUserAsync(user.Id);
var otp = new OtpCode { UserId = user.Id, Code = code, ExpiresAt = DateTime.UtcNow.AddMinutes(5) };
await _otpRepository.AddAsync(otp);
```

### Frontend auto-refresh (AuthContext.tsx)
```ts
const { data: refreshedToken, error: refreshError } = useQuery({
  queryKey: ['token-refresh'],
  queryFn: fetchRefreshedToken,
  refetchInterval: 14 * 60 * 1000,
  refetchOnWindowFocus: true,
  refetchOnMount: false,
  enabled: isAuthenticated,
  staleTime: Infinity,
  retry: false,
});
```

### visibilitychange — trigger refresh on expired token
```ts
const expiresInMs = exp * 1000 - Date.now();
if (expiresInMs < 2 * 60 * 1000 || expiresInMs < 0) {
  queryClient.invalidateQueries({ queryKey: ['token-refresh'] });
}
```

## Dependencies
- `Microsoft.AspNetCore.Identity` — user management, password hashing
- `System.IdentityModel.Tokens.Jwt` — JWT generation and validation
- `System.Net.Mail.SmtpClient` — SMTP email (no third-party SDK)
- `jwt-decode` (npm) — decoding JWT on the frontend without verification
- `@tanstack/react-query` — auto-refresh via `useQuery`

## How to extend

### Add a new auth endpoint
1. Add method to `IAuthService` and `AuthService`
2. Add DTO in `DTOs/Auth/`
3. Add action in `AuthController` with `[AllowAnonymous]` if pre-auth, or `[Authorize]` if post-auth
4. Register nothing new — `AuthService` is already scoped in `Config/ServiceExtensions.cs`

### Change token lifetime
- Access token: `TokenService.cs` — change `expires: DateTime.UtcNow.AddMinutes(15)`
- Refresh token: `TokenService.cs` — change `ExpiresAt = DateTime.UtcNow.AddDays(7)`
- Frontend interval: `AuthContext.tsx` — change `refetchInterval: 14 * 60 * 1000`
