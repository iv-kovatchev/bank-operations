import { useState } from 'react';
import { Box, Flex } from '@radix-ui/themes';
import { useAuth } from '../../context/auth/useAuth';
import Header from '../Header/Header';
import Sidebar from '../Sidebar/Sidebar';
import type { PageLayoutProps } from './PageLayout.types';
import './PageLayout.styles.css';

const PageLayout = ({ children }: PageLayoutProps) => {
  const { isAuthenticated, role } = useAuth();
  const hasSidebar = role === 'Employee' || role === 'Admin';

  const [isSidebarOpen, setIsSidebarOpen] = useState(false);

  const handleToggleSidebar = () => setIsSidebarOpen(prev => !prev);
  const handleCloseSidebar = () => setIsSidebarOpen(false);

  return (
    <Flex direction="column" height="100vh" overflow="hidden">
      <Header
        isAuthenticated={isAuthenticated}
        isSidebarOpen={isSidebarOpen}
        onToggleSidebar={hasSidebar ? handleToggleSidebar : undefined}
      />
      <Flex flexGrow="1" overflow="hidden">
        {hasSidebar && <Sidebar isOpen={isSidebarOpen} />}
        {hasSidebar && isSidebarOpen && (
          <div className="sidebar-overlay" onClick={handleCloseSidebar} />
        )}
        <Box flexGrow="1" overflow="auto">
          {children}
        </Box>
      </Flex>
    </Flex>
  );
};

export default PageLayout;
