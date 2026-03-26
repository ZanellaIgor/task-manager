import api from './api'
import type {Category, CategoryFilters, PaginatedResponse} from '../types'
import type {CategoryFormData} from '../schemas/categorySchema'

export const categoryService = {
    getAll: (filters?: CategoryFilters): Promise<PaginatedResponse<Category>> =>
        api.get<PaginatedResponse<Category>>('/categories', {params: filters}).then((response) => response.data),

    getById: (id: number): Promise<Category | null> =>
        api.get<Category | null>(`/categories/${id}`).then((response) => response.data),

    create: (data: CategoryFormData): Promise<Category> =>
        api.post<Category>('/categories', data).then((response) => response.data),

    update: (id: number, data: CategoryFormData): Promise<Category> =>
        api.put<Category>(`/categories/${id}`, data).then((response) => response.data),

    deactivate: (id: number): Promise<Category> =>
        api.patch<Category>(`/categories/${id}/deactivate`).then((response) => response.data),

    activate: (id: number): Promise<Category> =>
        api.patch<Category>(`/categories/${id}/activate`).then((response) => response.data),
}
