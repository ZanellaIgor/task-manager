using FluentValidation;
using TaskManager.Application.DTOs.Categories;
using TaskManager.Application.Repositories.Interfaces;

namespace TaskManager.Application.Validators;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryValidator(ICategoryRepository categoryRepository)
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(60).WithMessage("Nome deve ter no máximo 60 caracteres.")
            .MustAsync(async (name, cancellationToken) =>
                !await categoryRepository.ExistsWithNameAsync(name.Trim(), null, cancellationToken))
            .WithMessage("Já existe uma categoria com esse nome.");

        RuleFor(x => x.Description)
            .MaximumLength(200).When(x => x.Description is not null)
            .WithMessage("Descrição deve ter no máximo 200 caracteres.");
    }
}
