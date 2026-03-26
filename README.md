# Task Manager

Aplicação full stack para gestão de tarefas, composta por um frontend em Vue 3 + TypeScript e uma API REST em ASP.NET
Core 8 com SQLite.

As especificações originais usadas na implementação estão
em [docs/vite.md](docs/vite.md)
e [docs/aspnet.md](docs/aspnet.md).

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
│   └── TaskManager.slnx
├── frontend/
└── docs/
```

## O que foi implementado

- Dashboard com indicadores e listas resumidas
- Gestão de tarefas com criação, edição, conclusão, cancelamento e exclusão
- Gestão de categorias com criação, edição, ativação e desativação
- Filtros, paginação e ordenação em tarefas e categorias
- Validação de formulários no frontend e no backend
- Persistência em SQLite com migrations, seed automático e campos auditáveis
- Swagger para documentação e teste da API
- Tratamento centralizado de erros com `ProblemDetails` e `traceId`
- Dashboard consumindo endpoint próprio de overview

## Como rodar

### Pré-requisitos

- Windows com PowerShell para usar os scripts `start-dev.ps1` e `stop-dev.ps1`
- .NET SDK 8
- Node.js 20+ e npm
- `dotnet-ef` apenas se você quiser criar novas migrations manualmente

Os comandos abaixo foram validados em PowerShell no Windows. Em outros sistemas, use a seção de execução manual adaptando apenas a sintaxe do shell quando necessário.

### Instalação inicial

Execute uma vez:

```powershell
cd backend
dotnet restore TaskManager.slnx

cd ..\frontend
npm install

cd ..
Copy-Item .\frontend\.env.example .\frontend\.env
```

O arquivo `frontend/.env` é necessário para que o frontend aponte para a API real. Se preferir usar o mock local, altere `VITE_USE_API_MOCK` para `true`.

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
- Health check: `http://localhost:5000/health`

### Configuração de ambiente

O backend e o frontend não dependem mais de URL e CORS fixos em código.

Arquivos de exemplo:

- Backend: `backend/TaskManager.Api/appsettings.example.json`
- Frontend: `frontend/.env.example`

Configuração padrão do backend em `backend/TaskManager.Api/appsettings.json`:

```json
{
  "Urls": "http://localhost:5000",
  "ConnectionStrings": {
    "Default": "Data Source=TaskManager.db"
  },
  "Frontend": {
    "Origins": [
      "http://localhost:5173",
      "http://127.0.0.1:5173"
    ]
  },
  "Runtime": {
    "ApplyMigrationsOnStartup": true,
    "SeedOnStartup": true
  }
}
```

Variáveis de ambiente úteis:

- `ASPNETCORE_ENVIRONMENT`: define o ambiente do ASP.NET Core, por exemplo `Development` ou `Production`.
- `ASPNETCORE_URLS`: sobrescreve a URL de escuta da API, por exemplo `http://localhost:5050`.
- `ConnectionStrings__Default`: sobrescreve a connection string usada pelo EF Core.
- `Frontend__Origins__0`, `Frontend__Origins__1`, ...: definem as origens permitidas no CORS para o frontend.
- `Runtime__ApplyMigrationsOnStartup`: controla migrations automáticas no boot.
- `Runtime__SeedOnStartup`: controla seed automático no boot.
- `VITE_API_URL`: define a base URL consumida pelo frontend, por exemplo `http://localhost:5000/api`.
- `VITE_USE_API_MOCK`: habilita o mock local do frontend apenas quando definido como `true`.

Exemplo de backend com variáveis de ambiente no PowerShell:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://localhost:5050"
$env:ConnectionStrings__Default = "Data Source=TaskManager.db"
$env:Frontend__Origins__0 = "http://localhost:5173"
$env:Frontend__Origins__1 = "http://127.0.0.1:5173"
$env:Runtime__ApplyMigrationsOnStartup = "true"
$env:Runtime__SeedOnStartup = "true"
dotnet run --project .\backend\TaskManager.Api\TaskManager.Api.csproj
```

Exemplo de `.env` no frontend:

```dotenv
VITE_API_URL=http://localhost:5050/api
VITE_USE_API_MOCK=false
```

### Rodar manualmente

### Backend

Pré-requisito: .NET SDK 8

```powershell
cd backend
dotnet restore TaskManager.slnx
dotnet run --project .\TaskManager.Api\TaskManager.Api.csproj
```

A API sobe na URL definida por `Urls` em `appsettings.json`, `launchSettings.json` ou `ASPNETCORE_URLS`.
No padrão deste repositório, ela sobe em `http://localhost:5000`.

