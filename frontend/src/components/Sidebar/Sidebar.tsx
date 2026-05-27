import { Box, Flex, Text } from '@radix-ui/themes';
import { NavLink } from 'react-router-dom';
import type { SidebarProps } from './Sidebar.types';
import './Sidebar.styles.css';

const Sidebar = ({ items }: SidebarProps) => (
  <Box width="220px" height="100vh" flexShrink="0" className="sidebar">
    <Flex direction="column" gap="1" p="3">
      {items.map((item) => (
        <NavLink
          key={item.path}
          to={item.path}
          className={({ isActive }) =>
            isActive ? 'sidebar-link sidebar-link-active' : 'sidebar-link'
          }
        >
          <Flex align="center" gap="2" px="3" py="2">
            {item.icon}
            <Text size="2" weight="medium">
              {item.label}
            </Text>
          </Flex>
        </NavLink>
      ))}
    </Flex>
  </Box>
);

export default Sidebar;
