import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useAuth } from '../../context/auth/useAuth';
import { useGetProfile } from '../../api/settings/useGetProfile';
import { useUpdateProfile } from '../../api/settings/useUpdateProfile';
import { useChangePassword } from '../../api/settings/useChangePassword';
import {
  profileFormSchema,
  passwordFormSchema,
  type ProfileFormData,
  type PasswordFormData,
} from './settingsForm.schema';

export const useSettingsPage = () => {
  const { role } = useAuth();
  const canEditProfile = role === 'Admin' || role === 'Employee';

  const { data: profile, isLoading } = useGetProfile(canEditProfile);
  const { mutate: updateProfile, isPending: isUpdatingProfile, error: updateProfileError } = useUpdateProfile();
  const { mutate: changePassword, isPending: isChangingPassword, error: changePasswordError } = useChangePassword();

  const [profileSuccess, setProfileSuccess] = useState(false);
  const [passwordSuccess, setPasswordSuccess] = useState(false);

  const {
    register: registerProfile,
    handleSubmit: handleProfileSubmit,
    reset: resetProfile,
    formState: { errors: profileErrors },
  } = useForm<ProfileFormData>({
    resolver: zodResolver(profileFormSchema),
    defaultValues: { firstName: '', lastName: '' },
  });

  useEffect(() => {
    if (profile) {
      resetProfile({ firstName: profile.firstName, lastName: profile.lastName });
    }
  }, [profile, resetProfile]);

  const onProfileSubmit = (data: ProfileFormData) => {
    setProfileSuccess(false);
    updateProfile(data, { onSuccess: () => setProfileSuccess(true) });
  };

  const {
    register: registerPassword,
    handleSubmit: handlePasswordSubmit,
    reset: resetPassword,
    formState: { errors: passwordErrors },
  } = useForm<PasswordFormData>({
    resolver: zodResolver(passwordFormSchema),
    defaultValues: { currentPassword: '', newPassword: '', confirmNewPassword: '' },
  });

  const onPasswordSubmit = (data: PasswordFormData) => {
    setPasswordSuccess(false);
    changePassword(
      { currentPassword: data.currentPassword, newPassword: data.newPassword },
      {
        onSuccess: () => {
          setPasswordSuccess(true);
          resetPassword({ currentPassword: '', newPassword: '', confirmNewPassword: '' });
        },
      }
    );
  };

  return {
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
  };
};
