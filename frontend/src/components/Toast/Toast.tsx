import { Callout } from '@radix-ui/themes';
import { InfoCircledIcon, CheckCircledIcon, ExclamationTriangleIcon } from '@radix-ui/react-icons';
import type { ToastProps } from './Toast.types';

const toastConfig = {
  error: { color: 'red', icon: <ExclamationTriangleIcon /> },
  success: { color: 'green', icon: <CheckCircledIcon /> },
  warning: { color: 'yellow', icon: <ExclamationTriangleIcon /> },
} as const;

const Toast = ({ message, type }: ToastProps) => {
  const { color, icon } = toastConfig[type];

  return (
    <Callout.Root color={color}>
      <Callout.Icon>{icon}</Callout.Icon>
      <Callout.Text>{message}</Callout.Text>
    </Callout.Root>
  );
};

export default Toast;
