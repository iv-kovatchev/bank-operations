import { jwtDecode } from 'jwt-decode';
import { useTheme } from '../../context/theme/useTheme';
import { useAuth } from '../../context/auth/useAuth';
import { useLogout } from '../../api/auth/useLogout';

type JwtPayload = {
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'?: string;
  unique_name?: string;
  name?: string;
  email?: string;
  role: string;
};

type RoleColor = 'red' | 'blue' | 'green' | 'gray';

const roleColors: Record<string, RoleColor> = {
  Admin: 'red',
  Employee: 'blue',
  Client: 'green',
};

const getInitials = (name: string): string => {
  const parts = name.trim().split(' ').filter(Boolean);
  if (parts.length >= 2) return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  return parts[0]?.[0]?.toUpperCase() ?? '?';
};

export const useHeader = (isAuthenticated: boolean) => {
  const { theme, toggleTheme } = useTheme();
  const { accessToken } = useAuth();
  const { mutate: logout, isPending: isLoggingOut } = useLogout();

  let displayName = '';
  let initials = '?';
  let role = '';
  let roleColor: RoleColor = 'gray';

  if (isAuthenticated && accessToken) {
    try {
      const decoded = jwtDecode<JwtPayload>(accessToken);
      displayName = decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ?? decoded.unique_name ?? decoded.name ?? decoded.email ?? '';
      initials = getInitials(displayName);
      role = decoded.role;
      roleColor = roleColors[role] ?? 'gray';
    } catch {
      // token malformed — ignore
    }
  }

  return { theme, toggleTheme, displayName, initials, role, roleColor, logout, isLoggingOut };
};
