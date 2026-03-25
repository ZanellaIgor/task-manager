<script lang="ts" setup>
import {computed, ref, watch} from 'vue'
import type {LocationQueryRaw} from 'vue-router'
import {useRoute, useRouter} from 'vue-router'
import {useForm} from 'vee-validate'
import {toTypedSchema} from '@vee-validate/zod'
import {FolderX, Plus, RefreshCcw} from 'lucide-vue-next'
import AppHeader from '@/components/layout/AppHeader.vue'
import AppSidebar from '@/components/layout/AppSidebar.vue'
import PageLayout from '@/components/layout/PageLayout.vue'
import BaseButton from '@/components/shared/BaseButton.vue'
import BaseInput from '@/components/shared/BaseInput.vue'
import BaseModal from '@/components/shared/BaseModal.vue'
import BasePagination from '@/components/shared/BasePagination.vue'
import EmptyState from '@/components/shared/EmptyState.vue'
import BaseBadge from '@/components/shared/BaseBadge.vue'
import ConfirmDialog from '@/components/shared/ConfirmDialog.vue'
import ErrorState from '@/components/shared/ErrorState.vue'
import BaseSkeleton from '@/components/shared/BaseSkeleton.vue'
import {useToast} from '@/composables/useToast'
import {useCategories, useCreateCategory, useToggleCategoryStatus, useUpdateCategory} from '@/queries/categoryQueries'
import {readQueryBoolean, readQueryNumber, readQueryString, withQueryValue} from '@/router/query'
import {type CategoryFormData, categorySchema} from '@/schemas/categorySchema'
import {getErrorMessage} from '@/services/api'
import {useCategoryStore} from '@/stores/categories'
import SectionHeader from '@/components/shared/SectionHeader.vue'
import type {Category, CategoryFilters} from '@/types'

const PAGE_SIZE = 10

const route = useRoute()
const router = useRouter()
const store = useCategoryStore()
const toast = useToast()
const sidebarOpen = ref(false)

const search = computed(() => readQueryString(route.query, 'search') ?? '')
const showInactive = computed(() => readQueryBoolean(route.query, 'showInactive', true))
const categoryFilters = computed<CategoryFilters>(() => ({
  page: readQueryNumber(route.query, 'page', 1),
  pageSize: PAGE_SIZE,
  sortBy: 'name',
  sortDirection: 'Asc',
  ...(search.value ? {search: search.value} : {}),
  ...(showInactive.value ? {} : {isActive: true}),
}))

const {data: categoriesPage, isLoading, isError, error, refetch, isFetching} = useCategories(categoryFilters)
const createCategory = useCreateCategory()
const updateCategory = useUpdateCategory()
const toggleCategoryStatus = useToggleCategoryStatus()

const categoriesList = computed(() => categoriesPage.value?.items ?? [])
const totalItems = computed(() => categoriesPage.value?.totalItems ?? 0)
const totalPages = computed(() => categoriesPage.value?.totalPages ?? 0)
const currentPage = computed(() => categoriesPage.value?.page ?? categoryFilters.value.page ?? 1)
const editingCategory = computed(() => categoriesList.value.find((item) => item.id === store.editingCategoryId) ?? null)

const isSubmitting = computed(
  () => createCategory.isPending.value || updateCategory.isPending.value || toggleCategoryStatus.isPending.value,
)

const serverError = ref<string | null>(null)
const isConfirmingToggle = ref(false)
const categoryToToggle = ref<Category | null>(null)

const validationSchema = computed(() => {
  const duplicateNames = new Set(
    categoriesList.value
      .filter((category) => category.id !== editingCategory.value?.id)
      .map((category) => category.name.trim().toLowerCase()),
  )

  return toTypedSchema(
    categorySchema.superRefine((value, ctx) => {
      if (duplicateNames.has(value.name.trim().toLowerCase())) {
        ctx.addIssue({
          code: 'custom',
          path: ['name'],
          message: 'Ja existe uma categoria com esse nome.',
        })
      }
    }),
  )
})

const {handleSubmit, defineField, errors, resetForm, setValues} = useForm<CategoryFormData>({
  validationSchema,
  initialValues: {
    name: '',
    description: '',
  },
})

const [name, nameAttrs] = defineField('name')
const [description, descriptionAttrs] = defineField('description')

watch(editingCategory, (category) => {
  if (store.isFormOpen && category) {
    setValues({
      name: category.name,
      description: category.description ?? '',
    })
  }
})

