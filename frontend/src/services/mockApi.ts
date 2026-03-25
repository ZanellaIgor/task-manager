import type {AxiosResponse, InternalAxiosRequestConfig} from 'axios'
import type {Category, CategoryFilters, PaginatedResponse, SortDirection, Task, TaskFilters, TaskOverview, TaskStatus} from '../types'
import type {CategoryFormData} from '../schemas/categorySchema'
import type {TaskFormData} from '../schemas/taskSchema'

type MockMethod = 'get' | 'post' | 'put' | 'patch' | 'delete'

interface MockDatabase {
  tasks: Task[]
  categories: Category[]
}

interface MockResponseData {
  data: unknown
}

const STORAGE_KEY = 'TaskManager-task-manager:mock-db'

const initialDatabase: MockDatabase = {
  categories: [
    {
      id: 1,
      name: 'Operações',
      description: 'Rotinas e acompanhamento operacional.',
      isActive: true,
      createdAt: '2026-03-01T09:00:00.000Z',
      updatedAt: '2026-03-01T09:00:00.000Z',
    },
    {
      id: 2,
      name: 'Produto',
      description: 'Backlog e entregas de produto.',
      isActive: true,
      createdAt: '2026-03-02T09:00:00.000Z',
      updatedAt: '2026-03-02T09:00:00.000Z',
    },
    {
      id: 3,
      name: 'Suporte',
      description: 'Atendimento e resolução de incidentes.',
      isActive: false,
      createdAt: '2026-03-03T09:00:00.000Z',
      updatedAt: '2026-03-05T09:00:00.000Z',
    },
  ],
  tasks: [
    {
      id: 1,
      title: 'Preparar reunião semanal',
      description: 'Consolidar status dos itens críticos da semana.',
      categoryId: 1,
      category: {id: 1, name: 'Operações'},
      status: 'InProgress',
      priority: 'High',
      createdAt: '2026-03-18T14:00:00.000Z',
      updatedAt: '2026-03-22T10:00:00.000Z',
      dueDate: '2026-03-24T10:00:00.000Z',
    },
    {
      id: 2,
      title: 'Revisar backlog de produto',
      description: 'Separar demandas por impacto e esforço.',
      categoryId: 2,
      category: {id: 2, name: 'Produto'},
      status: 'Pending',
      priority: 'Medium',
      createdAt: '2026-03-19T10:00:00.000Z',
      updatedAt: '2026-03-19T10:00:00.000Z',
      dueDate: '2026-03-26T18:00:00.000Z',
    },
    {
      id: 3,
      title: 'Resolver ticket premium',
      description: 'Acompanhar a resposta do cliente.',
      categoryId: 3,
      category: {id: 3, name: 'Suporte'},
      status: 'Completed',
      priority: 'High',
      createdAt: '2026-03-17T08:30:00.000Z',
      updatedAt: '2026-03-20T12:30:00.000Z',
      dueDate: '2026-03-20T15:00:00.000Z',
      completedAt: '2026-03-20T12:30:00.000Z',
    },
  ],
}

function readDatabase(): MockDatabase {
  if (typeof window === 'undefined') {
    return structuredClone(initialDatabase)
  }

  const raw = window.localStorage.getItem(STORAGE_KEY)

  if (!raw) {
    window.localStorage.setItem(STORAGE_KEY, JSON.stringify(initialDatabase))
    return structuredClone(initialDatabase)
  }

  try {
    return JSON.parse(raw) as MockDatabase
  } catch {
    window.localStorage.setItem(STORAGE_KEY, JSON.stringify(initialDatabase))
    return structuredClone(initialDatabase)
  }
}

function writeDatabase(database: MockDatabase) {
  if (typeof window === 'undefined') {
    return
  }

  window.localStorage.setItem(STORAGE_KEY, JSON.stringify(database))
}

function timestamp() {
  return new Date().toISOString()
}

function nextId(items: Array<{ id: number }>) {
  return items.reduce((max, item) => Math.max(max, item.id), 0) + 1
}

function parseDate(value: unknown) {
  if (typeof value !== 'string' || !value.trim()) {
    return undefined
  }

  const parsed = new Date(value)
  return Number.isNaN(parsed.getTime()) ? undefined : parsed.toISOString()
}

