<script lang="ts" setup>
import {computed, ref, watch, onBeforeUnmount, nextTick, type CSSProperties} from 'vue'
import {ChevronDown, Check} from 'lucide-vue-next'

defineOptions({inheritAttrs: false})

type ValueMode = 'string' | 'number'

interface SelectOption {
  label: string
  value: string | number | null
  disabled?: boolean
}

const props = withDefaults(
    defineProps<{
      modelValue?: string | number | null
      label?: string
      id?: string
      name?: string
      placeholder?: string
      helpText?: string
      error?: string
      required?: boolean
      disabled?: boolean
      options: SelectOption[]
      valueMode?: ValueMode
    }>(),
    {
      required: false,
      disabled: false,
      valueMode: 'string',
    },
)

const emit = defineEmits<{
  'update:modelValue': [value: string | number | null]
  blur: [event: FocusEvent]
  focus: [event: FocusEvent]
}>()

const inputId = computed(() => props.id ?? props.name)
const isOpen = ref(false)
const highlightedIndex = ref(-1)
const triggerRef = ref<HTMLButtonElement | null>(null)
const listboxRef = ref<HTMLUListElement | null>(null)
const floatingStyle = ref<CSSProperties>({})

const allOptions = computed<SelectOption[]>(() => {
  if (props.placeholder) {
    return [{label: props.placeholder, value: null}, ...props.options]
  }
  return props.options
})

const selectedOption = computed(() =>
    allOptions.value.find((opt) => {
      if (props.modelValue === null || props.modelValue === undefined || props.modelValue === '') {
        return opt.value === null || opt.value === ''
      }
      return String(opt.value) === String(props.modelValue)
    }),
)

const displayLabel = computed(() => {
  if (selectedOption.value && selectedOption.value.value !== null && selectedOption.value.value !== '') {
    return selectedOption.value.label
  }
  return props.placeholder ?? ''
})

const isPlaceholder = computed(() => {
  if (!selectedOption.value) return true
  return selectedOption.value.value === null || selectedOption.value.value === ''
})

function normalize(value: string | number | null): string | number | null {
  if (value === null || value === '') return null
  return props.valueMode === 'number' ? Number(value) : value
}

function updateFloatingPosition() {
  const trigger = triggerRef.value
  if (!trigger) return

  const rect = trigger.getBoundingClientRect()
  const spaceBelow = window.innerHeight - rect.bottom
  const spaceAbove = rect.top
  const dropdownMaxHeight = 240
  const gap = 6

  const openAbove = spaceBelow < dropdownMaxHeight && spaceAbove > spaceBelow

  floatingStyle.value = {
    position: 'fixed',
    left: `${rect.left}px`,
    width: `${rect.width}px`,
    ...(openAbove
        ? {bottom: `${window.innerHeight - rect.top + gap}px`}
        : {top: `${rect.bottom + gap}px`}),
  }
}

function toggle() {
  if (props.disabled) return
  isOpen.value ? close() : open()
}

function open() {
  if (props.disabled) return
  isOpen.value = true
  const idx = allOptions.value.findIndex(
      (opt) => String(opt.value ?? '') === String(props.modelValue ?? ''),
  )
  highlightedIndex.value = idx >= 0 ? idx : 0
  updateFloatingPosition()
  nextTick(() => scrollToHighlighted())
}

function close() {
  isOpen.value = false
  highlightedIndex.value = -1
}

function select(option: SelectOption) {
  if (option.disabled) return
  emit('update:modelValue', normalize(option.value))
  close()
  triggerRef.value?.focus()
}

function scrollToHighlighted() {
  const list = listboxRef.value
  if (!list) return
  const item = list.children[highlightedIndex.value] as HTMLElement | undefined
  item?.scrollIntoView({block: 'nearest'})
}

function onKeydown(event: KeyboardEvent) {
  const opts = allOptions.value

  switch (event.key) {
    case 'Enter':
    case ' ':
      event.preventDefault()
      if (isOpen.value && highlightedIndex.value >= 0) {
        select(opts[highlightedIndex.value])
      } else {
        open()
      }
      break
    case 'ArrowDown':
      event.preventDefault()
      if (!isOpen.value) {
        open()
      } else {
        let next = highlightedIndex.value + 1
        while (next < opts.length && opts[next].disabled) next++
        if (next < opts.length) {
          highlightedIndex.value = next
          nextTick(() => scrollToHighlighted())
        }
      }
      break
    case 'ArrowUp':
      event.preventDefault()
      if (isOpen.value) {
        let prev = highlightedIndex.value - 1
        while (prev >= 0 && opts[prev].disabled) prev--
        if (prev >= 0) {
          highlightedIndex.value = prev
          nextTick(() => scrollToHighlighted())
        }
      }
      break
    case 'Escape':
      if (isOpen.value) {
        event.preventDefault()
        close()
        triggerRef.value?.focus()
      }
      break
    case 'Tab':
      if (isOpen.value) close()
      break
    case 'Home':
      if (isOpen.value) {
        event.preventDefault()
        highlightedIndex.value = 0
        nextTick(() => scrollToHighlighted())
      }
      break
    case 'End':
      if (isOpen.value) {
        event.preventDefault()
        highlightedIndex.value = opts.length - 1
        nextTick(() => scrollToHighlighted())
      }
      break
  }
}