watch(
  () => store.isFormOpen,
  (isOpen) => {
    if (isOpen) {
      serverError.value = null

      if (!store.editingCategoryId) {
        resetForm({
          values: {
            name: '',
            description: '',
          },
        })
      }
    }
  },
)

function buildQuery(nextSearch: string, nextShowInactive: boolean, page = 1) {
  const query: LocationQueryRaw = {}

  withQueryValue(query, 'page', page > 1 ? page : undefined)
  withQueryValue(query, 'search', nextSearch.trim() || undefined)
  withQueryValue(query, 'showInactive', nextShowInactive ? undefined : false)

  return query
}

async function updateSearch(value: string) {
  await router.replace({query: buildQuery(value, showInactive.value)})
}

async function toggleInactiveFilter() {
  await router.replace({query: buildQuery(search.value, !showInactive.value)})
}

async function goToPage(page: number) {
  await router.replace({query: buildQuery(search.value, showInactive.value, page)})
}

const onSubmit = handleSubmit(async (values) => {
  serverError.value = null

  try {
    if (store.editingCategoryId) {
      await updateCategory.mutateAsync({
        id: store.editingCategoryId,
        data: values,
      })
      toast.success('Categoria atualizada com sucesso.')
    } else {
      await createCategory.mutateAsync(values)
      toast.success('Categoria criada com sucesso.')
    }

    store.closeForm()
    resetForm({
      values: {
        name: '',
        description: '',
      },
    })
  } catch (cause) {
    const message = getErrorMessage(cause, 'Não foi possível salvar a categoria.')
    serverError.value = message
    toast.error(message)
  }
})

function confirmToggle(category: Category) {
  categoryToToggle.value = category
  isConfirmingToggle.value = true
}

async function executeToggle() {
  if (!categoryToToggle.value) return

  const category = categoryToToggle.value
  try {
    await toggleCategoryStatus.mutateAsync({
      id: category.id,
      isActive: !category.isActive,
    })
    toast.success(
      category.isActive
        ? `Categoria "${category.name}" desativada.`
        : `Categoria "${category.name}" ativada.`,
    )
  } catch (cause) {
    toast.error(getErrorMessage(cause, 'Não foi possível alterar o status da categoria.'))
  } finally {
    isConfirmingToggle.value = false
    categoryToToggle.value = null
  }
}
</script>

