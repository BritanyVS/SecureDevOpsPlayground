import { useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { taskApi } from '../api/taskApi';
import { TaskForm } from '../components/TaskForm';
import styles from './TaskFormPage.module.css';

export function CreateTaskPage() {
  const { user } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (title: string, description: string, priority: 'Low' | 'Medium' | 'High') => {
    if (!user) return;

    await taskApi.create({
      title,
      description,
      priority,
      createdByUserId: user.userId,
    });
    navigate('/tasks');
  };

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
      </header>

      <main className={styles.main}>
        <div className={styles.breadcrumb}>
          <button className={styles.backLink} onClick={() => navigate('/tasks')}>
            &larr; Back to Tasks
          </button>
        </div>
        <TaskForm onSubmit={handleSubmit} />
      </main>
    </div>
  );
}
