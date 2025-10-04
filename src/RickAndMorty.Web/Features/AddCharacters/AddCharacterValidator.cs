namespace RickAndMorty.Web.Features.AddCharacters;

using FluentValidation;

public sealed class AddCharacterValidator : AbstractValidator<AddCharacterRequest>
{
    public AddCharacterValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters")
            .Must(name => !name.Any(char.IsDigit) || name.Any(char.IsLetter))
            .WithMessage("Name must contain at least one letter");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(status => new[] { "Alive", "Dead", "unknown" }.Contains(status))
            .WithMessage("Status must be 'Alive', 'Dead', or 'unknown'");

        RuleFor(x => x.Species)
            .NotEmpty().WithMessage("Species is required")
            .MaximumLength(100).WithMessage("Species must not exceed 100 characters");

        RuleFor(x => x.OriginName)
            .NotEmpty().WithMessage("Origin is required")
            .MaximumLength(200).WithMessage("Origin must not exceed 200 characters");
    }
}