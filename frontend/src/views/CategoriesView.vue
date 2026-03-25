<script lang="ts" setup>
import {computed, ref, watch} from 'vue'
import {useForm} from 'vee-validate'
import {toTypedSchema} from '@vee-validate/zod'
import {Plus, RefreshCcw} from 'lucide-vue-next'
import AppHeader from '@/components/layout/AppHeader.vue'
import AppSidebar from '@/components/layout/AppSidebar.vue'
import BaseButton from '@/components/shared/BaseButton.vue'
import BaseInput from '@/components/shared/BaseInput.vue'
import BaseModal from '@/components/shared/BaseModal.vue'
import {useToast} from '@/composables/useToast'
import {useCategories, useCreateCategory, useToggleCategoryStatus, useUpdateCategory,} from '@/queries/categoryQueries'
import {type CategoryFormData, categorySchema} from '@/schemas/categorySchema'
import {useCategoryStore} from '@/stores/categories'
import type {Category} from '@/types'

const store = useCategoryStore()
const toast = useToast()
const sidebarOpen = ref(false)
const {data: categories, isLoading, isError, error, refetch, isFetching} = useCategories()
const createCategory = useCreateCategory()
const updateCategory = useUpdateCategory()
const toggleCategoryStatus = useToggleCategoryStatus()

const categoriesList = computed(() => categories.value ?? [])
const visibleCategories = computed(() => {
  const search = store.search.trim().toLowerCase()

  return categoriesList.value.filter((category) => {
    if (!store.showInactive && !category.isActive) {
      return false
    }

    if (!search) {
      return true
    }

    return [category.name, category.description ?? ''].join(' ').toLowerCase().includes(search)
  })
})

const editingCategory = computed(() =>
    categoriesList.value.find((item) => item.id === store.editingCategoryId) ?? null,
)

const isSubmitting = computed(
    () => createCategory.isPending.value || updateCategory.isPending.value || toggleCategoryStatus.isPending.value,
)

const serverError = ref<string | null>(null)

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
    const message = cause instanceof Error ? cause.message : 'Nao foi possivel salvar a categoria.'
    serverError.value = message
    toast.error(message)
  }
})

async function handleToggle(category: Category) {
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
  } catch {
    toast.error('Nao foi possivel alterar o status da categoria.')
  }
}
</script>

<template>
  <div class="flex min-h-screen bg-[#eef0f4] text-neutral-900">
    <AppSidebar :model-value="sidebarOpen" @update:model-value="sidebarOpen = $event"/>

    <div class="flex min-w-0 flex-1 flex-col lg:pl-[18.5rem]">
      <AppHeader
          description="Organize tarefas por dominio, mantenha descricoes claras e controle categorias ativas."
          title="Categorias"
          @toggle-sidebar="sidebarOpen = !sidebarOpen"
      >
        <template #actions>
          <div class="flex items-center gap-3">
            <BaseButton :disabled="isFetching" variant="secondary" @click="refetch()">
              <RefreshCcw class="h-4 w-4"/>
              Atualizar
            </BaseButton>
            <BaseButton @click="store.openCreate()">
              <Plus class="h-4 w-4"/>
              Nova categoria
            </BaseButton>
          </div>
        </template>
      </AppHeader>

      <main class="flex-1 space-y-4 px-4 py-4 sm:px-6 lg:px-8">
        <!-- Search + filter bar -->
        <section class="flex items-center gap-3 p-3 border border-neutral-200 rounded-xl bg-white shadow-sm">
          <div class="relative flex-1">
            <BaseInput
                :model-value="store.search"
                placeholder="Buscar por nome ou descrição…"
                @update:model-value="store.setSearch"
            />
          </div>
          <BaseButton variant="secondary" @click="store.toggleInactive()">
            {{ store.showInactive ? 'Ocultar inativas' : 'Mostrar inativas' }}
          </BaseButton>
        </section>

        <!-- Table card -->
        <section class="overflow-hidden rounded-xl border border-neutral-200 bg-white shadow-sm">
          <!-- Card header -->
          <div class="px-6 py-4 border-b border-neutral-100 flex items-center justify-between">
            <div>
              <h2 class="text-base font-semibold text-neutral-900">Categorias</h2>
              <p class="text-[0.8rem] text-neutral-400 mt-0.5">
                Categorias ativas alimentam os filtros das tarefas.
              </p>
            </div>
            <span class="text-[0.78rem] font-medium text-neutral-400">
              {{ visibleCategories.length }} {{ visibleCategories.length === 1 ? 'item' : 'itens' }}
            </span>
          </div>

          <div v-if="isError" class="px-6 py-8 text-sm text-danger">
            {{ error instanceof Error ? error.message : 'Nao foi possivel carregar as categorias.' }}
          </div>

          <div v-else-if="isLoading" class="space-y-2 px-6 py-4">
            <div v-for="index in 5" :key="index" class="h-12 animate-pulse rounded-lg bg-neutral-100"/>
          </div>

          <div v-else-if="visibleCategories.length" class="overflow-x-auto">
            <table class="min-w-full">
              <thead>
              <tr class="border-b border-neutral-100 text-left text-[0.72rem] font-semibold uppercase tracking-[0.12em] text-neutral-400 bg-neutral-50">
                <th class="px-6 py-2.5">Categoria</th>
                <th class="px-6 py-2.5">Descrição</th>
                <th class="px-6 py-2.5">Status</th>
                <th class="px-6 py-2.5 text-right">Ações</th>
              </tr>
              </thead>
              <tbody>
              <tr
                  v-for="category in visibleCategories"
                  :key="category.id"
                  class="border-b border-neutral-100 last:border-0 hover:bg-neutral-50 transition-colors duration-100"
              >
                <td class="px-6 py-3">
                  <p class="font-semibold text-[0.9rem] text-neutral-900">{{ category.name }}</p>
                </td>
                <td class="px-6 py-3 text-[0.875rem] text-neutral-500">
                  {{ category.description || '—' }}
                </td>
                <td class="px-6 py-3">
                  <span
                      :class="category.isActive
                        ? 'bg-emerald-50 text-emerald-700 border border-emerald-200'
                        : 'bg-neutral-100 text-neutral-500 border border-neutral-200'"
                      class="inline-flex items-center px-2.5 py-0.5 rounded-md text-[0.75rem] font-semibold"
                  >
                    {{ category.isActive ? 'Ativa' : 'Inativa' }}
                  </span>
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
                          ? 'text-amber-700 bg-amber-50 border-amber-200 hover:bg-amber-100'
                          : 'text-emerald-700 bg-emerald-50 border-emerald-200 hover:bg-emerald-100'"
                        :disabled="isSubmitting"
                        size="sm"
                        variant="ghost"
                        @click="handleToggle(category)"
                    >
                      {{ category.isActive ? 'Desativar' : 'Ativar' }}
                    </BaseButton>
                  </div>
                </td>
              </tr>
              </tbody>
            </table>
          </div>

          <div v-else class="px-6 py-12 text-center">
            <p class="font-semibold text-neutral-900">Nenhuma categoria encontrada</p>
            <p class="mt-1 text-sm text-neutral-400">Ajuste a busca ou crie uma nova categoria.</p>
          </div>
        </section>
      </main>

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
    </div>
  </div>
</template>
