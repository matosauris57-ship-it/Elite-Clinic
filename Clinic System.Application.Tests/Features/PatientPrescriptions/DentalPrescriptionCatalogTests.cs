using Clinic_System.Application.Features.PatientPrescriptions.Models;
using Clinic_System.Application.Features.PatientPrescriptions.Validators;
using Clinic_System.Core.Catalog;

namespace Clinic_System.Application.Tests.Features.PatientPrescriptions;

public class DentalPrescriptionCatalogTests
{
    [Fact]
    public void Catalog_HasOdontologyTemplates()
    {
        DentalPrescriptionCatalog.All.Should().HaveCountGreaterThan(5);
        DentalPrescriptionCatalog.Find("infeccion-amoxicilina").Should().NotBeNull();
        DentalPrescriptionCatalog.Find("infeccion-clindamicina")!.Lines.Should().Contain(x => x.MedicationName.Contains("Clindamicina"));
    }

    [Fact]
    public void CreateValidator_RequiresMedicationOrTemplate()
    {
        var validator = new CreatePatientPrescriptionValidator();
        validator.Validate(new CreatePatientPrescriptionCommand { PatientId = 1 }).IsValid.Should().BeFalse();
        validator.Validate(new CreatePatientPrescriptionCommand
        {
            PatientId = 1,
            TemplateKeys = ["infeccion-amoxicilina"]
        }).IsValid.Should().BeTrue();
    }
}
