<script lang="ts" setup>
import {computed, onBeforeUnmount, ref} from 'vue'
import type {LocationQueryRaw} from 'vue-router'
import {useRoute, useRouter} from 'vue-router'
import {FolderX, Plus, RefreshCcw, Tag} from 'lucide-vue-next'
import AppHeader from '@/components/layout/AppHeader.vue'
import AppSidebar from '@/components/layout/AppSidebar.vue'
import PageLayout from '@/components/layout/PageLayout.vue'
import CategoryForm from '@/components/categories/CategoryForm.vue'
import BaseButton from '@/components/shared/BaseButton.vue'
import BaseInput from '@/components/shared/BaseInput.vue'
import BaseModal from '@/components/shared/BaseModal.vue'
import BasePagination from '@/components/shared/BasePagination.vue'
import EmptyState from '@/components/shared/EmptyState.vue'
import BaseBadge from '@/components/shared/BaseBadge.vue'
import ConfirmDialog from '@/components/shared/ConfirmDialog.vue'
import ErrorState from '@/components/shared/ErrorState.vue'
import BaseSkeleton from '@/components/shared/BaseSkeleton.vue'
import SectionHeader from '@/components/shared/SectionHeader.vue'
import {useCategories} from '@/queries/categoryQueries'
import {readQueryBoolean, readQueryNumber, readQueryString, withQueryValue} from '@/router/query'
import {getErrorMessage} from '@/services/api'
import {useCategoryCrud} from '@/composables/useCategoryCrud'
import type {CategoryFilters} from '@/types'

const PAGE_SIZE = 10

const route = useRoute()
const router = useRouter()
const sidebarOpen = ref(false)

const search = computed(() => readQueryString(route.query, 'search') ?? '')
const showInactive = computed(() => readQueryBoolean(route.query, 'showInactive', false))
const categoriesSectionDescription = computed(() => (
  showInactive.value
    ? 'Acompanhe categorias ativas e arquivadas. As ativas continuam disponíveis nos fluxos de novas tarefas.'
    : 'Somente categorias ativas estão visíveis. Elas alimentam os filtros e o cadastro de novas tarefas.'
))
const categoryFilters = computed<CategoryFilters>(() => ({
  page: readQueryNumber(route.query, 'page', 1),
  pageSize: PAGE_SIZE,
  sortBy: 'name',
  sortDirection: 'Asc',
  ...(search.value ? {search: search.value} : {}),
  ...(showInactive.value ? {} : {isActive: true}),
}))

const {data: categoriesPage, isLoading, isError, error, refetch, isFetching} = useCategories(categoryFilters)

const categoriesList = computed(() => categoriesPage.value?.items ?? [])
const totalItems = computed(() => categoriesPage.value?.totalItems ?? 0)
const totalPages = computed(() => categoriesPage.value?.totalPages ?? 0)
const currentPage = computed(() => categoriesPage.value?.page ?? categoryFilters.value.page ?? 1)

const {
  store,
  editingCategory,
  isSubmitting,
  submitError,
  isConfirmingToggle,
  categoryToToggle,
  toggleCategoryStatus,
  existingNames,
  handleSubmit,
  confirmToggle,
  executeToggle,
  initialValues,
} = useCategoryCrud(() => categoriesList.value)

function buildQuery(nextSearch: string, nextShowInactive: boolean, page = 1) {
  const query: LocationQueryRaw = {}

  withQueryValue(query, 'page', page > 1 ? page : undefined)
  withQueryValue(query, 'search', nextSearch.trim() || undefined)
  withQueryValue(query, 'showInactive', nextShowInactive ? true : undefined)

  return query
}

let searchTimeout: number | undefined

function updateSearch(value: string) {
  clearTimeout(searchTimeout)
  searchTimeout = window.setTimeout(async () => {
    await router.replace({query: buildQuery(value, showInactive.value)})
  }, 350)
}

async function toggleInactiveFilter() {
  await router.replace({query: buildQuery(search.value, !showInactive.value)})
}

async function goToPage(page: number) {
  await router.replace({query: buildQuery(search.value, showInactive.value, page)})
}

onBeforeUnmount(() => {
  clearTimeout(searchTimeout)
})

function formatDate(value?: string) {
  if (!value) {
    return 'Sem registro'
  }

  const date = new Date(value)

  if (Number.isNaN(date.getTime())) {
    return value
  }

  return new Intl.DateTimeFormat('pt-BR', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  }).format(date)
}
</script>

