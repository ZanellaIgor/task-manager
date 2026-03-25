<script lang="ts" setup>
import {computed, ref} from 'vue'
import {RouterLink} from 'vue-router'
import {ArrowRight, CalendarClock, CircleCheckBig, Clock3, ListTodo} from 'lucide-vue-next'
import AppHeader from '@/components/layout/AppHeader.vue'
import AppSidebar from '@/components/layout/AppSidebar.vue'
import TaskStatusBadge from '@/components/tasks/TaskStatusBadge.vue'
import BaseButton from '@/components/shared/BaseButton.vue'
import {useTasks} from '@/queries/taskQueries'

const {data: tasks, isLoading, isError, error, refetch} = useTasks(computed(() => ({})))
const sidebarOpen = ref(false)

const allTasks = computed(() => tasks.value ?? [])
const summaryCards = computed(() => {
  const items = allTasks.value

  return [
    {
      title: 'Total',
      value: items.length,
      tone: 'default' as const,
      icon: ListTodo,
    },
    {
      title: 'Pendentes',
      value: items.filter((task) => task.status === 'Pending').length,
      tone: 'warning' as const,
      icon: Clock3,
    },
    {
      title: 'Em andamento',
      value: items.filter((task) => task.status === 'InProgress').length,
      tone: 'info' as const,
      icon: CalendarClock,
    },
    {
      title: 'Concluídas',
      value: items.filter((task) => task.status === 'Completed').length,
      tone: 'success' as const,
      icon: CircleCheckBig,
    },
  ]
})

const recentTasks = computed(() =>
    [...allTasks.value]
        .sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime())
        .slice(0, 5),
)

const upcomingTasks = computed(() => {
  const today = new Date()
  const limit = new Date()
  limit.setDate(today.getDate() + 3)

  return allTasks.value
      .filter((task) => {
        if (!task.dueDate || task.status === 'Completed' || task.status === 'Cancelled') {
          return false
        }

        const dueDate = new Date(task.dueDate)
        return dueDate >= today && dueDate <= limit
      })
      .sort((a, b) => new Date(a.dueDate ?? '').getTime() - new Date(b.dueDate ?? '').getTime())
})

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
  <div class="flex min-h-screen bg-bg-app text-neutral-900">
    <AppSidebar :model-value="sidebarOpen" @update:model-value="sidebarOpen = $event"/>

    <div class="flex min-w-0 flex-1 flex-col lg:pl-[18.5rem]">
      <AppHeader
          description="Visão geral das tarefas, prioridades e prazos do time."
          title="Dashboard"
          @toggle-sidebar="sidebarOpen = !sidebarOpen"
      >
        <template #actions>
          <RouterLink to="/tasks">
            <BaseButton>
              Ver tarefas
              <ArrowRight class="h-4 w-4"/>
            </BaseButton>
          </RouterLink>
        </template>
      </AppHeader>

      <main class="flex-1 space-y-8 px-4 py-6 sm:px-6 lg:px-8">
        <section class="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
          <article
              v-for="card in summaryCards"
              :key="card.title"
              class="rounded-card border border-white/70 bg-white p-5 shadow-card"
          >
            <div class="flex items-start justify-between gap-3">
              <div>
                <p class="text-sm font-medium text-neutral-400">{{ card.title }}</p>
                <p class="mt-3 font-display text-4xl font-extrabold text-neutral-900"
                   style="font-variant-numeric: tabular-nums; letter-spacing: -0.02em">
                  {{ card.value }}
                </p>
              </div>
              <div
                  :class="{
                  'bg-primary-soft text-primary': card.tone === 'default',
                  'bg-amber-50 text-warning': card.tone === 'warning',
                  'bg-cyan-50 text-accent': card.tone === 'info',
                  'bg-emerald-50 text-success': card.tone === 'success',
                }"
                  class="flex h-11 w-11 items-center justify-center rounded-full"
              >
                <component :is="card.icon" class="h-5 w-5"/>
              </div>
            </div>
          </article>
        </section>

        <section
            v-if="isError"
            class="rounded-card border border-danger/20 bg-white p-6 shadow-card"
        >
          <p class="text-sm font-medium text-danger">
            {{ error instanceof Error ? error.message : 'Nao foi possivel carregar o dashboard.' }}
          </p>
          <BaseButton class="mt-4" variant="secondary" @click="refetch()">Tentar novamente</BaseButton>
        </section>

        <div v-else class="grid gap-6 xl:grid-cols-[1.35fr_0.95fr]">
          <section class="rounded-card border border-white/70 bg-white p-6 shadow-card">
            <div class="flex items-center justify-between gap-4">
              <div>
                <h2 class="font-display text-2xl font-bold text-neutral-900">Atividade recente</h2>
                <p class="mt-1 text-sm text-neutral-400">Ultimas 5 tarefas atualizadas no sistema.</p>
              </div>
              <RouterLink
                  class="text-sm font-semibold text-primary transition duration-200 hover:text-primary-dark"
                  to="/tasks"
              >
                Abrir lista
              </RouterLink>
            </div>

            <div
                v-if="isLoading"
                class="mt-6 space-y-3"
            >
              <div
                  v-for="index in 5"
                  :key="index"
                  class="h-20 animate-pulse rounded-2xl bg-neutral-100"
              />
            </div>

            <div v-else-if="recentTasks.length" class="mt-6 space-y-3">
              <article
                  v-for="task in recentTasks"
                  :key="task.id"
                  class="rounded-2xl border border-neutral-100 bg-neutral-100/65 p-4"
              >
                <div class="flex flex-wrap items-center justify-between gap-3">
                  <div>
                    <h3 class="text-base font-semibold text-neutral-900">{{ task.title }}</h3>
                    <p class="mt-1 text-sm text-neutral-400">
                      {{ task.category?.name ?? 'Sem categoria' }} • Atualizada em
                      {{ formatDate(task.updatedAt) }}
                    </p>
                  </div>
                  <TaskStatusBadge :priority="task.priority" :status="task.status"/>
                </div>
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
            <div class="flex items-center justify-between gap-4">
              <div>
                <h2 class="font-display text-2xl font-bold text-neutral-900">Prazos proximos</h2>
                <p class="mt-1 text-sm text-neutral-400">Tarefas com vencimento nos proximos 3 dias.</p>
              </div>
            </div>

            <div
                v-if="isLoading"
                class="mt-6 space-y-3"
            >
              <div
                  v-for="index in 3"
                  :key="index"
                  class="h-24 animate-pulse rounded-2xl bg-neutral-100"
              />
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
                  <TaskStatusBadge :priority="task.priority" :status="task.status" compact/>
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
      </main>
    </div>
  </div>
</template>
