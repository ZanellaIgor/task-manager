<script lang="ts" setup>
import {computed, watch} from 'vue'
import {useForm} from 'vee-validate'
import {toTypedSchema} from '@vee-validate/zod'
import BaseButton from '@/components/shared/BaseButton.vue'
import BaseInput from '@/components/shared/BaseInput.vue'
import BaseSelect from '@/components/shared/BaseSelect.vue'
import {useCategoryOptions} from '@/queries/categoryQueries'
import {type TaskFormData, taskSchema} from '@/schemas/taskSchema'

const props = withDefaults(
    defineProps<{
      initialValues?: Partial<TaskFormData> | null
      loading?: boolean
      serverError?: string | null
      submitLabel?: string
      cancelLabel?: string
    }>(),
    {
      loading: false,
      serverError: null,
      submitLabel: 'Salvar tarefa',
      cancelLabel: 'Cancelar',
    },
)

const emit = defineEmits<{
  submit: [value: TaskFormData]
  cancel: []
}>()

const categoriesQuery = useCategoryOptions()

const defaultValues: TaskFormData = {
  title: '',
  description: '',
  categoryId: 0,
  priority: 'Medium',
  dueDate: '',
}

const {handleSubmit, defineField, errors, resetForm} = useForm<TaskFormData>({
  validationSchema: toTypedSchema(taskSchema),
  initialValues: defaultValues,
})

const [title, titleAttrs] = defineField('title')
const [description, descriptionAttrs] = defineField('description')
const [categoryId, categoryIdAttrs] = defineField('categoryId')
const [priority, priorityAttrs] = defineField('priority')
const [dueDate, dueDateAttrs] = defineField('dueDate')

const categoryOptions = computed(() => [
  {label: 'Selecione uma categoria', value: 0},
  ...((categoriesQuery.data.value ?? [])
      .filter((category) => category.isActive)
      .map((category) => ({
        label: category.name,
        value: category.id,
      }))),
])

function buildValues(values?: Partial<TaskFormData> | null): TaskFormData {
  return {
    title: values?.title ?? '',
    description: values?.description ?? '',
    categoryId: values?.categoryId ?? 0,
    priority: values?.priority ?? 'Medium',
    dueDate: values?.dueDate ?? '',
  }
}

watch(
    () => props.initialValues,
    (values) => {
      resetForm({values: buildValues(values)})
    },
    {immediate: true, deep: true},
)

const onSubmit = handleSubmit((values) => {
  emit('submit', values)
})
</script>

<template>
  <form class="grid gap-5" @submit.prevent="onSubmit">
    <div class="grid gap-4">
      <BaseInput
          v-model="title"
          :disabled="loading"
          :error="errors.title"
          label="Título"
          placeholder="Ex.: Revisar backlog da sprint"
          required
          v-bind="titleAttrs"
      />

      <BaseInput
          v-model="description"
          :disabled="loading"
          :error="errors.description"
          :rows="5"
          label="Descrição"
          placeholder="Descreva o contexto da tarefa"
          textarea
          v-bind="descriptionAttrs"
      />

      <div class="grid grid-cols-2 gap-4 max-sm:grid-cols-1">
        <BaseSelect
            v-model="categoryId"
            :disabled="loading"
            :error="errors.categoryId"
            :options="categoryOptions"
            label="Categoria"
            required
            v-bind="categoryIdAttrs"
            value-mode="number"
        />

        <BaseSelect
            v-model="priority"
            :disabled="loading"
            :error="errors.priority"
            :options="[
            { label: 'Baixa', value: 'Low' },
            { label: 'Média', value: 'Medium' },
            { label: 'Alta', value: 'High' },
          ]"
            label="Prioridade"
            v-bind="priorityAttrs"
        />
      </div>

      <BaseInput
          v-model="dueDate"
          :disabled="loading"
          :error="errors.dueDate"
          label="Prazo"
          type="date"
          v-bind="dueDateAttrs"
      />
    </div>

    <p v-if="serverError" class="text-[0.9375rem] font-semibold text-danger-text">
      {{ serverError }}
    </p>

    <div class="flex justify-end gap-3 pt-1 max-sm:flex-col">
      <BaseButton
          :disabled="loading"
          class="max-sm:w-full"
          type="button"
          variant="secondary"
          @click="emit('cancel')"
      >
        {{ cancelLabel }}
      </BaseButton>
      <BaseButton :loading="loading" class="max-sm:w-full" type="submit">
        {{ submitLabel }}
      </BaseButton>
    </div>
  </form>
</template>
