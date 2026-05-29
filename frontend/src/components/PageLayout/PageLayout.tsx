import { Flex } from '@radix-ui/themes';
import { useAuth } from '../../context/auth/useAuth';
import Header from '../Header/Header';
import type { PageLayoutProps } from './PageLayout.types';
import './PageLayout.styles.css';

const PageLayout = ({ children }: PageLayoutProps) => {
  const { isAuthenticated } = useAuth();

  return (
    <Flex direction="column" height="100vh" overflow="hidden">
      <Header isAuthenticated={isAuthenticated} />
      {children}
    </Flex>
  );
};

export default PageLayout;
