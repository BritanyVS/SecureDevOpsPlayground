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

  delete: async (id: string): Promise<void> => {
    await axiosClient.delete(`/taskitem/${id}`);
  },
};
