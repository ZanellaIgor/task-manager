import {defineStore} from 'pinia'
import {ref} from 'vue'
import type {TaskFilters} from '../types'

export const useTaskStore = defineStore('tasks', () => {
    const filters = ref<TaskFilters>({})
    const isFormOpen = ref(false)
    const editingTaskId = ref<number | null>(null)

    function setFilters(nextFilters: TaskFilters) {
        filters.value = {...nextFilters}
    }

    function patchFilters(nextFilters: Partial<TaskFilters>) {
        filters.value = {...filters.value, ...nextFilters}
    }

    function resetFilters() {
        filters.value = {}
    }

    function openCreate() {
        editingTaskId.value = null
        isFormOpen.value = true
    }

    function openEdit(id: number) {
        editingTaskId.value = id
        isFormOpen.value = true
    }

    function closeForm() {
        isFormOpen.value = false
        editingTaskId.value = null
    }

    return {
        filters,
        isFormOpen,
        editingTaskId,
        setFilters,
        patchFilters,
        resetFilters,
        openCreate,
        openEdit,
        closeForm,
    }
})
