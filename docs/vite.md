Você irá implementar o frontend completo de um sistema de gestão de tarefas chamado TaskManager Task Manager. Trata-se
de uma prova técnica avaliada por recrutadores e desenvolvedores — qualidade visual, organização do código e experiência
de uso são tão importantes quanto a funcionalidade.
O backend será uma API REST em ASP.NET Core (ainda não implementada). O frontend deve ser construído de forma
desacoplada, com chamadas HTTP via Axios + TanStack Query, e preparado para se conectar à API assim que disponível.

Stack Obrigatória
CamadaTecnologiaFrameworkVue.js 3 — Composition API com <script setup lang="ts">LinguagemTypeScript (strict mode)
BundlerViteRoteamentoVue Router 4Estado globalPiniaRequisições HTTPAxios + TanStack Query v5 (@tanstack/vue-query)
FormuláriosVeeValidate 4 + Zod v4 (@vee-validate/zod)EstilizaçãoTailwind CSS v4 (sem tailwind.config.js — configuração
via CSS)

⚠️ Não use Nuxt. Não use Options API. Não use JavaScript puro.

Configuração do Tailwind CSS v4
No Tailwind v4 não existe tailwind.config.js. A configuração é inteiramente via CSS usando a diretiva @theme. Configure
o design token em src/assets/main.css:
css@import "tailwindcss";

@theme {
--color-primary: #1B4FD8;
--color-primary-dark: #1239A8;
--color-primary-light: #3B6FFF;
--color-primary-soft: #EEF2FF;
--color-accent: #06B6D4;
--color-success: #10B981;
--color-warning: #F59E0B;
--color-danger: #EF4444;
--color-neutral-900: #0F172A;
--color-neutral-700: #334155;
--color-neutral-400: #94A3B8;
--color-neutral-100: #F1F5F9;

--font-sans: "DM Sans", sans-serif;
--font-display: "Syne", sans-serif;

--radius-card: 0.5rem;
--shadow-card: 0 1px 3px 0 rgb(0 0 0 / 0.08), 0 1px 2px -1px rgb(0 0 0 / 0.06);
}

```

Use as classes geradas automaticamente pelos tokens: `bg-primary`, `text-primary-dark`, `font-display`, etc.

---

### Identidade Visual

**Paleta:** base azul corporativo forte — `#1B4FD8` como cor dominante.

**Tipografia** (importar via Google Fonts no `index.html`):
- Títulos/display: `Syne` — 700, 800
- Interface/corpo: `DM Sans` — 400, 500

**Estilo:** Refinado e profissional. Fundo claro (`#F8FAFC`), sidebar azul com texto branco, cards com sombra suave, bordas `rounded-lg`, transições de `200ms` em hover. Sem glassmorphism, sem gradientes pesados. O visual deve transmitir **confiança e precisão**.

---

