export interface ProfileResponse {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
}

export interface UpdateProfileDto {
  firstName: string;
  lastName: string;
}

export interface ChangePasswordDto {
  currentPassword: string;
  newPassword: string;
}
