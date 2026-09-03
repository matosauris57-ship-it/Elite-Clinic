namespace Clinic_System.Application.Tests.Features.Patients.CommandsTests.ValidatorsTests
{
    public class CreatePatientValidatorTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IDoctorRepository> _mockDoctorRepo;
        private readonly Mock<IPatientRepository> _mockPatientRepo;
        private readonly CreatePatientValidator _validator;

        public CreatePatientValidatorTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockDoctorRepo = new Mock<IDoctorRepository>();
            _mockPatientRepo = new Mock<IPatientRepository>();
            _mockUnitOfWork.SetupGet(u => u.DoctorsRepository).Returns(_mockDoctorRepo.Object);
            _mockUnitOfWork.SetupGet(u => u.PatientsRepository).Returns(_mockPatientRepo.Object);
            _validator = new CreatePatientValidator(_mockUnitOfWork.Object);
        }

        private static CreatePatientCommand ValidCommand() => new()
        {
            FullName = "Juan Perez",
            Address = "123 Main St",
            Phone = "+12345678901",
            DateOfBirth = DateTime.Now.AddYears(-30),
            Gender = Gender.Male
        };

        [Fact]
        public async Task PatientName_NotEmpty_ShouldNotHaveValidationError()
        {
            var result = await _validator.TestValidateAsync(ValidCommand());
            result.ShouldNotHaveValidationErrorFor(c => c.FullName);
        }

        [Fact]
        public async Task PatientName_Empty_ShouldHaveValidationError()
        {
            var command = ValidCommand();
            command.FullName = "";
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(c => c.FullName);
        }

        [Fact]
        public async Task Address_Empty_ShouldHaveValidationError()
        {
            var command = ValidCommand();
            command.Address = "";
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(c => c.Address);
        }

        [Fact]
        public async Task Phone_InvalidFormat_ShouldHaveValidationError()
        {
            var command = ValidCommand();
            command.Phone = "InvalidPhoneNumber";
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(c => c.Phone);
        }

        [Fact]
        public async Task DateOfBirth_InFuture_ShouldHaveValidationError()
        {
            var command = ValidCommand();
            command.DateOfBirth = DateTime.Now.AddYears(1);
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(c => c.DateOfBirth);
        }

        [Fact]
        public async Task All_ValidFields_ShouldNotHaveAnyValidationErrors()
        {
            _mockDoctorRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Doctor, bool>>>()))
                .ReturnsAsync(new List<Doctor>());
            _mockPatientRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Patient, bool>>>()))
                .ReturnsAsync(new List<Patient>());

            var result = await _validator.TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Phone_WhenAlreadyExistsInPatient_ShouldHaveValidationError()
        {
            var command = new CreatePatientCommand { Phone = "01507489484" };
            _mockDoctorRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Doctor, bool>>>()))
                .ReturnsAsync(new List<Doctor>());
            _mockPatientRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Patient, bool>>>()))
                .ReturnsAsync(new List<Patient> { new Patient() });

            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(c => c.Phone)
                .WithErrorMessage("Phone number is already exists");
        }

        [Fact]
        public async Task Phone_WhenNotExistsInSystem_ShouldNotHaveValidationError()
        {
            var command = new CreatePatientCommand { Phone = "01507489484" };
            _mockDoctorRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Doctor, bool>>>()))
                .ReturnsAsync(new List<Doctor>());
            _mockPatientRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Patient, bool>>>()))
                .ReturnsAsync(new List<Patient>());

            var result = await _validator.TestValidateAsync(command);
            result.ShouldNotHaveValidationErrorFor(c => c.Phone);
        }

        [Fact]
        public async Task Email_Omitted_ShouldNotHaveValidationError()
        {
            var command = ValidCommand();
            command.Email = " ";
            _mockDoctorRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Doctor, bool>>>()))
                .ReturnsAsync(new List<Doctor>());
            _mockPatientRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Patient, bool>>>()))
                .ReturnsAsync(new List<Patient>());

            var result = await _validator.TestValidateAsync(command);
            result.ShouldNotHaveValidationErrorFor(c => c.Email);
        }

        [Fact]
        public async Task Email_InvalidSyntax_ShouldHaveValidationError()
        {
            var command = ValidCommand();
            command.Email = "no-es-correo";
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(c => c.Email);
        }

        [Fact]
        public async Task Email_InvalidDomain_ShouldHaveValidationError()
        {
            var command = ValidCommand();
            command.Email = "paciente@localhost";
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(c => c.Email);
        }

        [Fact]
        public async Task Email_Valid_ShouldNotHaveValidationError()
        {
            var command = ValidCommand();
            command.Email = "paciente@gmail.com";
            _mockDoctorRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Doctor, bool>>>()))
                .ReturnsAsync(new List<Doctor>());
            _mockPatientRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Patient, bool>>>()))
                .ReturnsAsync(new List<Patient>());

            var result = await _validator.TestValidateAsync(command);
            result.ShouldNotHaveValidationErrorFor(c => c.Email);
        }
    }
}
