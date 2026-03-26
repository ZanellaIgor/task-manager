<script lang="ts" setup>
import {computed, ref} from 'vue'
import {Filter, Search, X} from 'lucide-vue-next'
import BaseButton from '@/components/shared/BaseButton.vue'
import BaseInput from '@/components/shared/BaseInput.vue'
import BaseModal from '@/components/shared/BaseModal.vue'
import BaseSelect from '@/components/shared/BaseSelect.vue'
import {
  TASK_STATUS_LABELS,
  TASK_PRIORITY_LABELS,
  TASK_STATUSES,
  TASK_PRIORITIES,
  type TaskStatus,
  type TaskPriority,
} from '@/types'

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
    modelValue: () => ({}),
    categories: () => [],
    loading: false,
  },
)

const emit = defineEmits<{
  'update:modelValue': [value: TaskFiltersValue]
  clear: []
}>()

const mobileFiltersOpen = ref(false)

const statusOptions = [
  {label: 'Todas as situações', value: ''},
  ...TASK_STATUSES.map((s) => ({label: TASK_STATUS_LABELS[s], value: s})),
]

const priorityOptions = [
  {label: 'Todas as prioridades', value: ''},
  ...TASK_PRIORITIES.map((p) => ({label: TASK_PRIORITY_LABELS[p], value: p})),
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

const hasActiveSelectFilters = computed(() =>
  Boolean(
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

const activeSelectFiltersCount = computed(() =>
  [filters.value.status, filters.value.priority, filters.value.categoryId].filter(
    (v) => v !== '' && v !== undefined,
  ).length,
)

function patchFilters<K extends keyof TaskFiltersValue>(key: K, value: TaskFiltersValue[K]) {
  emit('update:modelValue', {...filters.value, [key]: value})
}

function clearFilters() {
  emit('clear')
}

function clearSelectFilters() {
  emit('update:modelValue', {
    ...filters.value,
    status: '',
    priority: '',
    categoryId: '',
  })
  mobileFiltersOpen.value = false
}
</script>

<template>
  <section class="grid gap-3 rounded-xl border border-neutral-200 bg-white p-3 shadow-sm">
    <!-- Desktop: tudo inline -->
    <div class="hidden items-center gap-2.5 lg:grid lg:grid-cols-[1fr_auto_auto_auto_auto]">
      <div class="search-wrapper relative">
        <BaseInput
          :disabled="loading"
          :model-value="filters.search ?? ''"
          aria-label="Buscar tarefas"
          placeholder="Buscar por título ou descrição"
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
        placeholder="Situação"
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

    <!-- Mobile: search + botão filtros -->
    <div class="flex items-center gap-2 lg:hidden">
      <div class="search-wrapper relative flex-1">
        <BaseInput
          :disabled="loading"
          :model-value="filters.search ?? ''"
          aria-label="Buscar tarefas"
          placeholder="Buscar por título ou descrição"
          @update:modelValue="patchFilters('search', $event)"
        />
        <Search
          :size="15"
          class="pointer-events-none absolute right-3.5 top-1/2 -translate-y-1/2 text-neutral-400"
        />
      </div>

      <BaseButton
        class="relative shrink-0"
        size="sm"
        variant="secondary"
        @click="mobileFiltersOpen = true"
      >
        <Filter :size="14"/>
        Filtros
        <span
          v-if="activeSelectFiltersCount > 0"
          class="absolute -right-1.5 -top-1.5 flex h-4 w-4 items-center justify-center rounded-full bg-primary text-[0.625rem] font-bold text-white"
        >
          {{ activeSelectFiltersCount }}
        </span>
      </BaseButton>

      <BaseButton
        v-if="hasActiveFilters"
        size="sm"
        variant="ghost"
        @click="clearFilters"
      >
        <X :size="14"/>
      </BaseButton>
    </div>

    <!-- Filter state bar -->
    <div class="-mx-3 -mb-3 flex items-center gap-2 border-t border-neutral-100 bg-neutral-50/50 px-4 py-2">
      <span class="text-[0.8125rem] font-medium text-neutral-500">
        {{ hasActiveFilters ? 'Filtros ativos aplicados à listagem.' : 'Nenhum filtro adicional aplicado.' }}
      </span>
      <span
        v-if="hasActiveFilters"
        class="inline-flex h-4 w-4 items-center justify-center rounded-full bg-primary text-[0.625rem] font-bold text-white"
      >
        {{ activeFiltersCount }}
      </span>
    </div>
  </section>

  <!-- Mobile filters modal -->
  <BaseModal
    :model-value="mobileFiltersOpen"
    size="sm"
    title="Filtros"
    description="Refine a lista de tarefas."
    @update:model-value="mobileFiltersOpen = $event"
  >
    <div class="grid gap-4">
      <BaseSelect
        :disabled="loading"
        :model-value="filters.status ?? ''"
        :options="statusOptions"
        label="Situação"
        placeholder="Situação"
        @update:modelValue="patchFilters('status', $event as TaskStatus | '')"
      />

      <BaseSelect
        :disabled="loading"
        :model-value="filters.priority ?? ''"
        :options="priorityOptions"
        label="Prioridade"
        placeholder="Prioridade"
        @update:modelValue="patchFilters('priority', $event as TaskPriority | '')"
      />

      <BaseSelect
        :disabled="loading"
        :model-value="filters.categoryId ?? ''"
        :options="categoryOptions"
        label="Categoria"
        placeholder="Categoria"
        value-mode="number"
        @update:modelValue="patchFilters('categoryId', $event as number | '')"
      />

      <div class="flex gap-3 pt-2">
        <BaseButton
          :disabled="!hasActiveSelectFilters"
          class="flex-1"
          variant="secondary"
          @click="clearSelectFilters"
        >
          Limpar filtros
        </BaseButton>
        <BaseButton
          class="flex-1"
          @click="mobileFiltersOpen = false"
        >
          Aplicar
        </BaseButton>
      </div>
    </div>
  </BaseModal>
</template>

<style scoped>
.search-wrapper :deep(input) {
  padding-right: 2.25rem;
}
</style>
