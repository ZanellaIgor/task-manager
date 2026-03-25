export type TaskStatus = 'Pending' | 'InProgress' | 'Completed' | 'Cancelled'

export type TaskPriority = 'Low' | 'Medium' | 'High'

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

export interface TaskFilters {
    status?: TaskStatus
    priority?: TaskPriority
    categoryId?: number
    search?: string
}

