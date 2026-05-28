import { Theme } from '@radix-ui/themes';
import { BrowserRouter } from 'react-router-dom';
import { AuthContextProvider } from './context/auth/AuthContext';
import { useTheme } from './context/theme/useTheme';
import AppRoutes from './routes';

const App = () => {
  const { theme } = useTheme();
  return (
    <Theme appearance={theme} accentColor="green" grayColor="gray">
      <BrowserRouter>
        <AuthContextProvider>
          <AppRoutes />
        </AuthContextProvider>
      </BrowserRouter>
    </Theme>
  );
};

export default App;
