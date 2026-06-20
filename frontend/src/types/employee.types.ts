export interface EmployeeResponse {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  isActive: boolean;
  createdAt: string;
}

export interface CreateEmployeeDto {
  firstName: string;
  lastName: string;
  email: string;
}

export interface UpdateEmployeeDto {
  firstName: string;
  lastName: string;
  email: string;
}
