import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../context/auth/useAuth';

const AuthenticatedRoutes = () => {
  const { isAuthenticated } = useAuth();
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  return <Outlet />;
};

export default AuthenticatedRoutes;
