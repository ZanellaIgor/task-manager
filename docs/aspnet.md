Contexto do Projeto
Você irá implementar o backend completo de um sistema de gestão de tarefas chamado TaskManager Task Manager. Trata-se de
uma prova técnica avaliada por recrutadores e desenvolvedores — organização arquitetural, qualidade do código, modelagem
de domínio e consistência das regras de negócio são tão importantes quanto a funcionalidade.
O frontend já está implementado em Vue.js e consumirá esta API via HTTP. O backend deve ser uma API REST desacoplada,
sem acoplamento com o frontend.

Stack Obrigatória
CamadaTecnologiaFrameworkASP.NET Core 8 Web APILinguagemC# 12ORMEntity Framework Core 8Banco de
dadosSQLiteValidaçãoFluentValidationMapeamentoMapsterDocumentaçãoSwagger / Scalar (Swashbuckle)

⚠️ Não use MediatR, não use CQRS, não use AutoMapper. Mantenha a solução simples, direta e bem estruturada.

Estrutura de Arquivos
backend/
├── TaskManager.Api/
│ ├── Controllers/
│ │ ├── TasksController.cs
│ │ └── CategoriesController.cs
│ ├── Middlewares/
│ │ └── ExceptionHandlingMiddleware.cs
│ ├── Extensions/
│ │ └── ServiceCollectionExtensions.cs
│ └── Program.cs
├── TaskManager.Application/
│ ├── DTOs/
│ │ ├── Tasks/
│ │ │ ├── TaskDto.cs
│ │ │ ├── CreateTaskDto.cs
│ │ │ └── UpdateTaskDto.cs
│ │ └── Categories/
│ │ ├── CategoryDto.cs
│ │ ├── CreateCategoryDto.cs
│ │ └── UpdateCategoryDto.cs
│ ├── Filters/
│ │ └── TaskFilterDto.cs
│ ├── Validators/
│ │ ├── CreateTaskValidator.cs
│ │ ├── UpdateTaskValidator.cs
│ │ ├── CreateCategoryValidator.cs
│ │ └── UpdateCategoryValidator.cs
│ └── Services/
│ ├── Interfaces/
│ │ ├── ITaskService.cs
│ │ └── ICategoryService.cs
│ ├── TaskService.cs
│ └── CategoryService.cs
├── TaskManager.Domain/
│ ├── Entities/
│ │ ├── TaskItem.cs
│ │ └── Category.cs
│ └── Enums/
│ ├── TaskStatus.cs
│ └── TaskPriority.cs
├── TaskManager.Infrastructure/
│ ├── Data/
│ │ ├── AppDbContext.cs
│ │ └── Configurations/
│ │ ├── TaskItemConfiguration.cs
│ │ └── CategoryConfiguration.cs
│ └── Repositories/
│ ├── Interfaces/
│ │ ├── ITaskRepository.cs
│ │ └── ICategoryRepository.cs
│ ├── TaskRepository.cs
│ └── CategoryRepository.cs
└── TaskManager.sln

Camada de Domínio (TaskManager.Domain)
Enums:
csharp// Enums/TaskStatus.cs
namespace TaskManager.Domain.Enums;

public enum TaskStatus
{
Pending = 1,
InProgress = 2,
Completed = 3,
Cancelled = 4
}

// Enums/TaskPriority.cs
namespace TaskManager.Domain.Enums;

public enum TaskPriority
{
Low = 1,
Medium = 2,
High = 3
}
Entidades — use a classe base BaseEntity para campos auditáveis:
csharp// Entities/BaseEntity.cs
public abstract class BaseEntity
{
public int Id { get; set; }
public DateTime CreatedAt { get; set; }
public DateTime UpdatedAt { get; set; }
}

// Entities/Category.cs
public class Category : BaseEntity
{
public string Name { get; set; } = string.Empty;
public string? Description { get; set; }
public bool IsActive { get; set; } = true;
public ICollection<TaskItem> Tasks { get; set; } = [];
}

