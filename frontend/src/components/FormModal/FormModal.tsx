import type { ReactNode } from 'react';
import { Dialog } from '@radix-ui/themes';

interface FormModalProps {
  open: boolean;
  title: string;
  onClose: () => void;
  children: ReactNode;
}

const FormModal = ({ open, title, onClose, children }: FormModalProps) => (
  <Dialog.Root open={open} onOpenChange={(o) => !o && onClose()}>
    <Dialog.Content maxWidth="480px" aria-describedby={undefined}>
      <Dialog.Title>{title}</Dialog.Title>
      {children}
    </Dialog.Content>
  </Dialog.Root>
);

export default FormModal;