Swagger:

```text
http://localhost:5000/swagger
```

No primeiro boot, a aplicação:

- aplica as migrations automaticamente
- cria o banco SQLite
- popula dados iniciais suficientes para paginação de tarefas e categorias

O arquivo do banco é criado em `backend/TaskManager.Api/TaskManager.db`.
Se você já executou o projeto antes e quiser recriar o seed localmente, remova esse arquivo e inicie a API novamente.

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

Crie `frontend/.env` a partir de `frontend/.env.example` antes de executar o frontend.

Por padrão, o frontend consome:

```text
http://localhost:5000/api
```

Se necessário, é possível sobrescrever via `VITE_API_URL`.

Observação: o frontend usa a API real por padrão. O mock local com `localStorage` só deve ser ativado
explicitamente com `VITE_USE_API_MOCK=true`, por exemplo em testes ou demonstrações controladas.
Sem mock, `VITE_API_URL` precisa estar definido.

## Endpoints principais

### Tasks

- `GET /api/tasks?page=1&pageSize=9&status=Pending&priority=High&categoryId=1&search=backlog&sortBy=createdAt&sortDirection=Desc`
- `GET /api/tasks/overview`
- `GET /api/tasks/{id}`
- `POST /api/tasks`
- `PUT /api/tasks/{id}`
- `PATCH /api/tasks/{id}/complete`
- `PATCH /api/tasks/{id}/cancel`
- `DELETE /api/tasks/{id}`

### Categories

- `GET /api/categories?page=1&pageSize=10&search=produto&isActive=true&sortBy=name&sortDirection=Asc`
- `GET /api/categories/{id}`
- `POST /api/categories`
- `PUT /api/categories/{id}`
- `PATCH /api/categories/{id}/deactivate`
- `PATCH /api/categories/{id}/activate`

## Arquitetura do backend

- `TaskManager.Api`: composição da aplicação, controllers, middleware global de erros, CORS, Swagger e health check
- `TaskManager.Application`: DTOs, filtros paginados, validators e services com as regras de negócio
- `TaskManager.Domain`: entidades (`TaskItem`, `Category`), enums e exceções de domínio
- `TaskManager.Infrastructure`: `AppDbContext`, configurações do EF Core, repositórios, migrations e seed inicial

## Contratos principais da API

Todos os enums são serializados como string no JSON. Valores aceitos:

- `TaskStatus`: `Pending`, `InProgress`, `Completed`, `Cancelled`
- `TaskPriority`: `Low`, `Medium`, `High`
- `SortDirection`: `Asc`, `Desc`

Payload de criação e atualização de tarefa:

```json
{
  "title": "Implementar exportação CSV",
  "description": "Permitir exportação da listagem principal em CSV.",
  "categoryId": 1,
  "priority": "High",
  "dueDate": "2026-03-30T18:00:00Z"
}
```

Regras de validação desse payload:

- `title` é obrigatório e aceita até 100 caracteres
- `description` aceita até 500 caracteres
- `categoryId` deve ser maior que zero
- `priority` deve ser `Low`, `Medium` ou `High`
- `dueDate` deve ser futura ou `null`

Payload de criação e atualização de categoria:

```json
{
  "name": "Arquitetura",
  "description": "Demandas de desenho técnico e revisão estrutural."
}
```

Regras de validação desse payload:

- `name` é obrigatório, aceita até 60 caracteres e deve ser único sem diferenciar maiúsculas/minúsculas
- `description` aceita até 200 caracteres

