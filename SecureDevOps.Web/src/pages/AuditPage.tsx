import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { taskApi } from '../api/taskApi';
import { Button } from '../components/Button';
import styles from './TasksPage.module.css';

interface AuditEntry {
  timestamp: string;
  level: string;
  actor: string;
  email: string;
  message: string;
}

export function AuditPage() {
  const [logs, setLogs] = useState<AuditEntry[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    taskApi.getAuditLogs()
      .then(r => setLogs(r.logs))
      .catch(() => setLogs([]))
      .finally(() => setIsLoading(false));
  }, []);

  return (
    <div className={styles.container}>
      <header className={styles.header}>
        <div className={styles.headerLeft}>
          <div className={styles.logo}>S</div>
          <div className={styles.titleGroup}>
            <h1 className={styles.title}>Audit Logs</h1>
            <p className={styles.subtitle}>Welcome, {user?.username}</p>
          </div>
        </div>
        <div style={{ display: 'flex', gap: '0.5rem' }}>
          <Button variant="secondary" onClick={() => navigate('/tasks')}>Tasks</Button>
          <Button variant="secondary" onClick={() => navigate('/users')}>Users</Button>
          <Button variant="secondary" onClick={logout}>Logout</Button>
        </div>
      </header>

      <main className={styles.main}>
        {isLoading ? (
          <div className={styles.loading}>Loading audit logs...</div>
        ) : logs.length === 0 ? (
          <div className={styles.empty}>No audit entries.</div>
        ) : (
          logs.map((log, i) => (
            <div
              key={i}
              style={{
                fontFamily: 'Consolas, monospace',
                fontSize: '0.8125rem',
                border: '1px solid var(--color-border, #e2e2e2)',
                borderRadius: '8px',
                padding: '0.6rem 0.9rem',
                marginBottom: '0.4rem',
                background: '#16001f',
                color: '#e8e6f0',
              }}
            >
              <span style={{ color: '#9d8cff' }}>{log.timestamp}</span>{' '}
              <span style={{ color: '#4ade80' }}>[{log.level}]</span>{' '}
              {log.actor} &lt;{log.email}&gt; — {log.message}
            </div>
          ))
        )}
      </main>
    </div>
  );
}