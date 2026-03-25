<script lang="ts" setup>
import {AlertCircle} from 'lucide-vue-next'
import BaseButton from '@/components/shared/BaseButton.vue'

const props = withDefaults(
  defineProps<{
    title?: string
    description: string
    retryLabel?: string
    compact?: boolean
    showRetry?: boolean
  }>(),
  {
    title: 'Falha ao carregar dados',
    retryLabel: 'Tentar novamente',
    compact: false,
    showRetry: false,
  },
)

const emit = defineEmits<{
  retry: []
}>()
</script>

<template>
  <section
    :class="compact ? 'px-6 py-8' : 'p-6'"
    class="rounded-card border border-danger/15 bg-white shadow-card"
  >
    <div class="flex items-start gap-3">
      <AlertCircle class="mt-0.5 h-5 w-5 text-danger" />
      <div>
        <p class="font-semibold text-neutral-900">{{ props.title }}</p>
        <p class="mt-1 text-sm text-neutral-400">
          {{ props.description }}
        </p>
        <BaseButton
          v-if="showRetry"
          class="mt-4"
          variant="secondary"
          @click="emit('retry')"
        >
          {{ props.retryLabel }}
        </BaseButton>
      </div>
    </div>
  </section>
</template>
