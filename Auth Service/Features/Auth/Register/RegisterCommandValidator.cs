using FluentValidation;

namespace Auth.Features.Auth.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.RegisterDto.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MinimumLength(3).WithMessage("First name must be at least 3 characters")
                .Matches("^[A-Za-z]+$").WithMessage("First name must contain letters only");

            RuleFor(x => x.RegisterDto.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MinimumLength(3).WithMessage("Last name must be at least 3 characters")
                .Matches("^[A-Za-z]+$").WithMessage("Last name must contain letters only");

            RuleFor(x => x.RegisterDto.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.RegisterDto.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\+?[0-9]{10,15}$").WithMessage("Invalid phone number");

            RuleFor(x => x.RegisterDto.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches("[0-9]").WithMessage("Password must contain at least one number")
                .Matches(@"[!@#$%^&*(),.?""{}|<>]").WithMessage("Password must contain at least one special character");

            RuleFor(x => x.RegisterDto.ConfirmPassword)
                .Equal(x => x.RegisterDto.Password)
                .WithMessage("Passwords do not match");
        }
    }
}
