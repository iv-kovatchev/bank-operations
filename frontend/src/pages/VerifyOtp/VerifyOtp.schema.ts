import { z } from 'zod';

export const verifyOtpSchema = z.object({
  code: z
    .string()
    .length(6, 'Code must be exactly 6 digits')
    .regex(/^\d+$/, 'Code must contain digits only'),
});

export type VerifyOtpFormData = z.infer<typeof verifyOtpSchema>;