<template>
  <PageLayout>
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
            <BaseButton :disabled="isFetching" variant="secondary" @click="refetch()">
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

    <section class="flex items-center gap-3 rounded-xl border border-neutral-200 bg-white p-3 shadow-sm">
      <div class="relative flex-1">
        <BaseInput
          :model-value="search"
          placeholder="Buscar por nome ou descrição…"
          @update:model-value="updateSearch"
        />
      </div>
      <BaseButton variant="secondary" @click="toggleInactiveFilter">
        {{ showInactive ? 'Ocultar inativas' : 'Mostrar inativas' }}
      </BaseButton>
    </section>

    <section class="overflow-hidden rounded-xl border border-neutral-200 bg-white shadow-sm">
      <SectionHeader
        :description="`${totalItems} ${totalItems === 1 ? 'item catalogado' : 'itens catalogados'}. Categorias ativas alimentam os filtros das tarefas.`"
        class="border-b border-neutral-100 px-6 py-4"
        title="Listagem de Categorias"
      />

      <ErrorState
        v-if="isError"
        :description="getErrorMessage(error, 'Não foi possível carregar as categorias.')"
        compact
        show-retry
        title="Falha ao carregar categorias"
        @retry="refetch"
      />

      <div v-else-if="isLoading" class="space-y-5 px-6 py-5">
        <div v-for="index in 5" :key="index" class="flex flex-col gap-3">
          <div class="flex items-start justify-between">
            <div class="flex-1 space-y-2">
              <BaseSkeleton height="1.25rem" width="30%" rounded="md" />
              <BaseSkeleton height="0.875rem" width="60%" rounded="sm" />
            </div>
            <BaseSkeleton height="1.5rem" width="4rem" rounded="full" />
          </div>
          <div class="flex gap-2">
            <BaseSkeleton height="2.25rem" width="5rem" rounded="lg" />
            <BaseSkeleton height="2.25rem" width="5rem" rounded="lg" />
          </div>
          <div v-if="index < 5" class="border-b border-neutral-50 pt-2" />
        </div>
      </div>

      <div v-else-if="categoriesList.length">
        <!-- Desktop Table View -->
        <div class="hidden overflow-x-auto md:block">
          <table class="min-w-full">
          <thead>
            <tr class="bg-neutral-50 text-left text-[0.72rem] font-semibold uppercase tracking-[0.12em] text-neutral-400">
              <th class="px-6 py-2.5">Categoria</th>
              <th class="px-6 py-2.5">Descrição</th>
              <th class="px-6 py-2.5">Status</th>
              <th class="px-6 py-2.5 text-right">Ações</th>
            </tr>
          </thead>
          <TransitionGroup
            name="list"
            tag="tbody"
          >
            <tr
              v-for="category in categoriesList"
              :key="category.id"
              class="border-b border-neutral-100 last:border-0 transition-colors duration-100 hover:bg-neutral-50"
            >
              <td class="px-6 py-3">
                <p class="text-[0.9rem] font-semibold text-neutral-900">{{ category.name }}</p>
              </td>
              <td class="px-6 py-3 text-[0.875rem] text-neutral-500">
                {{ category.description || '—' }}
              </td>
              <td class="px-6 py-3">
                <BaseBadge
                  :variant="category.isActive ? 'success' : 'neutral'"
                  rounded="full"
                  size="sm"
                >
                  {{ category.isActive ? 'Ativa' : 'Inativa' }}
                </BaseBadge>
              </td>
              <td class="px-6 py-3">
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
                      ? 'border-amber-200 bg-amber-50 text-amber-700 hover:bg-amber-100'
                      : 'border-emerald-200 bg-emerald-50 text-emerald-700 hover:bg-emerald-100'"
                    :disabled="isSubmitting"
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

      <!-- Mobile Cards View -->
      <TransitionGroup
        class="grid gap-4 p-4 md:hidden"
        name="list"
        tag="div"
      >
        <article
          v-for="category in categoriesList"
          :key="category.id"
          class="flex flex-col gap-3 rounded-2xl border border-neutral-100 bg-neutral-50/50 p-4 transition-all hover:bg-white hover:shadow-sm"
        >
          <div class="flex items-start justify-between gap-3">
            <div>
              <h3 class="font-bold text-neutral-900">{{ category.name }}</h3>
              <p class="mt-1 text-[0.82rem] leading-relaxed text-neutral-500">
                {{ category.description || 'Sem descrição' }}
              </p>
            </div>
            <BaseBadge
              :variant="category.isActive ? 'success' : 'neutral'"
              rounded="full"
              size="sm"
            >
              {{ category.isActive ? 'Ativa' : 'Inativa' }}
            </BaseBadge>
          </div>
          <div class="flex gap-2 border-t border-neutral-100 pt-3">
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
                ? 'border-amber-200 bg-amber-50 text-amber-700'
                : 'border-emerald-200 bg-emerald-50 text-emerald-700'"
              :disabled="isSubmitting"
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

      <EmptyState
        v-else
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

    <BasePagination
      v-if="!isError && !isLoading && categoriesList.length"
      :disabled="isFetching"
      :page="currentPage"
      :page-size="categoriesPage?.pageSize ?? PAGE_SIZE"
      :total-items="totalItems"
      :total-pages="totalPages"
      @update:page="goToPage"
    />

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
          ? 'Atualize as informacoes da categoria.'
          : 'Registre uma nova categoria para classificar tarefas.'
      "
      :model-value="store.isFormOpen"
      :title="store.editingCategoryId ? 'Editar categoria' : 'Nova categoria'"
      @update:model-value="(value) => (!value ? store.closeForm() : undefined)"
    >
      <form class="space-y-5" @submit.prevent="onSubmit">
        <BaseInput
          v-model="name"
          :error="errors.name"
          label="Nome"
          placeholder="Ex.: Operacoes"
          v-bind="nameAttrs"
        />

        <BaseInput
          v-model="description"
          :error="errors.description"
          label="Descricao"
          placeholder="Resumo curto para orientar o uso da categoria"
          v-bind="descriptionAttrs"
        />

        <p v-if="serverError" class="text-sm font-medium text-danger">
          {{ serverError }}
        </p>

        <div class="flex justify-end gap-3">
          <BaseButton type="button" variant="ghost" @click="store.closeForm()">Cancelar</BaseButton>
          <BaseButton :disabled="isSubmitting" type="submit">
            {{ isSubmitting ? 'Salvando...' : 'Salvar categoria' }}
          </BaseButton>
        </div>
      </form>
    </BaseModal>
  </PageLayout>
</template>
