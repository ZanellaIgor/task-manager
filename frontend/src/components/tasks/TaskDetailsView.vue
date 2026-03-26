<script lang="ts" setup>
import {computed} from 'vue'
import {CalendarDays, CheckCheck, CircleAlert, Clock3, FolderKanban, History, Tag} from 'lucide-vue-next'
import BaseBadge from '@/components/shared/BaseBadge.vue'
import BaseButton from '@/components/shared/BaseButton.vue'
import {TASK_PRIORITY_LABELS, TASK_STATUS_LABELS, type Task} from '@/types'

const props = defineProps<{
  task: Task
}>()

const emit = defineEmits<{
  close: []
  edit: []
}>()

const canEdit = computed(() =>
  props.task.status !== 'Completed' && props.task.status !== 'Cancelled',
)

const statusVariant = computed(() => {
  switch (props.task.status) {
    case 'Completed':
      return 'success' as const
    case 'InProgress':
      return 'info' as const
    case 'Cancelled':
      return 'neutral' as const
    default:
      return 'warning' as const
  }
})

const priorityVariant = computed(() => {
  switch (props.task.priority) {
    case 'High':
      return 'danger' as const
    case 'Medium':
      return 'warning' as const
    default:
      return 'success' as const
  }
})

const isOverdue = computed(() => {
  if (!props.task.dueDate || props.task.status === 'Completed' || props.task.status === 'Cancelled') {
    return false
  }

  const dueDate = new Date(props.task.dueDate)
  if (Number.isNaN(dueDate.getTime())) {
    return false
  }

  const today = new Date()
  today.setHours(0, 0, 0, 0)
  dueDate.setHours(23, 59, 59, 999)
  return dueDate < today
})

function formatDate(value?: string, includeTime = false) {
  if (!value) {
    return 'Não informado'
  }

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return value
  }

  return new Intl.DateTimeFormat(
    'pt-BR',
    includeTime
      ? {
          day: '2-digit',
          month: 'short',
          year: 'numeric',
          hour: '2-digit',
          minute: '2-digit',
        }
      : {
          day: '2-digit',
          month: 'short',
          year: 'numeric',
        },
  ).format(date)
}
</script>

<template>
  <div class="grid gap-5">
    <div class="flex flex-wrap items-start justify-between gap-3">
      <div class="min-w-0">
        <p class="text-[0.75rem] font-semibold uppercase tracking-[0.18em] text-neutral-400">
          Tarefa #{{ task.id }}
        </p>
        <h3 class="mt-2 text-[1.3rem] font-bold leading-[1.15] tracking-[-0.02em] text-neutral-900">
          {{ task.title }}
        </h3>
      </div>

      <div class="flex flex-wrap items-center gap-2">
        <BaseBadge :variant="statusVariant" rounded="full" size="sm">
          {{ TASK_STATUS_LABELS[task.status] }}
        </BaseBadge>
        <BaseBadge :variant="priorityVariant" rounded="full" size="sm">
          {{ TASK_PRIORITY_LABELS[task.priority] }}
        </BaseBadge>
      </div>
    </div>

    <div class="grid gap-3 sm:grid-cols-2">
      <div class="rounded-xl border border-neutral-200 bg-neutral-50/70 px-4 py-3">
        <p class="flex items-center gap-2 text-[0.75rem] font-semibold uppercase tracking-[0.14em] text-neutral-400">
          <FolderKanban class="h-4 w-4" />
          Categoria
        </p>
        <p class="mt-2 text-sm font-semibold text-neutral-900">
          {{ task.category?.name || 'Não vinculada' }}
        </p>
      </div>

      <div class="rounded-xl border border-neutral-200 bg-neutral-50/70 px-4 py-3">
        <p class="flex items-center gap-2 text-[0.75rem] font-semibold uppercase tracking-[0.14em] text-neutral-400">
          <CalendarDays class="h-4 w-4" />
          Prazo
        </p>
        <div class="mt-2 flex items-center gap-2">
          <p :class="isOverdue ? 'text-danger-text' : 'text-neutral-900'" class="text-sm font-semibold">
            {{ formatDate(task.dueDate) }}
          </p>
          <span
            v-if="isOverdue"
            class="inline-flex items-center gap-1 rounded-full bg-danger-soft px-2 py-0.5 text-[0.7rem] font-semibold text-danger-text"
          >
            <CircleAlert class="h-3.5 w-3.5" />
            Atrasada
          </span>
        </div>
      </div>
    </div>

    <div class="grid gap-2">
      <p class="flex items-center gap-2 text-[0.75rem] font-semibold uppercase tracking-[0.14em] text-neutral-400">
        <Tag class="h-4 w-4" />
        Descrição
      </p>
      <div class="max-h-64 overflow-y-auto rounded-xl border border-neutral-200 bg-neutral-50/70 px-4 py-4 text-[0.95rem] leading-7 whitespace-pre-wrap text-neutral-700">
        {{ task.description?.trim() || 'Nenhuma descrição foi informada para esta tarefa.' }}
      </div>
    </div>

    <div class="grid gap-3 sm:grid-cols-3">
      <div class="rounded-xl border border-neutral-200 bg-neutral-50/70 px-4 py-3">
        <p class="flex items-center gap-2 text-[0.75rem] font-semibold uppercase tracking-[0.14em] text-neutral-400">
          <History class="h-4 w-4" />
          Criada em
        </p>
        <p class="mt-2 text-sm font-semibold text-neutral-800">
          {{ formatDate(task.createdAt, true) }}
        </p>
      </div>

      <div class="rounded-xl border border-neutral-200 bg-neutral-50/70 px-4 py-3">
        <p class="flex items-center gap-2 text-[0.75rem] font-semibold uppercase tracking-[0.14em] text-neutral-400">
          <Clock3 class="h-4 w-4" />
          Atualizada em
        </p>
        <p class="mt-2 text-sm font-semibold text-neutral-800">
          {{ formatDate(task.updatedAt, true) }}
        </p>
      </div>

      <div class="rounded-xl border border-neutral-200 bg-neutral-50/70 px-4 py-3">
        <p class="flex items-center gap-2 text-[0.75rem] font-semibold uppercase tracking-[0.14em] text-neutral-400">
          <CheckCheck class="h-4 w-4" />
          Concluída em
        </p>
        <p class="mt-2 text-sm font-semibold text-neutral-800">
          {{ formatDate(task.completedAt, true) }}
        </p>
      </div>
    </div>

    <div class="flex justify-end gap-3 border-t border-neutral-100 pt-1 max-sm:flex-col">
      <BaseButton class="max-sm:w-full" type="button" variant="secondary" @click="emit('close')">
        Fechar
      </BaseButton>
      <BaseButton
        v-if="canEdit"
        class="max-sm:w-full"
        type="button"
        @click="emit('edit')"
      >
        Editar tarefa
      </BaseButton>
    </div>
  </div>
</template>
