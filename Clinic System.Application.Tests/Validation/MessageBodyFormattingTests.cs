using Clinic_System.Core.Messaging;

namespace Clinic_System.Application.Tests.Validation;

public class MessageBodyFormattingTests
{
    [Fact]
    public void ToWhatsApp_ConvertsBoldAndLineBreaks()
    {
        var html = "<p>Hola <strong>María</strong>,</p><p>su cita es mañana.</p>";
        var text = MessageBodyFormatting.ToWhatsApp(html);
        text.Should().Contain("*María*");
        text.Should().Contain("su cita es mañana.");
    }

    [Fact]
    public void IsBlank_TreatsEmptyParagraphAsBlank()
    {
        MessageBodyFormatting.IsBlank("<p><br></p>").Should().BeTrue();
        MessageBodyFormatting.IsBlank("<p>Hola</p>").Should().BeFalse();
    }

    [Fact]
    public void ApplyTokens_EncodesHtmlValues()
    {
        var html = MessageBodyFormatting.ApplyTokens(
            "<p>Hola {nombre}</p>",
            new Dictionary<string, string> { ["{nombre}"] = "Ana <b>X</b>" });
        html.Should().Contain("Ana &lt;b&gt;X&lt;/b&gt;");
    }

    [Fact]
    public void Sanitize_RemovesScripts()
    {
        var clean = MessageBodyFormatting.Sanitize("<p>Hola</p><script>alert(1)</script>");
        clean.Should().Be("<p>Hola</p>");
    }
}
