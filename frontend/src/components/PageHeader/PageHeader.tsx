import { Flex, Heading, Text } from '@radix-ui/themes';
import type { PageHeaderProps } from './PageHeader.types';

const PageHeader = ({ title, subtitle }: PageHeaderProps) => (
  <Flex direction="column" gap="1" mb="5">
    <Heading size="6">{title}</Heading>
    {subtitle && (
      <Text size="2" color="gray">
        {subtitle}
      </Text>
    )}
  </Flex>
);

export default PageHeader;
