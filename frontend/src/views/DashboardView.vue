<script lang="ts" setup>
import {computed, ref} from 'vue'
import {useRouter} from 'vue-router'
import {ArrowRight, CalendarClock, CalendarDays, CircleCheckBig, Clock3, ListTodo} from 'lucide-vue-next'
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

const router = useRouter()
const {data: overview, isLoading, isError, error, refetch} = useTaskOverview()
const sidebarOpen = ref(false)
const toggleSidebar = () => { sidebarOpen.value = !sidebarOpen.value }

const completionPercent = computed(() => {
  const total = overview.value?.totalCount ?? 0
  if (total === 0) return 0
  return Math.round(((overview.value?.completedCount ?? 0) / total) * 100)
})

const summaryCards = computed(() => {
  const data = overview.value
  const total = data?.totalCount ?? 0

  return [
    {
      title: 'Total',
      value: total,
      tone: 'default' as const,
      icon: ListTodo,
      barPercent: total > 0 ? completionPercent.value : 0,
      barLabel: `${completionPercent.value}% concluídas`,
    },
    {
      title: 'Pendentes',
      value: data?.pendingCount ?? 0,
      tone: 'warning' as const,
      icon: Clock3,
      barPercent: total > 0 ? Math.round(((data?.pendingCount ?? 0) / total) * 100) : 0,
    },
    {
      title: 'Em andamento',
      value: data?.inProgressCount ?? 0,
      tone: 'info' as const,
      icon: CalendarClock,
      barPercent: total > 0 ? Math.round(((data?.inProgressCount ?? 0) / total) * 100) : 0,
    },
    {
      title: 'Concluídas',
      value: data?.completedCount ?? 0,
      tone: 'success' as const,
      icon: CircleCheckBig,
      barPercent: total > 0 ? Math.round(((data?.completedCount ?? 0) / total) * 100) : 0,
    },
  ]
})

const recentTasks = computed(() => overview.value?.recentTasks ?? [])
const upcomingTasks = computed(() => overview.value?.upcomingTasks ?? [])

function formatDate(value?: string) {
  if (!value) return 'Sem prazo'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value
  return new Intl.DateTimeFormat('pt-BR', {day: '2-digit', month: 'short', year: 'numeric'}).format(date)
}

function formatShortDate(value?: string) {
  if (!value) return 'Sem prazo'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value
  return new Intl.DateTimeFormat('pt-BR', {day: '2-digit', month: 'short'}).format(date)
}

function daysUntil(value?: string): number | null {
  if (!value) return null
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return null
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  date.setHours(0, 0, 0, 0)
  return Math.ceil((date.getTime() - today.getTime()) / (1000 * 60 * 60 * 24))
}

function urgencyLabel(days: number | null): string {
  if (days === null) return ''
  if (days < 0) return 'Atrasada'
  if (days === 0) return 'Hoje'
  if (days === 1) return 'Amanhã'
  return `Em ${days} dias`
}

function urgencyClass(days: number | null): string {
  if (days === null) return 'text-neutral-400'
  if (days < 0) return 'text-danger font-semibold'
  if (days === 0) return 'text-danger font-medium'
  if (days === 1) return 'text-warning-text font-medium'
  return 'text-neutral-500'
}

