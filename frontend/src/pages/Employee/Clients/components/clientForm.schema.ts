import { z } from 'zod';

export const individualClientSchema = z.object({
  firstName: z.string().min(2, 'First name must be at least 2 characters'),
  lastName: z.string().min(2, 'Last name must be at least 2 characters'),
  egn: z.string().regex(/^\d{10}$/, 'EGN must be exactly 10 digits'),
  email: z.string().email('Enter a valid email address'),
});

export type IndividualClientFormData = z.infer<typeof individualClientSchema>;

export const corporateClientSchema = z.object({
  companyName: z.string().min(2, 'Company name must be at least 2 characters'),
  eik: z.string().regex(/^\d{9}$/, 'EIK must be exactly 9 digits'),
  representativeFirstName: z.string().min(2, 'First name must be at least 2 characters'),
  representativeLastName: z.string().min(2, 'Last name must be at least 2 characters'),
  email: z.string().email('Enter a valid email address'),
});

export type CorporateClientFormData = z.infer<typeof corporateClientSchema>;
