import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../context/auth/useAuth';

const roleDashboards: Record<string, string> = {
  Admin: '/admin/dashboard',
  Employee: '/employee/dashboard',
  Client: '/client/dashboard',
};

const PublicRoutes = () => {
  const { isAuthenticated, role } = useAuth();
  if (isAuthenticated) return <Navigate to={roleDashboards[role!] ?? '/login'} replace />;
  return <Outlet />;
};

export default PublicRoutes;
