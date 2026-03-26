import api from './api'
import type {PaginatedResponse, Task, TaskFilters, TaskOverview} from '../types'
import type {TaskFormData} from '../schemas/taskSchema'

export const taskService = {
    getAll: (filters?: TaskFilters): Promise<PaginatedResponse<Task>> =>
        api.get<PaginatedResponse<Task>>('/tasks', {params: filters}).then((response) => response.data),

    getOverview: (): Promise<TaskOverview> =>
        api.get<TaskOverview>('/tasks/overview').then((response) => response.data),

    getById: (id: number): Promise<Task | null> =>
        api.get<Task | null>(`/tasks/${id}`).then((response) => response.data),

    create: (data: TaskFormData): Promise<Task> =>
        api.post<Task>('/tasks', data).then((response) => response.data),

    update: (id: number, data: TaskFormData): Promise<Task> =>
        api.put<Task>(`/tasks/${id}`, data).then((response) => response.data),

    complete: (id: number): Promise<Task> =>
        api.patch<Task>(`/tasks/${id}/complete`).then((response) => response.data),

    cancel: (id: number): Promise<Task> =>
        api.patch<Task>(`/tasks/${id}/cancel`).then((response) => response.data),

    remove: (id: number): Promise<void> =>
        api.delete<void>(`/tasks/${id}`).then((response) => response.data),
}
