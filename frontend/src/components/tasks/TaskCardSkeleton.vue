<script lang="ts" setup>
import BaseSkeleton from '@/components/shared/BaseSkeleton.vue'
</script>

<template>
  <div class="flex flex-col overflow-hidden rounded-2xl bg-white ring-1 ring-neutral-200/80 shadow-sm">
    <BaseSkeleton height="0.25rem" width="100%" rounded="none" :animate="true" />

    <div class="flex flex-col gap-3 p-4">
      <!-- Header -->
      <div class="flex items-center justify-between">
        <BaseSkeleton height="1.25rem" width="5rem" rounded="full" />
        <BaseSkeleton height="0.875rem" width="4rem" rounded="sm" />
      </div>

      <!-- Title + Description -->
      <div class="space-y-2">
        <BaseSkeleton height="1rem" width="75%" rounded="md" />
        <BaseSkeleton height="0.8rem" width="90%" rounded="sm" />
      </div>

      <!-- Category chip -->
      <BaseSkeleton height="1.25rem" width="5.5rem" rounded="md" />

      <!-- Actions -->
      <div class="flex items-center gap-2 border-t border-neutral-100 -mx-4 -mb-4 px-3 py-2.5 bg-neutral-50/50">
        <BaseSkeleton height="2rem" width="5.5rem" rounded="lg" />
        <BaseSkeleton height="2rem" width="2rem" rounded="lg" />
        <BaseSkeleton class="ml-auto" height="2rem" width="2rem" rounded="lg" />
      </div>
    </div>
  </div>
</template>
