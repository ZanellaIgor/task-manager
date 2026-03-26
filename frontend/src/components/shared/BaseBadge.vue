<script lang="ts" setup>
import {computed} from 'vue'

interface Props {
  variant?: 'default' | 'success' | 'warning' | 'danger' | 'info' | 'neutral'
  size?: 'sm' | 'md'
  rounded?: 'full' | 'md'
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'default',
  size: 'md',
  rounded: 'md',
})

const variantClasses = computed(() => {
  switch (props.variant) {
    case 'success':
      return 'bg-success-soft text-success-text border-success-border'
    case 'warning':
      return 'bg-warning-soft text-warning-text border-warning-border'
    case 'danger':
      return 'bg-danger-soft text-danger-text border-danger-border'
    case 'info':
      return 'bg-info-soft text-info-text border-info-border'
    case 'neutral':
      return 'bg-neutral-100 text-neutral-600 border-neutral-200'
    default:
      return 'bg-primary-soft text-primary border-primary/20'
  }
})

const sizeClasses = computed(() => {
  return props.size === 'sm' ? 'px-1.5 py-0.5 text-[0.625rem]' : 'px-2 py-0.5 text-xs'
})

const roundedClasses = computed(() => {
  return props.rounded === 'full' ? 'rounded-full' : 'rounded-md'
})
</script>

<template>
  <span
    :class="[
      variantClasses,
      sizeClasses,
      roundedClasses,
      'inline-flex items-center font-bold border'
    ]"
  >
    <slot />
  </span>
</template>
