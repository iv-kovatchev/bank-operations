import { Button, Card, Flex, Heading, Text, TextField } from '@radix-ui/themes';
import { useLoginPage } from './useLoginPage';
import Toast from '../../components/Toast/Toast';
import './LoginPage.styles.css';

const LoginPage = () => {
  const { form, onSubmit, isPending, error } = useLoginPage();
  const { register, handleSubmit, formState: { errors } } = form;

  return (
    <Flex align="center" justify="center" className="login-page">
      <Card className="login-card">
        <Flex direction="column" gap="5">

          <Flex direction="column" gap="1">
            <Heading size="6">Sign in</Heading>
            <Text size="2" color="gray">
              Bank Operations System
            </Text>
          </Flex>

          {error && <Toast message={error.message} type="error" />}

          <form onSubmit={handleSubmit(onSubmit)} className="login-form">
            <Flex direction="column" gap="4">

              <Flex direction="column" gap="1">
                <Text as="label" size="2" weight="medium">
                  Email
                </Text>
                <TextField.Root
                  type="email"
                  placeholder="employee@bank.com"
                  autoComplete="email"
                  {...register('email')}
                />
                {errors.email && (
                  <Text size="1" color="red">
                    {errors.email.message}
                  </Text>
                )}
              </Flex>

              <Flex direction="column" gap="1">
                <Text as="label" size="2" weight="medium">
                  Password
                </Text>
                <TextField.Root
                  type="password"
                  placeholder="••••••••"
                  autoComplete="current-password"
                  {...register('password')}
                />
                {errors.password && (
                  <Text size="1" color="red">
                    {errors.password.message}
                  </Text>
                )}
              </Flex>

              <Button type="submit" loading={isPending} disabled={isPending}>
                Sign in
              </Button>

            </Flex>
          </form>

        </Flex>
      </Card>
    </Flex>
  );
};

export default LoginPage;
