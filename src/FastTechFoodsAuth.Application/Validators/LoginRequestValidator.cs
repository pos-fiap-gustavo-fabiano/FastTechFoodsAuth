using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FastTechFoodsAuth.Application.DTOs;
using FluentValidation;

namespace FastTechFoodsAuth.Application.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.EmailOrCpf)
                .NotEmpty().WithMessage("O campo emailOrCpf é obrigatório.")
                .MaximumLength(150).WithMessage("O campo Email ou CPF deve ter no máximo 150 caracteres.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("A senha é obrigatória.")
                .MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres.")
                .MaximumLength(100).WithMessage("A senha deve ter no máximo 100 caracteres.");
        }
    }
}
