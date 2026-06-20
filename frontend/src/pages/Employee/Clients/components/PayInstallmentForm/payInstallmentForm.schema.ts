import { z } from 'zod';

export const payInstallmentSchema = z.object({
  bankAccountId: z.string().min(1, 'Bank account is required'),
});

export type PayInstallmentFormInput = z.input<typeof payInstallmentSchema>;
export type PayInstallmentFormOutput = z.output<typeof payInstallmentSchema>;
