import api from './api'
import type {Category, CategoryFilters, PaginatedResponse} from '../types'
import type {CategoryFormData} from '../schemas/categorySchema'

export const categoryService = {
    getAll: (filters?: CategoryFilters) =>
        api.get<PaginatedResponse<Category>>('/categories', {params: filters}).then((response) => response.data),
    getById: (id: number) => api.get<Category | null>(`/categories/${id}`).then((response) => response.data),
    create: (data: CategoryFormData) => api.post<Category>('/categories', data).then((response) => response.data),
    update: (id: number, data: CategoryFormData) =>
        api.put<Category>(`/categories/${id}`, data).then((response) => response.data),
    deactivate: (id: number) => api.patch<Category>(`/categories/${id}/deactivate`).then((response) => response.data),
    activate: (id: number) => api.patch<Category>(`/categories/${id}/activate`).then((response) => response.data),
}
