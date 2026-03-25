<script lang="ts" setup>
import {computed, ref, watch} from 'vue'
import type {LocationQueryRaw} from 'vue-router'
import {useRoute, useRouter} from 'vue-router'
import {Plus, RefreshCcw} from 'lucide-vue-next'
import AppHeader from '@/components/layout/AppHeader.vue'
import AppSidebar from '@/components/layout/AppSidebar.vue'
import PageLayout from '@/components/layout/PageLayout.vue'
import TaskCard from '@/components/tasks/TaskCard.vue'
import TaskCardSkeleton from '@/components/tasks/TaskCardSkeleton.vue'
import TaskFilters from '@/components/tasks/TaskFilters.vue'
import TaskForm from '@/components/tasks/TaskForm.vue'
import BaseButton from '@/components/shared/BaseButton.vue'
import BaseModal from '@/components/shared/BaseModal.vue'
import BasePagination from '@/components/shared/BasePagination.vue'
import EmptyState from '@/components/shared/EmptyState.vue'
import ConfirmDialog from '@/components/shared/ConfirmDialog.vue'
import ErrorState from '@/components/shared/ErrorState.vue'
import {useToast} from '@/composables/useToast'
import {ListTodo} from 'lucide-vue-next'
import {useCategoryOptions} from '@/queries/categoryQueries'
import {
  useCancelTask,
  useCompleteTask,
  useCreateTask,
  useDeleteTask,
  useTask,
  useTasks,
  useUpdateTask,
} from '@/queries/taskQueries'
import {readQueryNumber, readQueryString, withQueryValue} from '@/router/query'
import {type TaskFormData} from '@/schemas/taskSchema'
import {getErrorMessage} from '@/services/api'
import {useTaskStore} from '@/stores/tasks'
import type {Task, TaskFilters as TaskQueryFilters, TaskPriority, TaskStatus} from '@/types'

interface TaskFiltersValue {
  status?: TaskStatus | ''
  priority?: TaskPriority | ''
  categoryId?: number | ''
  search?: string
}

const PAGE_SIZE = 9

const route = useRoute()
const router = useRouter()
const store = useTaskStore()
const toast = useToast()
const sidebarOpen = ref(false)
const isConfirmingDelete = ref(false)
const taskToDelete = ref<number | null>(null)

const filtersModel = computed<TaskFiltersValue>(() => {
  const categoryId = readQueryNumber(route.query, 'categoryId', 0)

  return {
    status: (readQueryString(route.query, 'status') as TaskStatus | undefined) ?? '',
    priority: (readQueryString(route.query, 'priority') as TaskPriority | undefined) ?? '',
    categoryId: categoryId > 0 ? categoryId : '',
    search: readQueryString(route.query, 'search') ?? '',
  }
})

const hasActiveFilters = computed(() => {
  return filtersModel.value.status !== '' ||
    filtersModel.value.priority !== '' ||
    filtersModel.value.categoryId !== '' ||
    filtersModel.value.search !== ''
})

const taskFilters = computed<TaskQueryFilters>(() => ({
  page: readQueryNumber(route.query, 'page', 1),
  pageSize: PAGE_SIZE,
  sortBy: 'createdAt',
  sortDirection: 'Desc',
  ...(filtersModel.value.status ? {status: filtersModel.value.status} : {}),
  ...(filtersModel.value.priority ? {priority: filtersModel.value.priority} : {}),
  ...(typeof filtersModel.value.categoryId === 'number' ? {categoryId: filtersModel.value.categoryId} : {}),
  ...(filtersModel.value.search ? {search: filtersModel.value.search} : {}),
}))

const {data: tasksPage, isLoading, isError, error, refetch, isFetching} = useTasks(taskFilters)
const categoryOptionsQuery = useCategoryOptions()
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

const tasks = computed(() => tasksPage.value?.items ?? [])
const totalItems = computed(() => tasksPage.value?.totalItems ?? 0)
const totalPages = computed(() => tasksPage.value?.totalPages ?? 0)
const currentPage = computed(() => tasksPage.value?.page ?? taskFilters.value.page ?? 1)
const submitError = ref<string | null>(null)

watch(
  () => store.isFormOpen,
  (isOpen) => {
    if (!isOpen) {
      submitError.value = null
    }
  },
)

function buildQuery(nextFilters: TaskFiltersValue, page = 1) {
  const query: LocationQueryRaw = {}

  withQueryValue(query, 'page', page > 1 ? page : undefined)
  withQueryValue(query, 'status', nextFilters.status || undefined)
  withQueryValue(query, 'priority', nextFilters.priority || undefined)
  withQueryValue(query, 'categoryId', typeof nextFilters.categoryId === 'number' ? nextFilters.categoryId : undefined)
  withQueryValue(query, 'search', nextFilters.search?.trim() || undefined)

  return query
}

