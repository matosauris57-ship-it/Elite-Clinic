namespace Clinic_System.Application.Features.PatientPrescriptions.Validators;

public class ListPatientPrescriptionsValidator : AbstractValidator<ListPatientPrescriptionsQuery>
{
    public ListPatientPrescriptionsValidator() => RuleFor(x => x.PatientId).GreaterThan(0);
}

public class GetPatientPrescriptionValidator : AbstractValidator<GetPatientPrescriptionQuery>
{
    public GetPatientPrescriptionValidator() => RuleFor(x => x.PrescriptionId).GreaterThan(0);
}

public class CreatePatientPrescriptionValidator : AbstractValidator<CreatePatientPrescriptionCommand>
{
    public CreatePatientPrescriptionValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0);
        RuleFor(x => x)
            .Must(x => x.TemplateKeys.Count > 0 || x.Items.Any(i => !string.IsNullOrWhiteSpace(i.MedicationName)))
            .WithMessage("Agregue una plantilla o al menos un medicamento.");
        RuleForEach(x => x.Items).SetValidator(new PatientPrescriptionItemValidator());
    }
}

public class UpdatePatientPrescriptionValidator : AbstractValidator<UpdatePatientPrescriptionCommand>
{
    public UpdatePatientPrescriptionValidator()
    {
        RuleFor(x => x.PrescriptionId).GreaterThan(0);
        RuleFor(x => x)
            .Must(x => x.TemplateKeys.Count > 0 || x.Items.Any(i => !string.IsNullOrWhiteSpace(i.MedicationName)))
            .WithMessage("Agregue una plantilla o al menos un medicamento.");
        RuleForEach(x => x.Items).SetValidator(new PatientPrescriptionItemValidator());
    }
}

public class DeletePatientPrescriptionValidator : AbstractValidator<DeletePatientPrescriptionCommand>
{
    public DeletePatientPrescriptionValidator() => RuleFor(x => x.PrescriptionId).GreaterThan(0);
}

public class PatientPrescriptionItemValidator : AbstractValidator<PatientPrescriptionItemDTO>
{
    public PatientPrescriptionItemValidator()
    {
        RuleFor(x => x.MedicationName).MaximumLength(200);
        RuleFor(x => x.Dosage).MaximumLength(100);
        RuleFor(x => x.Frequency).MaximumLength(120);
        RuleFor(x => x.DurationDays).InclusiveBetween(1, 60)
            .When(x => !string.IsNullOrWhiteSpace(x.MedicationName));
        RuleFor(x => x.SpecialInstructions).MaximumLength(500);
    }
}
