import {computed, ref, watch} from 'vue'
import {useToast} from './useToast'
import {
  useCancelTask,
  useCompleteTask,
  useCreateTask,
  useDeleteTask,
  useTask,
  useUpdateTask,
} from '@/queries/taskQueries'
import {type TaskFormData} from '@/schemas/taskSchema'
import {getErrorMessage} from '@/services/api'
import {useTaskStore} from '@/stores/tasks'
import type {Task} from '@/types'

export function useTaskCrud() {
  const store = useTaskStore()
  const toast = useToast()

  const createTask = useCreateTask()
  const updateTask = useUpdateTask()
  const completeTask = useCompleteTask()
  const cancelTask = useCancelTask()
  const deleteTask = useDeleteTask()

  const editingId = computed(() => store.editingTaskId)
  const editingQuery = useTask(editingId)
  const viewingId = computed(() => store.viewingTaskId)
  const viewingQuery = useTask(viewingId)

  const submitting = computed(
    () => createTask.isPending.value || updateTask.isPending.value || editingQuery.isLoading.value,
  )

  const submitError = ref<string | null>(null)

  const isConfirmingDelete = ref(false)
  const taskToDelete = ref<number | null>(null)

  const completingTaskId = ref<number | null>(null)
  const cancellingTaskId = ref<number | null>(null)

  watch(
    () => store.isFormOpen,
    (isOpen) => {
      if (!isOpen) {
        submitError.value = null
      }
    },
  )

  async function handleSubmit(values: TaskFormData) {
    submitError.value = null

    try {
      if (store.editingTaskId) {
        await updateTask.mutateAsync({id: store.editingTaskId, data: values})
        toast.success('Tarefa atualizada com sucesso.')
      } else {
        await createTask.mutateAsync(values)
        toast.success('Tarefa criada com sucesso.')
      }

      store.closeForm()
    } catch (cause) {
      const message = getErrorMessage(cause, 'Não foi possível salvar a tarefa.')
      submitError.value = message
      toast.error(message)
    }
  }

  async function handleComplete(taskId: number) {
    completingTaskId.value = taskId
    try {
      await completeTask.mutateAsync(taskId)
      toast.success('Tarefa marcada como concluída.')
    } catch (cause) {
      toast.error(getErrorMessage(cause, 'Não foi possível concluir a tarefa.'))
    } finally {
      completingTaskId.value = null
    }
  }

  async function handleCancel(taskId: number) {
    cancellingTaskId.value = taskId
    try {
      await cancelTask.mutateAsync(taskId)
      toast.info('Tarefa cancelada.')
    } catch (cause) {
      toast.error(getErrorMessage(cause, 'Não foi possível cancelar a tarefa.'))
    } finally {
      cancellingTaskId.value = null
    }
  }

  function confirmDelete(taskId: number) {
    taskToDelete.value = taskId
    isConfirmingDelete.value = true
  }

  async function handleDelete() {
    if (!taskToDelete.value) return

    try {
      await deleteTask.mutateAsync(taskToDelete.value)
      toast.success('Tarefa excluída.')
    } catch (cause) {
      toast.error(getErrorMessage(cause, 'Não foi possível excluir a tarefa.'))
    } finally {
      isConfirmingDelete.value = false
      taskToDelete.value = null
    }
  }

  function initialValues(task?: Task | null): Partial<TaskFormData> | undefined {
    if (!task) return undefined

    return {
      title: task.title,
      description: task.description ?? '',
      categoryId: task.categoryId,
      priority: task.priority,
      dueDate: task.dueDate ? task.dueDate.slice(0, 10) : '',
    }
  }

  return {
    store,
    editingQuery,
    viewingQuery,
    submitting,
    submitError,
    isConfirmingDelete,
    deleteTask,
    completingTaskId,
    cancellingTaskId,
    handleSubmit,
    handleComplete,
    handleCancel,
    confirmDelete,
    handleDelete,
    initialValues,
  }
}
