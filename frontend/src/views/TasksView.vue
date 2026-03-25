<script lang="ts" setup>
import {computed, ref, watch} from 'vue'
import {AlertCircle, Plus, RefreshCcw} from 'lucide-vue-next'
import AppHeader from '@/components/layout/AppHeader.vue'
import AppSidebar from '@/components/layout/AppSidebar.vue'
import TaskCard from '@/components/tasks/TaskCard.vue'
import TaskFilters from '@/components/tasks/TaskFilters.vue'
import TaskForm from '@/components/tasks/TaskForm.vue'
import BaseButton from '@/components/shared/BaseButton.vue'
import BaseModal from '@/components/shared/BaseModal.vue'
import {useToast} from '@/composables/useToast'
import {
  useCancelTask,
  useCompleteTask,
  useCreateTask,
  useDeleteTask,
  useTask,
  useTasks,
  useUpdateTask,
} from '@/queries/taskQueries'
import {useTaskStore} from '@/stores/tasks'
import type {Task} from '@/types'
import type {TaskFormData} from '@/schemas/taskSchema'

const store = useTaskStore()
const toast = useToast()
const sidebarOpen = ref(false)

const filters = computed(() => store.filters)
const {data: tasks, isLoading, isError, error, refetch, isFetching} = useTasks(filters)
const editingId = computed(() => store.editingTaskId)
const editingQuery = useTask(editingId)

const createTask = useCreateTask()
const updateTask = useUpdateTask()
const completeTask = useCompleteTask()
const cancelTask = useCancelTask()
const deleteTask = useDeleteTask()

const submitting = computed(
    () => createTask.isPending.value || updateTask.isPending.value || editingQuery.isLoading.value,
)

const submitError = ref<string | null>(null)

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
    const message = cause instanceof Error ? cause.message : 'Nao foi possivel salvar a tarefa.'
    submitError.value = message
    toast.error(message)
  }
}

async function handleComplete(taskId: number) {
  try {
    await completeTask.mutateAsync(taskId)
    toast.success('Tarefa marcada como concluída.')
  } catch {
    toast.error('Nao foi possivel concluir a tarefa.')
  }
}

async function handleCancel(taskId: number) {
  try {
    await cancelTask.mutateAsync(taskId)
    toast.info('Tarefa cancelada.')
  } catch {
    toast.error('Nao foi possivel cancelar a tarefa.')
  }
}

async function handleDelete(taskId: number) {
  try {
    await deleteTask.mutateAsync(taskId)
    toast.success('Tarefa excluída.')
  } catch {
    toast.error('Nao foi possivel excluir a tarefa.')
  }
}

function initialValues(task?: Task | null): Partial<TaskFormData> | undefined {
  if (!task) {
    return undefined
  }

  return {
    title: task.title,
    description: task.description ?? '',
    categoryId: task.categoryId,
    priority: task.priority,
    dueDate: task.dueDate ? task.dueDate.slice(0, 10) : '',
  }
}
</script>

<template>
  <div class="flex min-h-screen bg-[#eef0f4] text-neutral-900">
    <AppSidebar :model-value="sidebarOpen" @update:model-value="sidebarOpen = $event"/>

    <div class="flex min-w-0 flex-1 flex-col lg:pl-[18.5rem]">
      <AppHeader
          description="Gerencie prioridades, acompanhe status e mantenha o backlog organizado."
          title="Tarefas"
          @toggle-sidebar="sidebarOpen = !sidebarOpen"
      >
        <template #actions>
          <div class="flex items-center gap-2.5">
            <BaseButton :disabled="isFetching" variant="secondary" @click="refetch()">
              <RefreshCcw :class="isFetching && 'animate-spin'" class="h-3.5 w-3.5"/>
              Atualizar
            </BaseButton>
            <BaseButton @click="store.openCreate()">
              <Plus class="h-3.5 w-3.5"/>
              Nova tarefa
            </BaseButton>
          </div>
        </template>
      </AppHeader>

      <main class="flex-1 space-y-4 px-4 py-4 sm:px-6 lg:px-8">
        <TaskFilters/>

        <section
            v-if="isError"
            class="rounded-card border border-danger/15 bg-white p-6 shadow-card"
        >
          <div class="flex items-start gap-3">
            <AlertCircle class="mt-0.5 h-5 w-5 text-danger"/>
            <div>
              <p class="font-semibold text-neutral-900">Falha ao carregar tarefas</p>
              <p class="mt-1 text-sm text-neutral-400">
                {{ error instanceof Error ? error.message : 'Verifique a conexao e tente novamente.' }}
              </p>
              <BaseButton class="mt-4" variant="secondary" @click="refetch()">Tentar novamente</BaseButton>
            </div>
          </div>
        </section>

        <section v-else-if="isLoading" class="grid gap-3.5 lg:grid-cols-2 2xl:grid-cols-3">
          <div
              v-for="index in 6"
              :key="index"
              class="h-44 animate-pulse rounded-[1.1rem] bg-white shadow-card"
          />
        </section>

        <section
            v-else-if="tasks?.length"
            class="grid gap-3.5 lg:grid-cols-2 2xl:grid-cols-3"
        >
          <TaskCard
              v-for="task in tasks"
              :key="task.id"
              :task="task"
              @cancel="handleCancel(task.id)"
              @complete="handleComplete(task.id)"
              @delete="handleDelete(task.id)"
              @edit="store.openEdit(task.id)"
          />
        </section>

        <section
            v-else
            class="rounded-card border border-dashed border-neutral-200 bg-white px-6 py-14 text-center shadow-card"
        >
          <p class="font-display text-2xl font-bold text-neutral-900">Nenhuma tarefa encontrada</p>
          <p class="mt-2 text-sm text-neutral-400">
            Ajuste os filtros ou crie a primeira tarefa para iniciar o fluxo.
          </p>
          <BaseButton class="mt-6" @click="store.openCreate()">Criar tarefa</BaseButton>
        </section>
      </main>

      <BaseModal
          :description="
          store.editingTaskId
            ? 'Atualize os dados da tarefa selecionada.'
            : 'Preencha os campos para registrar uma nova tarefa.'
        "
          :model-value="store.isFormOpen"
          :title="store.editingTaskId ? 'Editar tarefa' : 'Nova tarefa'"
          size="lg"
          @update:model-value="(value) => (!value ? store.closeForm() : undefined)"
      >
        <TaskForm
            :key="store.editingTaskId ?? 'new-task'"
            :initial-values="initialValues(editingQuery.data.value)"
            :loading="submitting"
            :server-error="submitError"
            @cancel="store.closeForm()"
            @submit="handleSubmit"
        />
      </BaseModal>
    </div>
  </div>
</template>
