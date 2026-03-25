<script lang="ts" setup>
import {computed} from 'vue'
import {Search, X} from 'lucide-vue-next'
import BaseButton from '@/components/shared/BaseButton.vue'
import BaseInput from '@/components/shared/BaseInput.vue'
import BaseSelect from '@/components/shared/BaseSelect.vue'
import {useCategories} from '@/queries/categoryQueries'
import {useTaskStore} from '@/stores/tasks'
import type {TaskFilters} from '@/types'

type TaskStatus = 'Pending' | 'InProgress' | 'Completed' | 'Cancelled'
type TaskPriority = 'Low' | 'Medium' | 'High'

interface CategoryOption {
  id: number
  name: string
  isActive?: boolean
}

interface TaskFiltersValue {
  status?: TaskStatus | ''
  priority?: TaskPriority | ''
  categoryId?: number | ''
  search?: string
}

const props = withDefaults(
    defineProps<{
      modelValue?: TaskFiltersValue
      categories?: CategoryOption[]
      loading?: boolean
    }>(),
    {
      categories: () => [],
      loading: false,
    },
)

const emit = defineEmits<{
  'update:modelValue': [value: TaskFiltersValue]
  clear: []
}>()

const store = useTaskStore()
const categoriesQuery = useCategories()

const statusOptions = [
  {label: 'Todas as situacoes', value: ''},
  {label: 'Pendente', value: 'Pending'},
  {label: 'Em andamento', value: 'InProgress'},
  {label: 'Concluida', value: 'Completed'},
  {label: 'Cancelada', value: 'Cancelled'},
]

const priorityOptions = [
  {label: 'Todas as prioridades', value: ''},
  {label: 'Baixa', value: 'Low'},
  {label: 'Media', value: 'Medium'},
  {label: 'Alta', value: 'High'},
]

const categorySource = computed(() => {
  if (props.categories.length > 0) return props.categories
  return (categoriesQuery.data.value ?? []) as CategoryOption[]
})

const categoryOptions = computed(() => [
  {label: 'Todas as categorias', value: ''},
  ...categorySource.value.map((category) => ({
    label: category.name,
    value: category.id,
  })),
])

const filters = computed<TaskFiltersValue>(() => props.modelValue ?? store.filters)

const hasActiveFilters = computed(() =>
    Boolean(
        filters.value.search ||
        filters.value.status ||
        filters.value.priority ||
        filters.value.categoryId,
    ),
)

function patchFilters<K extends keyof TaskFiltersValue>(key: K, value: TaskFiltersValue[K]) {
  const next = {...filters.value, [key]: value}

  if (props.modelValue) {
    emit('update:modelValue', next)
  }

  const normalized: TaskFilters = {
    ...(next.status ? {status: next.status} : {}),
    ...(next.priority ? {priority: next.priority} : {}),
    ...(typeof next.categoryId === 'number' ? {categoryId: next.categoryId} : {}),
    ...(next.search ? {search: next.search} : {}),
  }

  store.setFilters(normalized)
}

function clearFilters() {
  const empty: TaskFiltersValue = {}
  if (props.modelValue) emit('update:modelValue', empty)
  emit('clear')
  store.resetFilters()
}
</script>

<template>
  <section
      class="grid gap-3 p-3 border border-neutral-200 rounded-xl bg-white shadow-sm"
  >
    <!-- Desktop: single row layout -->
    <div class="hidden lg:grid lg:grid-cols-[1fr_auto_auto_auto_auto] lg:gap-2.5 lg:items-center">
      <div class="search-wrapper relative">
        <BaseInput
            :disabled="loading"
            :model-value="filters.search ?? ''"
            aria-label="Buscar tarefas"
            placeholder="Buscar por titulo ou descricao"
            @update:modelValue="patchFilters('search', $event)"
        />
        <Search
            :size="15"
            class="absolute right-3.5 top-1/2 -translate-y-1/2 text-neutral-400 pointer-events-none"
        />
      </div>

      <BaseSelect
          :disabled="loading"
          :model-value="filters.status ?? ''"
          :options="statusOptions"
          placeholder="Situacao"
          class="w-44"
          @update:modelValue="patchFilters('status', $event as TaskStatus | '')"
      />

      <BaseSelect
          :disabled="loading"
          :model-value="filters.priority ?? ''"
          :options="priorityOptions"
          placeholder="Prioridade"
          class="w-40"
          @update:modelValue="patchFilters('priority', $event as TaskPriority | '')"
      />

      <BaseSelect
          :disabled="loading"
          :model-value="filters.categoryId ?? ''"
          :options="categoryOptions"
          placeholder="Categoria"
          value-mode="number"
          class="w-40"
          @update:modelValue="patchFilters('categoryId', $event as number | '')"
      />

      <BaseButton
          :disabled="!hasActiveFilters"
          size="sm"
          variant="secondary"
          @click="clearFilters"
      >
        <X :size="14"/>
        Limpar
      </BaseButton>
    </div>

    <!-- Mobile: stacked layout -->
    <div class="lg:hidden grid gap-3">
      <div class="search-wrapper relative">
        <BaseInput
            :disabled="loading"
            :model-value="filters.search ?? ''"
            aria-label="Buscar tarefas"
            placeholder="Buscar por titulo ou descricao"
            @update:modelValue="patchFilters('search', $event)"
        />
        <Search
            :size="15"
            class="absolute right-3.5 top-1/2 -translate-y-1/2 text-neutral-400 pointer-events-none"
        />
      </div>

      <div class="grid grid-cols-1 gap-2.5 min-[600px]:grid-cols-3">
        <BaseSelect
            :disabled="loading"
            :model-value="filters.status ?? ''"
            :options="statusOptions"
            placeholder="Situacao"
            @update:modelValue="patchFilters('status', $event as TaskStatus | '')"
        />

        <BaseSelect
            :disabled="loading"
            :model-value="filters.priority ?? ''"
            :options="priorityOptions"
            placeholder="Prioridade"
            @update:modelValue="patchFilters('priority', $event as TaskPriority | '')"
        />

        <BaseSelect
            :disabled="loading"
            :model-value="filters.categoryId ?? ''"
            :options="categoryOptions"
            placeholder="Categoria"
            value-mode="number"
            @update:modelValue="patchFilters('categoryId', $event as number | '')"
        />
      </div>

      <div class="flex justify-end">
        <BaseButton
            :disabled="!hasActiveFilters"
            size="sm"
            variant="secondary"
            @click="clearFilters"
        >
          <X :size="14"/>
          Limpar filtros
        </BaseButton>
      </div>
    </div>
  </section>
</template>

<style scoped>
.search-wrapper :deep(input) {
  padding-right: 2.25rem;
}
</style>
