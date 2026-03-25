using FluentValidation;
using TaskManager.Application.DTOs.Tasks;
using TaskPriority = TaskManager.Domain.Enums.TaskPriority;

namespace TaskManager.Application.Validators;

public class UpdateTaskValidator : AbstractValidator<UpdateTaskDto>
{
    public UpdateTaskValidator()
    {
        RuleFor(x => x.Title)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Título é obrigatório.")
            .MaximumLength(100).WithMessage("Título deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => x.Description is not null)
            .WithMessage("Descrição deve ter no máximo 500 caracteres.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Categoria é obrigatória.");

        RuleFor(x => x.Priority)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Prioridade é obrigatória.")
            .Must(priority => Enum.TryParse<TaskPriority>(priority, true, out _))
            .WithMessage("Prioridade inválida. Use: Low, Medium ou High.");

        RuleFor(x => x.DueDate)
            .Must((_, dueDate) => !dueDate.HasValue || dueDate.Value > DateTime.UtcNow)
            .WithMessage("Prazo deve ser uma data futura.");
    }
}
