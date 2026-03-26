<script lang="ts" setup>
import {computed, ref} from 'vue'
import {RouterLink} from 'vue-router'
import {ArrowRight, CalendarClock, CircleCheckBig, Clock3, ListTodo} from 'lucide-vue-next'
import AppHeader from '@/components/layout/AppHeader.vue'
import AppSidebar from '@/components/layout/AppSidebar.vue'
import TaskStatusBadge from '@/components/tasks/TaskStatusBadge.vue'
import BaseButton from '@/components/shared/BaseButton.vue'
import ErrorState from '@/components/shared/ErrorState.vue'
import BaseSkeleton from '@/components/shared/BaseSkeleton.vue'
import {useTaskOverview} from '@/queries/taskQueries'
import {getErrorMessage} from '@/services/api'
import PageLayout from '@/components/layout/PageLayout.vue'
import SectionHeader from '@/components/shared/SectionHeader.vue'

const {data: overview, isLoading, isError, error, refetch} = useTaskOverview()
const sidebarOpen = ref(false)
const toggleSidebar = () => { sidebarOpen.value = !sidebarOpen.value }

const summaryCards = computed(() => {
  const data = overview.value

  return [
    {
      title: 'Total',
      value: data?.totalCount ?? 0,
      tone: 'default' as const,
      icon: ListTodo,
    },
    {
      title: 'Pendentes',
      value: data?.pendingCount ?? 0,
      tone: 'warning' as const,
      icon: Clock3,
    },
    {
      title: 'Em andamento',
      value: data?.inProgressCount ?? 0,
      tone: 'info' as const,
      icon: CalendarClock,
    },
    {
      title: 'Concluídas',
      value: data?.completedCount ?? 0,
      tone: 'success' as const,
      icon: CircleCheckBig,
    },
  ]
})

const recentTasks = computed(() => overview.value?.recentTasks ?? [])
const upcomingTasks = computed(() => overview.value?.upcomingTasks ?? [])

function formatDate(value?: string) {
  if (!value) {
    return 'Sem prazo'
  }

  return new Intl.DateTimeFormat('pt-BR', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  }).format(new Date(value))
}
</script>

