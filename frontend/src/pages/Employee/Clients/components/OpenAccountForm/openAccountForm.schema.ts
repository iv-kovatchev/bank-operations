import { z } from 'zod';

export const openAccountSchema = z.object({
  iban: z
    .string()
    .min(1, 'IBAN is required')
    .regex(/^BG\d{2}BANK\d{14}$/, 'Invalid IBAN format'),
  initialBalance: z.number().min(0, 'Balance cannot be negative'),
});

export type OpenAccountFormData = z.infer<typeof openAccountSchema>;
