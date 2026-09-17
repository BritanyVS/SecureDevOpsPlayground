import { useState, useEffect } from 'react';
import type { FormEvent } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { taskApi } from '../api/taskApi';
import type { TaskItem, TaskComment } from '../api/taskApi';
import { Input } from '../components/Input';
import { Button } from '../components/Button';
import styles from './TaskFormPage.module.css';
import formStyles from '../components/TaskForm.module.css';

export function EditTaskPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [task, setTask] = useState<TaskItem | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);

  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [priority, setPriority] = useState<'Low' | 'Medium' | 'High'>('Medium');
  const [status, setStatus] = useState<'Pending' | 'InProgress' | 'Completed'>('Pending');
  const [errors, setErrors] = useState<{ title?: string; server?: string }>({});

  const [comments, setComments] = useState<TaskComment[]>([]);
  const [newComment, setNewComment] = useState('');
  const [isAddingComment, setIsAddingComment] = useState(false);

  useEffect(() => {
    if (!id) return;
    taskApi.getById(id)
      .then(t => {
        setTask(t);
        setTitle(t.title);
        setDescription(t.description || '');
        setPriority(t.priority);
        setStatus(t.status);
      })
      .catch(() => navigate('/tasks'))
      .finally(() => setIsLoading(false));

    taskApi.getComments(id)
      .then(setComments)
      .catch(() => setComments([]));
  }, [id, navigate]);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setErrors({});

    if (!title.trim()) {
      setErrors({ title: 'Title is required' });
      return;
    }
    if (!task) return;

    setIsSaving(true);
    try {
      await taskApi.update(task.id, {
        title: title.trim(),
        description: description.trim(),
        priority,
        status,
      });
      navigate('/tasks');
    } catch {
      setErrors({ server: 'Failed to update task' });
    } finally {
      setIsSaving(false);
    }
  };

  const handleAddComment = async (e: FormEvent) => {
    e.preventDefault();
    if (!task || !newComment.trim()) return;
    setIsAddingComment(true);
    try {
      const authorUserId = localStorage.getItem('user')
        ? JSON.parse(localStorage.getItem('user')!).userId
        : undefined;
      const created = await taskApi.addComment(task.id, newComment.trim(), authorUserId);
      setComments([created, ...comments]);
      setNewComment('');
    } catch {
      /* comentario no guardado */
    } finally {
      setIsAddingComment(false);
    }
  };

  if (isLoading) {
    return (
      <div className={styles.container}>
        <main className={styles.main}>
          <div className={styles.loading}>Loading task...</div>
        </main>
      </div>
    );
  }

  if (!task) return null;

  return (
    <div className={styles.container}>
      <header className={styles.header}>
        <div className={styles.headerLeft}>
          <div className={styles.logo}>S</div>
          <div className={styles.titleGroup}>
            <h1 className={styles.title}>Tasks</h1>
            <p className={styles.subtitle}>Edit task</p>
          </div>
        </div>
      </header>

      <main className={styles.main}>
        <div className={styles.breadcrumb}>
          <button className={styles.backLink} onClick={() => navigate('/tasks')}>
            &larr; Back to Tasks
          </button>
        </div>

        <div className={formStyles.form}>
          <h2 className={formStyles.formTitle}>Edit Task</h2>
          <form onSubmit={handleSubmit}>
            <Input
              label="Title"
              type="text"
              placeholder="Enter task title"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              error={errors.title}
            />

            <div className={formStyles.field}>
              <label className={formStyles.label}>Description</label>
              <textarea
                className={formStyles.textarea}
                placeholder="Enter description (optional)"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                rows={3}
              />
            </div>

            <div className={formStyles.field}>
              <label className={formStyles.label}>Priority</label>
              <select
                className={formStyles.select}
                value={priority}
                onChange={(e) => setPriority(e.target.value as 'Low' | 'Medium' | 'High')}
              >
                <option value="Low">Low</option>
                <option value="Medium">Medium</option>
                <option value="High">High</option>
              </select>
            </div>

            <div className={formStyles.field}>
              <label className={formStyles.label}>Status</label>
              <select
                className={formStyles.select}
                value={status}
                onChange={(e) => setStatus(e.target.value as 'Pending' | 'InProgress' | 'Completed')}
              >
                <option value="Pending">Pending</option>
                <option value="InProgress">In Progress</option>
                <option value="Completed">Completed</option>
              </select>
            </div>

            {errors.server && (
              <div className={formStyles.field}>
                <span style={{ color: 'var(--color-danger)', fontSize: '0.8125rem', fontWeight: 500 }}>
                  {errors.server}
                </span>
              </div>
            )}

            <Button type="submit" isLoading={isSaving}>
              Save Changes
            </Button>
          </form>
        </div>

        <div style={{ marginTop: '1.5rem' }}>
          <h2 style={{ marginBottom: '0.5rem' }}>Comments ({comments.length})</h2>
          <form onSubmit={handleAddComment} style={{ display: 'flex', gap: '0.5rem', marginBottom: '1rem' }}>
            <textarea
              placeholder="Add a comment..."
              value={newComment}
              onChange={(e) => setNewComment(e.target.value)}
              rows={2}
              style={{ flex: 1 }}
            />
            <Button type="submit" isLoading={isAddingComment} disabled={!newComment.trim()}>
              Add
            </Button>
          </form>
          {(comments ?? []).map(c => (
            <div
              key={c.id}
              style={{
                border: '1px solid var(--color-border, #e2e2e2)',
                borderRadius: '8px',
                padding: '0.6rem 0.9rem',
                marginBottom: '0.5rem',
                background: '#fff',
              }}
            >
              <div style={{ fontSize: '0.75rem', color: '#666' }}>
                <strong>{c.authorUsername || 'anonymous'}</strong> · {new Date(c.createdAt).toLocaleString()}
              </div>
              <div style={{ whiteSpace: 'pre-wrap' }}>{c.content}</div>
            </div>
          ))}
        </div>
      </main>
    </div>
  );
}
