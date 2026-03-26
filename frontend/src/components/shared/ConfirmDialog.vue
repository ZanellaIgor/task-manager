<script lang="ts" setup>
import {AlertTriangle} from 'lucide-vue-next'
import BaseButton from '@/components/shared/BaseButton.vue'
import BaseModal from '@/components/shared/BaseModal.vue'

const props = withDefaults(
    defineProps<{
      modelValue: boolean
      title?: string
      description?: string
      confirmLabel?: string
      cancelLabel?: string
      loading?: boolean
      variant?: 'danger' | 'warning'
    }>(),
    {
      title: 'Confirmar ação',
      description: 'Esta ação não pode ser desfeita.',
      confirmLabel: 'Confirmar',
      cancelLabel: 'Cancelar',
      loading: false,
      variant: 'danger',
    },
)

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  confirm: []
  cancel: []
}>()

function handleCancel() {
  emit('update:modelValue', false)
  emit('cancel')
}

function handleConfirm() {
  emit('confirm')
}
</script>

<template>
  <BaseModal
      :model-value="modelValue"
      :persistent="loading"
      size="sm"
      @update:model-value="!loading && emit('update:modelValue', $event)"
  >
    <div class="flex flex-col items-center gap-4 py-2 text-center">
      <!-- Icon -->
      <div
          :class="variant === 'danger' ? 'bg-danger-soft text-danger-text' : 'bg-warning-soft text-warning-text'"
          class="flex h-12 w-12 items-center justify-center rounded-full"
      >
        <AlertTriangle :size="22" aria-hidden="true"/>
      </div>

      <!-- Text -->
      <div>
        <h3 class="text-base font-bold text-neutral-900">{{ title }}</h3>
        <p class="mt-1.5 text-sm text-neutral-500 leading-relaxed max-w-[20rem]">{{ description }}</p>
      </div>

      <!-- Actions -->
      <div class="flex w-full gap-3 pt-1">
        <BaseButton
            :disabled="loading"
            class="flex-1"
            full-width
            type="button"
            variant="ghost"
            @click="handleCancel"
        >
          {{ cancelLabel }}
        </BaseButton>
        <BaseButton
            :loading="loading"
            class="flex-1"
            full-width
            type="button"
            :variant="variant === 'danger' ? 'danger' : 'primary'"
            @click="handleConfirm"
        >
          {{ confirmLabel }}
        </BaseButton>
      </div>
    </div>
  </BaseModal>
</template>
