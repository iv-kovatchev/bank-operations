import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../context/auth/useAuth';

const AdminRoutes = () => {
  const { isAuthenticated, role } = useAuth();
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (role !== 'Admin') return <Navigate to="/login" replace />;
  return <Outlet />;
};

export default AdminRoutes;
