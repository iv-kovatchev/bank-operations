import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../context/auth/useAuth';

const ClientRoutes = () => {
  const { isAuthenticated, role } = useAuth();
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (role !== 'Client') return <Navigate to="/login" replace />;
  return <Outlet />;
};

export default ClientRoutes;
