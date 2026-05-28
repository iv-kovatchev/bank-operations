import { Button, Heading, Text } from '@radix-ui/themes';
import { useNavigate } from 'react-router-dom';
import './NotFound.styles.css';

const NotFound = () => {
  const navigate = useNavigate();

  return (
    <div className="not-found-page">
      <Heading size="9">404</Heading>
      <Text size="4" color="gray">Page not found</Text>
      <Button mt="4" onClick={() => navigate('/login')}>Go to home</Button>
    </div>
  );
};

export default NotFound;