// Entities/TaskItem.cs
// IMPORTANTE: não nomear "Task" para evitar conflito com System.Threading.Tasks.Task
public class TaskItem : BaseEntity
{
public string Title { get; set; } = string.Empty;
public string? Description { get; set; }
public int CategoryId { get; set; }
public Category Category { get; set; } = null!;
public TaskStatus Status { get; set; } = TaskStatus.Pending;
public TaskPriority Priority { get; set; } = TaskPriority.Medium;
public DateTime? DueDate { get; set; }
public DateTime? CompletedAt { get; set; }
}

Camada de Infraestrutura (TaskManager.Infrastructure)
AppDbContext.cs:
csharppublic class AppDbContext : DbContext
{
public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        SetAuditDates();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        SetAuditDates();
        return base.SaveChangesAsync(ct);
    }

    private void SetAuditDates()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = now;
            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Entity.UpdatedAt = now;
        }
    }

}
Configurações EF Core — use Fluent API, não Data Annotations:
csharp// Configurations/TaskItemConfiguration.cs
public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
public void Configure(EntityTypeBuilder<TaskItem> builder)
{
builder.ToTable("Tasks");
builder.HasKey(t => t.Id);
builder.Property(t => t.Title).IsRequired().HasMaxLength(100);
builder.Property(t => t.Description).HasMaxLength(500);
builder.Property(t => t.Status).IsRequired()
.HasConversion<string>(); // persiste como string legível
builder.Property(t => t.Priority).IsRequired()
.HasConversion<string>();
builder.HasOne(t => t.Category)
.WithMany(c => c.Tasks)
.HasForeignKey(t => t.CategoryId)
.OnDelete(DeleteBehavior.Restrict);
}
}

// Configurations/CategoryConfiguration.cs
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
public void Configure(EntityTypeBuilder<Category> builder)
{
builder.ToTable("Categories");
builder.HasKey(c => c.Id);
builder.Property(c => c.Name).IsRequired().HasMaxLength(60);
builder.Property(c => c.Description).HasMaxLength(200);
builder.HasIndex(c => c.Name).IsUnique();
}
}
Repositórios — interface + implementação:
csharp// Repositories/Interfaces/ITaskRepository.cs
public interface ITaskRepository
{
Task<IEnumerable<TaskItem>> GetAllAsync(TaskFilterDto filters);
Task<TaskItem?> GetByIdAsync(int id);
Task<TaskItem> AddAsync(TaskItem task);
Task UpdateAsync(TaskItem task);
Task DeleteAsync(TaskItem task);
}

// Repositories/TaskRepository.cs
public class TaskRepository : ITaskRepository
{
private readonly AppDbContext _context;
public TaskRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<TaskItem>> GetAllAsync(TaskFilterDto filters)
    {
        var query = _context.Tasks
            .Include(t => t.Category)
            .AsQueryable();

        if (filters.Status is not null)
            query = query.Where(t => t.Status == filters.Status);
        if (filters.Priority is not null)
            query = query.Where(t => t.Priority == filters.Priority);
        if (filters.CategoryId is not null)
            query = query.Where(t => t.CategoryId == filters.CategoryId);
        if (!string.IsNullOrWhiteSpace(filters.Search))
            query = query.Where(t => t.Title.Contains(filters.Search));

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(int id) =>
        await _context.Tasks.Include(t => t.Category).FirstOrDefaultAsync(t => t.Id == id);

    public async Task<TaskItem> AddAsync(TaskItem task)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task UpdateAsync(TaskItem task)
    {
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TaskItem task)
    {
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
    }

}
Criar ICategoryRepository e CategoryRepository com o mesmo padrão, incluindo método ExistsWithNameAsync(string name,
int? excludeId) para validação de unicidade.

Camada de Aplicação (TaskManager.Application)
DTOs:
csharp// DTOs/Tasks/TaskDto.cs
public record TaskDto(
int Id,
string Title,
string? Description,
int CategoryId,
CategorySummaryDto? Category,
string Status,
string Priority,
DateTime CreatedAt,
DateTime UpdatedAt,
DateTime? DueDate,
DateTime? CompletedAt
);

public record CategorySummaryDto(int Id, string Name);

// DTOs/Tasks/CreateTaskDto.cs
public record CreateTaskDto(
string Title,
string? Description,
int CategoryId,
string Priority,
DateTime? DueDate
);

// DTOs/Tasks/UpdateTaskDto.cs
public record UpdateTaskDto(
string Title,
string? Description,
int CategoryId,
string Priority,
DateTime? DueDate
);

// Filters/TaskFilterDto.cs
public record TaskFilterDto(
TaskStatus? Status,
TaskPriority? Priority,
int? CategoryId,
string? Search
);
Validadores com FluentValidation:
csharp// Validators/CreateTaskValidator.cs
public class CreateTaskValidator : AbstractValidator<CreateTaskDto>
{
public CreateTaskValidator()
{
RuleFor(x => x.Title)
.NotEmpty().WithMessage("Título é obrigatório.")
.MaximumLength(100).WithMessage("Título deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => x.Description is not null);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Categoria é obrigatória.");

        RuleFor(x => x.Priority)
            .NotEmpty()
            .Must(p => Enum.TryParse<TaskPriority>(p, out _))
            .WithMessage("Prioridade inválida. Use: Low, Medium ou High.");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Prazo deve ser uma data futura.")
            .When(x => x.DueDate is not null);
    }

}
Criar UpdateTaskValidator com as mesmas regras. Criar CreateCategoryValidator e UpdateCategoryValidator para o domínio
de categorias.
Interfaces de serviço:
csharp// Services/Interfaces/ITaskService.cs
public interface ITaskService
{
Task<IEnumerable<TaskDto>> GetAllAsync(TaskFilterDto filters);
Task<TaskDto> GetByIdAsync(int id);
Task<TaskDto> CreateAsync(CreateTaskDto dto);
Task<TaskDto> UpdateAsync(int id, UpdateTaskDto dto);
Task<TaskDto> CompleteAsync(int id);
Task<TaskDto> CancelAsync(int id);
Task DeleteAsync(int id);
}
Implementação de serviço — regras de negócio aqui, nunca no controller:
csharp// Services/TaskService.cs
public class TaskService : ITaskService
{
private readonly ITaskRepository _taskRepo;
private readonly ICategoryRepository _categoryRepo;

