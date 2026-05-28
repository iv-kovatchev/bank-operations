import { useNavigate } from 'react-router-dom';
import { Button, Card, Flex, Heading, Text, TextField } from '@radix-ui/themes';
import { useVerifyOtpPage } from './useVerifyOtpPage';
import Toast from '../../components/Toast/Toast';
import './VerifyOtpPage.styles.css';

const VerifyOtpPage = () => {
  const navigate = useNavigate();
  const { form, onSubmit, isPending, error, email } = useVerifyOtpPage();
  const { register, handleSubmit, formState: { errors } } = form;

  return (
    <Flex align="center" justify="center" className="verify-otp-page">
      <Card className="verify-otp-card">
        <Flex direction="column" gap="5">

          <Flex direction="column" gap="1">
            <Heading size="6">Check your email</Heading>
            <Text size="2" color="gray">
              We sent a 6-digit code to{' '}
              <Text size="2" weight="medium" color="gray">
                {email}
              </Text>
            </Text>
          </Flex>

          {error && <Toast message={error.message} type="error" />}

          <form onSubmit={handleSubmit(onSubmit)} className="verify-otp-form">
            <Flex direction="column" gap="4">

              <Flex direction="column" gap="1">
                <Text as="label" size="2" weight="medium">
                  Verification code
                </Text>
                <TextField.Root
                  type="text"
                  placeholder="123456"
                  maxLength={6}
                  autoComplete="one-time-code"
                  inputMode="numeric"
                  {...register('code')}
                />
                {errors.code && (
                  <Text size="1" color="red">
                    {errors.code.message}
                  </Text>
                )}
              </Flex>

              <Button type="submit" loading={isPending} disabled={isPending}>
                Verify
              </Button>

              <Button
                type="button"
                variant="ghost"
                onClick={() => navigate('/login', { replace: true })}
              >
                Back to sign in
              </Button>

            </Flex>
          </form>

        </Flex>
      </Card>
    </Flex>
  );
};

export default VerifyOtpPage;
