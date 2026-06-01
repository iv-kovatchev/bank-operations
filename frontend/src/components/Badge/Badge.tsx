import type { ReactNode } from 'react';
import { Badge as RadixBadge } from '@radix-ui/themes';
import './Badge.styles.css';

interface BadgeProps {
  children: ReactNode;
  color?: 'green' | 'red' | 'blue' | 'gray';
}

const Badge = ({ children, color = 'gray' }: BadgeProps) => (
  <RadixBadge variant="outline" color={color} className="badge-outline">
    {children}
  </RadixBadge>
);

export default Badge;
