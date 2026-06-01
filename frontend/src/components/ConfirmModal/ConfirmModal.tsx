import { Dialog, Flex, Text } from '@radix-ui/themes';
import Button from '../Button/Button';

interface ConfirmModalProps {
  open: boolean;
  title: string;
  description: string;
  confirmLabel?: string;
  cancelLabel?: string;
  confirmColor?: 'red' | 'green' | 'blue';
  isLoading: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

const ConfirmModal = ({
  open,
  title,
  description,
  confirmLabel = 'Confirm',
  cancelLabel = 'Cancel',
  confirmColor = 'red',
  isLoading,
  onConfirm,
  onCancel,
}: ConfirmModalProps) => (
  <Dialog.Root open={open} onOpenChange={(o) => !o && onCancel()}>
    <Dialog.Content maxWidth="400px">
      <Dialog.Title>{title}</Dialog.Title>
      <Text as="p" size="2" color="gray" mb="4">
        {description}
      </Text>
      <Flex gap="3" justify="end">
        <Button variant="soft" color="gray" onClick={onCancel} disabled={isLoading}>
          {cancelLabel}
        </Button>
        <Button color={confirmColor} loading={isLoading} disabled={isLoading} onClick={onConfirm}>
          {confirmLabel}
        </Button>
      </Flex>
    </Dialog.Content>
  </Dialog.Root>
);

export default ConfirmModal;
