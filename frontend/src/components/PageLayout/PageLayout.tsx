import { Flex } from '@radix-ui/themes';
import type { PageLayoutProps } from './PageLayout.types';

const PageLayout = ({ children }: PageLayoutProps) => (
  <Flex height="100vh" overflow="hidden">
    {children}
  </Flex>
);

export default PageLayout;
