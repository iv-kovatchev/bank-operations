import { useState, type ReactNode } from 'react';
import { ThemeContext, type Theme } from './themeContextDef';

const getStoredTheme = (): Theme => {
  const stored = localStorage.getItem('theme');
  return stored === 'light' ? 'light' : 'dark';
};

export const ThemeContextProvider = ({ children }: { children: ReactNode }) => {
  const [theme, setTheme] = useState<Theme>(getStoredTheme);

  const toggleTheme = () => {
    setTheme(prev => {
      const next = prev === 'dark' ? 'light' : 'dark';
      localStorage.setItem('theme', next);
      return next;
    });
  };

  return (
    <ThemeContext.Provider value={{ theme, toggleTheme }}>
      {children}
    </ThemeContext.Provider>
  );
};
