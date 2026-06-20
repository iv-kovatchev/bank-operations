import { z } from 'zod';

export const corporateClientSchema = z.object({
  companyName: z.string().min(2, 'Company name must be at least 2 characters'),
  eik: z.string().regex(/^\d{9}$/, 'EIK must be exactly 9 digits'),
  representativeFirstName: z.string().min(2, 'First name must be at least 2 characters'),
  representativeLastName: z.string().min(2, 'Last name must be at least 2 characters'),
  email: z.string().email('Enter a valid email address'),
});

export type CorporateClientFormData = z.infer<typeof corporateClientSchema>;
