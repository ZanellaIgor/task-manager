<script lang="ts" setup>
import {computed} from 'vue'
import {Ban, CheckCircle2, Clock, Loader2} from 'lucide-vue-next'
import BaseBadge from '@/components/shared/BaseBadge.vue'

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
      return {label: 'Concluída', icon: CheckCircle2, variant: 'success' as const}
    case 'InProgress':
      return {label: 'Em andamento', icon: Loader2, variant: 'info' as const}
    case 'Cancelled':
      return {label: 'Cancelada', icon: Ban, variant: 'neutral' as const}
    default:
      return {label: 'Pendente', icon: Clock, variant: 'warning' as const}
  }
})

const priorityConfig = computed(() => {
  switch (props.priority) {
    case 'High':
      return {label: 'Alta', dot: 'bg-red-500'}
    case 'Low':
      return {label: 'Baixa', dot: 'bg-emerald-500'}
    case 'Medium':
      return {label: 'Média', dot: 'bg-amber-400'}
    default:
      return null
  }
})

const iconSize = computed(() => (props.compact || props.size === 'sm' ? 11 : 13))
</script>

<template>
  <div class="inline-flex flex-wrap gap-2">
    <BaseBadge
      :rounded="'full'"
      :size="size"
      :variant="statusConfig.variant"
      class="gap-1.5 font-bold"
    >
      <component
        :is="statusConfig.icon"
        :class="status === 'InProgress' && 'animate-spin'"
        :size="iconSize"
        aria-hidden="true"
        class="shrink-0"
      />
      {{ statusConfig.label }}
    </BaseBadge>

    <BaseBadge
      v-if="priorityConfig"
      :rounded="'full'"
      :size="size"
      class="gap-1.5 font-bold"
      variant="neutral"
    >
      <span
        :class="priorityConfig.dot"
        aria-hidden="true"
        class="h-1.5 w-1.5 shrink-0 rounded-full"
      />
      {{ priorityConfig.label }}
    </BaseBadge>
  </div>
</template>
