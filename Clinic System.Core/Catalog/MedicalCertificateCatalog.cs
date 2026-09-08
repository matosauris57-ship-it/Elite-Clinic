namespace Clinic_System.Core.Catalog;

public static class MedicalCertificateCatalog
{
    public static readonly IReadOnlyList<string> CertificateTypes =
    [
        "Certificado Médico General",
        "Reposo Médico",
        "Certificado Odontológico",
        "Certificado de Asistencia",
        "Certificado de Aptitud",
        "Otro"
    ];

    public static readonly IReadOnlyList<string> Purposes =
    [
        "Laboral",
        "Escolar",
        "Personal",
        "Seguro Médico",
        "Otro"
    ];

    public static readonly IReadOnlyList<string> DiagnosisSuggestions =
    [
        "Gingivitis",
        "Extracción dental",
        "Control odontológico",
        "Infección respiratoria"
    ];
}