let filtersTimeout: number | undefined

async function updateFilters(nextFilters: TaskFiltersValue) {
  clearTimeout(filtersTimeout)

  if (nextFilters.search !== filtersModel.value.search) {
    filtersTimeout = window.setTimeout(async () => {
      await router.replace({query: buildQuery(nextFilters)})
    }, 350)
  } else {
    await router.replace({query: buildQuery(nextFilters)})
  }
}

async function clearFilters() {
  await router.replace({query: {}})
}

async function goToPage(page: number) {
  await router.replace({query: buildQuery(filtersModel.value, page)})
}

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
  try {
    await completeTask.mutateAsync(taskId)
    toast.success('Tarefa marcada como concluída.')
  } catch (cause) {
    toast.error(getErrorMessage(cause, 'Não foi possível concluir a tarefa.'))
  }
}

async function handleCancel(taskId: number) {
  try {
    await cancelTask.mutateAsync(taskId)
    toast.info('Tarefa cancelada.')
  } catch (cause) {
    toast.error(getErrorMessage(cause, 'Não foi possível cancelar a tarefa.'))
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
  <PageLayout>
    <template #sidebar>
      <AppSidebar :model-value="sidebarOpen" @update:model-value="sidebarOpen = $event" />
    </template>

    <template #header>
      <AppHeader
        description="Gerencie prioridades, acompanhe status e mantenha o backlog organizado."
        title="Tarefas"
        @toggle-sidebar="sidebarOpen = !sidebarOpen"
      >
        <template #actions>
          <div class="flex items-center gap-2.5">
            <BaseButton :disabled="isFetching" variant="secondary" @click="refetch()">
              <RefreshCcw :class="isFetching && 'animate-spin'" class="h-3.5 w-3.5" />
              Atualizar
            </BaseButton>
            <BaseButton @click="store.openCreate()">
              <Plus class="h-3.5 w-3.5" />
              Nova tarefa
            </BaseButton>
          </div>
        </template>
      </AppHeader>
    </template>

    <TaskFilters
      :categories="categoryOptionsQuery.data.value ?? []"
      :loading="isFetching || categoryOptionsQuery.isLoading.value"
      :model-value="filtersModel"
      :total-results="totalItems"
      @clear="clearFilters"
      @update:model-value="updateFilters"
    />

    <ErrorState
      v-if="isError"
      :description="getErrorMessage(error, 'Verifique a conexão e tente novamente.')"
      show-retry
      title="Falha ao carregar tarefas"
      @retry="refetch"
    />

    <section v-else-if="isLoading" class="grid gap-3.5 lg:grid-cols-2 2xl:grid-cols-3">
      <TaskCardSkeleton
        v-for="index in PAGE_SIZE"
        :key="index"
      />
    </section>

    <template v-else-if="tasks.length">
      <TransitionGroup
        class="grid gap-3.5 lg:grid-cols-2 2xl:grid-cols-3"
        name="list"
        tag="section"
      >
        <TaskCard
          v-for="task in tasks"
          :key="task.id"
          :task="task"
          @cancel="handleCancel(task.id)"
          @complete="handleComplete(task.id)"
          @delete="confirmDelete(task.id)"
          @edit="store.openEdit(task.id)"
        />
      </TransitionGroup>

      <BasePagination
        :disabled="isFetching"
        :page="currentPage"
        :page-size="tasksPage?.pageSize ?? PAGE_SIZE"
        :total-items="totalItems"
        :total-pages="totalPages"
        @update:page="goToPage"
      />
    </template>

    <section
      v-else
      class="rounded-card border border-dashed border-neutral-200 bg-white shadow-card"
    >
      <EmptyState
        :icon="ListTodo"
        :description="hasActiveFilters ? 'Nenhuma tarefa corresponde aos filtros selecionados.' : 'Crie a primeira tarefa para começar a organizar seu dia.'"
        title="Nenhuma tarefa encontrada"
      >
        <template #actions>
          <div class="flex flex-wrap items-center justify-center gap-3">
            <BaseButton v-if="hasActiveFilters" variant="secondary" @click="clearFilters">
              Limpar filtros
            </BaseButton>
            <BaseButton @click="store.openCreate()">Nova tarefa</BaseButton>
          </div>
        </template>
      </EmptyState>
    </section>

    <ConfirmDialog
      v-model="isConfirmingDelete"
      :loading="deleteTask.isPending.value"
      description="Tem certeza que deseja excluir esta tarefa? Esta operação não pode ser revertida."
      title="Excluir tarefa"
      @confirm="handleDelete"
    />

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
  </PageLayout>
</template>
