export const AccountStatus = {
  Active: 'Active',
  Closed: 'Closed',
} as const;

export type AccountStatus = typeof AccountStatus[keyof typeof AccountStatus];

export interface BankAccountResponse {
  id: string;
  iban: string;
  balance: number;
  status: AccountStatus;
  clientId: string;
  createdAt: string;
  createdByUserId: string;
}

export interface CreateBankAccountDto {
  iban: string;
  initialBalance: number;
}
