import { Navigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

export function HomeRedirect() {
  const { isAuthenticated } = useAuth();
  return isAuthenticated ? <Navigate to="/tasks" replace /> : <Navigate to="/login" replace />;
}