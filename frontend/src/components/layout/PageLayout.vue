<script lang="ts" setup>
interface Props {
  withSidebarPadding?: boolean
  lockViewport?: boolean
}

withDefaults(defineProps<Props>(), {
  withSidebarPadding: true,
  lockViewport: false,
})
</script>

<template>
  <div
    :class="[
      'flex bg-bg-app text-neutral-900',
      lockViewport ? 'h-screen overflow-hidden' : 'min-h-screen',
    ]"
  >
    <slot name="sidebar" />

    <div
      :class="[
        'flex min-w-0 flex-1 flex-col transition-all duration-300',
        withSidebarPadding ? 'lg:pl-[18.5rem]' : '',
        lockViewport ? 'overflow-hidden' : '',
      ]"
    >
      <slot name="header" />

      <main
        :class="[
          'flex min-h-0 flex-1 flex-col gap-6 px-4 py-6 sm:px-6 lg:px-8',
          lockViewport ? 'overflow-hidden' : '',
        ]"
      >
        <slot />
      </main>
    </div>
  </div>
</template>
