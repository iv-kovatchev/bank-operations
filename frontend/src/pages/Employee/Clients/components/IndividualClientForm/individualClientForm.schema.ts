import { z } from 'zod';

export const individualClientSchema = z.object({
  firstName: z.string().min(2, 'First name must be at least 2 characters'),
  lastName: z.string().min(2, 'Last name must be at least 2 characters'),
  egn: z.string().regex(/^\d{10}$/, 'EGN must be exactly 10 digits'),
  email: z.string().email('Enter a valid email address'),
});

export type IndividualClientFormData = z.infer<typeof individualClientSchema>;
