import {defineStore} from 'pinia'
import {ref} from 'vue'

export const useCategoryStore = defineStore('categories', () => {
    const isFormOpen = ref(false)
    const editingCategoryId = ref<number | null>(null)

    function openCreate() {
        editingCategoryId.value = null
        isFormOpen.value = true
    }

    function openEdit(id: number) {
        editingCategoryId.value = id
        isFormOpen.value = true
    }

    function closeForm() {
        isFormOpen.value = false
        editingCategoryId.value = null
    }

    return {
        isFormOpen,
        editingCategoryId,
        openCreate,
        openEdit,
        closeForm,
    }
})
