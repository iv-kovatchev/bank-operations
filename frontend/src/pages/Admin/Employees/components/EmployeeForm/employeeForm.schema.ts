import { z } from 'zod';

export const employeeFormSchema = z.object({
  firstName: z.string().min(1, 'First name is required').max(100),
  lastName: z.string().min(1, 'Last name is required').max(100),
  email: z.string().email('Enter a valid email address'),
});

export type EmployeeFormData = z.infer<typeof employeeFormSchema>;
