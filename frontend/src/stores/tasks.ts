import {defineStore} from 'pinia'
import {ref} from 'vue'

export const useTaskStore = defineStore('tasks', () => {
    const isFormOpen = ref(false)
    const editingTaskId = ref<number | null>(null)

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
        isFormOpen,
        editingTaskId,
        openCreate,
        openEdit,
        closeForm,
    }
})
