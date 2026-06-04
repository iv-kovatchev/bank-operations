export const ClientType = {
  Individual: 'Individual',
  Corporate: 'Corporate',
} as const;

export type ClientType = typeof ClientType[keyof typeof ClientType];

export interface ClientResponse {
  id: string;
  type: ClientType;
  email: string;
  isActive: boolean;
  createdAt: string;
  createdByUserId: string;
  firstName?: string;
  lastName?: string;
  egn?: string;
  companyName?: string;
  eik?: string;
  representativeFirstName?: string;
  representativeLastName?: string;
}

export interface IndividualClientResponse {
  id: string;
  type: ClientType;
  email: string;
  isActive: boolean;
  createdAt: string;
  createdByUserId: string;
  firstName: string;
  lastName: string;
  egn: string;
}

export interface CorporateClientResponse {
  id: string;
  type: ClientType;
  email: string;
  isActive: boolean;
  createdAt: string;
  createdByUserId: string;
  companyName: string;
  eik: string;
  representativeFirstName: string;
  representativeLastName: string;
}

export type ClientDetailResponse = IndividualClientResponse | CorporateClientResponse;

export interface CreateIndividualClientDto {
  firstName: string;
  lastName: string;
  egn: string;
  email: string;
}

export interface CreateCorporateClientDto {
  companyName: string;
  eik: string;
  representativeFirstName: string;
  representativeLastName: string;
  email: string;
}

export interface UpdateIndividualClientDto {
  firstName: string;
  lastName: string;
  email: string;
}

export interface UpdateCorporateClientDto {
  companyName: string;
  representativeFirstName: string;
  representativeLastName: string;
  email: string;
}