function buildAxiosResponse<T>(config: InternalAxiosRequestConfig, data: T): AxiosResponse<T> {
  return {
    data,
    status: 200,
    statusText: 'OK',
    headers: {},
    config,
  }
}

function parseRequestData<T>(data: unknown) {
  if (typeof data !== 'string') {
    return data as T
  }

  try {
    return JSON.parse(data) as T
  } catch {
    return data as T
  }
}

function parseRequestParams(config: InternalAxiosRequestConfig) {
  return (config.params ?? {}) as Record<string, string | number | boolean | undefined>
}

function getPathParts(url?: string) {
  if (!url) {
    return []
  }

  const resolved = new URL(url, 'http://mock.api.local/api')
  const path = resolved.pathname.replace(/^\/api/, '')
  return path.split('/').filter(Boolean)
}

function getTaskCategory(database: MockDatabase, categoryId: number) {
  return database.categories.find((category) => category.id === categoryId)
}

function hydrateTask(database: MockDatabase, task: Task): Task {
  const category = database.categories.find((item) => item.id === task.categoryId)

  return {
    ...task,
    category: category ? {id: category.id, name: category.name} : task.category,
  }
}

function serializeTaskInput(input: TaskFormData, base: Partial<Task> = {}): Task {
  const createdAt = base.createdAt ?? timestamp()
  const updatedAt = timestamp()
  const dueDate = parseDate(input.dueDate)

  return {
    id: base.id ?? 0,
    title: input.title.trim(),
    description: input.description?.trim() || undefined,
    categoryId: input.categoryId,
    status: base.status ?? 'Pending',
    priority: input.priority,
    createdAt,
    updatedAt,
    dueDate,
    completedAt: base.completedAt,
  }
}

function serializeCategoryInput(input: CategoryFormData, base: Partial<Category> = {}): Category {
  const currentTimestamp = timestamp()

  return {
    id: base.id ?? 0,
    name: input.name.trim(),
    description: input.description?.trim() || undefined,
    isActive: base.isActive ?? true,
    createdAt: base.createdAt ?? currentTimestamp,
    updatedAt: currentTimestamp,
  }
}

function compareValues(left: string | number | undefined, right: string | number | undefined, direction: SortDirection) {
  const leftValue = left ?? ''
  const rightValue = right ?? ''

  if (leftValue === rightValue) {
    return 0
  }

  if (direction === 'Asc') {
    return leftValue > rightValue ? 1 : -1
  }

  return leftValue > rightValue ? -1 : 1
}

function paginate<T>(items: T[], page = 1, pageSize = 10): PaginatedResponse<T> {
  const safePage = page > 0 ? page : 1
  const safePageSize = pageSize > 0 ? pageSize : 10
  const totalItems = items.length
  const totalPages = totalItems === 0 ? 0 : Math.ceil(totalItems / safePageSize)
  const start = (safePage - 1) * safePageSize

  return {
    items: items.slice(start, start + safePageSize),
    page: safePage,
    pageSize: safePageSize,
    totalItems,
    totalPages,
  }
}

function filterTasks(tasks: Task[], filters: TaskFilters | undefined, database: MockDatabase) {
  const items = tasks.filter((task) => {
    if (filters?.status && task.status !== filters.status) {
      return false
    }

    if (filters?.priority && task.priority !== filters.priority) {
      return false
    }

    if (filters?.categoryId && task.categoryId !== filters.categoryId) {
      return false
    }

    const search = filters?.search?.trim().toLowerCase()
    if (!search) {
      return true
    }

    const category = database.categories.find((item) => item.id === task.categoryId)
    const searchable = [task.title, task.description ?? '', category?.name ?? ''].join(' ').toLowerCase()
    return searchable.includes(search)
  })

  const sortBy = filters?.sortBy ?? 'createdAt'
  const direction = filters?.sortDirection ?? 'Desc'

  return [...items].sort((left, right) => {
    if (sortBy === 'title') {
      return compareValues(left.title, right.title, direction)
    }

    if (sortBy === 'updatedAt') {
      return compareValues(left.updatedAt, right.updatedAt, direction)
    }

    if (sortBy === 'dueDate') {
      return compareValues(left.dueDate, right.dueDate, direction)
    }

    if (sortBy === 'priority') {
      return compareValues(left.priority, right.priority, direction)
    }

    if (sortBy === 'status') {
      return compareValues(left.status, right.status, direction)
    }

    return compareValues(left.createdAt, right.createdAt, direction)
  })
}