function navigateToTasks() {
  router.push('/tasks')
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
          <BaseButton @click="navigateToTasks">
            Ver tarefas
            <ArrowRight class="h-4 w-4" />
          </BaseButton>
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
              <div v-if="overview?.totalCount" class="mt-4">
                <div class="h-1.5 w-full overflow-hidden rounded-full bg-neutral-100">
                  <div
                    :class="{
                      'bg-primary': card.tone === 'default',
                      'bg-warning': card.tone === 'warning',
                      'bg-accent': card.tone === 'info',
                      'bg-success': card.tone === 'success',
                    }"
                    :style="{ width: `${card.barPercent}%` }"
                    class="h-full transition-all duration-700 ease-out"
                  />
                </div>
                <p v-if="card.barLabel" class="mt-1.5 text-xs text-neutral-400">
                  {{ card.barLabel }}
                </p>
              </div>
            </div>
            <div
              :class="{
                'bg-primary-soft text-primary': card.tone === 'default',
                'bg-warning-soft text-warning': card.tone === 'warning',
                'bg-info-soft text-accent': card.tone === 'info',
                'bg-success-soft text-success': card.tone === 'success',
              }"
              class="flex h-11 w-11 shrink-0 items-center justify-center rounded-2xl shadow-sm"
            >
              <component :is="card.icon" class="h-5 w-5" aria-hidden="true" />
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
              <button
                class="text-sm font-semibold text-primary transition duration-200 hover:text-primary-dark cursor-pointer"
                type="button"
                @click="navigateToTasks"
              >
                Abrir lista
              </button>
            </template>
          </SectionHeader>

          <div v-if="isLoading" class="mt-6 divide-y divide-neutral-100">
            <div
              v-for="index in 5"
              :key="index"
              class="flex items-center justify-between gap-4 py-3"
            >
              <div class="flex-1 space-y-2">
                <BaseSkeleton height="0.9rem" width="55%" rounded="md" />
                <BaseSkeleton height="0.7rem" width="40%" rounded="sm" />
              </div>
              <BaseSkeleton height="1.5rem" width="5rem" rounded="full" />
            </div>
          </div>

          <div v-else-if="recentTasks.length" class="mt-6 divide-y divide-neutral-100">
            <button
              v-for="task in recentTasks"
              :key="task.id"
              class="flex w-full items-center justify-between gap-4 py-3 text-left transition-colors hover:bg-neutral-50 -mx-2 px-2 rounded-xl cursor-pointer"
              type="button"
              @click="router.push({name: 'tasks', query: {search: task.title}})"
            >
              <div class="min-w-0 flex-1">
                <h3 class="text-[0.9375rem] font-semibold text-neutral-900 truncate">{{ task.title }}</h3>
                <p class="mt-0.5 text-xs text-neutral-400 truncate">
                  {{ task.category?.name ?? 'Sem categoria' }} · {{ formatDate(task.updatedAt) }}
                </p>
              </div>
              <TaskStatusBadge :priority="task.priority" :status="task.status" size="sm" />
            </button>
          </div>

          <div
            v-else
            class="mt-6 rounded-2xl border border-dashed border-neutral-200 bg-neutral-50/50 p-8 text-center text-sm text-neutral-400"
          >
            Nenhuma atividade recente.
          </div>
        </section>

        <section class="rounded-card border border-white/70 bg-white p-6 shadow-card">
          <SectionHeader
            description="Tarefas com vencimento nos próximos 3 dias."
            title="Prazos próximos"
          />

          <div v-if="isLoading" class="mt-6 space-y-3">
            <div
              v-for="index in 3"
              :key="index"
              class="rounded-2xl border border-neutral-100 p-4 space-y-3"
            >
              <div class="flex justify-between items-start gap-3">
                <div class="flex-1 space-y-2">
                  <BaseSkeleton height="1rem" width="70%" rounded="md" />
                  <BaseSkeleton height="0.75rem" width="40%" rounded="sm" />
                </div>
                <BaseSkeleton height="1.5rem" width="4rem" rounded="full" />
              </div>
            </div>
          </div>

          <div v-else-if="upcomingTasks.length" class="mt-6 space-y-3">
            <article
              v-for="task in upcomingTasks"
              :key="task.id"
              class="rounded-2xl border border-neutral-100 p-4 transition-colors hover:border-neutral-200"
            >
              <div class="flex items-start justify-between gap-3">
                <div class="min-w-0 flex-1">
                  <h3 class="font-semibold text-neutral-900 truncate">{{ task.title }}</h3>
                  <div class="mt-1.5 flex items-center gap-2 text-xs">
                    <span class="inline-flex items-center gap-1 text-neutral-400">
                      <CalendarDays :size="12" aria-hidden="true" />
                      {{ formatShortDate(task.dueDate) }}
                    </span>
                    <span :class="urgencyClass(daysUntil(task.dueDate))">
                      {{ urgencyLabel(daysUntil(task.dueDate)) }}
                    </span>
                  </div>
                </div>
                <TaskStatusBadge :priority="task.priority" :status="task.status" size="sm" />
              </div>
            </article>
          </div>

          <div
            v-else
            class="mt-6 rounded-2xl border border-dashed border-neutral-200 bg-neutral-50/50 p-8 text-center text-sm text-neutral-400"
          >
            Nenhuma tarefa com vencimento imediato.
          </div>
        </section>
      </div>
    </template>
  </PageLayout>
</template>
