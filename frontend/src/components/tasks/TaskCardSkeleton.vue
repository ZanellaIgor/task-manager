<script lang="ts" setup>
import BaseSkeleton from '@/components/shared/BaseSkeleton.vue'
</script>

<template>
  <div class="flex flex-col gap-3 p-4 border border-neutral-200 rounded-xl bg-white shadow-[0_2px_12px_rgba(15,23,42,0.06)]">
    <div class="flex flex-col gap-2.5">
      <BaseSkeleton height="1.2rem" width="70%" rounded="md" />
      <div class="space-y-1.5">
        <BaseSkeleton height="0.8rem" width="95%" rounded="sm" />
        <BaseSkeleton height="0.8rem" width="60%" rounded="sm" />
      </div>
    </div>

    <div class="flex flex-wrap items-center gap-2 mt-1.5">
      <BaseSkeleton height="1.25rem" width="5.5rem" rounded="md" />
      <BaseSkeleton height="1.25rem" width="6.5rem" rounded="md" />
    </div>

    <div class="flex items-center justify-between gap-3 mt-1">
      <div class="flex gap-1.5">
        <BaseSkeleton height="1.5rem" width="3.5rem" rounded="full" />
        <BaseSkeleton height="1.5rem" width="3.5rem" rounded="full" />
      </div>
      <BaseSkeleton height="1.25rem" width="4rem" rounded="full" />
    </div>

    <div class="-mx-4 border-t border-neutral-50 mt-1" />

    <div class="flex items-center gap-2 pt-1">
      <BaseSkeleton height="2rem" width="5.5rem" rounded="lg" />
      <BaseSkeleton height="2rem" width="6rem" rounded="lg" />
      <BaseSkeleton class="ml-auto" height="2rem" width="2rem" rounded="lg" />
    </div>
  </div>
</template>
