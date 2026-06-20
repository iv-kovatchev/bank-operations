import { jwtDecode } from 'jwt-decode';
import { useAuth } from '../../../context/auth/useAuth';
import { useGetClientAccounts } from '../../../api/bank-accounts/useGetClientAccounts';
import { useGetClientCredits } from '../../../api/credits/useGetClientCredits';
import { AccountStatus } from '../../../types/bank-account.types';
import { CreditStatus } from '../../../types/credit.types';

type JwtPayload = {
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier': string;
};

export const useClientDashboard = () => {
  const { accessToken, role } = useAuth();

  let userId = '';
  if (accessToken) {
    try {
      const decoded = jwtDecode<JwtPayload>(accessToken);
      userId = decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
    } catch {
      // malformed token — ignore
    }
  }

  const { data: accounts } = useGetClientAccounts(userId);
  const { data: credits } = useGetClientCredits(userId);

  const totalBalance = (accounts ?? [])
    .filter((account) => account.status === AccountStatus.Active)
    .reduce((sum, account) => sum + account.balance, 0);

  const activeCredits = (credits ?? []).filter((credit) => credit.status === CreditStatus.Active);

  const totalCreditAmount = activeCredits.reduce((sum, credit) => sum + credit.amount, 0);

  return {
    userId,
    role,
    totalBalance,
    activeCreditsCount: activeCredits.length,
    totalCreditAmount,
  };
};
