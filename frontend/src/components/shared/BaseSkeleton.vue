<script lang="ts" setup>
type Rounded = 'none' | 'sm' | 'md' | 'lg' | 'xl' | '2xl' | 'full'

interface Props {
  width?: string
  height?: string
  circle?: boolean
  rounded?: Rounded
  animate?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  width: '100%',
  height: '1rem',
  circle: false,
  rounded: 'md',
  animate: true,
})

const roundedMap: Record<Rounded, string> = {
  none: 'rounded-none',
  sm: 'rounded-sm',
  md: 'rounded-md',
  lg: 'rounded-lg',
  xl: 'rounded-xl',
  '2xl': 'rounded-2xl',
  full: 'rounded-full',
}
</script>

<template>
  <div
    :class="[
      'bg-neutral-200/60',
      props.animate ? 'animate-pulse' : '',
      props.circle ? 'rounded-full' : roundedMap[props.rounded]
    ]"
    :style="{ width, height }"
  />
</template>
