<script lang="ts" setup>
interface Props {
  width?: string
  height?: string
  circle?: boolean
  rounded?: 'none' | 'sm' | 'md' | 'lg' | 'xl' | '2xl' | 'full'
  animate?: boolean
}

withDefaults(defineProps<Props>(), {
  width: '100%',
  height: '1rem',
  circle: false,
  rounded: 'md',
  animate: true,
})
</script>

<template>
  <div
    :class="[
      'bg-neutral-200/60',
      animate ? 'animate-pulse' : '',
      circle ? 'rounded-full' : `rounded-${rounded}`
    ]"
    :style="{ width, height }"
  />
</template>
