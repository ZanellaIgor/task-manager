import api from './api'
import type {Task, TaskFilters} from '../types'
import type {TaskFormData} from '../schemas/taskSchema'

export const taskService = {
    getAll: (filters?: TaskFilters) => api.get<Task[]>('/tasks', {params: filters}).then((response) => response.data),
    getById: (id: number) => api.get<Task | null>(`/tasks/${id}`).then((response) => response.data),
    create: (data: TaskFormData) => api.post<Task>('/tasks', data).then((response) => response.data),
    update: (id: number, data: TaskFormData) => api.put<Task>(`/tasks/${id}`, data).then((response) => response.data),
    complete: (id: number) => api.patch<Task>(`/tasks/${id}/complete`).then((response) => response.data),
    cancel: (id: number) => api.patch<Task>(`/tasks/${id}/cancel`).then((response) => response.data),
    remove: (id: number) => api.delete<void>(`/tasks/${id}`).then((response) => response.data),
}
