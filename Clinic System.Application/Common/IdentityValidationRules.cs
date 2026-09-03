namespace Clinic_System.Application.Common
{
    public static class IdentityValidationRules
    {
        public static IRuleBuilderOptions<T, string> PasswordRule<T>(
            this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters");
        }
    }
}