function filterCategories(categories: Category[], filters: CategoryFilters | undefined) {
  const items = categories.filter((category) => {
    if (typeof filters?.isActive === 'boolean' && category.isActive !== filters.isActive) {
      return false
    }

    const search = filters?.search?.trim().toLowerCase()
    if (!search) {
      return true
    }

    return [category.name, category.description ?? ''].join(' ').toLowerCase().includes(search)
  })

  const sortBy = filters?.sortBy ?? 'name'
  const direction = filters?.sortDirection ?? 'Asc'

  return [...items].sort((left, right) => {
    if (sortBy === 'createdAt') {
      return compareValues(left.createdAt, right.createdAt, direction)
    }

    if (sortBy === 'updatedAt') {
      return compareValues(left.updatedAt, right.updatedAt, direction)
    }

    return compareValues(left.name, right.name, direction)
  })
}

function getTaskOverview(database: MockDatabase): TaskOverview {
  const tasks = database.tasks.map((task) => hydrateTask(database, task))
  const today = new Date()
  const limit = new Date()
  limit.setDate(today.getDate() + 3)

  return {
    totalCount: tasks.length,
    pendingCount: tasks.filter((task) => task.status === 'Pending').length,
    inProgressCount: tasks.filter((task) => task.status === 'InProgress').length,
    completedCount: tasks.filter((task) => task.status === 'Completed').length,
    recentTasks: [...tasks]
      .sort((left, right) => new Date(right.updatedAt).getTime() - new Date(left.updatedAt).getTime())
      .slice(0, 5),
    upcomingTasks: tasks
      .filter((task) => {
        if (!task.dueDate || task.status === 'Completed' || task.status === 'Cancelled') {
          return false
        }

        const dueDate = new Date(task.dueDate)
        return dueDate >= today && dueDate <= limit
      })
      .sort((left, right) => new Date(left.dueDate ?? '').getTime() - new Date(right.dueDate ?? '').getTime())
      .slice(0, 5),
  }
}

function handleTasksRequest(
  method: MockMethod,
  parts: string[],
  config: InternalAxiosRequestConfig,
  database: MockDatabase,
): MockResponseData | undefined {
  if (parts[0] !== 'tasks') {
    return undefined
  }

  if (method === 'get' && parts.length === 2 && parts[1] === 'overview') {
    return {data: getTaskOverview(database)}
  }

  if (method === 'get' && parts.length === 1) {
    const filters = parseRequestParams(config) as TaskFilters
    const tasks = filterTasks(database.tasks.map((task) => hydrateTask(database, task)), filters, database)
    return {data: paginate(tasks, Number(filters.page ?? 1), Number(filters.pageSize ?? 10))}
  }

  if (method === 'get' && parts.length === 2) {
    const id = Number(parts[1])
    const task = database.tasks.find((item) => item.id === id)
    return {data: task ? hydrateTask(database, task) : null}
  }

  if (method === 'post' && parts.length === 1) {
    const payload = parseRequestData<TaskFormData>(config.data)
    const task = serializeTaskInput(payload, {id: nextId(database.tasks)})
    const category = getTaskCategory(database, task.categoryId)
    const hydrated = {
      ...task,
      category: category ? {id: category.id, name: category.name} : undefined,
    }

    database.tasks.unshift(hydrated)
    writeDatabase(database)
    return {data: hydrated}
  }

  if (method === 'put' && parts.length === 2) {
    const id = Number(parts[1])
    const index = database.tasks.findIndex((item) => item.id === id)
    if (index < 0) {
      return {data: null}
    }

    const existing = database.tasks[index]
    const payload = parseRequestData<TaskFormData>(config.data)
    const task = serializeTaskInput(payload, existing)
    const category = getTaskCategory(database, task.categoryId)
    const hydrated = {
      ...task,
      id,
      createdAt: existing.createdAt,
      completedAt: task.status === 'Completed' ? existing.completedAt ?? timestamp() : undefined,
      category: category ? {id: category.id, name: category.name} : undefined,
    }

    database.tasks[index] = hydrated
    writeDatabase(database)
    return {data: hydrated}
  }

  if (method === 'patch' && parts.length === 3) {
    const id = Number(parts[1])
    const action = parts[2]
    const index = database.tasks.findIndex((item) => item.id === id)
    if (index < 0) {
      return {data: null}
    }

    const task = database.tasks[index]
    const status: TaskStatus = action === 'complete' ? 'Completed' : 'Cancelled'
    const updated: Task = {
      ...task,
      status,
      completedAt: status === 'Completed' ? timestamp() : undefined,
      updatedAt: timestamp(),
    }

    database.tasks[index] = updated
    writeDatabase(database)
    return {data: hydrateTask(database, updated)}
  }

  if (method === 'delete' && parts.length === 2) {
    const id = Number(parts[1])
    database.tasks = database.tasks.filter((item) => item.id !== id)
    writeDatabase(database)
    return {data: null}
  }

  return undefined
}

