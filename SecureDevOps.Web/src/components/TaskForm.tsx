import { useState } from 'react';
import type { FormEvent } from 'react';
import { Input } from './Input';
import { Button } from './Button';
import styles from './TaskForm.module.css';

interface TaskFormProps {
  onSubmit: (title: string, description: string, priority: 'Low' | 'Medium' | 'High') => void;
  isLoading?: boolean;
}

export function TaskForm({ onSubmit, isLoading }: TaskFormProps) {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [priority, setPriority] = useState<'Low' | 'Medium' | 'High'>('Medium');
  const [errors, setErrors] = useState<{ title?: string }>({});

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    setErrors({});

    if (!title.trim()) {
      setErrors({ title: 'Title is required' });
      return;
    }

    onSubmit(title.trim(), description.trim(), priority);
    setTitle('');
    setDescription('');
    setPriority('Medium');
  };

  return (
    <div className={styles.form}>
      <h2 className={styles.formTitle}>New Task</h2>
      <form onSubmit={handleSubmit}>
        <Input
          label="Title"
          type="text"
          placeholder="Enter task title"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          error={errors.title}
        />

        <div className={styles.field}>
          <label className={styles.label}>Description</label>
          <textarea
            className={styles.textarea}
            placeholder="Enter description (optional)"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            rows={3}
          />
        </div>

        <div className={styles.field}>
          <label className={styles.label}>Priority</label>
          <select
            className={styles.select}
            value={priority}
            onChange={(e) => setPriority(e.target.value as 'Low' | 'Medium' | 'High')}
          >
            <option value="Low">Low</option>
            <option value="Medium">Medium</option>
            <option value="High">High</option>
          </select>
        </div>

        <Button type="submit" isLoading={isLoading}>
          Create Task
        </Button>
      </form>
    </div>
  );
}
