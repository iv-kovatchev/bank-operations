import { Button as RadixButton } from '@radix-ui/themes';
import type { ComponentProps } from 'react';
import './Button.styles.css';

type ButtonProps = ComponentProps<typeof RadixButton>;

const Button = ({ className, ...props }: ButtonProps) => (
  <RadixButton className={className ? `btn ${className}` : 'btn'} {...props} />
);

export default Button;
