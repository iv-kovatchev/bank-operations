import { Outlet, Route, Routes } from 'react-router-dom';
import { Flex } from '@radix-ui/themes';
import PublicRoutes from './PublicRoutes';
import AdminRoutes from './AdminRoutes';
import EmployeeRoutes from './EmployeeRoutes';
import ClientRoutes from './ClientRoutes';
import PageLayout from '../components/PageLayout/PageLayout';
import Header from '../components/Header/Header';
import LoginPage from '../pages/Login/LoginPage';
import VerifyOtpPage from '../pages/VerifyOtp/VerifyOtpPage';
import AdminDashboard from '../pages/Admin/Dashboard/AdminDashboard';
import EmployeeDashboard from '../pages/Employee/Dashboard/EmployeeDashboard';
import ClientDashboard from '../pages/Client/Dashboard/ClientDashboard';
import NotFound from '../pages/NotFound/NotFound';

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
    <Route element={<PublicLayout />}>
      <Route element={<PublicRoutes />}>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/verify-otp" element={<VerifyOtpPage />} />
      </Route>
    </Route>

    <Route element={<AuthenticatedLayout />}>
      <Route element={<AdminRoutes />}>
        <Route path="/admin/dashboard" element={<AdminDashboard />} />
      </Route>

      <Route element={<EmployeeRoutes />}>
        <Route path="/employee/dashboard" element={<EmployeeDashboard />} />
      </Route>

      <Route element={<ClientRoutes />}>
        <Route path="/client/dashboard" element={<ClientDashboard />} />
      </Route>
    </Route>

    <Route element={<PublicLayout />}>
      <Route path="*" element={<NotFound />} />
    </Route>
  </Routes>
);

export default AppRoutes;
