using FastTechFoodsAuth.Application.DTOs;
using FluentValidation;

namespace FastTechFoodsAuth.Application.Validators
{
    public class RegisterUserValidator : AbstractValidator<RegisterUserDto>
    {
        public RegisterUserValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("O campo email é obrigatório.")
                .EmailAddress().WithMessage("O campo email deve ser um endereço de email válido.")
                .MaximumLength(150).WithMessage("O campo email deve ter no máximo 150 caracteres.");
            RuleFor(x => x.CPF)
                .NotEmpty().WithMessage("O campo CPF é obrigatório.")
                .Matches(@"^\d{11}$").WithMessage("O campo CPF deve conter exatamente 11 dígitos numéricos.");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("A senha é obrigatória.")
                .MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres.")
                .MaximumLength(100).WithMessage("A senha deve ter no máximo 100 caracteres.");
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("O nome é obrigatório.")
                .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres.");
            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("O papel é obrigatório.")
                .Must(role => role == "Admin" || role == "User")
                .WithMessage("O papel deve ser 'Admin' ou 'User'.");
        }
    }
}
