<script lang="ts" setup>
import {CheckCircle2, Info, X, XCircle} from 'lucide-vue-next'
import {type ToastMessage, useToast} from '@/composables/useToast'

const {toasts, remove} = useToast()

const iconByType: Record<ToastMessage['type'], typeof CheckCircle2> = {
  success: CheckCircle2,
  error: XCircle,
  info: Info,
}

const styleByType: Record<ToastMessage['type'], string> = {
  success: 'border-emerald-200 bg-white text-emerald-700',
  error:   'border-red-200 bg-white text-red-700',
  info:    'border-primary/20 bg-white text-primary',
}

const iconStyleByType: Record<ToastMessage['type'], string> = {
  success: 'text-emerald-500',
  error:   'text-red-500',
  info:    'text-primary',
}
</script>

<template>
  <Teleport to="body">
    <div
        aria-live="polite"
        aria-atomic="false"
        class="fixed bottom-5 right-5 z-[200] flex flex-col gap-2.5 w-[min(22rem,calc(100vw-2.5rem))]"
        role="region"
    >
      <TransitionGroup name="toast">
        <div
            v-for="toast in toasts"
            :key="toast.id"
            :class="styleByType[toast.type]"
            class="flex items-start gap-3 rounded-xl border px-4 py-3 shadow-[0_4px_20px_rgba(15,23,42,0.12)] backdrop-blur-sm"
            role="alert"
        >
          <component
              :is="iconByType[toast.type]"
              :class="iconStyleByType[toast.type]"
              :size="18"
              aria-hidden="true"
              class="mt-0.5 shrink-0"
          />
          <p class="flex-1 text-[0.875rem] font-medium text-neutral-800 leading-[1.4]">
            {{ toast.message }}
          </p>
          <button
              :aria-label="`Fechar notificação: ${toast.message}`"
              class="shrink-0 mt-0.5 text-neutral-400 hover:text-neutral-600 transition-colors cursor-pointer"
              type="button"
              @click="remove(toast.id)"
          >
            <X :size="15" aria-hidden="true"/>
          </button>
        </div>
      </TransitionGroup>
    </div>
  </Teleport>
</template>

<style scoped>
.toast-enter-active,
.toast-leave-active {
  transition: all 280ms cubic-bezier(0.16, 1, 0.3, 1);
}

.toast-enter-from {
  opacity: 0;
  transform: translateX(1.5rem) translateY(0.5rem);
}

.toast-leave-to {
  opacity: 0;
  transform: translateX(1.5rem);
}

.toast-move {
  transition: transform 200ms ease;
}
</style>
