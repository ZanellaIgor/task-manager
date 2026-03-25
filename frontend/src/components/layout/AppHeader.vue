<script lang="ts" setup>
import {computed, useSlots} from 'vue'
import {Menu, Search} from 'lucide-vue-next'
import BaseButton from '@/components/shared/BaseButton.vue'

const props = withDefaults(
    defineProps<{
      title: string
      description?: string
      subtitle?: string
      sidebarOpen?: boolean
    }>(),
    {
      sidebarOpen: false,
    },
)

const emit = defineEmits<{
  'toggle-sidebar': []
}>()

const slots = useSlots()
const hasSearchSlot = computed(() => Boolean(slots.search))
const hasActionsSlot = computed(() => Boolean(slots.actions || slots.default))
</script>

<template>
  <header
      class="flex items-center justify-between gap-4 px-6 py-3 border-b border-neutral-200 bg-white shadow-sm max-[960px]:flex-wrap max-[960px]:items-start"
  >
    <div class="flex items-center gap-3.5 min-w-0">
      <BaseButton
          class="inline-flex min-[721px]:hidden"
          size="sm"
          variant="secondary"
          @click="emit('toggle-sidebar')"
      >
        <Menu :size="16"/>
      </BaseButton>

      <div>
        <h1 class="text-[clamp(1.35rem,2.2vw,1.75rem)] font-bold leading-[1.1] tracking-[-0.02em] text-neutral-900">
          {{ props.title }}
        </h1>
        <p
            v-if="props.description || props.subtitle"
            class="mt-1 text-[0.84rem] leading-normal text-neutral-500"
        >
          {{ props.description || props.subtitle }}
        </p>
      </div>
    </div>

    <div
        v-if="hasSearchSlot || hasActionsSlot"
        class="flex items-center gap-3 ml-auto max-[960px]:w-full max-[960px]:ml-0 max-[960px]:justify-between"
    >
      <div v-if="hasSearchSlot" class="relative flex items-center max-[960px]:flex-1">
        <Search :size="16" class="absolute left-4 text-neutral-400 pointer-events-none"/>
        <slot name="search"/>
      </div>
      <slot name="actions">
        <slot/>
      </slot>
    </div>
  </header>
</template>

<style scoped>
header :deep(.relative input) {
  min-width: min(26rem, 32vw);
  padding-left: 2.3rem;
}

@media (max-width: 960px) {
  header :deep(.relative input) {
    min-width: 0;
    width: 100%;
  }
}
</style>
