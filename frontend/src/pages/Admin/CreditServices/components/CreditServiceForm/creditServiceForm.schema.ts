import { z } from 'zod';

export const creditServiceSchema = z.object({
  name: z.string().min(1, 'Name is required').max(100),
  type: z.enum(['Consumer', 'Mortgage']),
  interestRate: z.preprocess(Number, z.number().min(0.01, 'Interest rate must be greater than 0').max(100, 'Interest rate must be at most 100')),
  maxAmount: z.preprocess(Number, z.number().min(1, 'Max amount must be at least 1')),
  maxTermMonths: z.preprocess(Number, z.number().int('Max term must be a whole number').min(1, 'Max term must be at least 1').max(600, 'Max term must be at most 600')),
});

export type CreditServiceFormInput = z.input<typeof creditServiceSchema>;
export type CreditServiceFormOutput = z.output<typeof creditServiceSchema>;
