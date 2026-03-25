import {computed, type MaybeRefOrGetter, toValue} from 'vue'
import {useMutation, useQuery, useQueryClient} from '@tanstack/vue-query'
import {categoryService} from '../services/categoryService'
import type {CategoryFormData} from '../schemas/categorySchema'

export const categoryKeys = {
    all: ['categories'] as const,
    detail: (id: number) => ['categories', id] as const,
}

export function useCategories() {
    return useQuery({
        queryKey: categoryKeys.all,
        queryFn: () => categoryService.getAll(),
        staleTime: 30_000,
    })
}

export function useCategory(id: MaybeRefOrGetter<number | null | undefined>) {
    return useQuery({
        queryKey: computed(() => categoryKeys.detail(Number(toValue(id) ?? 0))),
        queryFn: () => categoryService.getById(Number(toValue(id))),
        enabled: computed(() => Number.isFinite(Number(toValue(id))) && Number(toValue(id)) > 0),
    })
}

function invalidateCategories(queryClient: ReturnType<typeof useQueryClient>) {
    return Promise.all([queryClient.invalidateQueries({queryKey: categoryKeys.all})])
}

export function useCreateCategory() {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: (data: CategoryFormData) => categoryService.create(data),
        onSuccess: async () => {
            await invalidateCategories(queryClient)
        },
    })
}

export function useUpdateCategory() {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: ({id, data}: { id: number; data: CategoryFormData }) => categoryService.update(id, data),
        onSuccess: async (_category, variables) => {
            await Promise.all([
                queryClient.invalidateQueries({queryKey: categoryKeys.detail(variables.id)}),
                invalidateCategories(queryClient),
            ])
        },
    })
}

export function useDeactivateCategory() {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: (id: number) => categoryService.deactivate(id),
        onSuccess: async (_category, id) => {
            await Promise.all([
                queryClient.invalidateQueries({queryKey: categoryKeys.detail(id)}),
                invalidateCategories(queryClient),
            ])
        },
    })
}

export function useToggleCategoryStatus() {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: (payload: number | { id: number; isActive?: boolean }) => {
            const id = typeof payload === 'number' ? payload : payload.id
            const isActive = typeof payload === 'number' ? undefined : payload.isActive

            if (isActive === true) {
                return categoryService.activate(id)
            }

            if (isActive === false) {
                return categoryService.deactivate(id)
            }

            return categoryService.deactivate(id)
        },
        onSuccess: async (_category, payload) => {
            const id = typeof payload === 'number' ? payload : payload.id
            await Promise.all([
                queryClient.invalidateQueries({queryKey: categoryKeys.detail(id)}),
                invalidateCategories(queryClient),
            ])
        },
    })
}

export function useDeleteCategory() {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: (id: number) => categoryService.remove(id),
        onSuccess: async (_category, id) => {
            await Promise.all([
                queryClient.removeQueries({queryKey: categoryKeys.detail(id)}),
                invalidateCategories(queryClient),
            ])
        },
    })
}
