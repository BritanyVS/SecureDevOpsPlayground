import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { taskApi } from '../api/taskApi';
import type { AppUser } from '../api/taskApi';
import { Button } from '../components/Button';
import styles from './TasksPage.module.css';

export function UsersPage() {
  const [users, setUsers] = useState<AppUser[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    taskApi.getUsers()
      .then(setUsers)
      .catch(() => setUsers([]))
      .finally(() => setIsLoading(false));
  }, []);

  return (
    <div className={styles.container}>
      <header className={styles.header}>
        <div className={styles.headerLeft}>
          <div className={styles.logo}>S</div>
          <div className={styles.titleGroup}>
            <h1 className={styles.title}>Users</h1>
            <p className={styles.subtitle}>Welcome, {user?.username}</p>
          </div>
        </div>
        <div style={{ display: 'flex', gap: '0.5rem' }}>
          <Button variant="secondary" onClick={() => navigate('/tasks')}>Tasks</Button>
          <Button variant="secondary" onClick={() => navigate('/audit')}>Audit</Button>
          <Button variant="secondary" onClick={logout}>Logout</Button>
        </div>
      </header>

      <main className={styles.main}>
        {isLoading ? (
          <div className={styles.loading}>Loading users...</div>
        ) : users.length === 0 ? (
          <div className={styles.empty}>No users found.</div>
        ) : (
          users.map(u => (
            <div
              key={u.id}
              style={{
                border: '1px solid var(--color-border, #e2e2e2)',
                borderRadius: '10px',
                padding: '0.9rem 1rem',
                marginBottom: '0.6rem',
                background: '#fff',
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
              }}
            >
              <div>
                <div style={{ fontWeight: 600 }}>{u.username}</div>
                <div style={{ fontSize: '0.8125rem', color: '#666' }}>{u.email}</div>
              </div>
              <span
                style={{
                  padding: '0.2rem 0.6rem',
                  borderRadius: '999px',
                  fontSize: '0.75rem',
                  background: u.role === 'Admin' ? '#4d3c61' : '#e2e2e2',
                  color: u.role === 'Admin' ? '#fff' : '#333',
                }}
              >
                {u.role}
              </span>
            </div>
          ))
        )}
      </main>
    </div>
  );
}