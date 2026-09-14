import { useAuth } from '../auth/AuthContext';

export function AuthCheckPage() {
  const { isAuthenticated, user } = useAuth();

  if (!isAuthenticated) {
    return (
      <div data-testid="auth-status" data-value="anonymous" style={{ padding: '2rem', fontFamily: 'monospace' }}>
        ANONYMOUS
      </div>
    );
  }

  return (
    <div data-testid="auth-status" data-value="authenticated" style={{ padding: '2rem', fontFamily: 'monospace' }}>
      AUTHENTICATED - {user?.email}
    </div>
  );
}