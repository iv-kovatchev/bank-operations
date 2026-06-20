import { z } from 'zod';

export const mortgageCreditSchema = z.object({
  creditServiceId: z.string().min(1, 'Credit service is required'),
  amount: z.preprocess(Number, z.number().min(0.01, 'Amount must be greater than 0')),
  termMonths: z.preprocess(Number, z.number().int().min(1, 'Term must be at least 1 month').max(600)),
  propertyAddress: z.string().min(1, 'Property address is required'),
  propertyType: z.enum(['Apartment', 'House', 'Commercial']),
});

export type MortgageCreditFormInput = z.input<typeof mortgageCreditSchema>;
export type MortgageCreditFormOutput = z.output<typeof mortgageCreditSchema>;