function onClickOutside(event: MouseEvent) {
  const target = event.target as Node
  if (
      !triggerRef.value?.contains(target) &&
      !listboxRef.value?.contains(target)
  ) {
    close()
  }
}

function onScroll() {
  if (isOpen.value) updateFloatingPosition()
}

watch(isOpen, (open) => {
  if (open) {
    document.addEventListener('mousedown', onClickOutside)
    window.addEventListener('scroll', onScroll, true)
    window.addEventListener('resize', onScroll)
  } else {
    document.removeEventListener('mousedown', onClickOutside)
    window.removeEventListener('scroll', onScroll, true)
    window.removeEventListener('resize', onScroll)
  }
})

onBeforeUnmount(() => {
  document.removeEventListener('mousedown', onClickOutside)
  window.removeEventListener('scroll', onScroll, true)
  window.removeEventListener('resize', onScroll)
})

const triggerClasses = computed(() => [
  'w-full rounded-xl border bg-white/95 px-3.5 py-3 sm:py-2.5',
  'text-[0.9375rem] shadow-[0_1px_2px_rgba(15,23,42,0.04)]',
  'transition-all duration-200 ease-out',
  'flex items-center justify-between gap-2 text-left',
  isOpen.value
      ? 'border-primary-light/70 shadow-[0_0_0_3px_rgba(59,111,255,0.10)]'
      : 'border-neutral-400/40',
  props.disabled
      ? 'cursor-not-allowed bg-neutral-50 text-neutral-500'
      : 'cursor-pointer',
])
</script>

<template>
  <div class="flex flex-col gap-1.5" v-bind="$attrs">
    <label v-if="label" :for="inputId" class="text-sm font-semibold text-neutral-900">
      {{ label }}
      <span v-if="required" class="ml-0.5 text-danger">*</span>
    </label>

    <div class="relative">
      <button
          :id="inputId"
          ref="triggerRef"
          :aria-describedby="helpText || error ? `${inputId}-meta` : undefined"
          :aria-expanded="isOpen"
          :aria-invalid="Boolean(error) || undefined"
          :class="triggerClasses"
          :disabled="disabled"
          :name="name"
          aria-haspopup="listbox"
          role="combobox"
          type="button"
          @blur="$emit('blur', $event)"
          @click="toggle"
          @focus="$emit('focus', $event)"
          @keydown="onKeydown"
      >
        <span :class="isPlaceholder ? 'text-neutral-500' : 'text-neutral-900'" class="truncate">
          {{ displayLabel }}
        </span>
        <ChevronDown
            :class="isOpen && 'rotate-180'"
            :size="16"
            class="shrink-0 text-neutral-400 transition-transform duration-200"
        />
      </button>
    </div>

    <p
        v-if="error || helpText"
        :id="`${inputId}-meta`"
        :class="error ? 'text-danger-text' : 'text-neutral-500'"
        class="text-[0.8125rem]"
    >
      {{ error || helpText }}
    </p>
  </div>

  <Teleport to="body">
    <Transition
        enter-active-class="transition duration-150 ease-out"
        enter-from-class="opacity-0 -translate-y-1"
        enter-to-class="opacity-100 translate-y-0"
        leave-active-class="transition duration-100 ease-in"
        leave-from-class="opacity-100 translate-y-0"
        leave-to-class="opacity-0 -translate-y-1"
    >
      <ul
          v-if="isOpen"
          ref="listboxRef"
          :aria-labelledby="inputId"
          :style="floatingStyle"
          class="z-[100] max-h-60 overflow-auto rounded-xl border border-neutral-200 bg-white py-1 shadow-lg"
          role="listbox"
      >
        <li
            v-for="(option, index) in allOptions"
            :key="`${option.label}-${option.value}`"
            :aria-disabled="option.disabled || undefined"
            :aria-selected="String(option.value ?? '') === String(modelValue ?? '')"
            :class="[
              'flex cursor-pointer items-center gap-2 px-3.5 py-2.5 text-[0.9375rem] transition-colors duration-100',
              option.disabled
                ? 'cursor-not-allowed text-neutral-400'
                : index === highlightedIndex
                  ? 'bg-primary-soft text-primary-dark'
                  : 'text-neutral-900 hover:bg-neutral-50',
              String(option.value ?? '') === String(modelValue ?? '') && !option.disabled
                ? 'font-medium'
                : '',
            ]"
            role="option"
            @click="select(option)"
            @mouseenter="highlightedIndex = index"
        >
          <span class="flex-1 truncate">{{ option.label }}</span>
          <Check
              v-if="String(option.value ?? '') === String(modelValue ?? '')"
              :size="15"
              class="shrink-0 text-primary"
          />
        </li>
      </ul>
    </Transition>
  </Teleport>
</template>
