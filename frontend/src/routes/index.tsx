import { Navigate, Outlet, Route, Routes } from 'react-router-dom';
import { Flex } from '@radix-ui/themes';
import PublicRoutes from './PublicRoutes';
import AdminRoutes from './AdminRoutes';
import EmployeeRoutes from './EmployeeRoutes';
import ClientRoutes from './ClientRoutes';
import AuthenticatedRoutes from './AuthenticatedRoutes';
import PageLayout from '../components/PageLayout/PageLayout';
import Header from '../components/Header/Header';
import LoginPage from '../pages/Login/LoginPage';
import VerifyOtpPage from '../pages/VerifyOtp/VerifyOtpPage';
import AdminDashboard from '../pages/Admin/Dashboard/AdminDashboard';
import EmployeeDashboard from '../pages/Employee/Dashboard/EmployeeDashboard';
import ClientDashboard from '../pages/Client/Dashboard/ClientDashboard';
import NotFound from '../pages/NotFound/NotFound';
import ClientsListPage from '../pages/Employee/Clients/ClientsListPage/ClientsListPage';
import ClientDetailPage from '../pages/Employee/Clients/ClientDetailPage/ClientDetailPage';
import CreditServicesPage from '../pages/Admin/CreditServices/CreditServicesPage';
import EmployeesListPage from '../pages/Admin/Employees/EmployeesListPage';
import ActivityLogPage from '../pages/Admin/ActivityLog/ActivityLogPage';
import SettingsPage from '../pages/Settings/SettingsPage';
import { useAuth } from '../context/auth/useAuth';

const RootRedirect = () => {
  const { isAuthenticated, role } = useAuth();
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (role === 'Admin') return <Navigate to="/admin/dashboard" replace />;
  if (role === 'Employee') return <Navigate to="/employee/dashboard" replace />;
  return <Navigate to="/client/dashboard" replace />;
};

const PublicLayout = () => (
  <Flex direction="column" height="100vh">
    <Header isAuthenticated={false} />
    <Outlet />
  </Flex>
);

const AuthenticatedLayout = () => (
  <PageLayout>
    <Outlet />
  </PageLayout>
);

const AppRoutes = () => (
  <Routes>
    <Route path="/" element={<RootRedirect />} />

    <Route element={<PublicLayout />}>
      <Route element={<PublicRoutes />}>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/verify-otp" element={<VerifyOtpPage />} />
      </Route>
    </Route>

    <Route element={<AuthenticatedLayout />}>
      <Route element={<AdminRoutes />}>
        <Route path="/admin/dashboard" element={<AdminDashboard />} />
        <Route path="/admin/clients" element={<ClientsListPage />} />
        <Route path="/admin/clients/:id" element={<ClientDetailPage />} />
        <Route path="/admin/credit-services" element={<CreditServicesPage />} />
        <Route path="/admin/employees" element={<EmployeesListPage />} />
        <Route path="/admin/activity-log" element={<ActivityLogPage />} />
      </Route>

      <Route element={<EmployeeRoutes />}>
        <Route path="/employee/dashboard" element={<EmployeeDashboard />} />
        <Route path="/employee/clients" element={<ClientsListPage />} />
        <Route path="/employee/clients/:id" element={<ClientDetailPage />} />
      </Route>

      <Route element={<ClientRoutes />}>
        <Route path="/client/dashboard" element={<ClientDashboard />} />
      </Route>

      <Route element={<AuthenticatedRoutes />}>
        <Route path="/settings" element={<SettingsPage />} />
      </Route>
    </Route>

    <Route element={<PublicLayout />}>
      <Route path="*" element={<NotFound />} />
    </Route>
  </Routes>
);

export default AppRoutes;
