export const CreditStatus = {
  Active: 'Active',
  PaidOff: 'PaidOff',
  Defaulted: 'Defaulted',
} as const;
export type CreditStatus = typeof CreditStatus[keyof typeof CreditStatus];

export const CreditPurpose = {
  CarPurchase: 'CarPurchase',
  HomeRenovation: 'HomeRenovation',
  Education: 'Education',
  Other: 'Other',
} as const;
export type CreditPurpose = typeof CreditPurpose[keyof typeof CreditPurpose];

export const PropertyType = {
  Apartment: 'Apartment',
  House: 'House',
  Commercial: 'Commercial',
} as const;
export type PropertyType = typeof PropertyType[keyof typeof PropertyType];

export interface CreditResponse {
  id: string;
  clientId: string;
  creditServiceId: string;
  creditType: string;
  amount: number;
  termMonths: number;
  status: CreditStatus;
  createdAt: string;
  createdByUserId: string;
  purpose?: string;
  propertyAddress?: string;
  propertyType?: string;
}

export interface RepaymentInstallmentResponse {
  id: string;
  installmentNumber: number;
  dueDate: string;
  principalPart: number;
  interestPart: number;
  totalAmount: number;
  remainingBalance: number;
  paidAt: string | null;
  isPaid: boolean;
}

export interface RepaymentPlanResponse {
  creditId: string;
  monthlyInstallment: number;
  generatedAt: string;
  installments: RepaymentInstallmentResponse[];
}

export interface CreateConsumerCreditRequest {
  creditServiceId: string;
  amount: number;
  termMonths: number;
  purpose: CreditPurpose;
}

export interface CreateMortgageCreditRequest {
  creditServiceId: string;
  amount: number;
  termMonths: number;
  propertyAddress: string;
  propertyType: PropertyType;
}

export interface UpdateConsumerCreditRequest {
  creditServiceId: string;
  amount: number;
  termMonths: number;
  purpose: CreditPurpose;
}

export interface UpdateMortgageCreditRequest {
  creditServiceId: string;
  amount: number;
  termMonths: number;
  propertyAddress: string;
  propertyType: PropertyType;
}

export interface PayInstallmentRequest {
  bankAccountId: string;
}
