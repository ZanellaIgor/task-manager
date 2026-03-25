<script lang="ts" setup>
import {computed} from 'vue'

type TaskStatus = 'Pending' | 'InProgress' | 'Completed' | 'Cancelled'
type TaskPriority = 'Low' | 'Medium' | 'High'

const props = withDefaults(
    defineProps<{
      status: TaskStatus
      priority?: TaskPriority
      compact?: boolean
      size?: 'sm' | 'md'
    }>(),
    {
      compact: false,
      size: 'md',
    },
)

const statusConfig = computed(() => {
  switch (props.status) {
    case 'Completed':
      return {label: 'Concluida', classes: 'bg-success/12 text-emerald-700'}
    case 'InProgress':
      return {label: 'Em andamento', classes: 'bg-primary/12 text-primary-dark'}
    case 'Cancelled':
      return {label: 'Cancelada', classes: 'bg-neutral-200 text-neutral-600'}
    default:
      return {label: 'Pendente', classes: 'bg-warning/[0.14] text-amber-700'}
  }
})

const priorityConfig = computed(() => {
  switch (props.priority) {
    case 'High':
      return {label: 'Alta', classes: 'bg-danger/12 text-red-700'}
    case 'Low':
      return {label: 'Baixa', classes: 'bg-success/12 text-emerald-700'}
    case 'Medium':
      return {label: 'Media', classes: 'bg-warning/[0.14] text-amber-700'}
    default:
      return null
  }
})

const sizeClasses = computed(() =>
    props.compact || props.size === 'sm'
        ? 'px-2 py-1 text-[0.74rem]'
        : 'px-2.5 py-1.5 text-[0.8rem]',
)
</script>

<template>
  <span class="inline-flex flex-wrap gap-1.5">
    <span
        :class="[
        'inline-flex items-center justify-center rounded-full font-semibold leading-none tracking-[0.01em]',
        sizeClasses,
        statusConfig.classes,
      ]"
    >
      {{ statusConfig.label }}
    </span>
    <span
        v-if="priorityConfig"
        :class="[
        'inline-flex items-center justify-center rounded-full font-semibold leading-none tracking-[0.01em]',
        sizeClasses,
        priorityConfig.classes,
      ]"
    >
      {{ priorityConfig.label }}
    </span>
  </span>
</template>
