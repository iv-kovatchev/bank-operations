export const CreditType = {
  Consumer: 'Consumer',
  Mortgage: 'Mortgage',
} as const;

export type CreditType = typeof CreditType[keyof typeof CreditType];

export interface CreditServiceResponse {
  id: string;
  name: string;
  type: CreditType;
  interestRate: number;
  maxAmount: number;
  maxTermMonths: number;
}

export interface CreateCreditServiceDto {
  name: string;
  type: CreditType;
  interestRate: number;
  maxAmount: number;
  maxTermMonths: number;
}

export interface UpdateCreditServiceDto {
  name: string;
  type: CreditType;
  interestRate: number;
  maxAmount: number;
  maxTermMonths: number;
}
