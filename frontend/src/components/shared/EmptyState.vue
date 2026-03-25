<script lang="ts" setup>
import type {Component} from 'vue'

const props = withDefaults(
    defineProps<{
      icon?: Component
      iconClass?: string
      title: string
      description?: string
      compact?: boolean
    }>(),
    {
      compact: false,
    },
)
</script>

<template>
  <div
      :class="compact ? 'py-10' : 'py-16'"
      class="flex flex-col items-center justify-center text-center px-6"
  >
    <!-- Icon -->
    <div
        v-if="icon"
        class="mb-5 flex h-14 w-14 items-center justify-center rounded-2xl bg-neutral-100 text-neutral-400"
    >
      <component :is="icon" :size="26" aria-hidden="true"/>
    </div>

    <!-- Text -->
    <p class="text-[1.05rem] font-bold text-neutral-900">{{ title }}</p>
    <p v-if="description" class="mt-1.5 max-w-xs text-sm text-neutral-400 leading-relaxed">
      {{ description }}
    </p>

    <!-- CTA slot -->
    <div v-if="$slots.default" class="mt-6">
      <slot/>
    </div>
  </div>
</template>
