import { Box, Flex } from '@radix-ui/themes';
import { Link, useLocation } from 'react-router-dom';
import { DashboardIcon, PersonIcon } from '@radix-ui/react-icons';
import { useAuth } from '../../context/auth/useAuth';
import type { SidebarItem } from './Sidebar.types';
import './Sidebar.styles.css';

const EMPLOYEE_ITEMS: SidebarItem[] = [
  { label: 'Dashboard', path: '/employee/dashboard', icon: <DashboardIcon /> },
  { label: 'Clients', path: '/employee/clients', icon: <PersonIcon /> },
];

const ADMIN_ITEMS: SidebarItem[] = [
  { label: 'Dashboard', path: '/admin/dashboard', icon: <DashboardIcon /> },
  { label: 'Clients', path: '/admin/clients', icon: <PersonIcon /> },
];

const ITEMS_BY_ROLE: Record<string, SidebarItem[]> = {
  Employee: EMPLOYEE_ITEMS,
  Admin: ADMIN_ITEMS,
};

interface SidebarProps {
  isOpen: boolean;
}

const Sidebar = ({ isOpen }: SidebarProps) => {
  const { role } = useAuth();
  const { pathname } = useLocation();

  const items: SidebarItem[] = role ? (ITEMS_BY_ROLE[role] ?? []) : [];

  return (
    <Box
      width="220px"
      flexShrink="0"
      className={isOpen ? 'sidebar sidebar-open' : 'sidebar'}
    >
      <Box className="sidebar-nav">
        <Flex direction="column" gap="1">
          {items.map((item) => (
            <Link
              key={item.path}
              to={item.path}
              className={
                pathname.startsWith(item.path)
                  ? 'sidebar-link sidebar-link-active'
                  : 'sidebar-link'
              }
            >
              {item.icon}
              {item.label}
            </Link>
          ))}
        </Flex>
      </Box>
    </Box>
  );
};

export default Sidebar;
