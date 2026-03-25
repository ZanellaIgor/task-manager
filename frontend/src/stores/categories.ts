import {defineStore} from 'pinia'
import {ref} from 'vue'

export const useCategoryStore = defineStore('categories', () => {
    const isFormOpen = ref(false)
    const editingCategoryId = ref<number | null>(null)
    const showInactive = ref(true)
    const search = ref('')

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

    function setSearch(value: string) {
        search.value = value
    }

    function toggleInactive() {
        showInactive.value = !showInactive.value
    }

    return {
        isFormOpen,
        editingCategoryId,
        showInactive,
        search,
        openCreate,
        openEdit,
        closeForm,
        setSearch,
        toggleInactive,
    }
})
