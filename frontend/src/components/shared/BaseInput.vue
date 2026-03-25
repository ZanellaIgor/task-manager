<script lang="ts" setup>
import {computed} from 'vue'

defineOptions({inheritAttrs: false})

const props = withDefaults(
    defineProps<{
      modelValue?: string | number | null
      label?: string
      id?: string
      name?: string
      type?: string
      placeholder?: string
      helpText?: string
      error?: string
      required?: boolean
      disabled?: boolean
      textarea?: boolean
      rows?: number
    }>(),
    {
      type: 'text',
      required: false,
      disabled: false,
      textarea: false,
      rows: 4,
    },
)

const emit = defineEmits<{
  'update:modelValue': [value: string]
  blur: [event: FocusEvent]
  focus: [event: FocusEvent]
}>()

const inputId = computed(() => props.id ?? props.name)

function onInput(event: Event) {
  const target = event.target as HTMLInputElement | HTMLTextAreaElement
  emit('update:modelValue', target.value)
}

const controlClasses = [
  'w-full rounded-xl border border-neutral-400/40 bg-white/95 px-3.5 py-2.5',
  'text-[0.9375rem] text-neutral-900 shadow-[0_1px_2px_rgba(15,23,42,0.04)]',
  'transition-all duration-200 ease-out',
  'placeholder:text-neutral-400',
  'focus:outline-none focus:border-primary-light/70 focus:shadow-[0_0_0_3px_rgba(59,111,255,0.10)]',
  'disabled:cursor-not-allowed disabled:bg-neutral-50 disabled:text-neutral-500',
].join(' ')
</script>

<template>
  <label :for="inputId" class="flex flex-col gap-[0.45rem]">
    <span v-if="label" class="text-sm font-semibold text-neutral-900">
      {{ label }}
      <span v-if="required" class="ml-0.5 text-danger">*</span>
    </span>

    <component
        :is="textarea ? 'textarea' : 'input'"
        :id="inputId"
        :aria-describedby="helpText || error ? `${inputId}-meta` : undefined"
        :aria-invalid="Boolean(error) || undefined"
        :class="[controlClasses, textarea && 'resize-y min-h-[6.5rem]']"
        :disabled="disabled"
        :name="name"
        :placeholder="placeholder"
        :rows="textarea ? rows : undefined"
        :type="textarea ? undefined : type"
        :value="modelValue ?? ''"
        v-bind="$attrs"
        @blur="$emit('blur', $event)"
        @focus="$emit('focus', $event)"
        @input="onInput"
    />

    <p
        v-if="error || helpText"
        :id="`${inputId}-meta`"
        :class="error ? 'text-red-600' : 'text-neutral-500'"
        class="text-[0.82rem]"
    >
      {{ error || helpText }}
    </p>
  </label>
</template>
