using Clinic_System.Application.Common;
using Clinic_System.Application.DTOs.Patients;
using Clinic_System.Application.Service.Implemention;
using Clinic_System.Core.Enums;
using Clinic_System.Core.Interfaces.Repositories.IEntitiesRepository;
using Clinic_System.Core.Interfaces.UnitOfWork;
using FluentValidation;
using Moq;

namespace Clinic_System.Application.Tests.Common;

public class PatientCsvImportTests
{
    [Fact]
    public void Parse_ReadsSpanishHeaders_AndNormalizesPhone()
    {
        var csv = """
                  Nombre,Genero,FechaNacimiento,Direccion,Telefono,Cedula,Correo,Celular
                  Ana Pérez,Femenino,1990-05-12,Calle 1,+595 981 000 001,123,ana@dominio.com,+595-981-000-002
                  """;

        var rows = PatientCsvImport.Parse(csv, out var error);
        error.Should().BeNull();
        rows.Should().HaveCount(1);
        rows[0].FullName.Should().Be("Ana Pérez");
        rows[0].Phone.Should().Be("+595981000001");
        rows[0].MobilePhone.Should().Be("+595981000002");
        rows[0].NationalId.Should().Be("123");
    }

    [Fact]
    public void ValidateRow_RejectsDuplicatePhoneInBatch()
    {
        var phones = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var row = new PatientImportConfirmRow
        {
            RowNumber = 2,
            FullName = "Ana",
            Gender = "Female",
            DateOfBirth = new DateTime(1990, 1, 1),
            Address = "Calle",
            Phone = "+595981111111"
        };

        PatientCsvImport.ValidateRow(row, phones, ids, out _, out _).Should().BeNull();
        PatientCsvImport.ValidateRow(row, phones, ids, out _, out _).Should().Contain("duplicado");
    }
}

public class PatientImportServiceTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IPatientRepository> _patients = new();
    private readonly Mock<IDoctorRepository> _doctors = new();
    private readonly PatientImportService _sut;

    public PatientImportServiceTests()
    {
        _uow.SetupGet(x => x.PatientsRepository).Returns(_patients.Object);
        _uow.SetupGet(x => x.DoctorsRepository).Returns(_doctors.Object);
        _patients.Setup(x => x.GetImportIdentityKeysAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((new HashSet<string>(StringComparer.OrdinalIgnoreCase), new HashSet<string>(StringComparer.OrdinalIgnoreCase)));
        _doctors.Setup(x => x.GetAllPhonesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        _uow.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _sut = new PatientImportService(_uow.Object);
    }

    [Fact]
    public async Task PreviewAsync_MarksInvalidRows()
    {
        var csv = """
                  FullName,Gender,DateOfBirth,Address,Phone
                  ,Female,1990-01-01,Calle,+595981000001
                  Ana,Female,1990-01-01,Calle,+595981000002
                  """;

        var preview = await _sut.PreviewAsync(csv);
        preview.TotalRows.Should().Be(2);
        preview.ValidCount.Should().Be(1);
        preview.ErrorCount.Should().Be(1);
    }

    [Fact]
    public async Task ImportAsync_SavesValidPatients()
    {
        var rows = new List<PatientImportConfirmRow>
        {
            new()
            {
                RowNumber = 2,
                FullName = "Ana Pérez",
                Gender = "Female",
                DateOfBirth = new DateTime(1990, 5, 12),
                Address = "Calle 1",
                Phone = "+595981000010"
            }
        };

        Clinic_System.Core.Entities.Patient? added = null;
        _patients.Setup(x => x.AddAsync(It.IsAny<Clinic_System.Core.Entities.Patient>(), It.IsAny<CancellationToken>()))
            .Callback<Clinic_System.Core.Entities.Patient, CancellationToken>((p, _) =>
            {
                p.Id = 99;
                added = p;
            })
            .Returns(Task.CompletedTask);

        var result = await _sut.ImportAsync(rows);
        result.Imported.Should().Be(1);
        result.Skipped.Should().Be(0);
        added.Should().NotBeNull();
        added!.FullName.Should().Be("Ana Pérez");
        result.Rows[0].PatientId.Should().Be(99);
    }

    [Fact]
    public async Task ImportAsync_RejectsEmptyPayload()
    {
        var act = () => _sut.ImportAsync([]);
        await act.Should().ThrowAsync<ValidationException>();
    }
}
