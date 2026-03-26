<script lang="ts" setup>
import {onBeforeUnmount, watch} from 'vue'

defineOptions({inheritAttrs: false})

const props = withDefaults(
    defineProps<{
      modelValue: boolean
      title?: string
      description?: string
      size?: 'sm' | 'md' | 'lg' | 'xl'
      persistent?: boolean
    }>(),
    {
      size: 'md',
      persistent: false,
    },
)

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  close: []
}>()

const sizeClass: Record<string, string> = {
  sm: 'w-[min(100%,28rem)]',
  md: 'w-[min(100%,44rem)]',
  lg: 'w-[min(100%,56rem)]',
  xl: 'w-[min(100%,70rem)]',
}

function close() {
  emit('update:modelValue', false)
  emit('close')
}

function onKeydown(event: KeyboardEvent) {
  if (event.key === 'Escape' && !props.persistent) close()
}

watch(
    () => props.modelValue,
    (isOpen) => {
      if (isOpen) {
        window.addEventListener('keydown', onKeydown)
        document.body.style.overflow = 'hidden'
      } else {
        window.removeEventListener('keydown', onKeydown)
        document.body.style.overflow = ''
      }
    },
    {immediate: true},
)

onBeforeUnmount(() => {
  window.removeEventListener('keydown', onKeydown)
  document.body.style.overflow = ''
})
</script>

<template>
  <Teleport to="body">
    <Transition name="modal">
      <div
          v-if="modelValue"
          class="fixed inset-0 z-[60] grid place-items-center p-4 bg-neutral-900/50 backdrop-blur-sm"
          @click.self="!persistent && close()"
      >
        <div
            :aria-label="title || 'Dialog'"
            :class="[
            'max-h-[min(90vh,52rem)] overflow-auto rounded-[1.25rem] border border-neutral-400/20 bg-white shadow-[0_28px_60px_rgba(15,23,42,0.2)]',
            sizeClass[size],
          ]"
            aria-modal="true"
            role="dialog"
            v-bind="$attrs"
        >
          <header
              v-if="title || description"
              class="flex items-start justify-between gap-4 px-5 pt-5 pb-2"
          >
            <div>
              <h2
                  v-if="title"
                  class="text-[1.45rem] font-bold leading-[1.1] tracking-[-0.02em] text-neutral-900"
              >
                {{ title }}
              </h2>
              <p v-if="description" class="mt-1.5 text-[0.9375rem] leading-normal text-neutral-600">
                {{ description }}
              </p>
            </div>
            <button
                aria-label="Close modal"
                class="grid size-11 shrink-0 place-items-center rounded-full border border-neutral-400/20 bg-neutral-50 text-neutral-900 cursor-pointer transition-colors hover:bg-neutral-100 sm:size-9"
                type="button"
                @click="close"
            >
              <span aria-hidden="true">×</span>
            </button>
          </header>

          <div class="px-5 pt-3 pb-5">
            <slot/>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.modal-enter-active,
.modal-leave-active {
  transition: opacity 180ms ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.modal-enter-active [role="dialog"],
.modal-leave-active [role="dialog"] {
  transition: transform 180ms ease,
  opacity 180ms ease;
}

.modal-enter-from [role="dialog"],
.modal-leave-to [role="dialog"] {
  transform: translateY(12px) scale(0.98);
  opacity: 0;
}
</style>
