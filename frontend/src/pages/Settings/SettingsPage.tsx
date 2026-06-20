import { Box, Card, Flex, Heading, Text, TextField } from '@radix-ui/themes';
import Button from '../../components/Button/Button';
import Toast from '../../components/Toast/Toast';
import LoadingSpinner from '../../components/LoadingSpinner/LoadingSpinner';
import { useSettingsPage } from './useSettingsPage';

const SettingsPage = () => {
  const {
    canEditProfile,
    profile,
    isLoading,
    registerProfile,
    handleProfileSubmit,
    profileErrors,
    isUpdatingProfile,
    updateProfileError,
    profileSuccess,
    onProfileSubmit,
    registerPassword,
    handlePasswordSubmit,
    passwordErrors,
    isChangingPassword,
    changePasswordError,
    passwordSuccess,
    onPasswordSubmit,
  } = useSettingsPage();

  if (isLoading) return <LoadingSpinner />;

  return (
    <Box p="6">
      <Heading size="6" mb="5">Settings</Heading>

      {canEditProfile && (
      <Card mb="5">
        <Box p="2">
          <Heading size="4" mb="4">Profile</Heading>
          <form onSubmit={handleProfileSubmit(onProfileSubmit)}>
            <Flex direction="column" gap="4">
              {updateProfileError && <Toast message={updateProfileError.message} type="error" />}
              {profileSuccess && <Toast message="Profile updated successfully." type="success" />}

              <Flex direction="column" gap="1">
                <Text size="2" color="gray">Email</Text>
                <Text size="3">{profile?.email}</Text>
              </Flex>
              <Flex direction="column" gap="1">
                <Text size="2" color="gray">Role</Text>
                <Text size="3">{profile?.role}</Text>
              </Flex>

              <Flex direction="column" gap="1">
                <Text as="label" size="2" weight="medium">First Name</Text>
                <TextField.Root placeholder="Ivan" {...registerProfile('firstName')} />
                {profileErrors.firstName && <Text size="1" color="red">{profileErrors.firstName.message}</Text>}
              </Flex>
              <Flex direction="column" gap="1">
                <Text as="label" size="2" weight="medium">Last Name</Text>
                <TextField.Root placeholder="Ivanov" {...registerProfile('lastName')} />
                {profileErrors.lastName && <Text size="1" color="red">{profileErrors.lastName.message}</Text>}
              </Flex>

              <Flex justify="end" mt="2">
                <Button type="submit" loading={isUpdatingProfile} disabled={isUpdatingProfile}>
                  Save Changes
                </Button>
              </Flex>
            </Flex>
          </form>
        </Box>
      </Card>
      )}

      <Card>
        <Box p="2">
          <Heading size="4" mb="4">Change Password</Heading>
          <form onSubmit={handlePasswordSubmit(onPasswordSubmit)}>
            <Flex direction="column" gap="4">
              {changePasswordError && <Toast message={changePasswordError.message} type="error" />}
              {passwordSuccess && <Toast message="Password changed successfully." type="success" />}

              <Flex direction="column" gap="1">
                <Text as="label" size="2" weight="medium">Current Password</Text>
                <TextField.Root type="password" {...registerPassword('currentPassword')} />
                {passwordErrors.currentPassword && (
                  <Text size="1" color="red">{passwordErrors.currentPassword.message}</Text>
                )}
              </Flex>
              <Flex direction="column" gap="1">
                <Text as="label" size="2" weight="medium">New Password</Text>
                <TextField.Root type="password" {...registerPassword('newPassword')} />
                {passwordErrors.newPassword && (
                  <Text size="1" color="red">{passwordErrors.newPassword.message}</Text>
                )}
              </Flex>
              <Flex direction="column" gap="1">
                <Text as="label" size="2" weight="medium">Confirm New Password</Text>
                <TextField.Root type="password" {...registerPassword('confirmNewPassword')} />
                {passwordErrors.confirmNewPassword && (
                  <Text size="1" color="red">{passwordErrors.confirmNewPassword.message}</Text>
                )}
              </Flex>

              <Flex justify="end" mt="2">
                <Button type="submit" loading={isChangingPassword} disabled={isChangingPassword}>
                  Change Password
                </Button>
              </Flex>
            </Flex>
          </form>
        </Box>
      </Card>
    </Box>
  );
};

export default SettingsPage;
