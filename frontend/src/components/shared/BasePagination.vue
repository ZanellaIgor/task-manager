<script lang="ts" setup>
import {computed} from 'vue'
import BaseButton from '@/components/shared/BaseButton.vue'

const props = withDefaults(
  defineProps<{
    page: number
    totalPages: number
    totalItems: number
    pageSize: number
    disabled?: boolean
  }>(),
  {
    disabled: false,
  },
)

const emit = defineEmits<{
  'update:page': [value: number]
}>()

const startItem = computed(() => (props.totalItems === 0 ? 0 : (props.page - 1) * props.pageSize + 1))
const endItem = computed(() => Math.min(props.page * props.pageSize, props.totalItems))
</script>

<template>
  <div class="flex flex-col gap-3 rounded-xl border border-neutral-200 bg-white px-4 py-3 shadow-sm sm:flex-row sm:items-center sm:justify-between">
    <p class="text-sm text-neutral-500">
      Exibindo {{ startItem }}-{{ endItem }} de {{ totalItems }} itens
    </p>

    <div class="flex items-center gap-2 self-end sm:self-auto">
      <BaseButton
        :disabled="disabled || page <= 1"
        size="sm"
        variant="secondary"
        @click="emit('update:page', page - 1)"
      >
        Anterior
      </BaseButton>
      <span class="min-w-24 text-center text-sm font-medium text-neutral-600">
        Página {{ totalPages === 0 ? 0 : page }} de {{ totalPages }}
      </span>
      <BaseButton
        :disabled="disabled || page >= totalPages || totalPages === 0"
        size="sm"
        variant="secondary"
        @click="emit('update:page', page + 1)"
      >
        Próxima
      </BaseButton>
    </div>
  </div>
</template>