function handleCategoriesRequest(
  method: MockMethod,
  parts: string[],
  config: InternalAxiosRequestConfig,
  database: MockDatabase,
): MockResponseData | undefined {
  if (parts[0] !== 'categories') {
    return undefined
  }

  if (method === 'get' && parts.length === 1) {
    const filters = parseRequestParams(config) as CategoryFilters
    const categories = filterCategories(database.categories, filters)
    return {data: paginate(categories, Number(filters.page ?? 1), Number(filters.pageSize ?? 10))}
  }

  if (method === 'get' && parts.length === 2) {
    const id = Number(parts[1])
    const category = database.categories.find((item) => item.id === id)
    return {data: category ?? null}
  }

  if (method === 'post' && parts.length === 1) {
    const payload = parseRequestData<CategoryFormData>(config.data)
    const category = serializeCategoryInput(payload, {id: nextId(database.categories)})

    database.categories.unshift(category)
    writeDatabase(database)
    return {data: category}
  }

  if (method === 'put' && parts.length === 2) {
    const id = Number(parts[1])
    const index = database.categories.findIndex((item) => item.id === id)
    if (index < 0) {
      return {data: null}
    }

    const existing = database.categories[index]
    const payload = parseRequestData<CategoryFormData>(config.data)
    const category = serializeCategoryInput(payload, {...existing, id})

    database.categories[index] = category
    database.tasks = database.tasks.map((task) =>
      task.categoryId === id ? {...task, category: {id, name: category.name}} : task,
    )
    writeDatabase(database)
    return {data: category}
  }

  if (method === 'patch' && parts.length === 3 && parts[2] === 'deactivate') {
    const id = Number(parts[1])
    const index = database.categories.findIndex((item) => item.id === id)
    if (index < 0) {
      return {data: null}
    }

    const category = {
      ...database.categories[index],
      isActive: false,
      updatedAt: timestamp(),
    }

    database.categories[index] = category
    writeDatabase(database)
    return {data: category}
  }

  if (method === 'patch' && parts.length === 3 && parts[2] === 'activate') {
    const id = Number(parts[1])
    const index = database.categories.findIndex((item) => item.id === id)
    if (index < 0) {
      return {data: null}
    }

    const category = {
      ...database.categories[index],
      isActive: true,
      updatedAt: timestamp(),
    }

    database.categories[index] = category
    writeDatabase(database)
    return {data: category}
  }

  return undefined
}

export async function mockRequest(config: InternalAxiosRequestConfig) {
  const database = readDatabase()
  const method = (config.method ?? 'get').toLowerCase() as MockMethod
  const parts = getPathParts(config.url)

  const taskResponse = handleTasksRequest(method, parts, config, database)
  if (taskResponse) {
    return buildAxiosResponse(config, taskResponse.data)
  }

  const categoryResponse = handleCategoriesRequest(method, parts, config, database)
  if (categoryResponse) {
    return buildAxiosResponse(config, categoryResponse.data)
  }

  return null
}
