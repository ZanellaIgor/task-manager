<script lang="ts" setup>
import {computed} from 'vue'
import {Ban, CalendarDays, CheckCheck, CircleAlert, PencilLine, Trash2} from 'lucide-vue-next'
import TaskStatusBadge from '@/components/tasks/TaskStatusBadge.vue'
import BaseButton from '@/components/shared/BaseButton.vue'
import type {Task} from '@/types'

type TaskCardTask = Pick<Task, 'id' | 'title' | 'description' | 'category' | 'status' | 'priority' | 'dueDate' | 'createdAt' | 'updatedAt'>

const props = defineProps<{
  task: TaskCardTask
  loadingComplete?: boolean
  loadingCancel?: boolean
}>()

const emit = defineEmits<{
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
  if (!props.task.dueDate) return 'Sem prazo'
  const date = new Date(props.task.dueDate)
  if (Number.isNaN(date.getTime())) return props.task.dueDate
  return new Intl.DateTimeFormat('pt-BR', {dateStyle: 'medium'}).format(date)
})
</script>

<template>
  <article
      :aria-label="`Tarefa: ${task.title}`"
      :class="
      isOverdue
        ? 'border-danger/[0.24] shadow-[0_16px_34px_rgba(239,68,68,0.08)]'
        : 'border-neutral-200 shadow-[0_2px_12px_rgba(15,23,42,0.06)] hover:shadow-[0_8px_24px_rgba(15,23,42,0.10)] hover:border-primary-light/[0.25]'
    "
      class="flex flex-col gap-3 p-4 border rounded-xl bg-white transition-all duration-200 ease-out hover:-translate-y-0.5"
  >
    <!-- Title + Description -->
    <div class="flex flex-col gap-1">
      <h3
          :class="isTerminal && 'line-through text-neutral-400'"
          class="text-[1rem] font-bold leading-[1.25] tracking-[-0.01em] text-neutral-900"
      >
        {{ task.title }}
      </h3>
      <p v-if="task.description" class="text-[0.84rem] leading-[1.55] text-neutral-500 line-clamp-2">
        {{ task.description }}
      </p>
    </div>

    <!-- Metadata: category + date -->
    <div class="flex flex-wrap items-center gap-2 text-[0.78rem] leading-none text-neutral-500">
      <span
          v-if="task.category"
          class="inline-flex items-center gap-1 px-2 py-1 rounded-md bg-neutral-100 font-medium text-neutral-600"
      >
        {{ task.category.name }}
      </span>
      <span
          :class="isOverdue && !isTerminal ? 'text-red-500 font-semibold' : 'text-neutral-400'"
          class="inline-flex items-center gap-1"
      >
        <CalendarDays :size="12"/>
        {{ dueDateLabel }}
      </span>
    </div>

    <!-- Status badges + overdue alert -->
    <div class="flex items-center justify-between gap-3">
      <div class="flex flex-wrap items-center gap-1.5">
        <TaskStatusBadge :priority="task.priority" :status="task.status" size="sm"/>
      </div>
      <span v-if="isOverdue" class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-red-50 text-[0.72rem] font-semibold text-red-600">
        <CircleAlert :size="11" aria-hidden="true"/>
        Atrasada
      </span>
    </div>

    <!-- Divider -->
    <div class="-mx-4 border-t border-neutral-100"/>

    <!-- Actions -->
    <div class="flex items-center gap-2">
      <template v-if="!isTerminal">
        <BaseButton
            aria-label="Editar tarefa"
            size="sm"
            variant="ghost"
            @click="emit('edit')"
        >
          <PencilLine :size="13" aria-hidden="true"/>
          Editar
        </BaseButton>

        <BaseButton
            v-if="task.status !== 'Completed'"
            :disabled="loadingComplete"
            :loading="loadingComplete"
            aria-label="Marcar tarefa como concluída"
            class="text-emerald-700 bg-emerald-50 border-emerald-100 hover:bg-emerald-100"
            size="sm"
            variant="ghost"
            @click="emit('complete')"
        >
          <CheckCheck :size="13" aria-hidden="true"/>
          Concluir
        </BaseButton>

        <BaseButton
            v-if="task.status === 'Pending' || task.status === 'InProgress'"
            :disabled="loadingCancel"
            :loading="loadingCancel"
            aria-label="Cancelar tarefa"
            class="text-amber-700 bg-amber-50 border-amber-100 hover:bg-amber-100"
            size="sm"
            variant="ghost"
            @click="emit('cancel')"
        >
          <Ban :size="13" aria-hidden="true"/>
          Cancelar
        </BaseButton>
      </template>

      <span
          v-else
          :class="task.status === 'Completed' ? 'text-emerald-700' : 'text-neutral-500'"
          class="text-[0.78rem] font-semibold"
      >
        {{ task.status === 'Completed' ? '✓ Concluída' : '✕ Cancelada' }}
      </span>

      <BaseButton
          aria-label="Excluir tarefa"
          class="ml-auto text-neutral-400 hover:text-red-600 hover:bg-red-50"
          size="sm"
          variant="ghost"
          @click="emit('delete')"
      >
        <Trash2 :size="13" aria-hidden="true"/>
        Excluir
      </BaseButton>
    </div>
  </article>
</template>
