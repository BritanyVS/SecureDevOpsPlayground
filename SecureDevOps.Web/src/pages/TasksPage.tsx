import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { taskApi } from '../api/taskApi';
import type { TaskItem } from '../api/taskApi';
import { TaskCard } from '../components/TaskCard';
import { Button } from '../components/Button';
import styles from './TasksPage.module.css';

type PriorityFilter = 'All' | 'Low' | 'Medium' | 'High';

export function TasksPage() {
  const [tasks, setTasks] = useState<TaskItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [filter, setFilter] = useState<PriorityFilter>('All');
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const loadTasks = async () => {
    try {
      const data = await taskApi.getAll();
      setTasks(data);
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
    } catch (error) {
      console.error('Error deleting task:', error);
    }
  };

  const handleEdit = (id: string) => {
    navigate(`/tasks/edit/${id}`);
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
        <Button variant="secondary" onClick={logout}>
          Logout
        </Button>
      </header>

      <main className={styles.main}>
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
            />
          ))
        )}
      </main>
    </div>
  );
}
