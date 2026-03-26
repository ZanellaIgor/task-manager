using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;
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

        var categories = CreateCategories();
        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        var categoryMap = categories.ToDictionary(category => category.Name, StringComparer.OrdinalIgnoreCase);
        var tasks = CreateTasks(categoryMap, DateTime.UtcNow);

        context.Tasks.AddRange(tasks);
        await context.SaveChangesAsync();
    }

    private static List<Category> CreateCategories()
    {
        return
        [
            new()
            {
                Name = "Desenvolvimento",
                Description = "Implementação, correções e refatorações.",
            },
            new()
            {
                Name = "Design",
                Description = "Interface, experiência e consistência visual.",
            },
            new()
            {
                Name = "Infraestrutura",
                Description = "Deploy, ambientes, automação e operação.",
            },
            new()
            {
                Name = "Documentação",
                Description = "Guias, referências técnicas e onboarding.",
            },
            new()
            {
                Name = "Produto",
                Description = "Planejamento, refinamento e priorização.",
            },
            new()
            {
                Name = "QA",
                Description = "Testes, validação funcional e prevenção de regressões.",
            },
            new()
            {
                Name = "Dados",
                Description = "Relatórios, integrações analíticas e consistência de dados.",
            },
            new()
            {
                Name = "Segurança",
                Description = "Controles de acesso, revisão de riscos e endurecimento da aplicação.",
            },
            new()
            {
                Name = "Atendimento",
                Description = "Ajustes solicitados pelo suporte e melhorias em fluxos operacionais.",
            },
            new()
            {
                Name = "Marketing",
                Description = "Campanhas, landing pages e ativos de comunicação.",
            },
            new()
            {
                Name = "Financeiro",
                Description = "Demandas internas de faturamento, custos e conciliação.",
            },
            new()
            {
                Name = "Operações",
                Description = "Rotinas operacionais, acompanhamento e melhorias de processo.",
            },
            new()
            {
                Name = "Backoffice",
                Description = "Demandas internas temporariamente fora de operação.",
                IsActive = false,
            },
            new()
            {
                Name = "RH",
                Description = "Materiais internos e processos de pessoas temporariamente arquivados.",
                IsActive = false,
            },
        ];
    }

    private static List<TaskItem> CreateTasks(IDictionary<string, Category> categoryMap, DateTime now)
    {
        return
        [
            new()
            {
                Title = "Implementar exportação CSV",
                Description = "Permitir exportação da listagem principal para auditoria manual.",
                CategoryId = categoryMap["Desenvolvimento"].Id,
                Status = TaskStatus.InProgress,
                Priority = TaskPriority.High,
                DueDate = now.AddDays(1),
            },
            new()
            {
                Title = "Refinar filtros do dashboard",
                Description = "Ajustar ordenação, paginação e resumo visual do painel executivo.",
                CategoryId = categoryMap["Design"].Id,
                Status = TaskStatus.Pending,
                Priority = TaskPriority.Medium,
                DueDate = now.AddDays(2),
            },
            new()
            {
                Title = "Configurar backup automatizado",
                Description = "Padronizar rotina de backup e validar retenção mínima.",
                CategoryId = categoryMap["Infraestrutura"].Id,
                Status = TaskStatus.Pending,
                Priority = TaskPriority.High,
                DueDate = now.AddDays(3),
            },
            new()
            {
                Title = "Escrever guia de contribuição",
                Description = "Documentar convenções de branch, commit e revisão técnica.",
                CategoryId = categoryMap["Documentação"].Id,
                Status = TaskStatus.Completed,
                Priority = TaskPriority.Medium,
                DueDate = now.AddDays(-6),
                CompletedAt = now.AddDays(-5),
            },
            new()
            {
                Title = "Repriorizar backlog do trimestre",
                Description = "Revisar entregas de maior impacto com o time de produto.",
                CategoryId = categoryMap["Produto"].Id,
                Status = TaskStatus.InProgress,
                Priority = TaskPriority.High,
                DueDate = now.AddDays(5),
            },
            new()
            {
                Title = "Documentar regras de status",
                Description = "Explicar transições válidas entre Pending, InProgress, Completed e Cancelled.",
                CategoryId = categoryMap["Documentação"].Id,
                Status = TaskStatus.Pending,
                Priority = TaskPriority.Low,
                DueDate = now.AddDays(4),
            },
            new()
            {
                Title = "Ajustar paginação da listagem",
                Description = "Corrigir comportamento de navegação quando filtros são combinados.",
                CategoryId = categoryMap["Desenvolvimento"].Id,
                Status = TaskStatus.Pending,
                Priority = TaskPriority.Medium,
                DueDate = now.AddDays(7),
            },
            new()
            {
                Title = "Atualizar pipeline de build",
                Description = "Reduzir tempo de execução e padronizar variáveis de ambiente.",
                CategoryId = categoryMap["Infraestrutura"].Id,
                Status = TaskStatus.Completed,
                Priority = TaskPriority.High,
                DueDate = now.AddDays(-4),
                CompletedAt = now.AddDays(-3),
            },
            new()
            {
                Title = "Mapear fluxo de onboarding",
                Description = "Revisar artefatos entregues para novas pessoas do time.",
                CategoryId = categoryMap["Produto"].Id,
                Status = TaskStatus.Cancelled,
                Priority = TaskPriority.Low,
                DueDate = now.AddDays(6),
            },
            new()
            {
                Title = "Revisar acessibilidade do formulário",
                Description = "Melhorar contraste, foco visível e hierarquia semântica.",
                CategoryId = categoryMap["Design"].Id,
                Status = TaskStatus.InProgress,
                Priority = TaskPriority.Medium,
                DueDate = now.AddDays(2),
            },
            new()
            {
                Title = "Consolidar inventário de endpoints",
                Description = "Criar referência única para integração e suporte.",
                CategoryId = categoryMap["Documentação"].Id,
                Status = TaskStatus.Pending,
                Priority = TaskPriority.Low,
                DueDate = null,
            },
            new()
            {
                Title = "Corrigir alerta de jobs falhos",
                Description = "Investigar execução recorrente com falha e configurar notificação mínima.",
                CategoryId = categoryMap["Infraestrutura"].Id,
                Status = TaskStatus.Pending,
                Priority = TaskPriority.High,
                DueDate = now.AddDays(2),
            },
            new()
            {
                Title = "Cobrir fluxo de criação com testes manuais",
                Description = "Executar checklist funcional nas telas de cadastro antes do fechamento da sprint.",
                CategoryId = categoryMap["QA"].Id,
                Status = TaskStatus.Pending,
                Priority = TaskPriority.Medium,
                DueDate = now.AddDays(1),
            },
            new()
            {
                Title = "Automatizar cenários de regressão do dashboard",
                Description = "Preparar massa de dados e roteiro para validar indicadores principais.",
                CategoryId = categoryMap["QA"].Id,
                Status = TaskStatus.InProgress,
                Priority = TaskPriority.High,
                DueDate = now.AddDays(3),
            },
            new()
            {
                Title = "Revisar consistência das métricas semanais",
                Description = "Comparar relatórios operacionais com os números apresentados no painel.",
                CategoryId = categoryMap["Dados"].Id,
                Status = TaskStatus.Pending,
                Priority = TaskPriority.Medium,
                DueDate = now.AddDays(4),
            },
            new()
            {
                Title = "Criar exportação de indicadores por categoria",
                Description = "Disponibilizar resumo analítico segmentado para acompanhamento gerencial.",
                CategoryId = categoryMap["Dados"].Id,
                Status = TaskStatus.Pending,
                Priority = TaskPriority.Low,
                DueDate = now.AddDays(8),
            },
            new()
            {
                Title = "Revisar permissões administrativas",
                Description = "Mapear operações críticas e preparar matriz inicial de acesso.",
                CategoryId = categoryMap["Segurança"].Id,
                Status = TaskStatus.InProgress,
                Priority = TaskPriority.High,
                DueDate = now.AddDays(2),
            },
            new()
            {
                Title = "Registrar política mínima de senhas",
                Description = "Consolidar requisitos de autenticação para futura evolução da plataforma.",
                CategoryId = categoryMap["Segurança"].Id,
                Status = TaskStatus.Pending,
                Priority = TaskPriority.Medium,
                DueDate = now.AddDays(6),
            },
            new()
            {
                Title = "Padronizar respostas para chamados recorrentes",
                Description = "Agrupar orientações rápidas para suporte em dúvidas frequentes.",
                CategoryId = categoryMap["Atendimento"].Id,
                Status = TaskStatus.Pending,
                Priority = TaskPriority.Low,
                DueDate = now.AddDays(5),
            },
            new()
            {
                Title = "Registrar falhas reportadas por clientes",
                Description = "Consolidar evidências e priorizar encaminhamento técnico com contexto suficiente.",
                CategoryId = categoryMap["Atendimento"].Id,
                Status = TaskStatus.InProgress,
                Priority = TaskPriority.High,
                DueDate = now.AddDays(1),
            },
            new()
            {
                Title = "Atualizar textos da landing page",
                Description = "Ajustar proposta de valor e CTAs do material de aquisição.",
                CategoryId = categoryMap["Marketing"].Id,
                Status = TaskStatus.Pending,
                Priority = TaskPriority.Medium,
                DueDate = now.AddDays(9),
            },
            new()
            {
                Title = "Organizar calendário de campanhas internas",
                Description = "Distribuir marcos de comunicação para o próximo ciclo mensal.",
                CategoryId = categoryMap["Marketing"].Id,
                Status = TaskStatus.Cancelled,
                Priority = TaskPriority.Low,
                DueDate = now.AddDays(10),
            },
            new()
            {
                Title = "Conferir regras de fechamento mensal",
                Description = "Validar dependências entre lançamentos e relatórios de acompanhamento.",
                CategoryId = categoryMap["Financeiro"].Id,
                Status = TaskStatus.Pending,
                Priority = TaskPriority.High,
                DueDate = now.AddDays(2),
            },
            new()
            {
                Title = "Documentar critérios de aprovação de despesas",
                Description = "Formalizar fluxo interno para reduzir retrabalho em solicitações.",
                CategoryId = categoryMap["Financeiro"].Id,
                Status = TaskStatus.Completed,
                Priority = TaskPriority.Medium,
                DueDate = now.AddDays(-8),
                CompletedAt = now.AddDays(-7),
            },
            new()
            {
                Title = "Acompanhar SLA das rotinas operacionais",
                Description = "Medir atraso médio e levantar gargalos nas atividades do time.",
                CategoryId = categoryMap["Operações"].Id,
                Status = TaskStatus.Pending,
                Priority = TaskPriority.Medium,
                DueDate = now.AddDays(3),
            },
            new()
            {
                Title = "Revisar checklist de abertura diária",
                Description = "Padronizar verificações essenciais para início da operação.",
                CategoryId = categoryMap["Operações"].Id,
                Status = TaskStatus.Completed,
                Priority = TaskPriority.Low,
                DueDate = now.AddDays(-3),
                CompletedAt = now.AddDays(-2),
            },
            new()
            {
                Title = "Preparar guideline visual para badges de status",
                Description = "Definir uso de cor e contraste para estados principais da interface.",
                CategoryId = categoryMap["Design"].Id,
                Status = TaskStatus.Pending,
                Priority = TaskPriority.Medium,
                DueDate = now.AddDays(4),
            },
            new()
            {
                Title = "Extrair componente reutilizável de paginação",
                Description = "Padronizar navegação entre listagens de tarefas e categorias.",
                CategoryId = categoryMap["Desenvolvimento"].Id,
                Status = TaskStatus.InProgress,
                Priority = TaskPriority.High,
                DueDate = now.AddDays(2),
            },
            new()
            {
                Title = "Criar endpoint de auditoria operacional",
                Description = "Expor eventos resumidos para conferência de alterações críticas.",
                CategoryId = categoryMap["Desenvolvimento"].Id,
                Status = TaskStatus.Pending,
                Priority = TaskPriority.High,
                DueDate = now.AddDays(6),
            },
            new()
            {
                Title = "Escrever runbook de recuperação local",
                Description = "Documentar restauração rápida do ambiente de desenvolvimento após falhas.",
                CategoryId = categoryMap["Documentação"].Id,
                Status = TaskStatus.Pending,
                Priority = TaskPriority.Low,
                DueDate = now.AddDays(11),
            },
            new()
            {
                Title = "Revisar roadmap de integrações externas",
                Description = "Refinar escopo técnico das próximas entregas com dependências mapeadas.",
                CategoryId = categoryMap["Produto"].Id,
                Status = TaskStatus.Pending,
                Priority = TaskPriority.Medium,
                DueDate = now.AddDays(12),
            },
            new()
            {
                Title = "Eliminar warnings de configuração do ambiente",
                Description = "Reduzir ruído operacional e alinhar variáveis obrigatórias para desenvolvimento.",
                CategoryId = categoryMap["Infraestrutura"].Id,
                Status = TaskStatus.InProgress,
                Priority = TaskPriority.Medium,
                DueDate = now.AddDays(1),
            },
        ];
    }
}
