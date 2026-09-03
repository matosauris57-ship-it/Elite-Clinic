using Clinic_System.Core.Validation;

namespace Clinic_System.Application.Tests.Validation;

public class ContactEmailTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Omitted_IsValid(string? value)
    {
        ContactEmail.TryValidate(value, out var normalized, out var error).Should().BeTrue();
        normalized.Should().BeNull();
        error.Should().BeNull();
        ContactEmail.NormalizeOrNull(value).Should().BeNull();
    }

    [Theory]
    [InlineData("paciente@gmail.com")]
    [InlineData("  MARIA.PEREZ+citas@clinicadental.com.do  ")]
    public void ValidAddress_Normalizes(string value)
    {
        ContactEmail.TryValidate(value, out var normalized, out var error).Should().BeTrue();
        error.Should().BeNull();
        normalized.Should().Be(value.Trim().ToLowerInvariant());
    }

    [Theory]
    [InlineData("sin-arroba")]
    [InlineData("a@b")]
    [InlineData("paciente@localhost")]
    [InlineData("paciente@example.com")]
    [InlineData("paciente@.com")]
    [InlineData("paciente@dominio.c")]
    public void InvalidAddress_Fails(string value)
    {
        ContactEmail.TryValidate(value, out var normalized, out var error).Should().BeFalse();
        normalized.Should().BeNull();
        error.Should().NotBeNullOrWhiteSpace();
    }
}
