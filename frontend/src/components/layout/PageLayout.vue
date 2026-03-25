<script lang="ts" setup>
interface Props {
  withSidebarPadding?: boolean
}

withDefaults(defineProps<Props>(), {
  withSidebarPadding: true,
})
</script>

<template>
  <div class="flex min-h-screen bg-bg-app text-neutral-900">
    <slot name="sidebar" />

    <div
      :class="[
        'flex min-w-0 flex-1 flex-col transition-all duration-300',
        withSidebarPadding ? 'lg:pl-[18.5rem]' : ''
      ]"
    >
      <slot name="header" />

      <main class="flex-1 space-y-6 px-4 py-6 sm:px-6 lg:px-8">
        <slot />
      </main>
    </div>
  </div>
</template>
