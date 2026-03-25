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
      return 'bg-emerald-50 text-emerald-700 border-emerald-200'
    case 'warning':
      return 'bg-amber-50 text-amber-700 border-amber-200'
    case 'danger':
      return 'bg-rose-50 text-rose-700 border-rose-200'
    case 'info':
      return 'bg-cyan-50 text-cyan-700 border-cyan-200'
    case 'neutral':
      return 'bg-neutral-100 text-neutral-600 border-neutral-200'
    default:
      return 'bg-primary-soft text-primary border-primary/20'
  }
})

const sizeClasses = computed(() => {
  return props.size === 'sm' ? 'px-1.5 py-0.5 text-[0.65rem]' : 'px-2 py-0.5 text-[0.75rem]'
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
