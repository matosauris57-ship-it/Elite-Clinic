namespace Clinic_System.Application.Tests.Features.Patients.CommandsTests.HandlersTests
{
    public class CreatePatientCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPatientService> _mockPatientService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<CreatePatientCommandHandler>> _mockLogger;
        private readonly CreatePatientCommandHandler _handler;

        public CreatePatientCommandHandlerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockPatientService = new Mock<IPatientService>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<CreatePatientCommandHandler>>();
            _handler = new CreatePatientCommandHandler(
                _mockPatientService.Object,
                _mockMapper.Object,
                _mockUnitOfWork.Object,
                _mockLogger.Object);
        }

        private static CreatePatientCommand Command() => new()
        {
            FullName = "Juan Perez",
            Gender = Gender.Male,
            DateOfBirth = new DateTime(1980, 1, 1),
            Phone = "01507489484",
            Address = "123 Main"
        };

        [Fact]
        public async Task Handle_ShouldCreatePatientWithoutUserAccount()
        {
            var command = Command();
            var patientEntity = new Patient { Id = 1, FullName = command.FullName };
            var expectedDto = new CreatePatientDTO { Id = 1, FullName = command.FullName };

            _mockMapper.Setup(m => m.Map<Patient>(command)).Returns(patientEntity);
            _mockMapper.Setup(m => m.Map<CreatePatientDTO>(patientEntity)).Returns(expectedDto);
            _mockUnitOfWork.Setup(u => u.SaveAsync()).ReturnsAsync(1);

            var result = await _handler.Handle(command, CancellationToken.None);

            _mockPatientService.Verify(s => s.CreatePatientAsync(patientEntity, It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Once);
            patientEntity.ApplicationUserId.Should().BeNull();
            result.Succeeded.Should().BeTrue();
            result.Message.Should().Be("Created");
        }

        [Fact]
        public async Task Handle_SaveAsyncReturnsZero_ShouldReturnBadRequest()
        {
            var command = Command();
            var patientEntity = new Patient { Id = 1, FullName = command.FullName };
            _mockMapper.Setup(m => m.Map<Patient>(command)).Returns(patientEntity);
            _mockUnitOfWork.Setup(u => u.SaveAsync()).ReturnsAsync(0);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.False(result.Succeeded);
            Assert.Equal("Failed to create patient", result.Message);
        }

        [Fact]
        public async Task Handle_CreatePatientServiceThrowsException_ShouldReturnBadRequest()
        {
            var command = Command();
            var patientEntity = new Patient { Id = 1, FullName = command.FullName };
            _mockMapper.Setup(m => m.Map<Patient>(command)).Returns(patientEntity);
            _mockPatientService
                .Setup(c => c.CreatePatientAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database connection timeout"));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.False(result.Succeeded);
            Assert.Contains("database connection", result.Message.ToLower());
            _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Never);
        }
    }
}
