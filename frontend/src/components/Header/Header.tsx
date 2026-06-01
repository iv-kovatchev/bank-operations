import { Avatar, Box, DropdownMenu, Flex, IconButton, Text } from '@radix-ui/themes';
import { SunIcon, MoonIcon, HamburgerMenuIcon } from '@radix-ui/react-icons';
import { Link } from 'react-router-dom';
import logo from '../../assets/logo.png';
import { useHeader } from './useHeader';
import type { HeaderProps } from './Header.types';
import './Header.styles.css';

const Header = ({ isAuthenticated, isSidebarOpen, onToggleSidebar }: HeaderProps) => {
  const { theme, toggleTheme, displayName, initials, role, roleColor, logout, isLoggingOut } = useHeader(isAuthenticated);

  return (
    <header className="app-header">
      <Flex align="center" justify="between" px="4" height="100%">

        <Flex align="center" gap="3">
          {onToggleSidebar && (
            <button
              type="button"
              className="header-hamburger"
              onClick={onToggleSidebar}
              aria-label="Toggle navigation"
              aria-expanded={isSidebarOpen}
            >
              <HamburgerMenuIcon width="18" height="18" />
            </button>
          )}
          <Link to="/" className="header-brand-link">
            <Flex align="center" gap="2">
              <img src={logo} alt="Bank Operations" className="header-logo" />
              <Text size="4" weight="bold">Bank Operations</Text>
            </Flex>
          </Link>
        </Flex>

        <Flex align="center" gap="5">
          {isAuthenticated && (
            <Flex align="center" gap="3">
              <Text size="2" className="header-username">{displayName}</Text>

              <DropdownMenu.Root>
                <DropdownMenu.Trigger>
                  <button className="header-avatar-trigger">
                    <Avatar
                      fallback={initials}
                      color={roleColor}
                      radius="full"
                      size="2"
                    />
                  </button>
                </DropdownMenu.Trigger>

                <DropdownMenu.Content align="end" className="header-dropdown">
                  <Box px="3">
                    <Text as="p" size="1" color="gray">{role}</Text>
                  </Box>

                  <DropdownMenu.Item className="header-dropdown-item">Settings</DropdownMenu.Item>

                  <DropdownMenu.Item
                    className="header-dropdown-item"
                    disabled={isLoggingOut}
                    onSelect={() => logout()}
                  >
                    Sign out
                  </DropdownMenu.Item>
                </DropdownMenu.Content>
              </DropdownMenu.Root>
            </Flex>
          )}

          <IconButton variant="ghost" onClick={toggleTheme} aria-label="Toggle theme">
            {theme === 'dark' ? <MoonIcon /> : <SunIcon />}
          </IconButton>
        </Flex>

      </Flex>
    </header>
  );
};

export default Header;