<template>
  <PageLayout>
    <template #sidebar>
      <AppSidebar :model-value="sidebarOpen" @update:model-value="sidebarOpen = $event" />
    </template>

    <template #header>
      <AppHeader
        description="Visão geral das tarefas, prioridades e prazos do time."
        title="Dashboard"
        @toggle-sidebar="toggleSidebar"
      >
        <template #actions>
          <RouterLink to="/tasks">
            <BaseButton>
              Ver tarefas
              <ArrowRight class="h-4 w-4" />
            </BaseButton>
          </RouterLink>
        </template>
      </AppHeader>
    </template>

    <ErrorState
      v-if="isError"
      :description="getErrorMessage(error, 'Não foi possível carregar o dashboard.')"
      show-retry
      title="Falha ao carregar dashboard"
      @retry="refetch"
    />

    <template v-else>
      <section class="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <template v-if="isLoading">
          <div
            v-for="i in 4"
            :key="i"
            class="rounded-card border border-white/70 bg-white p-5 shadow-card"
          >
            <div class="flex justify-between items-start">
              <div class="flex-1 space-y-3">
                <BaseSkeleton height="0.75rem" width="40%" rounded="md" />
                <BaseSkeleton height="2.5rem" width="60%" rounded="lg" />
                <BaseSkeleton height="0.375rem" width="100%" rounded="full" />
              </div>
              <BaseSkeleton height="2.75rem" width="2.75rem" rounded="2xl" />
            </div>
          </div>
        </template>
        <article
          v-else
          v-for="card in summaryCards"
          :key="card.title"
          class="rounded-card border border-white/70 bg-white p-5 shadow-card transition-all hover:shadow-lg"
        >
          <div class="flex items-start justify-between gap-3">
            <div class="flex-1">
              <p class="text-xs font-bold uppercase tracking-wider text-neutral-400">{{ card.title }}</p>
              <p
                class="mt-2 font-display text-4xl font-extrabold tracking-[-0.02em] tabular-nums text-neutral-900"
              >
                {{ card.value }}
              </p>
              <div v-if="overview?.totalCount" class="mt-4 h-1.5 w-full overflow-hidden rounded-full bg-neutral-100">
                <div
                  :class="{
                    'bg-primary': card.tone === 'default',
                    'bg-warning': card.tone === 'warning',
                    'bg-accent': card.tone === 'info',
                    'bg-success': card.tone === 'success',
                  }"
                  :style="{ width: `${(card.value / overview.totalCount) * 100}%` }"
                  class="h-full transition-all duration-700 ease-out"
                />
              </div>
            </div>
            <div
              :class="{
                'bg-primary-soft text-primary': card.tone === 'default',
                'bg-amber-50 text-warning': card.tone === 'warning',
                'bg-cyan-50 text-accent': card.tone === 'info',
                'bg-emerald-50 text-success': card.tone === 'success',
              }"
              class="flex h-11 w-11 shrink-0 items-center justify-center rounded-2xl shadow-sm"
            >
              <component :is="card.icon" class="h-5 w-5" />
            </div>
          </div>
        </article>
      </section>

      <div class="grid gap-6 xl:grid-cols-[1.35fr_0.95fr]">
        <section class="rounded-card border border-white/70 bg-white p-6 shadow-card">
        <SectionHeader
          description="Últimas 5 tarefas atualizadas no sistema."
          title="Atividade recente"
        >
          <template #actions>
            <RouterLink
              class="text-sm font-semibold text-primary transition duration-200 hover:text-primary-dark"
              to="/tasks"
            >
              Abrir lista
            </RouterLink>
          </template>
        </SectionHeader>

        <div v-if="isLoading" class="mt-8 space-y-4">
          <div
            v-for="index in 5"
            :key="index"
            class="flex items-center justify-between gap-4 px-2"
          >
            <div class="flex items-center gap-4 flex-1">
              <BaseSkeleton height="2.5rem" width="2.5rem" rounded="xl" />
              <div class="flex-1 space-y-2">
                <BaseSkeleton height="0.9rem" width="45%" rounded="md" />
                <BaseSkeleton height="0.7rem" width="65%" rounded="sm" />
              </div>
            </div>
            <BaseSkeleton height="1.5rem" width="5rem" rounded="full" />
          </div>
        </div>

        <div v-else-if="recentTasks.length" class="mt-8 space-y-4">
          <article
            v-for="task in recentTasks"
            :key="task.id"
            class="group relative flex items-center justify-between gap-4 rounded-2xl border border-transparent p-2 transition-all hover:bg-neutral-50 hover:px-4"
          >
            <div class="flex items-center gap-4">
              <div
                class="flex h-10 w-10 items-center justify-center rounded-xl bg-neutral-100 text-neutral-400 group-hover:bg-white group-hover:shadow-sm"
              >
                <ListTodo class="h-5 w-5" />
              </div>
              <div>
                <h3 class="text-[0.93rem] font-bold text-neutral-900">{{ task.title }}</h3>
                <p class="text-xs text-neutral-400">
                  {{ task.category?.name ?? 'Sem categoria' }} • Atualizada em
                  {{ formatDate(task.updatedAt) }}
                </p>
              </div>
            </div>
            <TaskStatusBadge :priority="task.priority" :status="task.status" size="sm" />
          </article>
        </div>

        <div
          v-else
          class="mt-6 rounded-2xl border border-dashed border-neutral-200 bg-neutral-100/50 p-8 text-center text-sm text-neutral-400"
        >
          Nenhuma tarefa registrada ainda.
        </div>
        </section>

        <section class="rounded-card border border-white/70 bg-white p-6 shadow-card">
        <SectionHeader
          description="Tarefas com vencimento nos próximos 3 dias."
          title="Prazos próximos"
        />

        <div v-if="isLoading" class="mt-6 space-y-4 overflow-hidden">
          <div
            v-for="index in 3"
            :key="index"
            class="rounded-2xl border border-neutral-100 p-4 space-y-3"
          >
            <div class="flex justify-between items-start gap-3">
              <div class="flex-1 space-y-2">
                <BaseSkeleton height="1.1rem" width="70%" rounded="md" />
                <BaseSkeleton height="0.8rem" width="40%" rounded="sm" />
              </div>
              <BaseSkeleton height="1.75rem" width="2.5rem" rounded="full" />
            </div>
          </div>
        </div>

        <div v-else-if="upcomingTasks.length" class="mt-6 space-y-3">
          <article
            v-for="task in upcomingTasks"
            :key="task.id"
            class="rounded-2xl border border-neutral-100 p-4"
          >
            <div class="flex items-center justify-between gap-3">
              <div>
                <h3 class="text-base font-semibold text-neutral-900">{{ task.title }}</h3>
                <p class="mt-1 text-sm text-neutral-400">Entrega em {{ formatDate(task.dueDate) }}</p>
              </div>
              <TaskStatusBadge :priority="task.priority" :status="task.status" compact />
            </div>
          </article>
        </div>

        <div
          v-else
          class="mt-6 rounded-2xl border border-dashed border-neutral-200 bg-neutral-100/50 p-8 text-center text-sm text-neutral-400"
        >
          Nenhuma tarefa com vencimento imediato.
        </div>
        </section>
      </div>
    </template>
  </PageLayout>
</template>
