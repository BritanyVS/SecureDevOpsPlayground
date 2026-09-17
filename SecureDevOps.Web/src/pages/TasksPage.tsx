import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { taskApi } from '../api/taskApi';
import type { TaskItem, DashboardStats } from '../api/taskApi';
import { TaskCard } from '../components/TaskCard';
import { Button } from '../components/Button';
import styles from './TasksPage.module.css';
import type { TaskStatus } from '../api/taskApi';

type PriorityFilter = 'All' | 'Low' | 'Medium' | 'High';

export function TasksPage() {
  const [tasks, setTasks] = useState<TaskItem[]>([]);
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [filter, setFilter] = useState<PriorityFilter>('All');
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const loadTasks = async () => {
    try {
      const [data, stat] = await Promise.all([
        taskApi.getAll(),
        taskApi.getStats().catch(() => null),
      ]);
      setTasks(data);
      setStats(stat);
    } catch (error) {
      console.error('Error loading tasks:', error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadTasks();
  }, []);

  const handleDelete = async (id: string) => {
    try {
      await taskApi.delete(id);
      setTasks(tasks.filter(t => t.id !== id));
      loadTasks();
    } catch (error) {
      console.error('Error deleting task:', error);
    }
  };

  const handleEdit = (id: string) => {
    navigate(`/tasks/edit/${id}`);
  };

  const handleChangeStatus = async (id: string, status: TaskStatus) => {
    try {
      await taskApi.updateStatus(id, status);
      setTasks(tasks.map(t => (t.id === id ? { ...t, status } : t)));
      loadTasks();
    } catch (error) {
      console.error('Error changing status:', error);
    }
  };

  const filtered = filter === 'All'
    ? tasks
    : tasks.filter(t => t.priority === filter);

  return (
    <div className={styles.container}>
      <header className={styles.header}>
        <div className={styles.headerLeft}>
          <div className={styles.logo}>S</div>
          <div className={styles.titleGroup}>
            <h1 className={styles.title}>Tasks</h1>
            <p className={styles.subtitle}>Welcome, {user?.username}</p>
          </div>
        </div>
        <div style={{ display: 'flex', gap: '0.5rem' }}>
          <Button variant="secondary" onClick={() => navigate('/users')}>
            Users
          </Button>
          <Button variant="secondary" onClick={() => navigate('/lab')}>
            Lab
          </Button>
          <Button variant="secondary" onClick={() => taskApi.exportCsv()}>
            Export CSV
          </Button>
          <Button variant="secondary" onClick={logout}>
            Logout
          </Button>
        </div>
      </header>

      <main className={styles.main}>
        {stats && (
          <div style={{
            display: 'flex',
            gap: '1rem',
            flexWrap: 'wrap',
            marginBottom: '1rem',
          }}>
            {[
              { label: 'Total', value: stats.totalTasks, color: '#4d3c61' },
              { label: 'Pending', value: stats.pending, color: '#f59e0b' },
              { label: 'In Progress', value: stats.inProgress, color: '#3b82f6' },
              { label: 'Completed', value: stats.completed, color: '#22c55e' },
              { label: 'High priority', value: stats.highPriority, color: '#ef4444' },
            ].map(kpi => (
              <div key={kpi.label} style={{
                flex: '1 1 120px',
                border: '1px solid var(--color-border, #e2e2e2)',
                borderRadius: '10px',
                padding: '0.75rem 1rem',
                background: '#fff',
              }}>
                <div style={{ fontSize: '0.75rem', color: '#666' }}>{kpi.label}</div>
                <div style={{ fontSize: '1.5rem', fontWeight: 700, color: kpi.color }}>{kpi.value}</div>
              </div>
            ))}
          </div>
        )}

        <div className={styles.topBar}>
          <div className={styles.filterBar}>
            {(['All', 'High', 'Medium', 'Low'] as PriorityFilter[]).map(p => (
              <button
                key={p}
                className={`${styles.filterButton} ${filter === p ? styles.filterActive : ''}`}
                onClick={() => setFilter(p)}
              >
                {p === 'All' ? 'All' : p}
              </button>
            ))}
          </div>
          <Button onClick={() => navigate('/tasks/new')}>
            + New Task
          </Button>
        </div>

        <div className={styles.listHeader}>
          <h2 className={styles.listTitle}>
            My Tasks
            <span className={styles.taskCount}>{filtered.length}</span>
          </h2>
        </div>

        {isLoading ? (
          <div className={styles.loading}>Loading tasks...</div>
        ) : filtered.length === 0 ? (
          <div className={styles.empty}>
            {tasks.length === 0
              ? 'No tasks yet. Create your first task!'
              : 'No tasks match this filter.'}
          </div>
        ) : (
          filtered.map(task => (
            <TaskCard
              key={task.id}
              task={task}
              onDelete={handleDelete}
              onEdit={handleEdit}
              onChangeStatus={handleChangeStatus}
            />
          ))
        )}
      </main>
    </div>
  );
}
