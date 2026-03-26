import {computed, ref, watch} from 'vue'
import {useToast} from './useToast'
import {useCreateCategory, useToggleCategoryStatus, useUpdateCategory} from '@/queries/categoryQueries'
import type {CategoryFormData} from '@/schemas/categorySchema'
import {getErrorMessage} from '@/services/api'
import {useCategoryStore} from '@/stores/categories'
import type {Category} from '@/types'

export function useCategoryCrud(categoriesList: () => Category[]) {
  const store = useCategoryStore()
  const toast = useToast()

  const createCategory = useCreateCategory()
  const updateCategory = useUpdateCategory()
  const toggleCategoryStatus = useToggleCategoryStatus()

  const editingCategory = computed(
    () => categoriesList().find((item) => item.id === store.editingCategoryId) ?? null,
  )

  const isSubmitting = computed(
    () => createCategory.isPending.value || updateCategory.isPending.value,
  )

  const submitError = ref<string | null>(null)
  const isConfirmingToggle = ref(false)
  const categoryToToggle = ref<Category | null>(null)

  const existingNames = computed(() =>
    categoriesList()
      .filter((category) => category.id !== editingCategory.value?.id)
      .map((category) => category.name),
  )

  watch(
    () => store.isFormOpen,
    (isOpen) => {
      if (!isOpen) {
        submitError.value = null
      }
    },
  )

  async function handleSubmit(values: CategoryFormData) {
    submitError.value = null

    try {
      if (store.editingCategoryId) {
        await updateCategory.mutateAsync({
          id: store.editingCategoryId,
          data: values,
        })
        toast.success('Categoria atualizada com sucesso.')
      } else {
        await createCategory.mutateAsync(values)
        toast.success('Categoria criada com sucesso.')
      }

      store.closeForm()
    } catch (cause) {
      const message = getErrorMessage(cause, 'Não foi possível salvar a categoria.')
      submitError.value = message
      toast.error(message)
    }
  }

  function confirmToggle(category: Category) {
    categoryToToggle.value = category
    isConfirmingToggle.value = true
  }

  async function executeToggle() {
    if (!categoryToToggle.value) return

    const category = categoryToToggle.value
    try {
      await toggleCategoryStatus.mutateAsync({
        id: category.id,
        isActive: !category.isActive,
      })
      toast.success(
        category.isActive
          ? `Categoria "${category.name}" desativada.`
          : `Categoria "${category.name}" ativada.`,
      )
    } catch (cause) {
      toast.error(getErrorMessage(cause, 'Não foi possível alterar o status da categoria.'))
    } finally {
      isConfirmingToggle.value = false
      categoryToToggle.value = null
    }
  }

  function initialValues(category?: Category | null): Partial<CategoryFormData> | undefined {
    if (!category) return undefined

    return {
      name: category.name,
      description: category.description ?? '',
    }
  }

  return {
    store,
    editingCategory,
    isSubmitting,
    submitError,
    isConfirmingToggle,
    categoryToToggle,
    toggleCategoryStatus,
    existingNames,
    handleSubmit,
    confirmToggle,
    executeToggle,
    initialValues,
  }
}
