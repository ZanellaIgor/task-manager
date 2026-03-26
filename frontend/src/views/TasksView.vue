<script lang="ts" setup>
import {computed, onBeforeUnmount, ref} from 'vue'
import type {LocationQueryRaw} from 'vue-router'
import {useRoute, useRouter} from 'vue-router'
import {ListTodo, Plus, RefreshCcw} from 'lucide-vue-next'
import AppHeader from '@/components/layout/AppHeader.vue'
import AppSidebar from '@/components/layout/AppSidebar.vue'
import PageLayout from '@/components/layout/PageLayout.vue'
import TaskCard from '@/components/tasks/TaskCard.vue'
import TaskCardSkeleton from '@/components/tasks/TaskCardSkeleton.vue'
import TaskDetailsView from '@/components/tasks/TaskDetailsView.vue'
import TaskFilters from '@/components/tasks/TaskFilters.vue'
import TaskForm from '@/components/tasks/TaskForm.vue'
import BaseButton from '@/components/shared/BaseButton.vue'
import BaseModal from '@/components/shared/BaseModal.vue'
import BasePagination from '@/components/shared/BasePagination.vue'
import EmptyState from '@/components/shared/EmptyState.vue'
import ConfirmDialog from '@/components/shared/ConfirmDialog.vue'
import ErrorState from '@/components/shared/ErrorState.vue'
import BaseSkeleton from '@/components/shared/BaseSkeleton.vue'
import {useCategoryOptions} from '@/queries/categoryQueries'
import {useTasks} from '@/queries/taskQueries'
import {readQueryNumber, readQueryString, withQueryValue} from '@/router/query'
import {getErrorMessage} from '@/services/api'
import {useTaskCrud} from '@/composables/useTaskCrud'
import type {TaskFilters as TaskQueryFilters, TaskPriority, TaskStatus} from '@/types'

interface TaskFiltersValue {
  status?: TaskStatus | ''
  priority?: TaskPriority | ''
  categoryId?: number | ''
  search?: string
}

const PAGE_SIZE = 9

const route = useRoute()
const router = useRouter()
const sidebarOpen = ref(false)

const {
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
} = useTaskCrud()

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

const tasks = computed(() => tasksPage.value?.items ?? [])
const totalItems = computed(() => tasksPage.value?.totalItems ?? 0)
const totalPages = computed(() => tasksPage.value?.totalPages ?? 0)
const currentPage = computed(() => tasksPage.value?.page ?? taskFilters.value.page ?? 1)

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

onBeforeUnmount(() => {
  clearTimeout(filtersTimeout)
})
</script>

<template>
  <PageLayout lock-viewport>
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
            <BaseButton :disabled="isFetching" :loading="isFetching" variant="soft" @click="refetch()">
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

    <div class="flex min-h-0 flex-1 flex-col gap-6 overflow-hidden">
      <TaskFilters
        :categories="categoryOptionsQuery.data.value ?? []"
        :loading="isFetching || categoryOptionsQuery.isLoading.value"
        :model-value="filtersModel"
        class="shrink-0"
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

      <section
        v-else-if="isLoading"
        class="flex min-h-0 flex-1 flex-col overflow-hidden rounded-xl border border-neutral-200 bg-white shadow-sm"
      >
        <div class="grid flex-1 content-start gap-3.5 overflow-y-auto p-4 sm:p-6 lg:grid-cols-2 2xl:grid-cols-3">
          <TaskCardSkeleton
            v-for="index in PAGE_SIZE"
            :key="index"
          />
        </div>
      </section>

      <section
        v-else-if="tasks.length"
        class="flex min-h-0 flex-1 flex-col overflow-hidden rounded-xl border border-neutral-200 bg-white shadow-sm"
      >
        <div class="min-h-0 flex-1 overflow-y-auto p-4 sm:p-6">
          <TransitionGroup
            class="grid auto-rows-max gap-3.5 lg:grid-cols-2 2xl:grid-cols-3"
            name="list"
            tag="div"
          >
            <TaskCard
              v-for="task in tasks"
              :key="task.id"
              :loading-cancel="cancellingTaskId === task.id"
              :loading-complete="completingTaskId === task.id"
              :task="task"
              @cancel="handleCancel(task.id)"
              @complete="handleComplete(task.id)"
              @delete="confirmDelete(task.id)"
              @edit="store.openEdit(task.id)"
              @view="store.openView(task.id)"
            />
          </TransitionGroup>
        </div>

        <div class="shrink-0 border-t border-neutral-100 px-4 py-4 sm:px-6">
          <BasePagination
            :disabled="isFetching"
            :page="currentPage"
            :page-size="tasksPage?.pageSize ?? PAGE_SIZE"
            :total-items="totalItems"
            :total-pages="totalPages"
            @update:page="goToPage"
          />
        </div>
      </section>

      <section
        v-else
        class="flex flex-1 items-center rounded-card border border-dashed border-neutral-200 bg-white shadow-card"
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
    </div>

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

    <BaseModal
      :model-value="store.isViewOpen"
      size="lg"
      title="Detalhes da tarefa"
      @update:model-value="(value) => (!value ? store.closeView() : undefined)"
    >
      <div v-if="viewingQuery.isLoading.value" class="grid gap-5">
        <div class="rounded-2xl border border-neutral-200 bg-neutral-50/70 px-4 py-4">
          <BaseSkeleton height="0.75rem" rounded="md" width="28%" />
          <BaseSkeleton class="mt-3" height="1.4rem" rounded="md" width="55%" />
        </div>
        <div class="grid gap-4">
          <BaseSkeleton height="4.25rem" rounded="xl" width="100%" />
          <BaseSkeleton height="8rem" rounded="xl" width="100%" />
          <div class="grid grid-cols-2 gap-4 max-sm:grid-cols-1">
            <BaseSkeleton height="4.25rem" rounded="xl" width="100%" />
            <BaseSkeleton height="4.25rem" rounded="xl" width="100%" />
            <BaseSkeleton height="4.25rem" rounded="xl" width="100%" />
            <BaseSkeleton height="4.25rem" rounded="xl" width="100%" />
          </div>
        </div>
      </div>

      <ErrorState
        v-else-if="viewingQuery.isError.value"
        :description="getErrorMessage(viewingQuery.error.value, 'Não foi possível carregar os detalhes da tarefa.')"
        compact
        show-retry
        title="Falha ao carregar tarefa"
        @retry="viewingQuery.refetch()"
      />

      <TaskDetailsView
        v-else-if="viewingQuery.data.value"
        :task="viewingQuery.data.value"
        @close="store.closeView()"
        @edit="store.openEdit(viewingQuery.data.value.id)"
      />
    </BaseModal>
  </PageLayout>
</template>
