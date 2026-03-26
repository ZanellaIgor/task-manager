<script lang="ts" setup>
import {computed, watch} from 'vue'
import {useForm} from 'vee-validate'
import {toTypedSchema} from '@vee-validate/zod'
import BaseButton from '@/components/shared/BaseButton.vue'
import BaseInput from '@/components/shared/BaseInput.vue'
import {type CategoryFormData, categorySchema} from '@/schemas/categorySchema'

const props = withDefaults(
    defineProps<{
      initialValues?: Partial<CategoryFormData> | null
      loading?: boolean
      serverError?: string | null
      submitLabel?: string
      cancelLabel?: string
      existingNames?: string[]
    }>(),
    {
      loading: false,
      serverError: null,
      submitLabel: 'Salvar categoria',
      cancelLabel: 'Cancelar',
      existingNames: () => [],
    },
)

const emit = defineEmits<{
  submit: [value: CategoryFormData]
  cancel: []
}>()

const validationSchema = computed(() => {
  const duplicateNames = new Set(
    props.existingNames.map((n) => n.trim().toLowerCase()),
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

const defaultValues: CategoryFormData = {
  name: '',
  description: '',
}

const {handleSubmit, defineField, errors, resetForm} = useForm<CategoryFormData>({
  validationSchema,
  initialValues: defaultValues,
})

const [name, nameAttrs] = defineField('name')
const [description, descriptionAttrs] = defineField('description')

function buildValues(values?: Partial<CategoryFormData> | null): CategoryFormData {
  return {
    name: values?.name ?? '',
    description: values?.description ?? '',
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
          v-model="name"
          :disabled="loading"
          :error="errors.name"
          label="Nome"
          placeholder="Ex.: Operacoes"
          required
          v-bind="nameAttrs"
      />

      <BaseInput
          v-model="description"
          :disabled="loading"
          :error="errors.description"
          label="Descricao"
          placeholder="Resumo curto para orientar o uso da categoria"
          v-bind="descriptionAttrs"
      />
    </div>

    <p v-if="serverError" class="text-[0.92rem] font-semibold text-red-600">
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
