import {defineStore} from 'pinia'
import {ref} from 'vue'

export const useTaskStore = defineStore('tasks', () => {
    const isFormOpen = ref(false)
    const editingTaskId = ref<number | null>(null)
    const isViewOpen = ref(false)
    const viewingTaskId = ref<number | null>(null)

    function openCreate() {
        closeView()
        editingTaskId.value = null
        isFormOpen.value = true
    }

    function openEdit(id: number) {
        closeView()
        editingTaskId.value = id
        isFormOpen.value = true
    }

    function openView(id: number) {
        isFormOpen.value = false
        editingTaskId.value = null
        viewingTaskId.value = id
        isViewOpen.value = true
    }

    function closeForm() {
        isFormOpen.value = false
        editingTaskId.value = null
    }

    function closeView() {
        isViewOpen.value = false
        viewingTaskId.value = null
    }

    return {
        isFormOpen,
        editingTaskId,
        isViewOpen,
        viewingTaskId,
        openCreate,
        openEdit,
        openView,
        closeForm,
        closeView,
    }
})
