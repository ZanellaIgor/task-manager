<script lang="ts" setup>
import {computed} from 'vue'
import {Ban, CalendarDays, CheckCheck, CircleAlert, Eye, PencilLine, Trash2} from 'lucide-vue-next'
import BaseBadge from '@/components/shared/BaseBadge.vue'
import BaseButton from '@/components/shared/BaseButton.vue'
import type {Task, TaskPriority} from '@/types'

type TaskCardTask = Pick<Task, 'id' | 'title' | 'description' | 'category' | 'status' | 'priority' | 'dueDate' | 'createdAt' | 'updatedAt'>

const props = defineProps<{
  task: TaskCardTask
  loadingComplete?: boolean
  loadingCancel?: boolean
}>()

const emit = defineEmits<{
  view: []
  edit: []
  complete: []
  cancel: []
  delete: []
}>()

const isTerminal = computed(
    () => props.task.status === 'Completed' || props.task.status === 'Cancelled',
)

const isOverdue = computed(() => {
  if (!props.task.dueDate || isTerminal.value) return false
  const dueDate = new Date(props.task.dueDate)
  if (Number.isNaN(dueDate.getTime())) return false
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  dueDate.setHours(23, 59, 59, 999)
  return dueDate < today
})

const dueDateLabel = computed(() => {
  if (!props.task.dueDate) return null
  const date = new Date(props.task.dueDate)
  if (Number.isNaN(date.getTime())) return props.task.dueDate
  return new Intl.DateTimeFormat('pt-BR', {day: '2-digit', month: 'short'}).format(date)
})

const priorityAccent: Record<TaskPriority, string> = {
  High: 'bg-danger',
  Medium: 'bg-warning',
  Low: 'bg-success',
}

const statusLabel = computed(() => {
  switch (props.task.status) {
    case 'Completed': return 'Concluída'
    case 'InProgress': return 'Em andamento'
    case 'Cancelled': return 'Cancelada'
    default: return 'Pendente'
  }
})

const statusVariant = computed(() => {
  switch (props.task.status) {
    case 'Completed': return 'success' as const
    case 'InProgress': return 'info' as const
    case 'Cancelled': return 'neutral' as const
    default: return 'warning' as const
  }
})
</script>

<template>
  <article
      :aria-label="`Tarefa: ${task.title}`"
      :class="[
        isOverdue
          ? 'ring-1 ring-danger/20'
          : 'ring-1 ring-neutral-200/80 hover:ring-primary-light/30',
        isTerminal && 'opacity-75',
      ]"
      class="group relative flex flex-col overflow-hidden rounded-2xl bg-white shadow-sm transition-all duration-200 hover:shadow-md"
  >
    <!-- Priority accent bar -->
    <div
        :class="priorityAccent[task.priority]"
        class="h-1 w-full"
    />

    <div class="flex flex-col gap-3 p-4">
      <!-- Header: status badge + date -->
      <div class="flex items-center justify-between gap-2">
        <BaseBadge :variant="statusVariant" rounded="full" size="sm">
          {{ statusLabel }}
        </BaseBadge>

        <div class="flex items-center gap-2">
          <span
              v-if="isOverdue"
              class="inline-flex items-center gap-1 text-xs font-semibold text-danger"
          >
            <CircleAlert :size="12" aria-hidden="true"/>
            Atrasada
          </span>
          <span
              v-if="dueDateLabel"
              :class="isOverdue && !isTerminal ? 'text-danger' : 'text-neutral-400'"
              class="inline-flex items-center gap-1 text-xs"
          >
            <CalendarDays :size="12" aria-hidden="true"/>
            {{ dueDateLabel }}
          </span>
        </div>
      </div>

      <!-- Title + Description -->
      <div>
        <h3
            :class="isTerminal ? 'line-through text-neutral-400' : 'text-neutral-900'"
            class="font-semibold leading-snug"
        >
          {{ task.title }}
        </h3>
        <p
            v-if="task.description"
            class="mt-1 text-[0.8125rem] leading-relaxed text-neutral-500 line-clamp-2"
        >
          {{ task.description }}
        </p>
      </div>

      <!-- Category chip -->
      <span
          v-if="task.category"
          class="inline-flex self-start items-center gap-1 px-2 py-0.5 rounded-md bg-neutral-100 text-xs font-medium text-neutral-500"
      >
        {{ task.category.name }}
      </span>

      <!-- Actions -->
      <div class="flex items-center gap-1 border-t border-neutral-100 -mx-4 -mb-4 px-3 py-2 bg-neutral-50/50">
        <template v-if="!isTerminal">
          <BaseButton
              aria-label="Visualizar tarefa"
              size="sm"
              variant="ghost"
              @click="emit('view')"
          >
            <Eye :size="14" aria-hidden="true"/>
          </BaseButton>

          <BaseButton
              v-if="task.status !== 'Completed'"
              :disabled="loadingComplete"
              :loading="loadingComplete"
              aria-label="Concluir tarefa"
              class="text-success-text bg-success-soft border-success-border hover:brightness-95"
              size="sm"
              variant="ghost"
              @click="emit('complete')"
          >
            <CheckCheck :size="14" aria-hidden="true"/>
            Concluir
          </BaseButton>

          <BaseButton
              aria-label="Editar tarefa"
              size="sm"
              variant="ghost"
              @click="emit('edit')"
          >
            <PencilLine :size="14" aria-hidden="true"/>
          </BaseButton>

          <BaseButton
              v-if="task.status === 'Pending' || task.status === 'InProgress'"
              :disabled="loadingCancel"
              :loading="loadingCancel"
              aria-label="Cancelar tarefa"
              class="text-warning-text hover:bg-warning-soft"
              size="sm"
              variant="ghost"
              @click="emit('cancel')"
          >
            <Ban :size="14" aria-hidden="true"/>
          </BaseButton>
        </template>

        <span
            v-else
            :class="task.status === 'Completed' ? 'text-success-text' : 'text-neutral-400'"
            class="text-[0.8125rem] font-medium px-1"
        >
          {{ task.status === 'Completed' ? '✓ Concluída' : '✕ Cancelada' }}
        </span>

        <BaseButton
            aria-label="Visualizar tarefa"
            size="sm"
            variant="ghost"
            @click="emit('view')"
        >
          <Eye :size="14" aria-hidden="true"/>
        </BaseButton>

        <BaseButton
            aria-label="Excluir tarefa"
            class="ml-auto text-neutral-300 hover:text-danger-text hover:bg-danger-soft"
            size="sm"
            variant="ghost"
            @click="emit('delete')"
        >
          <Trash2 :size="14" aria-hidden="true"/>
        </BaseButton>
      </div>
    </div>
  </article>
</template>