    public TaskService(ITaskRepository taskRepo, ICategoryRepository categoryRepo)
    {
        _taskRepo = taskRepo;
        _categoryRepo = categoryRepo;
    }

    public async Task<TaskDto> CompleteAsync(int id)
    {
        var task = await _taskRepo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Tarefa {id} não encontrada.");

        if (task.Status is TaskStatus.Completed)
            throw new BusinessException("Tarefa já está concluída.");

        if (task.Status is TaskStatus.Cancelled)
            throw new BusinessException("Tarefa cancelada não pode ser concluída.");

        task.Status = TaskStatus.Completed;
        task.CompletedAt = DateTime.UtcNow;

        await _taskRepo.UpdateAsync(task);
        return task.Adapt<TaskDto>(); // Mapster
    }

    public async Task<TaskDto> CancelAsync(int id)
    {
        var task = await _taskRepo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Tarefa {id} não encontrada.");

        if (task.Status is TaskStatus.Completed or TaskStatus.Cancelled)
            throw new BusinessException("Tarefa não pode ser cancelada no status atual.");

        task.Status = TaskStatus.Cancelled;
        await _taskRepo.UpdateAsync(task);
        return task.Adapt<TaskDto>();
    }

    public async Task<TaskDto> CreateAsync(CreateTaskDto dto)
    {
        var categoryExists = await _categoryRepo.ExistsActiveAsync(dto.CategoryId);
        if (!categoryExists)
            throw new BusinessException("Categoria não encontrada ou inativa.");

        var task = dto.Adapt<TaskItem>();
        await _taskRepo.AddAsync(task);

        var created = await _taskRepo.GetByIdAsync(task.Id);
        return created!.Adapt<TaskDto>();
    }

    // Implementar: GetAllAsync, GetByIdAsync, UpdateAsync, DeleteAsync

}

Camada de API (TaskManager.Api)
Controllers — somente orquestração, sem lógica de negócio:
csharp// Controllers/TasksController.cs
[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
private readonly ITaskService _service;
public TasksController(ITaskService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] TaskFilterDto filters)
    {
        var tasks = await _service.GetAllAsync(filters);
        return Ok(tasks);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var task = await _service.GetByIdAsync(id);
        return Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskDto dto)
    {
        var task = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskDto dto)
    {
        var task = await _service.UpdateAsync(id, dto);
        return Ok(task);
    }