### Estrutura de Arquivos
```

frontend/
├── src/
│ ├── assets/
│ │ └── main.css ← @theme Tailwind v4, imports de fontes
│ ├── components/
│ │ ├── layout/
│ │ │ ├── AppSidebar.vue
│ │ │ └── AppHeader.vue
│ │ ├── tasks/
│ │ │ ├── TaskCard.vue
│ │ │ ├── TaskForm.vue
│ │ │ ├── TaskFilters.vue
│ │ │ └── TaskStatusBadge.vue
│ │ └── shared/
│ │ ├── BaseButton.vue
│ │ ├── BaseInput.vue
│ │ ├── BaseSelect.vue
│ │ └── BaseModal.vue
│ ├── views/
│ │ ├── DashboardView.vue
│ │ ├── TasksView.vue
│ │ └── CategoriesView.vue
│ ├── stores/
│ │ ├── tasks.ts ← Pinia (estado de UI e filtros apenas)
│ │ └── categories.ts
│ ├── services/
│ │ ├── api.ts ← instância Axios
│ │ ├── taskService.ts
│ │ └── categoryService.ts
│ ├── queries/
│ │ ├── taskQueries.ts ← useQuery e useMutation de tarefas
│ │ └── categoryQueries.ts
│ ├── schemas/
│ │ ├── taskSchema.ts ← Zod schemas + tipos inferidos
│ │ └── categorySchema.ts
│ ├── types/
│ │ └── index.ts ← interfaces e enums do domínio
│ ├── router/
│ │ └── index.ts
│ └── main.ts
├── index.html
├── vite.config.ts
├── tsconfig.json
└── package.json

Tipagem do Domínio (src/types/index.ts)
tsexport type TaskStatus = 'Pending' | 'InProgress' | 'Completed' | 'Cancelled'
export type TaskPriority = 'Low' | 'Medium' | 'High'

export interface Category {
id: number
name: string
description?: string
isActive: boolean
createdAt: string
updatedAt: string
}

export interface Task {
id: number
title: string
description?: string
categoryId: number
category?: Pick<Category, 'id' | 'name'>
status: TaskStatus
priority: TaskPriority
createdAt: string
updatedAt: string
dueDate?: string
completedAt?: string
}

export interface TaskFilters {
status?: TaskStatus
priority?: TaskPriority
categoryId?: number
search?: string
}

Schemas de Validação com Zod v4 (src/schemas/)
taskSchema.ts:
tsimport { z } from 'zod'

export const taskSchema = z.object({
title: z.string().min(1, 'Título obrigatório').max(100, 'Máximo 100 caracteres'),
description: z.string().max(500).optional(),
categoryId: z.number({ required_error: 'Categoria obrigatória' }).int().positive(),
priority: z.enum(['Low', 'Medium', 'High']),
dueDate: z.string().optional(),
})

export type TaskFormData = z.infer<typeof taskSchema>
categorySchema.ts:
tsimport { z } from 'zod'

export const categorySchema = z.object({
name: z.string().min(1, 'Nome obrigatório').max(60),
description: z.string().max(200).optional(),
})

export type CategoryFormData = z.infer<typeof categorySchema>

Formulários com VeeValidate + Zod
Use useForm com zodResolver para conectar os schemas ao VeeValidate:
ts// Exemplo em TaskForm.vue
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { taskSchema, type TaskFormData } from '@/schemas/taskSchema'

const { handleSubmit, defineField, errors, resetForm } = useForm<TaskFormData>({
validationSchema: toTypedSchema(taskSchema),
initialValues: {
priority: 'Medium',
},
})

const [title, titleAttrs] = defineField('title')
const [description, descriptionAttrs] = defineField('description')
const [categoryId, categoryIdAttrs] = defineField('categoryId')
const [priority, priorityAttrs] = defineField('priority')
const [dueDate, dueDateAttrs] = defineField('dueDate')

const onSubmit = handleSubmit((values) => {
// chamar mutation do TanStack Query
})
Os componentes BaseInput.vue e BaseSelect.vue devem aceitar v-bind="attrs" e v-model para integração limpa com
defineField.

Camada HTTP (src/services/api.ts)
tsimport axios from 'axios'

const api = axios.create({
baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5000/api',
headers: { 'Content-Type': 'application/json' },
})

export default api
taskService.ts:
tsimport api from './api'
import type { Task, TaskFilters } from '@/types'
import type { TaskFormData } from '@/schemas/taskSchema'

export const taskService = {
getAll: (filters?: TaskFilters) =>
api.get<Task[]>('/tasks', { params: filters }).then(r => r.data),
getById: (id: number) =>
api.get<Task>(`/tasks/${id}`).then(r => r.data),
create: (data: TaskFormData) =>
api.post<Task>('/tasks', data).then(r => r.data),
update: (id: number, data: TaskFormData) =>
api.put<Task>(`/tasks/${id}`, data).then(r => r.data),
complete: (id: number) =>
api.patch<Task>(`/tasks/${id}/complete`).then(r => r.data),
cancel: (id: number) =>
api.patch<Task>(`/tasks/${id}/cancel`).then(r => r.data),
remove: (id: number) =>
api.delete(`/tasks/${id}`),
}
Criar categoryService.ts seguindo o mesmo padrão para os endpoints de categorias.

TanStack Query — Queries e Mutations (src/queries/)
taskQueries.ts:
tsimport { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query'
import { taskService } from '@/services/taskService'
import type { TaskFilters } from '@/types'
import type { TaskFormData } from '@/schemas/taskSchema'
import type { MaybeRef } from 'vue'

// Query keys centralizados
export const taskKeys = {
all: ['tasks'] as const,
filtered: (filters: TaskFilters) => ['tasks', filters] as const,
detail: (id: number) => ['tasks', id] as const,
}

export function useTasks(filters: MaybeRef<TaskFilters>) {
return useQuery({
queryKey: computed(() => taskKeys.filtered(toValue(filters))),
queryFn: () => taskService.getAll(toValue(filters)),
})
}

export function useCreateTask() {
const qc = useQueryClient()
return useMutation({
mutationFn: (data: TaskFormData) => taskService.create(data),
onSuccess: () => qc.invalidateQueries({ queryKey: taskKeys.all }),
})
}

export function useCompleteTask() {
const qc = useQueryClient()
return useMutation({
mutationFn: (id: number) => taskService.complete(id),
onSuccess: () => qc.invalidateQueries({ queryKey: taskKeys.all }),
})
}

// Criar também: useUpdateTask, useCancelTask, useDeleteTask
Criar categoryQueries.ts com o mesmo padrão.

Stores Pinia (somente estado de UI)
No TanStack Query, o cache de servidor é gerenciado pela própria lib. O Pinia deve guardar apenas estado de interface:
tasks.ts:
tsimport { defineStore } from 'pinia'
import type { TaskFilters } from '@/types'

export const useTaskStore = defineStore('tasks', () => {
const filters = ref<TaskFilters>({})
const isFormOpen = ref(false)
const editingTaskId = ref<number | null>(null)

function openCreate() {
editingTaskId.value = null
isFormOpen.value = true
}

function openEdit(id: number) {
editingTaskId.value = id
isFormOpen.value = true
}

function closeForm() {
isFormOpen.value = false
editingTaskId.value = null
}

return { filters, isFormOpen, editingTaskId, openCreate, openEdit, closeForm }
})

Funcionalidades por View
DashboardView.vue

Cards de resumo: total, pendentes, em andamento, concluídas — derivados de useTasks({})
Lista das 5 tarefas mais recentes
Lista de tarefas com prazo nos próximos 3 dias
Indicador visual de prioridade (badge colorido)

TasksView.vue

Listagem com TaskCard.vue
TaskFilters.vue atualiza filters no store → reativa a query automaticamente
Botão "Nova Tarefa" → store.openCreate() → abre BaseModal com TaskForm
Ações no card: Editar, Concluir, Cancelar, Excluir
Destaque visual para tarefas vencidas (dueDate < hoje e status não terminal)

CategoriesView.vue

Tabela com nome, descrição, status ativo/inativo
Criar e editar via modal com TaskForm adaptado para categorias
Desativar (sem exclusão permanente)
Validação local de nome único contra a lista em cache

Setup do main.ts
tsimport { createApp } from 'vue'
import { createPinia } from 'pinia'
import { VueQueryPlugin } from '@tanstack/vue-query'
import router from './router'
import App from './App.vue'
import './assets/main.css'

createApp(App)
.use(createPinia())
.use(router)
.use(VueQueryPlugin)
.mount('#app')

Entregável Esperado
Ao final, o projeto deve:

Iniciar com npm install && npm run dev sem erros
Compilar sem erros TypeScript (vue-tsc --noEmit)
Funcionar com a API mockada (retornos [] ou dados estáticos) enquanto o backend não existe
Ter layout responsivo funcional (sidebar colapsável em mobile)
Exibir estados de loading, erro e vazio em todas as listagens
Toda a interação de formulário validar em tempo real via VeeValidate + Zod