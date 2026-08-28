import { useState } from 'react';
import type { FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Input } from '../components/Input';
import { Button } from '../components/Button';
import { useAuth } from '../auth/AuthContext';
import { authApi } from '../api/authApi';
import styles from './LoginPage.module.css';

export function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [errors, setErrors] = useState<{ email?: string; password?: string; server?: string }>({});
  const [isLoading, setIsLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const validate = (): boolean => {
    const newErrors: typeof errors = {};

    if (!email) {
      newErrors.email = 'Email is required';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      newErrors.email = 'Invalid email format';
    }

    if (!password) {
      newErrors.password = 'Password is required';
    } else if (password.length < 8) {
      newErrors.password = 'Password must be at least 8 characters';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setErrors({});

    if (!validate()) return;

    setIsLoading(true);

    try {
      const loginResponse = await authApi.login({ email, password });
      localStorage.setItem('token', loginResponse.token);
      const profile = await authApi.getProfile();

      login(loginResponse.token, {
        userId: profile.userId,
        username: profile.username,
        email: profile.email,
        role: profile.role,
      });

      navigate('/tasks');
    } catch (err: unknown) {
      if (err && typeof err === 'object' && 'isAxiosError' in err) {
        const axiosError = err as { isAxiosError: boolean; response?: { status?: number; data?: string | { errors?: string[] } } };
        if (axiosError.response?.status === 401) {
          setErrors({ server: 'Invalid credentials' });
        } else if (axiosError.response?.data) {
          const data = axiosError.response.data;
          if (typeof data === 'string') {
            setErrors({ server: data });
          } else if (data.errors && data.errors.length > 0) {
            setErrors({ server: data.errors[0] });
          }
        } else {
          setErrors({ server: 'Connection error. Please try again.' });
        }
      } else {
        setErrors({ server: 'Connection error. Please try again.' });
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className={styles.container}>
      <div className={styles.card}>
        <div className={styles.header}>
          <div className={styles.logo}>S</div>
          <h1 className={styles.title}>Secure DevOps</h1>
          <p className={styles.subtitle}>Playground</p>
        </div>

        <form onSubmit={handleSubmit} className={styles.form}>
          <Input
            label="Email"
            type="email"
            placeholder="john@example.com"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            error={errors.email}
            autoComplete="email"
          />

          <Input
            label="Password"
            type="password"
            placeholder="••••••••"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            error={errors.password}
            autoComplete="current-password"
          />

          {errors.server && (
            <div className={styles.serverError}>{errors.server}</div>
          )}

          <Button type="submit" isLoading={isLoading}>
            Login
          </Button>
        </form>

        <div className={styles.footer}>
          <span>Don't have an account? </span>
          <Link to="/register" className={styles.link}>
            Register
          </Link>
        </div>
      </div>
    </div>
  );
}
