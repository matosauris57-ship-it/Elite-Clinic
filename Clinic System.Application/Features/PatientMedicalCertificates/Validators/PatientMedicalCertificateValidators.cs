namespace Clinic_System.Application.Features.PatientMedicalCertificates.Validators;

public class ListPatientMedicalCertificatesValidator : AbstractValidator<ListPatientMedicalCertificatesQuery>
{
    public ListPatientMedicalCertificatesValidator() => RuleFor(x => x.PatientId).GreaterThan(0);
}

public class GetPatientMedicalCertificateValidator : AbstractValidator<GetPatientMedicalCertificateQuery>
{
    public GetPatientMedicalCertificateValidator() => RuleFor(x => x.CertificateId).GreaterThan(0);
}

public class CreatePatientMedicalCertificateValidator : AbstractValidator<CreatePatientMedicalCertificateCommand>
{
    public CreatePatientMedicalCertificateValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0);
        Include(new PatientMedicalCertificateFieldsValidator<CreatePatientMedicalCertificateCommand>());
    }
}

public class UpdatePatientMedicalCertificateValidator : AbstractValidator<UpdatePatientMedicalCertificateCommand>
{
    public UpdatePatientMedicalCertificateValidator()
    {
        RuleFor(x => x.CertificateId).GreaterThan(0);
        Include(new PatientMedicalCertificateFieldsValidator<UpdatePatientMedicalCertificateCommand>());
    }
}

public class DeletePatientMedicalCertificateValidator : AbstractValidator<DeletePatientMedicalCertificateCommand>
{
    public DeletePatientMedicalCertificateValidator() => RuleFor(x => x.CertificateId).GreaterThan(0);
}

public class PatientMedicalCertificateFieldsValidator<T> : AbstractValidator<T>
    where T : class
{
    public PatientMedicalCertificateFieldsValidator()
    {
        RuleFor(x => GetCertificateType(x))
            .NotEmpty().WithMessage("Seleccione el tipo de certificado.")
            .MaximumLength(120);
        RuleFor(x => GetDiagnosis(x))
            .NotEmpty().WithMessage("Indique el motivo o diagnóstico.")
            .MaximumLength(1000);
        RuleFor(x => GetRecommendation(x))
            .NotEmpty().WithMessage("Indique la recomendación.")
            .MaximumLength(2000);
        RuleFor(x => GetObservations(x)).MaximumLength(2000);
        RuleFor(x => GetPurpose(x)).MaximumLength(80);
        RuleFor(x => GetRestDays(x))
            .InclusiveBetween(1, 365)
            .When(x => GetIncludesRest(x) && GetRestDays(x).HasValue);
        RuleFor(x => GetRestStartDate(x))
            .NotNull()
            .When(GetIncludesRest)
            .WithMessage("Indique la fecha de inicio del reposo.");
        RuleFor(x => GetRestEndDate(x))
            .NotNull()
            .When(GetIncludesRest)
            .WithMessage("Indique la fecha fin del reposo.");
        RuleFor(x => x)
            .Must(x => !GetIncludesRest(x)
                || !GetRestStartDate(x).HasValue
                || !GetRestEndDate(x).HasValue
                || GetRestEndDate(x)!.Value.Date >= GetRestStartDate(x)!.Value.Date)
            .WithMessage("La fecha fin del reposo no puede ser anterior a la fecha inicio.");
    }

    private static string GetCertificateType(T value) => value switch
    {
        CreatePatientMedicalCertificateCommand x => x.CertificateType,
        UpdatePatientMedicalCertificateCommand x => x.CertificateType,
        _ => string.Empty
    };

    private static string GetDiagnosis(T value) => value switch
    {
        CreatePatientMedicalCertificateCommand x => x.Diagnosis,
        UpdatePatientMedicalCertificateCommand x => x.Diagnosis,
        _ => string.Empty
    };

    private static string GetRecommendation(T value) => value switch
    {
        CreatePatientMedicalCertificateCommand x => x.Recommendation,
        UpdatePatientMedicalCertificateCommand x => x.Recommendation,
        _ => string.Empty
    };

    private static bool GetIncludesRest(T value) => value switch
    {
        CreatePatientMedicalCertificateCommand x => x.IncludesRest,
        UpdatePatientMedicalCertificateCommand x => x.IncludesRest,
        _ => false
    };

    private static DateTime? GetRestStartDate(T value) => value switch
    {
        CreatePatientMedicalCertificateCommand x => x.RestStartDate,
        UpdatePatientMedicalCertificateCommand x => x.RestStartDate,
        _ => null
    };

    private static DateTime? GetRestEndDate(T value) => value switch
    {
        CreatePatientMedicalCertificateCommand x => x.RestEndDate,
        UpdatePatientMedicalCertificateCommand x => x.RestEndDate,
        _ => null
    };

    private static int? GetRestDays(T value) => value switch
    {
        CreatePatientMedicalCertificateCommand x => x.RestDays,
        UpdatePatientMedicalCertificateCommand x => x.RestDays,
        _ => null
    };

    private static string? GetObservations(T value) => value switch
    {
        CreatePatientMedicalCertificateCommand x => x.Observations,
        UpdatePatientMedicalCertificateCommand x => x.Observations,
        _ => null
    };

    private static string? GetPurpose(T value) => value switch
    {
        CreatePatientMedicalCertificateCommand x => x.Purpose,
        UpdatePatientMedicalCertificateCommand x => x.Purpose,
        _ => null
    };
}
