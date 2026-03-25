export type TaskStatus = 'Pending' | 'InProgress' | 'Completed' | 'Cancelled'

export type TaskPriority = 'Low' | 'Medium' | 'High'

export type SortDirection = 'Asc' | 'Desc'

export interface ListParams {
    page?: number
    pageSize?: number
    search?: string
    sortBy?: string
    sortDirection?: SortDirection
}

export interface PaginatedResponse<T> {
    items: T[]
    page: number
    pageSize: number
    totalItems: number
    totalPages: number
}

export interface ApiProblemDetails {
    type?: string
    title?: string
    status?: number
    detail?: string
    instance?: string
    code?: string
    traceId?: string
    errors?: Record<string, string[]>
}

export interface Category {
    id: number
    name: string
    description?: string
    isActive: boolean
    createdAt: string
    updatedAt: string
}

export interface Task {
    id: number
    title: string
    description?: string
    categoryId: number
    category?: Pick<Category, 'id' | 'name'>
    status: TaskStatus
    priority: TaskPriority
    createdAt: string
    updatedAt: string
    dueDate?: string
    completedAt?: string
}

export interface TaskFilters extends ListParams {
    status?: TaskStatus
    priority?: TaskPriority
    categoryId?: number
}

export interface CategoryFilters extends ListParams {
    isActive?: boolean
}

export interface TaskOverview {
    totalCount: number
    pendingCount: number
    inProgressCount: number
    completedCount: number
    recentTasks: Task[]
    upcomingTasks: Task[]
}
