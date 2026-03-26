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

  return new Intl.DateTimeFormat('pt-BR', includeTime
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
      }).format(date)
}
</script>

<template>
  <section class="grid gap-5">
    <div class="flex flex-wrap items-start justify-between gap-3 rounded-2xl border border-neutral-200 bg-neutral-50/70 px-4 py-4">
      <div class="min-w-0">
        <p class="text-[0.75rem] font-semibold uppercase tracking-[0.18em] text-neutral-400">
          Resumo da tarefa
        </p>
        <h3 class="mt-2 text-[1.15rem] font-semibold leading-tight text-neutral-900">
          {{ task.title }}
        </h3>
      </div>

      <div class="flex flex-wrap items-center gap-2">
        <BaseBadge :variant="statusVariant" rounded="full" size="sm">
          {{ TASK_STATUS_LABELS[task.status] }}
        </BaseBadge>
        <BaseBadge rounded="full" size="sm" variant="neutral">
          {{ TASK_PRIORITY_LABELS[task.priority] }}
        </BaseBadge>
      </div>
    </div>

    <div class="grid gap-4">
      <div class="grid gap-1.5">
        <span class="text-sm font-semibold text-neutral-900">Título</span>
        <div class="rounded-xl border border-neutral-400/40 bg-neutral-50 px-3.5 py-3 text-[0.9375rem] text-neutral-900 shadow-[0_1px_2px_rgba(15,23,42,0.04)]">
          {{ task.title }}
        </div>
      </div>

      <div class="grid gap-1.5">
        <span class="text-sm font-semibold text-neutral-900">Descrição</span>
        <div class="min-h-[7.5rem] rounded-xl border border-neutral-400/40 bg-neutral-50 px-3.5 py-3 text-[0.9375rem] leading-relaxed whitespace-pre-wrap text-neutral-900 shadow-[0_1px_2px_rgba(15,23,42,0.04)]">
          {{ task.description?.trim() || 'Nenhuma descrição informada.' }}
        </div>
      </div>

      <div class="grid grid-cols-2 gap-4 max-sm:grid-cols-1">
        <div class="grid gap-1.5">
          <span class="text-sm font-semibold text-neutral-900">Categoria</span>
          <div class="flex min-h-[3rem] items-center gap-2 rounded-xl border border-neutral-400/40 bg-neutral-50 px-3.5 py-3 text-[0.9375rem] text-neutral-900 shadow-[0_1px_2px_rgba(15,23,42,0.04)]">
            <FolderKanban class="h-4 w-4 text-neutral-400" />
            <span>{{ task.category?.name || 'Categoria não vinculada' }}</span>
          </div>
        </div>

        <div class="grid gap-1.5">
          <span class="text-sm font-semibold text-neutral-900">Prioridade</span>
          <div class="flex min-h-[3rem] items-center gap-2 rounded-xl border border-neutral-400/40 bg-neutral-50 px-3.5 py-3 text-[0.9375rem] text-neutral-900 shadow-[0_1px_2px_rgba(15,23,42,0.04)]">
            <Tag class="h-4 w-4 text-neutral-400" />
            <span>{{ TASK_PRIORITY_LABELS[task.priority] }}</span>
          </div>
        </div>

        <div class="grid gap-1.5">
          <span class="text-sm font-semibold text-neutral-900">Status</span>
          <div class="flex min-h-[3rem] items-center gap-2 rounded-xl border border-neutral-400/40 bg-neutral-50 px-3.5 py-3 text-[0.9375rem] text-neutral-900 shadow-[0_1px_2px_rgba(15,23,42,0.04)]">
            <CheckCheck class="h-4 w-4 text-neutral-400" />
            <span>{{ TASK_STATUS_LABELS[task.status] }}</span>
            <span v-if="isOverdue" class="ml-auto inline-flex items-center gap-1 text-xs font-semibold text-danger">
              <CircleAlert class="h-3.5 w-3.5" />
              Atrasada
            </span>
          </div>
        </div>

        <div class="grid gap-1.5">
          <span class="text-sm font-semibold text-neutral-900">Prazo</span>
          <div class="flex min-h-[3rem] items-center gap-2 rounded-xl border border-neutral-400/40 bg-neutral-50 px-3.5 py-3 text-[0.9375rem] text-neutral-900 shadow-[0_1px_2px_rgba(15,23,42,0.04)]">
            <CalendarDays class="h-4 w-4 text-neutral-400" />
            <span>{{ formatDate(task.dueDate) }}</span>
          </div>
        </div>
      </div>

      <div class="grid grid-cols-3 gap-4 max-lg:grid-cols-1">
        <div class="rounded-2xl border border-neutral-200 bg-white px-4 py-3 shadow-[0_1px_2px_rgba(15,23,42,0.04)]">
          <p class="text-[0.75rem] font-semibold uppercase tracking-[0.16em] text-neutral-400">
            Criada em
          </p>
          <p class="mt-2 flex items-center gap-2 text-sm font-medium text-neutral-700">
            <History class="h-4 w-4 text-neutral-400" />
            {{ formatDate(task.createdAt, true) }}
          </p>
        </div>

        <div class="rounded-2xl border border-neutral-200 bg-white px-4 py-3 shadow-[0_1px_2px_rgba(15,23,42,0.04)]">
          <p class="text-[0.75rem] font-semibold uppercase tracking-[0.16em] text-neutral-400">
            Atualizada em
          </p>
          <p class="mt-2 flex items-center gap-2 text-sm font-medium text-neutral-700">
            <Clock3 class="h-4 w-4 text-neutral-400" />
            {{ formatDate(task.updatedAt, true) }}
          </p>
        </div>

        <div class="rounded-2xl border border-neutral-200 bg-white px-4 py-3 shadow-[0_1px_2px_rgba(15,23,42,0.04)]">
          <p class="text-[0.75rem] font-semibold uppercase tracking-[0.16em] text-neutral-400">
            Concluída em
          </p>
          <p class="mt-2 flex items-center gap-2 text-sm font-medium text-neutral-700">
            <CheckCheck class="h-4 w-4 text-neutral-400" />
            {{ formatDate(task.completedAt, true) }}
          </p>
        </div>
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
  </section>
</template>
