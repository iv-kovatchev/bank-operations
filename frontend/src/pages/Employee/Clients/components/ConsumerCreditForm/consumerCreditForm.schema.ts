import { z } from 'zod';

export const consumerCreditSchema = z.object({
  creditServiceId: z.string().min(1, 'Credit service is required'),
  amount: z.preprocess(Number, z.number().min(0.01, 'Amount must be greater than 0')),
  termMonths: z.preprocess(Number, z.number().int().min(1, 'Term must be at least 1 month').max(600)),
  purpose: z.enum(['CarPurchase', 'HomeRenovation', 'Education', 'Other']),
});

export type ConsumerCreditFormInput = z.input<typeof consumerCreditSchema>;
export type ConsumerCreditFormOutput = z.output<typeof consumerCreditSchema>;
