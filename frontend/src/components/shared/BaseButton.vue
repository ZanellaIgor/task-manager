<script lang="ts" setup>
import {computed} from 'vue'

type Variant = 'primary' | 'secondary' | 'soft' | 'ghost' | 'danger'
type Size = 'sm' | 'md' | 'lg'

defineOptions({inheritAttrs: false})

const props = withDefaults(
    defineProps<{
      variant?: Variant
      size?: Size
      type?: 'button' | 'submit' | 'reset'
      loading?: boolean
      disabled?: boolean
      fullWidth?: boolean
    }>(),
    {
      variant: 'primary',
      size: 'md',
      type: 'button',
      loading: false,
      disabled: false,
      fullWidth: false,
    },
)

const sizeMap: Record<Size, string> = {
  sm: 'h-9 px-3 text-sm sm:h-8',
  md: 'h-11 px-3.5 text-[0.9375rem] sm:h-9',
  lg: 'h-12 px-5 text-base sm:h-10',
}

const variantMap: Record<Variant, string> = {
  primary:
      'bg-gradient-to-b from-primary-light to-primary text-white shadow-[0_4px_12px_rgba(27,79,216,0.22)] hover:shadow-[0_6px_16px_rgba(27,79,216,0.30)]',
  secondary:
      'bg-white text-neutral-700 border-neutral-300 shadow-[0_1px_2px_rgba(15,23,42,0.06)] hover:bg-neutral-50 hover:border-neutral-400',
  soft:
      'bg-primary-soft text-primary border-transparent shadow-none hover:bg-primary-soft-hover hover:text-primary-dark',
  ghost: 'bg-transparent text-neutral-600 border-neutral-200 hover:bg-neutral-100 hover:text-neutral-800',
  danger: 'bg-danger text-white border-danger shadow-[0_4px_12px_rgba(220,38,38,0.20)] hover:brightness-90',
}

const classes = computed(() => [
  'relative inline-flex items-center justify-center gap-2 border border-transparent rounded-[0.875rem]',
  'font-semibold leading-none whitespace-nowrap cursor-pointer',
  'transition-all duration-200 ease-out',
  'focus-visible:outline-[3px] focus-visible:outline-primary-light/20 focus-visible:outline-offset-2',
  sizeMap[props.size],
  variantMap[props.variant],
  props.fullWidth && 'w-full',
  props.disabled || props.loading
      ? 'cursor-not-allowed opacity-50'
      : 'hover:-translate-y-px',
])
</script>

<template>
  <button
      :class="classes"
      :aria-busy="loading"
      :disabled="disabled || loading"
      :type="type"
      v-bind="$attrs"
  >
    <span
        v-if="loading"
        aria-hidden="true"
        class="pointer-events-none absolute inset-0 m-auto size-4 rounded-full border-2 border-current border-r-transparent animate-spin"
    />
    <span
        :class="loading ? 'opacity-0' : 'opacity-100'"
        class="inline-flex items-center gap-1.5 transition-opacity duration-150"
    >
      <slot/>
    </span>
  </button>
</template>
