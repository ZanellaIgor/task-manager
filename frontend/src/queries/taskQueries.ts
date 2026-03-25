import {computed, type MaybeRefOrGetter, toValue} from 'vue'
import {useMutation, useQuery, useQueryClient} from '@tanstack/vue-query'
import {taskService} from '../services/taskService'
import type {TaskFilters} from '../types'
import type {TaskFormData} from '../schemas/taskSchema'

export const taskKeys = {
    all: ['tasks'] as const,
    filtered: (filters: TaskFilters) => ['tasks', filters] as const,
    detail: (id: number) => ['tasks', id] as const,
}

export function useTasks(filters: MaybeRefOrGetter<TaskFilters> = {}) {
    return useQuery({
        queryKey: computed(() => taskKeys.filtered(toValue(filters))),
        queryFn: () => taskService.getAll(toValue(filters)),
        staleTime: 30_000,
    })
}

export function useTask(id: MaybeRefOrGetter<number | null | undefined>) {
    return useQuery({
        queryKey: computed(() => taskKeys.detail(Number(toValue(id) ?? 0))),
        queryFn: () => taskService.getById(Number(toValue(id))),
        enabled: computed(() => Number.isFinite(Number(toValue(id))) && Number(toValue(id)) > 0),
    })
}

function invalidateTasks(queryClient: ReturnType<typeof useQueryClient>) {
    return Promise.all([
        queryClient.invalidateQueries({queryKey: taskKeys.all}),
        queryClient.invalidateQueries({queryKey: ['tasks']}),
    ])
}

export function useCreateTask() {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: (data: TaskFormData) => taskService.create(data),
        onSuccess: async () => {
            await invalidateTasks(queryClient)
        },
    })
}

export function useUpdateTask() {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: ({id, data}: { id: number; data: TaskFormData }) => taskService.update(id, data),
        onSuccess: async (_task, variables) => {
            await Promise.all([
                queryClient.invalidateQueries({queryKey: taskKeys.detail(variables.id)}),
                invalidateTasks(queryClient),
            ])
        },
    })
}

export function useCompleteTask() {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: (id: number) => taskService.complete(id),
        onSuccess: async (_task, id) => {
            await Promise.all([
                queryClient.invalidateQueries({queryKey: taskKeys.detail(id)}),
                invalidateTasks(queryClient),
            ])
        },
    })
}

export function useCancelTask() {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: (id: number) => taskService.cancel(id),
        onSuccess: async (_task, id) => {
            await Promise.all([
                queryClient.invalidateQueries({queryKey: taskKeys.detail(id)}),
                invalidateTasks(queryClient),
            ])
        },
    })
}

export function useDeleteTask() {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: (id: number) => taskService.remove(id),
        onSuccess: async (_task, id) => {
            await Promise.all([
                queryClient.removeQueries({queryKey: taskKeys.detail(id)}),
                invalidateTasks(queryClient),
            ])
        },
    })
}
