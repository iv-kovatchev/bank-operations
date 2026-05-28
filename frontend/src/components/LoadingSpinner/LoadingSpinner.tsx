import { Flex, Spinner } from '@radix-ui/themes';
import type { LoadingSpinnerProps } from './LoadingSpinner.types';
import './LoadingSpinner.styles.css';

const LoadingSpinner = ({ size = '3' }: LoadingSpinnerProps) => (
  <Flex align="center" justify="center" height="100%">
    <Spinner size={size} />
  </Flex>
);

export default LoadingSpinner;