Formato das respostas paginadas em `GET /api/tasks` e `GET /api/categories`:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 10,
  "totalItems": 0,
  "totalPages": 0
}
```

Formato do resumo retornado em `GET /api/tasks/overview`:

```json
{
  "totalCount": 12,
  "pendingCount": 5,
  "inProgressCount": 3,
  "completedCount": 2,
  "recentTasks": [],
  "upcomingTasks": []
}
```

Formato padrão de erro:

```json
{
  "type": "https://httpstatuses.com/422",
  "title": "Violação de regra de negócio.",
  "status": 422,
  "detail": "Categoria não encontrada ou inativa.",
  "instance": "/api/tasks",
  "traceId": "00-abc123",
  "code": "business_rule_violation"
}
```

Para erros de validação, a API responde com `400` e inclui também a propriedade `errors` com os campos inválidos.

## Filtros e ordenação

### Tasks

- `page`: padrão `1`
- `pageSize`: padrão `10`, mínimo `1`, máximo `100`
- `status`: filtra por `Pending`, `InProgress`, `Completed` ou `Cancelled`
- `priority`: filtra por `Low`, `Medium` ou `High`
- `categoryId`: filtra por categoria
- `search`: busca em título, descrição e nome da categoria
- `sortBy`: `createdAt`, `updatedAt`, `title`, `dueDate`, `priority`, `status`
- `sortDirection`: `Asc` ou `Desc`

### Categories

- `page`: padrão `1`
- `pageSize`: padrão `10`, mínimo `1`, máximo `100`
- `isActive`: filtra categorias ativas ou inativas
- `search`: busca em nome e descrição
- `sortBy`: `name`, `createdAt`, `updatedAt`
- `sortDirection`: `Asc` ou `Desc`

## Regras de negócio relevantes

- tarefa nova sempre nasce com status `Pending`
- tarefa só pode ser criada ou editada com categoria ativa
- tarefa `Completed` não pode ser editada nem excluída
- tarefa `Cancelled` não pode ser editada
- tarefa `Cancelled` não pode ser concluída
- cancelar uma tarefa limpa `CompletedAt`
- ao concluir uma tarefa, `CompletedAt` é preenchido automaticamente
- nome de categoria é único de forma case-insensitive
- categoria desativada não é removida
- categorias inativas não aparecem no formulário de tarefa

## Comandos de validação

### Backend

```powershell
cd backend
dotnet restore TaskManager.slnx
dotnet build TaskManager.slnx
dotnet test .\TaskManager.Application.Tests\TaskManager.Application.Tests.csproj
```

### Frontend

```powershell
cd frontend
npm run build
```

## Migrations

A migration inicial já foi gerada e está disponível em [backend/TaskManager.Infrastructure/Data/Migrations](backend/TaskManager.Infrastructure/Data/Migrations).

Para rodar o projeto, não é necessário criar o banco manualmente. Com `Runtime:ApplyMigrationsOnStartup=true` e `Runtime:SeedOnStartup=true`, a API cria ou atualiza o schema no primeiro boot e executa a carga inicial de dados.

Se você quiser criar novas migrations, confirme primeiro que o comando abaixo está disponível:

```powershell
dotnet ef --version
```

Se não estiver, instale a ferramenta:

```powershell
dotnet tool install --global dotnet-ef --version 8.*
```

Se o schema do backend mudar, uma nova migration pode ser criada com:

```powershell
cd backend
dotnet ef migrations add NomeDaMigration --project .\TaskManager.Infrastructure\TaskManager.Infrastructure.csproj --startup-project .\TaskManager.Api\TaskManager.Api.csproj --output-dir Data\Migrations
```

## Arquivos centrais

- Frontend bootstrap: [frontend/src/main.ts](frontend/src/main.ts)
- Frontend router: [frontend/src/router/index.ts](frontend/src/router/index.ts)
- Frontend API client: [frontend/src/services/api.ts](frontend/src/services/api.ts)
- Backend bootstrap: [backend/TaskManager.Api/Program.cs](backend/TaskManager.Api/Program.cs)
- Backend DI/configuração: [backend/TaskManager.Api/Extensions/ServiceCollectionExtensions.cs](backend/TaskManager.Api/Extensions/ServiceCollectionExtensions.cs)
- Backend domínio de tarefas: [backend/TaskManager.Application/Services/TaskService.cs](backend/TaskManager.Application/Services/TaskService.cs)
- Backend persistência: [backend/TaskManager.Infrastructure/Data/AppDbContext.cs](backend/TaskManager.Infrastructure/Data/AppDbContext.cs)

## Observações

- O backend usa `Frontend:Origins` no `appsettings` ou `Frontend__Origins__0`, `Frontend__Origins__1`, ... em variáveis de ambiente. O formato legado `Frontend:Origin` ainda é aceito por compatibilidade.
- O frontend foi ajustado para ocultar categorias inativas no formulário de tarefa.
- O backend expõe `PATCH /api/categories/{id}/activate` para compatibilidade com a UI atual.
