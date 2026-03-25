# TaskManager Task Manager

Aplicação full stack para gestão de tarefas, composta por um frontend em Vue 3 + TypeScript e uma API REST em ASP.NET
Core 8 com SQLite.

As especificações originais usadas na implementação estão
em [docs/vite.md](/C:/Users/igorz/Documents/programacao/to-do/docs/vite.md)
e [docs/aspnet.md](/C:/Users/igorz/Documents/programacao/to-do/docs/aspnet.md).

## Stack

**Frontend**

- Vue 3 com `<script setup lang="ts">`
- Vite
- Vue Router 4
- Pinia
- Axios + TanStack Query v5
- VeeValidate 4 + Zod v4
- Tailwind CSS v4

**Backend**

- ASP.NET Core 8 Web API
- C# 12
- Entity Framework Core 8
- SQLite
- FluentValidation
- Mapster
- Swagger (Swashbuckle)

## Estrutura

```text
.
├── backend/
│   ├── TaskManager.Api/
│   ├── TaskManager.Application/
│   ├── TaskManager.Domain/
│   ├── TaskManager.Infrastructure/
│   └── TaskManager.sln
├── frontend/
└── docs/
```

## O que foi implementado

- Dashboard com indicadores e listas resumidas
- Gestão de tarefas com criação, edição, conclusão, cancelamento e exclusão
- Gestão de categorias com criação, edição, ativação e desativação
- Filtros de tarefas por status, prioridade, categoria e busca
- Validação de formulários no frontend e no backend
- Persistência em SQLite com migrations e seed automático
- Swagger para documentação e teste da API
- Tratamento centralizado de erros com payload `{ "error": "mensagem" }`

## Como rodar

### Instalação inicial

Execute uma vez:

```powershell
cd backend
dotnet restore TaskManager.sln

cd ..\frontend
npm install

cd ..
```

### Subir tudo com um comando

Da raiz do repositório:

```powershell
powershell -ExecutionPolicy Bypass -File .\start-dev.ps1
```

Para encerrar frontend e backend:

```powershell
powershell -ExecutionPolicy Bypass -File .\stop-dev.ps1
```

Os scripts gravam PID e logs em `.run/`.

URLs:

- Frontend: `http://127.0.0.1:5173`
- Backend: `http://localhost:5000`
- Swagger: `http://localhost:5000/swagger`

### Rodar manualmente

### Backend

Pré-requisito: .NET SDK 8

```powershell
cd backend
dotnet restore TaskManager.sln
dotnet run --project .\TaskManager.Api\TaskManager.Api.csproj
```

A API sobe em `http://localhost:5000`.

Swagger:

```text
http://localhost:5000/swagger
```

No primeiro boot, a aplicação:

- aplica as migrations automaticamente
- cria o banco SQLite
- popula 5 categorias e 10 tarefas de seed

O arquivo do banco é criado em `backend/TaskManager.Api/TaskManager.db`.

### Frontend

Pré-requisito: Node.js 20+ e npm

```powershell
cd frontend
npm install
npm run dev
```

Aplicação:

```text
http://localhost:5173
```

Por padrão, o frontend consome:

```text
http://localhost:5000/api
```

Se necessário, é possível sobrescrever via `VITE_API_URL`.

Observação: se a API não estiver disponível, o frontend usa fallback mockado com `localStorage` para continuar
funcional.

## Endpoints principais

### Tasks

- `GET /api/tasks`
- `GET /api/tasks/{id}`
- `POST /api/tasks`
- `PUT /api/tasks/{id}`
- `PATCH /api/tasks/{id}/complete`
- `PATCH /api/tasks/{id}/cancel`
- `DELETE /api/tasks/{id}`

### Categories

- `GET /api/categories`
- `GET /api/categories/{id}`
- `POST /api/categories`
- `PUT /api/categories/{id}`
- `PATCH /api/categories/{id}/deactivate`
- `PATCH /api/categories/{id}/activate`

## Regras de negócio relevantes

- tarefa nova sempre nasce com status `Pending`
- tarefa só pode ser criada ou editada com categoria ativa
- tarefa `Completed` não pode ser editada nem excluída
- tarefa `Cancelled` não pode ser concluída
- ao concluir uma tarefa, `CompletedAt` é preenchido automaticamente
- nome de categoria é único de forma case-insensitive
- categoria desativada não é removida
- categorias inativas não aparecem no formulário de tarefa

## Comandos de validação

### Backend

```powershell
cd backend
dotnet restore TaskManager.sln
dotnet build TaskManager.sln
```

### Frontend

```powershell
cd frontend
npm run build
```

## Migrations

A migration inicial já foi gerada
em [Data/Migrations](/C:/Users/igorz/Documents/programacao/to-do/backend/TaskManager.Infrastructure/Data/Migrations).

Se o schema do backend mudar, uma nova migration pode ser criada com:

```powershell
cd backend
dotnet ef migrations add NomeDaMigration --project .\TaskManager.Infrastructure\TaskManager.Infrastructure.csproj --startup-project .\TaskManager.Api\TaskManager.Api.csproj --output-dir Data\Migrations
```

## Arquivos centrais

- Frontend bootstrap: [main.ts](/C:/Users/igorz/Documents/programacao/to-do/frontend/src/main.ts)
- Frontend router: [index.ts](/C:/Users/igorz/Documents/programacao/to-do/frontend/src/router/index.ts)
- Frontend API client: [api.ts](/C:/Users/igorz/Documents/programacao/to-do/frontend/src/services/api.ts)
- Backend bootstrap: [Program.cs](/C:/Users/igorz/Documents/programacao/to-do/backend/TaskManager.Api/Program.cs)
- Backend
  DI/configuração: [ServiceCollectionExtensions.cs](/C:/Users/igorz/Documents/programacao/to-do/backend/TaskManager.Api/Extensions/ServiceCollectionExtensions.cs)
- Backend domínio de
  tarefas: [TaskService.cs](/C:/Users/igorz/Documents/programacao/to-do/backend/TaskManager.Application/Services/TaskService.cs)
- Backend
  persistência: [AppDbContext.cs](/C:/Users/igorz/Documents/programacao/to-do/backend/TaskManager.Infrastructure/Data/AppDbContext.cs)

## Observações

- O backend está configurado com CORS para `http://localhost:5173`.
- O frontend foi ajustado para ocultar categorias inativas no formulário de tarefa.
- O backend expõe `PATCH /api/categories/{id}/activate` para compatibilidade com a UI atual.
