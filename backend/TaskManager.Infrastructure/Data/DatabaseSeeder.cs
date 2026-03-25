using TaskManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using TaskPriority = TaskManager.Domain.Enums.TaskPriority;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Categories.AnyAsync())
        {
            return;
        }

        var categories = new List<Category>
        {
            new()
            {
                Name = "Desenvolvimento",
                Description = "Tarefas de código e engenharia",
            },
            new()
            {
                Name = "Design",
                Description = "Tarefas de UI/UX",
            },
            new()
            {
                Name = "Infraestrutura",
                Description = "DevOps e servidores",
            },
            new()
            {
                Name = "Documentação",
                Description = "Docs e wikis",
            },
            new()
            {
                Name = "Reuniões",
                Description = "Agendamentos e follow-ups",
                IsActive = false,
            },
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        var development = categories.Single(category => category.Name == "Desenvolvimento");
        var design = categories.Single(category => category.Name == "Design");
        var infrastructure = categories.Single(category => category.Name == "Infraestrutura");
        var documentation = categories.Single(category => category.Name == "Documentação");
        var meetings = categories.Single(category => category.Name == "Reuniões");

        var now = DateTime.UtcNow;

        var tasks = new List<TaskItem>
        {
            new()
            {
                Title = "Revisar backlog da sprint",
                Description = "Validar prioridades com o time de produto.",
                CategoryId = development.Id,
                Status = TaskStatus.InProgress,
                Priority = TaskPriority.High,
                DueDate = now.AddDays(3),
            },
            new()
            {
                Title = "Ajustar layout do dashboard",
                Description = "Melhorar a hierarquia visual da visão executiva.",
                CategoryId = design.Id,
                Status = TaskStatus.Pending,
                Priority = TaskPriority.Medium,
                DueDate = now.AddDays(5),
            },
            new()
            {
                Title = "Atualizar runbook de deploy",
                Description = "Documentar os passos do fluxo mais recente.",
                CategoryId = infrastructure.Id,
                Status = TaskStatus.Completed,
                Priority = TaskPriority.High,
                DueDate = now.AddDays(-2),
                CompletedAt = now.AddDays(-1),
            },
            new()
            {
                Title = "Revisar guia de onboarding",
                Description = "Padronizar informações para novas pessoas do time.",
                CategoryId = documentation.Id,
                Status = TaskStatus.Pending,
                Priority = TaskPriority.Low,
                DueDate = now.AddDays(7),
            },
            new()
            {
                Title = "Preparar pauta da retrospectiva",
                Description = "Consolidar pontos de melhoria dos squads.",
                CategoryId = meetings.Id,
                Status = TaskStatus.Cancelled,
                Priority = TaskPriority.Medium,
                DueDate = now.AddDays(1),
            },
            new()
            {
                Title = "Refatorar serviço de tarefas",
                Description = "Reduzir duplicação na camada de aplicação.",
                CategoryId = development.Id,
                Status = TaskStatus.Pending,
                Priority = TaskPriority.High,
                DueDate = now.AddDays(6),
            },
            new()
            {
                Title = "Criar nova paleta de interface",
                Description = "Explorar alternativas para o módulo de categorias.",
                CategoryId = design.Id,
                Status = TaskStatus.InProgress,
                Priority = TaskPriority.Medium,
                DueDate = now.AddDays(4),
            },
            new()
            {
                Title = "Padronizar backup do banco",
                Description = "Automatizar checagem diária do processo.",
                CategoryId = infrastructure.Id,
                Status = TaskStatus.Pending,
                Priority = TaskPriority.High,
                DueDate = now.AddDays(8),
            },
            new()
            {
                Title = "Escrever FAQ interna",
                Description = "Centralizar dúvidas recorrentes do time.",
                CategoryId = documentation.Id,
                Status = TaskStatus.Completed,
                Priority = TaskPriority.Low,
                DueDate = now.AddDays(-6),
                CompletedAt = now.AddDays(-5),
            },
            new()
            {
                Title = "Registrar decisões do comitê",
                Description = "Consolidar ações e responsáveis após a reunião.",
                CategoryId = meetings.Id,
                Status = TaskStatus.Pending,
                Priority = TaskPriority.Medium,
                DueDate = now.AddDays(2),
            },
        };

        context.Tasks.AddRange(tasks);
        await context.SaveChangesAsync();
    }
}
