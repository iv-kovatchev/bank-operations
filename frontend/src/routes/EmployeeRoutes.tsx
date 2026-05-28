import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../context/auth/useAuth';

const EmployeeRoutes = () => {
  const { isAuthenticated, role } = useAuth();
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (role !== 'Employee') return <Navigate to="/login" replace />;
  return <Outlet />;
};

export default EmployeeRoutes;
