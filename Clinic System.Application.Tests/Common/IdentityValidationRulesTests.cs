namespace Clinic_System.Application.Tests.Common
{
    public record class PasswordCommand(string Password);

    public class PasswordValidator : AbstractValidator<PasswordCommand>
    {
        public PasswordValidator()
        {
            RuleFor(x => x.Password).PasswordRule();
        }
    }

    public class IdentityValidationRulesTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("12345")]
        public void PasswordRule_WhenValidate_ThenValidationErrors(string Password)
        {
            // Arrange
            var validator = new PasswordValidator();
            var command = new PasswordCommand(Password);

            // Act
            var result = validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();

            result.Errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData("doctor1")]
        [InlineData("123456")]
        [InlineData("abcdef")]
        public void PasswordRule_WhenValidate_WithValidPassword_ThenNoError(string validPassword)
        {
            // Arrange
            var validator = new PasswordValidator();
            var command = new PasswordCommand(validPassword);

            // Act
            var result = validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
    }
}