<template>
  <PageLayout lock-viewport>
    <template #sidebar>
      <AppSidebar :model-value="sidebarOpen" @update:model-value="sidebarOpen = $event" />
    </template>

    <template #header>
      <AppHeader
        description="Organize tarefas por dominio, mantenha descricoes claras e controle categorias ativas."
        title="Categorias"
        @toggle-sidebar="sidebarOpen = !sidebarOpen"
      >
        <template #actions>
          <div class="flex items-center gap-3">
            <BaseButton :disabled="isFetching" :loading="isFetching" variant="soft" @click="refetch()">
              <RefreshCcw class="h-4 w-4" />
              Atualizar
            </BaseButton>
            <BaseButton @click="store.openCreate()">
              <Plus class="h-4 w-4" />
              Nova categoria
            </BaseButton>
          </div>
        </template>
      </AppHeader>
    </template>

    <div class="flex min-h-0 flex-1 flex-col gap-6 overflow-hidden">
      <section class="flex shrink-0 flex-col gap-3 rounded-xl border border-neutral-200 bg-white p-3 shadow-sm sm:flex-row sm:items-center">
        <div class="relative flex-1">
          <BaseInput
            :model-value="search"
            placeholder="Buscar por nome ou descrição…"
            @update:model-value="updateSearch"
          />
        </div>
        <BaseButton class="w-full sm:w-auto" variant="secondary" @click="toggleInactiveFilter">
          {{ showInactive ? 'Ocultar inativas' : 'Mostrar inativas' }}
        </BaseButton>
      </section>

      <ErrorState
        v-if="isError"
        :description="getErrorMessage(error, 'Não foi possível carregar as categorias.')"
        compact
        show-retry
        title="Falha ao carregar categorias"
        @retry="refetch"
      />

      <section
        v-else-if="isLoading"
        class="flex min-h-0 flex-1 flex-col overflow-hidden rounded-xl border border-neutral-200 bg-white shadow-sm"
      >
        <SectionHeader
          :description="categoriesSectionDescription"
          class="shrink-0 border-b border-neutral-100 px-6 py-4"
          title="Listagem de Categorias"
        />

        <div class="grid auto-rows-max gap-4 overflow-y-auto p-4 sm:p-6 lg:grid-cols-2 2xl:grid-cols-3">
          <article
            v-for="index in PAGE_SIZE"
            :key="index"
            class="overflow-hidden rounded-[1.65rem] border border-white/70 bg-white p-5 shadow-card"
          >
            <div class="flex items-start justify-between gap-4">
              <BaseSkeleton height="3rem" width="3rem" rounded="2xl" />
              <BaseSkeleton height="1.75rem" width="5.5rem" rounded="full" />
            </div>
            <div class="mt-5 space-y-3">
              <BaseSkeleton height="0.75rem" width="28%" rounded="md" />
              <BaseSkeleton height="1.25rem" width="52%" rounded="md" />
              <BaseSkeleton height="0.9rem" width="100%" rounded="sm" />
              <BaseSkeleton height="0.9rem" width="82%" rounded="sm" />
            </div>
            <div class="mt-5 grid grid-cols-2 gap-3">
              <BaseSkeleton height="4.75rem" width="100%" rounded="2xl" />
              <BaseSkeleton height="4.75rem" width="100%" rounded="2xl" />
            </div>
            <div class="mt-5 flex gap-2 border-t border-neutral-100 pt-4">
              <BaseSkeleton height="2.25rem" width="100%" rounded="lg" />
              <BaseSkeleton height="2.25rem" width="100%" rounded="lg" />
            </div>
          </article>
        </div>
      </section>

      <section
        v-else-if="categoriesList.length"
        class="flex min-h-0 flex-1 flex-col overflow-hidden rounded-xl border border-neutral-200 bg-white shadow-sm"
      >
        <SectionHeader
          :description="categoriesSectionDescription"
          class="shrink-0 border-b border-neutral-100 px-6 py-4"
          title="Listagem de Categorias"
        />

        <div class="hidden min-h-0 flex-1 overflow-auto px-4 py-4 md:block sm:px-6">
          <table class="min-w-full border-separate border-spacing-y-3">
            <thead class="sticky top-0 z-10 bg-white">
              <tr class="text-left text-[0.6875rem] font-bold uppercase tracking-[0.18em] text-neutral-400">
                <th class="px-4 pb-1">Categoria</th>
                <th class="px-4 pb-1">Descrição</th>
                <th class="px-4 pb-1">Atualizada</th>
                <th class="px-4 pb-1">Status</th>
                <th class="px-4 pb-1 text-right">Ações</th>
              </tr>
            </thead>
            <TransitionGroup
              name="list"
              tag="tbody"
            >
              <tr
                v-for="category in categoriesList"
                :key="category.id"
                class="group"
              >
                <td class="border-y border-l border-neutral-200 bg-neutral-50 px-4 py-4 align-top first:rounded-l-[1.25rem] group-hover:border-neutral-300 group-hover:bg-white">
                  <div class="flex items-start gap-3">
                    <div
                      :class="category.isActive
                        ? 'bg-primary-soft text-primary ring-primary/10'
                        : 'bg-neutral-100 text-neutral-500 ring-neutral-200/80'"
                      class="mt-0.5 flex size-10 shrink-0 items-center justify-center rounded-2xl ring-1"
                    >
                      <Tag class="h-4 w-4" />
                    </div>
                    <div class="min-w-0">
                      <p class="text-[0.6875rem] font-bold uppercase tracking-[0.16em] text-neutral-400">
                        {{ category.isActive ? 'Disponível' : 'Arquivada' }}
                      </p>
                      <p class="mt-1 text-[0.975rem] font-semibold tracking-[-0.02em] text-neutral-900">
                        {{ category.name }}
                      </p>
                    </div>
                  </div>
                </td>
                <td class="border-y border-neutral-200 bg-neutral-50 px-4 py-4 align-top group-hover:border-neutral-300 group-hover:bg-white">
                  <p class="max-w-xl text-sm leading-relaxed text-neutral-500">
                    {{ category.description || 'Sem descrição cadastrada para esta categoria.' }}
                  </p>
                </td>
                <td class="border-y border-neutral-200 bg-neutral-50 px-4 py-4 align-top group-hover:border-neutral-300 group-hover:bg-white">
                  <p class="text-sm font-semibold text-neutral-900">
                    {{ formatDate(category.updatedAt) }}
                  </p>
                  <p class="mt-1 text-[0.8125rem] text-neutral-400">
                    Criada em {{ formatDate(category.createdAt) }}
                  </p>
                </td>
                <td class="border-y border-neutral-200 bg-neutral-50 px-4 py-4 align-top group-hover:border-neutral-300 group-hover:bg-white">
                  <BaseBadge
                    :variant="category.isActive ? 'success' : 'neutral'"
                    rounded="full"
                    size="sm"
                  >
                    {{ category.isActive ? 'Ativa' : 'Inativa' }}
                  </BaseBadge>
                </td>
                <td class="border-y border-r border-neutral-200 bg-neutral-50 px-4 py-4 align-top last:rounded-r-[1.25rem] group-hover:border-neutral-300 group-hover:bg-white">
                  <div class="flex justify-end gap-2">
                    <BaseButton
                      :aria-label="`Editar categoria ${category.name}`"
                      size="sm"
                      variant="secondary"
                      @click="store.openEdit(category.id)"
                    >
                      Editar
                    </BaseButton>
                    <BaseButton
                      :aria-label="category.isActive ? `Desativar categoria ${category.name}` : `Ativar categoria ${category.name}`"
                      :class="category.isActive
                        ? 'border-warning-border bg-warning-soft text-warning-text hover:brightness-95'
                        : 'border-success-border bg-success-soft text-success-text hover:brightness-95'"
                      :disabled="toggleCategoryStatus.isPending.value"
                      :loading="toggleCategoryStatus.isPending.value"
                      size="sm"
                      variant="ghost"
                      @click="confirmToggle(category)"
                    >
                      {{ category.isActive ? 'Desativar' : 'Ativar' }}
                    </BaseButton>
                  </div>
                </td>
              </tr>
            </TransitionGroup>
          </table>
        </div>

        <div class="min-h-0 flex-1 overflow-y-auto p-4 md:hidden">
          <TransitionGroup
            class="grid auto-rows-max gap-4"
            name="list"
            tag="div"
          >
            <article
              v-for="category in categoriesList"
              :key="category.id"
              :class="category.isActive ? 'hover:border-primary/20' : 'hover:border-neutral-300'"
              class="group relative overflow-hidden rounded-[1.5rem] border border-neutral-200 bg-white p-4 shadow-[0_1px_3px_rgba(15,23,42,0.06),0_8px_24px_rgba(15,23,42,0.05)] transition-all duration-200 hover:-translate-y-0.5 hover:shadow-[0_10px_28px_rgba(15,23,42,0.12)]"
            >
              <div
                :class="category.isActive ? 'from-primary-light to-accent' : 'from-neutral-300 to-neutral-500'"
                class="absolute inset-x-0 top-0 h-1 bg-gradient-to-r"
              />

              <div class="flex items-start justify-between gap-3">
                <div class="flex items-start gap-3">
                  <div
                    :class="category.isActive
                      ? 'bg-primary-soft text-primary'
                      : 'bg-neutral-100 text-neutral-500'"
                    class="flex size-11 shrink-0 items-center justify-center rounded-2xl"
                  >
                    <Tag class="h-4.5 w-4.5" />
                  </div>
                  <div>
                    <p class="text-[0.6875rem] font-bold uppercase tracking-[0.16em] text-neutral-400">
                      {{ category.isActive ? 'Disponível no fluxo' : 'Categoria arquivada' }}
                    </p>
                    <h3 class="mt-1 text-[1rem] font-semibold tracking-[-0.02em] text-neutral-900">
                      {{ category.name }}
                    </h3>
                  </div>
                </div>
                <BaseBadge
                  :variant="category.isActive ? 'success' : 'neutral'"
                  rounded="full"
                  size="sm"
                >
                  {{ category.isActive ? 'Ativa' : 'Inativa' }}
                </BaseBadge>
              </div>

              <p class="mt-4 rounded-2xl border border-neutral-100 bg-neutral-50 px-3.5 py-3 text-[0.875rem] leading-relaxed text-neutral-500">
                {{ category.description || 'Sem descrição cadastrada para esta categoria.' }}
              </p>

              <div class="mt-4 grid grid-cols-2 gap-3">
                <div class="rounded-2xl border border-neutral-100 bg-white px-3.5 py-3">
                  <p class="text-[0.6875rem] font-bold uppercase tracking-[0.14em] text-neutral-400">
                    Criada em
                  </p>
                  <p class="mt-1 text-[0.875rem] font-semibold text-neutral-900">
                    {{ formatDate(category.createdAt) }}
                  </p>
                </div>
                <div class="rounded-2xl border border-neutral-100 bg-white px-3.5 py-3">
                  <p class="text-[0.6875rem] font-bold uppercase tracking-[0.14em] text-neutral-400">
                    Atualizada
                  </p>
                  <p class="mt-1 text-[0.875rem] font-semibold text-neutral-900">
                    {{ formatDate(category.updatedAt) }}
                  </p>
                </div>
              </div>

              <div class="mt-4 flex gap-2 border-t border-neutral-100 pt-4">
                <BaseButton
                  class="flex-1"
                  size="sm"
                  variant="secondary"
                  @click="store.openEdit(category.id)"
                >
                  Editar
                </BaseButton>
                <BaseButton
                  :class="category.isActive
                    ? 'border-warning-border bg-warning-soft text-warning-text hover:brightness-95'
                    : 'border-success-border bg-success-soft text-success-text hover:brightness-95'"
                  :disabled="toggleCategoryStatus.isPending.value"
                  :loading="toggleCategoryStatus.isPending.value"
                  class="flex-1"
                  size="sm"
                  variant="ghost"
                  @click="confirmToggle(category)"
                >
                  {{ category.isActive ? 'Desativar' : 'Ativar' }}
                </BaseButton>
              </div>
            </article>
          </TransitionGroup>
        </div>

        <div class="shrink-0 border-t border-neutral-100 px-4 py-4 sm:px-6">
          <BasePagination
            :disabled="isFetching"
            :page="currentPage"
            :page-size="categoriesPage?.pageSize ?? PAGE_SIZE"
            :total-items="totalItems"
            :total-pages="totalPages"
            @update:page="goToPage"
          />
        </div>
      </section>

      <section
        v-else
        class="flex flex-1 items-center rounded-xl border border-neutral-200 bg-white shadow-sm"
      >
        <EmptyState
          :icon="FolderX"
          :description="search ? `Nenhum resultado para '${search}'.` : 'Ajuste os filtros ou crie uma nova categoria.'"
          title="Nenhuma categoria encontrada"
        >
          <template v-if="search" #actions>
            <BaseButton variant="secondary" @click="updateSearch('')">
              Limpar busca
            </BaseButton>
          </template>
        </EmptyState>
      </section>
    </div>

    <ConfirmDialog
      v-model="isConfirmingToggle"
      :loading="toggleCategoryStatus.isPending.value"
      :description="
        categoryToToggle?.isActive
          ? `Tem certeza que deseja desativar a categoria '${categoryToToggle?.name}'? Tarefas vinculadas a ela podem não aparecer em alguns filtros.`
          : `Ativar a categoria '${categoryToToggle?.name}' tornará ela disponível para novas tarefas.`
      "
      :title="categoryToToggle?.isActive ? 'Desativar categoria' : 'Ativar categoria'"
      @confirm="executeToggle"
    />

    <BaseModal
      :description="
        store.editingCategoryId
          ? 'Atualize as informações da categoria.'
          : 'Registre uma nova categoria para classificar tarefas.'
      "
      :model-value="store.isFormOpen"
      :title="store.editingCategoryId ? 'Editar categoria' : 'Nova categoria'"
      @update:model-value="(value) => (!value ? store.closeForm() : undefined)"
    >
      <CategoryForm
        :key="store.editingCategoryId ?? 'new-category'"
        :existing-names="existingNames"
        :initial-values="initialValues(editingCategory)"
        :loading="isSubmitting"
        :server-error="submitError"
        @cancel="store.closeForm()"
        @submit="handleSubmit"
      />
    </BaseModal>
  </PageLayout>
</template>
