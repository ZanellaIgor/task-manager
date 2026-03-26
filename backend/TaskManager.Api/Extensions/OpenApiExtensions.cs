using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using TaskManager.Application.Common.Pagination;
using TaskManager.Application.DTOs.Categories;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Domain.Enums;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Api.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddTaskManagerOpenApi(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Task Manager API",
                Version = "v1",
                Description = "API REST para gestão de tarefas e categorias com filtros, paginação, ordenação e respostas padronizadas em ProblemDetails.",
            });

            options.SupportNonNullableReferenceTypes();
            options.SchemaFilter<TaskManagerSchemaFilter>();
            options.OperationFilter<TaskManagerOperationFilter>();

            foreach (var xmlPath in GetXmlDocumentationPaths())
            {
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                }
            }
        });

        return services;
    }

    private static IEnumerable<string> GetXmlDocumentationPaths()
    {
        var assemblies = new[]
        {
            typeof(global::Program).Assembly,
            typeof(CreateTaskDto).Assembly,
            typeof(TaskStatus).Assembly,
        };

        return assemblies
            .Select(assembly => Path.Combine(AppContext.BaseDirectory, $"{assembly.GetName().Name}.xml"))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }
}

internal sealed class TaskManagerSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Type = "string";
            schema.Format = null;
            schema.Enum = Enum.GetNames(context.Type)
                .Select(name => (IOpenApiAny)new OpenApiString(name))
                .ToList();
            schema.Description = $"Valores aceitos: {string.Join(", ", Enum.GetNames(context.Type))}.";
            return;
        }

        if (context.Type == typeof(CreateTaskDto) || context.Type == typeof(UpdateTaskDto))
        {
            schema.Description = "Payload para criação ou atualização completa de tarefa.";
            SetPropertyDescription(schema, "title", "Título curto e objetivo da tarefa.");
            SetPropertyDescription(schema, "description", "Descrição opcional com contexto adicional.");
            SetPropertyDescription(schema, "categoryId", "Identificador de uma categoria ativa.");
            SetPropertyDescription(schema, "priority", "Prioridade operacional da tarefa.");
            SetPropertyDescription(schema, "dueDate", "Prazo em UTC no formato ISO 8601. Pode ser nulo.");
        }

        if (context.Type == typeof(CreateCategoryDto) || context.Type == typeof(UpdateCategoryDto))
        {
            schema.Description = "Payload para criação ou atualização completa de categoria.";
            SetPropertyDescription(schema, "name", "Nome único da categoria.");
            SetPropertyDescription(schema, "description", "Descrição opcional da finalidade da categoria.");
        }

        if (context.Type == typeof(TaskOverviewDto))
        {
            schema.Description = "Resumo usado no dashboard com indicadores agregados e listas recentes.";
        }

        if (context.Type == typeof(ProblemDetails))
        {
            schema.Description = "Formato padronizado para erros da API.";
            schema.Example = CreateProblemDetailsExample(422, "Violação de regra de negócio.", "Categoria não encontrada ou inativa.", "business_rule_violation");
        }

        if (context.Type == typeof(ValidationProblemDetails))
        {
            schema.Description = "Formato padronizado para falhas de validação.";
            schema.Example = new OpenApiObject
            {
                ["type"] = new OpenApiString("https://httpstatuses.com/400"),
                ["title"] = new OpenApiString("Falha de validação."),
                ["status"] = new OpenApiInteger(400),
                ["detail"] = new OpenApiString("Um ou mais erros de validação ocorreram."),
                ["instance"] = new OpenApiString("/api/tasks"),
                ["traceId"] = new OpenApiString("00-abc123"),
                ["code"] = new OpenApiString("validation_error"),
                ["errors"] = new OpenApiObject
                {
                    ["Title"] = new OpenApiArray
                    {
                        new OpenApiString("Título é obrigatório."),
                    },
                },
            };
        }
    }

    private static void SetPropertyDescription(OpenApiSchema schema, string propertyName, string description)
    {
        if (schema.Properties.TryGetValue(propertyName, out var property))
        {
            property.Description = description;
        }
    }

    private static OpenApiObject CreateProblemDetailsExample(int status, string title, string detail, string code)
    {
        return new OpenApiObject
        {
            ["type"] = new OpenApiString($"https://httpstatuses.com/{status}"),
            ["title"] = new OpenApiString(title),
            ["status"] = new OpenApiInteger(status),
            ["detail"] = new OpenApiString(detail),
            ["instance"] = new OpenApiString("/api/tasks/999"),
            ["traceId"] = new OpenApiString("00-abc123"),
            ["code"] = new OpenApiString(code),
        };
    }
}

