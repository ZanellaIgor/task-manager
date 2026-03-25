<script lang="ts" setup>
interface Props {
  title: string
  description?: string
}

defineProps<Props>()
</script>

<template>
  <div class="flex flex-col gap-1 sm:flex-row sm:items-center sm:justify-between sm:gap-4">
    <div>
      <h2 class="font-display text-xl font-bold tracking-tight text-neutral-900 sm:text-2xl">
        {{ title }}
      </h2>
      <p v-if="description" class="mt-0.5 text-sm text-neutral-400">
        {{ description }}
      </p>
    </div>
    
    <div v-if="$slots.actions" class="mt-4 flex items-center gap-2.5 sm:mt-0">
      <slot name="actions" />
    </div>
  </div>
</template>
