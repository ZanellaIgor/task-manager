<script lang="ts" setup>
import {computed, ref, watch} from 'vue'
import {ChevronRight, LayoutDashboard, ListTodo, Tag, X} from 'lucide-vue-next'
import {RouterLink} from 'vue-router'
import BaseButton from '@/components/shared/BaseButton.vue'

interface NavItem {
  label: string
  to: string
  description?: string
  badge?: string
}

const props = withDefaults(
    defineProps<{
      modelValue?: boolean
      brand?: string
      tagline?: string
      items?: NavItem[]
    }>(),
    {
      modelValue: true,
      brand: 'Task Manager',
      tagline: 'Task Manager',
      items: () => [
        {label: 'Dashboard', to: '/dashboard', description: 'Resumo operacional'},
        {label: 'Tarefas', to: '/tasks', description: 'Fila e prioridades'},
        {label: 'Categorias', to: '/categories', description: 'Organizacao por dominio'},
      ],
    },
)

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
}>()

const iconByIndex = computed(() => [LayoutDashboard, ListTodo, Tag])
const internalOpen = ref(props.modelValue ?? true)

watch(
    () => props.modelValue,
    (value) => {
      if (value !== undefined) internalOpen.value = value
    },
    {immediate: true},
)

const isOpen = computed(() => internalOpen.value)

function close() {
  internalOpen.value = false
  emit('update:modelValue', false)
}
</script>

<template>
  <Teleport to="body">
    <Transition name="sidebar-fade">
      <div
          v-if="isOpen"
          class="fixed inset-0 z-40 bg-neutral-900/[0.48] min-[721px]:hidden"
          @click="close"
      />
    </Transition>
  </Teleport>

  <aside
      :data-open="isOpen"
      class="sidebar fixed inset-y-0 left-0 z-50 flex w-[18.5rem] flex-col gap-[1.2rem] p-[1.1rem] bg-gradient-to-b from-primary to-primary-dark text-white shadow-[24px_0_40px_rgba(15,23,42,0.16)]"
  >
    <div class="flex items-center gap-3.5">
      <div
          class="grid size-[2.8rem] place-items-center rounded-[0.95rem] bg-white/[0.14] text-[1.2rem] font-bold"
      >
        M
      </div>
      <div>
        <p class="text-[1.15rem] font-bold tracking-[-0.01em]">{{ brand }}</p>
        <p class="mt-0.5 text-[0.84rem] leading-[1.4] text-white/[0.82]">{{ tagline }}</p>
      </div>
      <BaseButton
          class="ml-auto text-white min-[721px]:hidden"
          size="sm"
          variant="ghost"
          @click="close"
      >
        <X :size="16"/>
      </BaseButton>
    </div>

    <nav aria-label="Main navigation" class="flex flex-col gap-[0.55rem]">
      <RouterLink
          v-for="(item, index) in items"
          :key="item.to"
          v-slot="{ isActive, navigate }"
          :to="item.to"
          class="text-inherit"
          custom
      >
        <button
            :class="isActive && 'bg-white/[0.16] border-white/[0.16]'"
            class="w-full grid grid-cols-[auto_minmax(0,1fr)_auto_auto] items-center gap-3 px-[0.95rem] py-[0.9rem] border border-transparent rounded-2xl bg-white/[0.08] text-left text-inherit cursor-pointer transition-all duration-200 ease-out hover:translate-x-0.5 hover:bg-white/[0.14]"
            type="button"
            @click="navigate(); close()"
        >
          <component
              :is="iconByIndex[index] ?? ChevronRight"
              :size="18"
              class="shrink-0"
          />
          <span class="flex flex-col gap-0.5 min-w-0">
            <strong class="text-[0.95rem]">{{ item.label }}</strong>
            <small
                v-if="item.description"
                class="text-[0.8rem] leading-[1.35] text-white/[0.82]"
            >
              {{ item.description }}
            </small>
          </span>
          <span
              v-if="item.badge"
              class="px-2 py-1 rounded-full bg-white/[0.14] text-xs font-bold"
          >
            {{ item.badge }}
          </span>
          <ChevronRight :size="16" class="shrink-0"/>
        </button>
      </RouterLink>
    </nav>

    <div class="mt-auto px-1">
      <div class="flex items-center gap-2 px-3 py-3 rounded-xl bg-white/[0.08]">
        <div class="grid size-7 shrink-0 place-items-center rounded-lg bg-white/[0.16] text-[0.7rem] font-bold">
          TM
        </div>
        <div class="min-w-0">
          <p class="text-[0.8rem] font-semibold leading-none text-white truncate">{{ brand }}</p>
          <p class="mt-0.5 text-[0.72rem] text-white/[0.55]">v1.0.0</p>
        </div>
      </div>
    </div>
  </aside>
</template>

<style scoped>
.sidebar-fade-enter-active,
.sidebar-fade-leave-active {
  transition: opacity 180ms ease;
}

.sidebar-fade-enter-from,
.sidebar-fade-leave-to {
  opacity: 0;
}

@media (max-width: 720px) {
  .sidebar {
    transform: translateX(-102%);
    transition: transform 220ms ease;
  }

  .sidebar[data-open="true"] {
    transform: translateX(0);
  }
}
</style>
