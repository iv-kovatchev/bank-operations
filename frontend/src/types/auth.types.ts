export type LoginRequest = {
  email: string;
  password: string;
};

export type VerifyOtpRequest = {
  email: string;
  code: string;
};

export type AuthResponse = {
  accessToken: string | null;
  refreshToken: string;
  requiresOtp: boolean;
  message: string;
};
