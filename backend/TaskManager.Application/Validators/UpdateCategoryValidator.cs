using FluentValidation;
using TaskManager.Application.DTOs.Categories;

namespace TaskManager.Application.Validators;

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryDto>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(60).WithMessage("Nome deve ter no máximo 60 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(200).When(x => x.Description is not null)
            .WithMessage("Descrição deve ter no máximo 200 caracteres.");
    }
}