internal sealed class TaskManagerOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var relativePath = context.ApiDescription.RelativePath?.ToLowerInvariant() ?? string.Empty;

        if (relativePath.StartsWith("api/tasks"))
        {
            DescribeTaskParameters(operation);
        }

        if (relativePath.StartsWith("api/categories"))
        {
            DescribeCategoryParameters(operation);
        }

        AddRequestExamples(operation, relativePath);
        AddProblemDetailsResponses(operation);
    }

    private static void DescribeTaskParameters(OpenApiOperation operation)
    {
        SetParameter(operation, "page", "Número da página. Valor padrão: 1.", new OpenApiInteger(1));
        SetParameter(operation, "pageSize", "Quantidade de itens por página. Intervalo aceito: 1 a 100.", new OpenApiInteger(10));
        SetParameter(operation, "search", "Busca por título, descrição ou nome da categoria.", new OpenApiString("dashboard"));
        SetParameter(operation, "status", "Filtro por status da tarefa.", new OpenApiString(nameof(TaskStatus.Pending)));
        SetParameter(operation, "priority", "Filtro por prioridade.", new OpenApiString(nameof(TaskPriority.High)));
        SetParameter(operation, "categoryId", "Filtro pelo identificador da categoria.", new OpenApiInteger(1));
        SetParameter(operation, "sortBy", "Campos aceitos: createdAt, updatedAt, title, dueDate, priority, status.", new OpenApiString("createdAt"));
        SetParameter(operation, "sortDirection", "Direção da ordenação: Asc ou Desc.", new OpenApiString(nameof(SortDirection.Desc)));
    }

    private static void DescribeCategoryParameters(OpenApiOperation operation)
    {
        SetParameter(operation, "page", "Número da página. Valor padrão: 1.", new OpenApiInteger(1));
        SetParameter(operation, "pageSize", "Quantidade de itens por página. Intervalo aceito: 1 a 100.", new OpenApiInteger(10));
        SetParameter(operation, "search", "Busca por nome ou descrição da categoria.", new OpenApiString("produto"));
        SetParameter(operation, "isActive", "Filtra categorias ativas ou inativas.", new OpenApiBoolean(true));
        SetParameter(operation, "sortBy", "Campos aceitos: name, createdAt, updatedAt.", new OpenApiString("name"));
        SetParameter(operation, "sortDirection", "Direção da ordenação: Asc ou Desc.", new OpenApiString(nameof(SortDirection.Asc)));
    }

    private static void AddRequestExamples(OpenApiOperation operation, string relativePath)
    {
        if (operation.RequestBody is null || !operation.RequestBody.Content.TryGetValue("application/json", out var content))
        {
            return;
        }

        content.Example = relativePath switch
        {
            "api/tasks" => new OpenApiObject
            {
                ["title"] = new OpenApiString("Implementar exportação CSV"),
                ["description"] = new OpenApiString("Permitir exportação da listagem principal em CSV."),
                ["categoryId"] = new OpenApiInteger(1),
                ["priority"] = new OpenApiString(nameof(TaskPriority.High)),
                ["dueDate"] = new OpenApiString("2026-03-30T18:00:00Z"),
            },
            var path when path.StartsWith("api/tasks/") && !path.EndsWith("/complete") && !path.EndsWith("/cancel") => new OpenApiObject
            {
                ["title"] = new OpenApiString("Ajustar paginação do dashboard"),
                ["description"] = new OpenApiString("Refinar paginação e ordenação do módulo executivo."),
                ["categoryId"] = new OpenApiInteger(2),
                ["priority"] = new OpenApiString(nameof(TaskPriority.Medium)),
                ["dueDate"] = new OpenApiString("2026-04-02T15:00:00Z"),
            },
            "api/categories" => new OpenApiObject
            {
                ["name"] = new OpenApiString("Arquitetura"),
                ["description"] = new OpenApiString("Demandas de desenho técnico e revisão estrutural."),
            },
            var path when path.StartsWith("api/categories/") => new OpenApiObject
            {
                ["name"] = new OpenApiString("Documentação"),
                ["description"] = new OpenApiString("Guias operacionais, onboarding e referência técnica."),
            },
            _ => content.Example,
        };
    }

    private static void AddProblemDetailsResponses(OpenApiOperation operation)
    {
        AddProblemDetailsResponse(operation, "400", true, new OpenApiObject
        {
            ["type"] = new OpenApiString("https://httpstatuses.com/400"),
            ["title"] = new OpenApiString("Falha de validação."),
            ["status"] = new OpenApiInteger(400),
            ["detail"] = new OpenApiString("Um ou mais erros de validação ocorreram."),
            ["instance"] = new OpenApiString("/api/tasks"),
            ["traceId"] = new OpenApiString("00-abc123"),
            ["code"] = new OpenApiString("validation_error"),
            ["errors"] = new OpenApiObject
            {
                ["PageSize"] = new OpenApiArray
                {
                    new OpenApiString("PageSize must be between 1 and 100."),
                },
            },
        });

        AddProblemDetailsResponse(operation, "404", false, new OpenApiObject
        {
            ["type"] = new OpenApiString("https://httpstatuses.com/404"),
            ["title"] = new OpenApiString("Recurso não encontrado."),
            ["status"] = new OpenApiInteger(404),
            ["detail"] = new OpenApiString("Tarefa 999 não encontrada."),
            ["instance"] = new OpenApiString("/api/tasks/999"),
            ["traceId"] = new OpenApiString("00-abc123"),
            ["code"] = new OpenApiString("resource_not_found"),
        });

        AddProblemDetailsResponse(operation, "422", false, new OpenApiObject
        {
            ["type"] = new OpenApiString("https://httpstatuses.com/422"),
            ["title"] = new OpenApiString("Violação de regra de negócio."),
            ["status"] = new OpenApiInteger(422),
            ["detail"] = new OpenApiString("Categoria não encontrada ou inativa."),
            ["instance"] = new OpenApiString("/api/tasks"),
            ["traceId"] = new OpenApiString("00-abc123"),
            ["code"] = new OpenApiString("business_rule_violation"),
        });

        AddProblemDetailsResponse(operation, "500", false, new OpenApiObject
        {
            ["type"] = new OpenApiString("https://httpstatuses.com/500"),
            ["title"] = new OpenApiString("Erro inesperado."),
            ["status"] = new OpenApiInteger(500),
            ["detail"] = new OpenApiString("Ocorreu um erro inesperado."),
            ["instance"] = new OpenApiString("/api/tasks"),
            ["traceId"] = new OpenApiString("00-abc123"),
            ["code"] = new OpenApiString("unexpected_error"),
        });
    }

    private static void AddProblemDetailsResponse(OpenApiOperation operation, string statusCode, bool validation, OpenApiObject example)
    {
        if (!operation.Responses.TryGetValue(statusCode, out var response))
        {
            return;
        }

        response.Content.Clear();
        response.Content["application/problem+json"] = new OpenApiMediaType
        {
            Schema = new OpenApiSchema
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = validation ? nameof(ValidationProblemDetails) : nameof(ProblemDetails),
                },
            },
            Example = example,
        };
    }

    private static void SetParameter(OpenApiOperation operation, string parameterName, string description, IOpenApiAny example)
    {
        var parameter = operation.Parameters.FirstOrDefault(item =>
            string.Equals(item.Name, parameterName, StringComparison.OrdinalIgnoreCase));

        if (parameter is null)
        {
            return;
        }

        parameter.Description = description;
        parameter.Example = example;
    }
}
