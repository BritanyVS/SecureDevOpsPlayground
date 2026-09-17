import { useNavigate } from 'react-router-dom';
import type { TaskItem, TaskStatus } from '../api/taskApi';
import styles from './TaskCard.module.css';

interface TaskCardProps {
  task: TaskItem;
  onDelete: (id: string) => void;
  onEdit: (id: string) => void;
  onChangeStatus?: (id: string, status: TaskStatus) => void;
}

const priorityColors = {
  High: '#ef4444',
  Medium: '#f59e0b',
  Low: '#22c55e',
};

const statusLabels = {
  Pending: 'Pending',
  InProgress: 'In Progress',
  Completed: 'Completed',
};

export function TaskCard({ task, onDelete, onEdit, onChangeStatus }: TaskCardProps) {
  const navigate = useNavigate();

  return (
    <div
      className={styles.card}
      onClick={() => navigate(`/tasks/edit/${task.id}`)}
    >
      <div className={styles.header}>
        <span
          className={styles.priority}
          style={{ backgroundColor: priorityColors[task.priority] }}
        >
          {task.priority}
        </span>
        {onChangeStatus ? (
          <select
            className={styles.status}
            value={task.status}
            onClick={(e) => e.stopPropagation()}
            onChange={(e) => onChangeStatus(task.id, e.target.value as TaskStatus)}
          >
            <option value="Pending">Pending</option>
            <option value="InProgress">In Progress</option>
            <option value="Completed">Completed</option>
          </select>
        ) : (
          <span className={styles.status}>{statusLabels[task.status]}</span>
        )}
        <div className={styles.actions}>
          <button
            className={styles.editButton}
            onClick={(e) => { e.stopPropagation(); onEdit(task.id); }}
          >
            Edit
          </button>
          <button
            className={styles.deleteButton}
            onClick={(e) => { e.stopPropagation(); onDelete(task.id); }}
          >
            Delete
          </button>
        </div>
      </div>

      <h3 className={styles.title}>{task.title}</h3>

      {task.description && (
        <p className={styles.description}>{task.description}</p>
      )}

      <div className={styles.footer}>
        <span className={styles.date}>
          Created: {new Date(task.createdAt).toLocaleDateString()}
        </span>
        {task.dueDate && (
          <span className={styles.dueDate}>
            Due: {new Date(task.dueDate).toLocaleDateString()}
          </span>
        )}
      </div>
    </div>
  );
}