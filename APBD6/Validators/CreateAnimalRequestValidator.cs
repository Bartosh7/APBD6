using APBD6.DTOs;
using FluentValidation;
namespace APBD6.Validators;

public class CreateAnimalRequestValidator : AbstractValidator<CreateAnimalsRequest>
{
    public CreateAnimalRequestValidator()
    {
        RuleFor(e => e.Name).MaximumLength(50).NotNull();
        RuleFor(e => e.Description).MaximumLength(200);
        RuleFor(e => e.Category).MaximumLength(50).NotNull();
        RuleFor(e => e.Area).MaximumLength(50).NotNull();
    }
}