    [HttpPatch("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id)
    {
        var task = await _service.CompleteAsync(id);
        return Ok(task);
    }

    [HttpPatch("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var task = await _service.CancelAsync(id);
        return Ok(task);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

}
Criar CategoriesController com endpoints: GET /api/categories, GET /api/categories/{id}, POST /api/categories, PUT
/api/categories/{id}, PATCH /api/categories/{id}/deactivate.
Middleware de tratamento de erros:
csharp// Middlewares/ExceptionHandlingMiddleware.cs
public class ExceptionHandlingMiddleware
{
private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (BusinessException ex)
        {
            context.Response.StatusCode = 422;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { error = "Erro interno.", detail = ex.Message });
        }
    }

}
Criar as exceções de domínio NotFoundException e BusinessException herdando de Exception.
Program.cs:
csharpvar builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
.AddJsonOptions(opts =>
{
opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddDbContext<AppDbContext>(opts =>
opts.UseSqlite(builder.Configuration.GetConnectionString("Default")
?? "Data Source=TaskManager.db"));

// Repositórios
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// Serviços
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskValidator>();
builder.Services.AddFluentValidationAutoValidation();

// CORS — liberar para o frontend Vue
builder.Services.AddCors(opts =>
opts.AddDefaultPolicy(policy =>
policy.WithOrigins("http://localhost:5173")
.AllowAnyHeader()
.AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Aplicar migrations automaticamente
using (var scope = app.Services.CreateScope())
{
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
db.Database.Migrate();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();

Regras de Negócio
Implementar e garantir cobertura nas seguintes regras dentro dos Services, nunca nos controllers:
ContextoRegraCriar tarefaCategoria deve existir e estar ativaCriar tarefaStatus inicial sempre PendingConcluir tarefaNão
permitir se já CompletedConcluir tarefaNão permitir se CancelledConcluir tarefaPreencher CompletedAt
automaticamenteCancelar tarefaNão permitir se já Completed ou CancelledEditar tarefaNão permitir edição de tarefas
Completed ou CancelledExcluir tarefaNão permitir exclusão de tarefas CompletedCategoriaNome deve ser único (
case-insensitive)CategoriaDesativação não exclui — apenas marca IsActive = falseCategoriaCategoria inativa não aparece
para novas tarefas

Configuração e Execução
appsettings.json:
json{
"ConnectionStrings": {
"Default": "Data Source=TaskManager.db"
},
"Logging": {
"LogLevel": {
"Default": "Information",
"Microsoft.EntityFrameworkCore": "Warning"
}
}
}
O banco SQLite será criado automaticamente via db.Database.Migrate() no startup. Incluir ao menos 5 categorias e 10
tarefas de seed para facilitar avaliação:
csharp// No AppDbContext ou em um DatabaseSeeder separado
public static void Seed(AppDbContext context)
{
if (context.Categories.Any()) return;

    var categories = new List<Category>
    {
        new() { Name = "Desenvolvimento", Description = "Tarefas de código e engenharia" },
        new() { Name = "Design", Description = "Tarefas de UI/UX" },
        new() { Name = "Infraestrutura", Description = "DevOps e servidores" },
        new() { Name = "Documentação", Description = "Docs e wikis" },
        new() { Name = "Reuniões", Description = "Agendamentos e follow-ups" },
    };
    context.Categories.AddRange(categories);
    context.SaveChanges();

    // Inserir tarefas com variação de status, prioridade e datas

}

Entregável Esperado
Ao final, o projeto deve:

Iniciar com dotnet run a partir de TaskManager.Api/ sem erros
Criar e migrar o banco SQLite automaticamente no primeiro boot
Expor o Swagger em /swagger com todos os endpoints documentados
Responder corretamente ao frontend Vue.js rodando em http://localhost:5173
Retornar erros estruturados ({ "error": "mensagem" }) com status HTTP corretos — nunca stack trace em produção
Compilar sem warnings com dotnet build
Você atingiu seu limite de gastos com uso extra ∙ Seu limite será redefinido às 14:00Comprar mais Sonnet
4.6EstendidoClaude é uma IA e pode cometer erros. Por favor, verifique as respostas.