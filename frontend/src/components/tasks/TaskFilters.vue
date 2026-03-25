<script lang="ts" setup>
import {computed} from 'vue'
import {Search, X} from 'lucide-vue-next'
import BaseButton from '@/components/shared/BaseButton.vue'
import BaseInput from '@/components/shared/BaseInput.vue'
import BaseSelect from '@/components/shared/BaseSelect.vue'

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
    totalResults?: number
  }>(),
  {
    modelValue: () => ({}),
    categories: () => [],
    loading: false,
    totalResults: 0,
  },
)

const emit = defineEmits<{
  'update:modelValue': [value: TaskFiltersValue]
  clear: []
}>()

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

const categoryOptions = computed(() => [
  {label: 'Todas as categorias', value: ''},
  ...props.categories.map((category) => ({
    label: category.name,
    value: category.id,
  })),
])

const filters = computed<TaskFiltersValue>(() => props.modelValue)

const hasActiveFilters = computed(() =>
  Boolean(
    filters.value.search ||
    filters.value.status ||
    filters.value.priority ||
    filters.value.categoryId,
  ),
)

const activeFiltersCount = computed(() =>
  [filters.value.status, filters.value.priority, filters.value.categoryId, filters.value.search].filter(
    (v) => v !== '' && v !== undefined,
  ).length,
)

function patchFilters<K extends keyof TaskFiltersValue>(key: K, value: TaskFiltersValue[K]) {
  emit('update:modelValue', {...filters.value, [key]: value})
}

function clearFilters() {
  emit('clear')
}
</script>

<template>
  <section class="grid gap-3 rounded-xl border border-neutral-200 bg-white p-3 shadow-sm">
    <div class="hidden items-center gap-2.5 lg:grid lg:grid-cols-[1fr_auto_auto_auto_auto]">
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
          class="pointer-events-none absolute right-3.5 top-1/2 -translate-y-1/2 text-neutral-400"
        />
      </div>

      <BaseSelect
        :disabled="loading"
        :model-value="filters.status ?? ''"
        :options="statusOptions"
        class="w-44"
        placeholder="Situacao"
        @update:modelValue="patchFilters('status', $event as TaskStatus | '')"
      />

      <BaseSelect
        :disabled="loading"
        :model-value="filters.priority ?? ''"
        :options="priorityOptions"
        class="w-40"
        placeholder="Prioridade"
        @update:modelValue="patchFilters('priority', $event as TaskPriority | '')"
      />

      <BaseSelect
        :disabled="loading"
        :model-value="filters.categoryId ?? ''"
        :options="categoryOptions"
        class="w-40"
        placeholder="Categoria"
        value-mode="number"
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

    <div class="grid gap-3 lg:hidden">
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
          class="pointer-events-none absolute right-3.5 top-1/2 -translate-y-1/2 text-neutral-400"
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

    <!-- Results count bar -->
    <div class="-mx-3 -mb-3 flex items-center gap-2 border-t border-neutral-100 bg-neutral-50/50 px-4 py-2">
      <span class="text-[0.78rem] font-medium text-neutral-500">
        {{ totalResults }} {{ totalResults === 1 ? 'tarefa encontrada' : 'tarefas encontradas' }}
        <span v-if="hasActiveFilters" class="text-neutral-400"> com filtros ativos</span>
      </span>
      <span
        v-if="hasActiveFilters"
        class="inline-flex h-4 w-4 items-center justify-center rounded-full bg-primary text-[0.6rem] font-bold text-white"
      >
        {{ activeFiltersCount }}
      </span>
    </div>
  </section>
</template>

<style scoped>
.search-wrapper :deep(input) {
  padding-right: 2.25rem;
}
</style>
