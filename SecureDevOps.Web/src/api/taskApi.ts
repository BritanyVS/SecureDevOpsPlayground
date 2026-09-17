import axiosClient from './axiosClient';

export interface TaskItem {
  id: string;
  title: string;
  description?: string;
  status: 'Pending' | 'InProgress' | 'Completed';
  priority: 'Low' | 'Medium' | 'High';
  createdByUserId: string;
  assignedToUserName?: string;
  dueDate?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTaskRequest {
  title: string;
  description?: string;
  priority: 'Low' | 'Medium' | 'High';
  createdByUserId: string;
}

export interface UpdateTaskRequest {
  title: string;
  description?: string;
  priority: 'Low' | 'Medium' | 'High';
  status: 'Pending' | 'InProgress' | 'Completed';
}

export interface TaskComment {
  id: string;
  taskItemId: string;
  authorUserId?: string;
  authorUsername?: string;
  content: string;
  createdAt: string;
}

export interface DashboardStats {
  totalTasks: number;
  pending: number;
  inProgress: number;
  completed: number;
  lowPriority: number;
  mediumPriority: number;
  highPriority: number;
  byStatus: Record<string, number>;
  byPriority: Record<string, number>;
  topAssignees: { username: string; count: number }[];
  recentTasks: TaskItem[];
}

export interface AppUser {
  id: string;
  username: string;
  email: string;
  role: string;
  isActive: boolean;
  createdAt: string;
}

export type TaskStatus = 'Pending' | 'InProgress' | 'Completed';

export const taskApi = {
  getAll: async (): Promise<TaskItem[]> => {
    const response = await axiosClient.get<TaskItem[]>('/taskitem');
    return response.data;
  },

  getById: async (id: string): Promise<TaskItem> => {
    const response = await axiosClient.get<TaskItem>(`/taskitem/${id}`);
    return response.data;
  },

  create: async (data: CreateTaskRequest): Promise<TaskItem> => {
    const response = await axiosClient.post<TaskItem>('/taskitem', data);
    return response.data;
  },

  update: async (id: string, data: UpdateTaskRequest): Promise<TaskItem> => {
    const response = await axiosClient.put<TaskItem>(`/taskitem/${id}`, data);
    return response.data;
  },

  updateStatus: async (id: string, status: TaskStatus): Promise<TaskItem> => {
    const response = await axiosClient.patch<TaskItem>(`/taskitem/${id}/status`, { status });
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await axiosClient.delete(`/taskitem/${id}`);
  },

  getComments: async (taskId: string): Promise<TaskComment[]> => {
    const response = await axiosClient.get<TaskComment[]>(`/taskitem/${taskId}/comments`);
    return response.data;
  },

  addComment: async (taskId: string, content: string, authorUserId?: string): Promise<TaskComment> => {
    const response = await axiosClient.post<TaskComment>(`/taskitem/${taskId}/comments`, {
      content,
      authorUserId,
    });
    return response.data;
  },

  exportCsv: async (): Promise<void> => {
    const response = await axiosClient.get<Blob>('/taskitem/export?format=csv', {
      responseType: 'blob',
    });
    const url = URL.createObjectURL(response.data);
    const link = document.createElement('a');
    link.href = url;
    link.download = 'tasks.csv';
    link.click();
    URL.revokeObjectURL(url);
  },

  getStats: async (): Promise<DashboardStats> => {
    const response = await axiosClient.get<DashboardStats>('/dashboard/stats');
    return response.data;
  },

  getUsers: async (): Promise<AppUser[]> => {
    const response = await axiosClient.get<AppUser[]>('/users');
    return response.data;
  },

  getAuditLogs: async (): Promise<{ logs: { timestamp: string; level: string; actor: string; email: string; message: string }[]; count: number }> => {
    const response = await axiosClient.get('/audit/logs');
    return response.data;
  },
};
