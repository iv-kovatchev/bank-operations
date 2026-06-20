import type { ReactNode } from 'react';
import { Dialog } from '@radix-ui/themes';

interface FormModalProps {
  open: boolean;
  title: string;
  onClose: () => void;
  children: ReactNode;
  maxWidth?: string;
}

const FormModal = ({ open, title, onClose, children, maxWidth }: FormModalProps) => (
  <Dialog.Root open={open} onOpenChange={(o) => !o && onClose()}>
    <Dialog.Content maxWidth={maxWidth ?? '480px'} maxHeight="80vh" aria-describedby={undefined}>
      <Dialog.Title>{title}</Dialog.Title>
      {children}
    </Dialog.Content>
  </Dialog.Root>
);

export default FormModal;
