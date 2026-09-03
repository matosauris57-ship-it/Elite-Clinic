namespace Clinic_System.Application.Tests.Features.AppointmentsTests.CommandsTests.HandlersTests
{
    public class BookAppointmentCommandHandlerTests
    {
        private readonly Mock<ICurrentUserService> _mockCurrentUserService;

        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IAppointmentService> _mockAppointmentService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IDoctorService> _mockDoctorService;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<ILogger<BookAppointmentCommandHandler>> _mockLogger;
        private readonly Mock<INotificationsService> _mockNotificationsService;
        private readonly BookAppointmentCommandHandler _handler;
        public BookAppointmentCommandHandlerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockAppointmentService = new Mock<IAppointmentService>();
            _mockMapper = new Mock<IMapper>();
            _mockDoctorService = new Mock<IDoctorService>();
            _mockCacheService = new Mock<ICacheService>();
            _mockNotificationsService = new Mock<INotificationsService>();
            _mockLogger = new Mock<ILogger<BookAppointmentCommandHandler>>();
            _mockCurrentUserService = new Mock<ICurrentUserService>();
            _mockCurrentUserService.Setup(s => s.IsAdmin).Returns(true);
            _handler = new BookAppointmentCommandHandler(
                 _mockCurrentUserService.Object, _mockAppointmentService.Object,
                _mockMapper.Object, _mockDoctorService.Object, _mockCacheService.Object, _mockUnitOfWork.Object,
                _mockLogger.Object, _mockNotificationsService.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_ShouldBookAppointmentSucceeded()
        {
            // Arrange
            var doctorId = 1;
            var patientId = 1;
            var command = new BookAppointmentCommand { DoctorId = doctorId, PatientId = patientId, AppointmentDate = DateTime.Now.AddDays(1) };
            // تأكد أن الـ Entity والـ DTO متطابقان في السيناريو
            var appointmentEntity = new Appointment { Id = 100, DoctorId = doctorId, PatientId = patientId };
            var expectedDto = new AppointmentDTO { Id = 100 };

            // 1. إعداد الـ Service ليعيد الـ Entity (بدلاً من الافتراضي Null)
            _mockAppointmentService
                .Setup(s => s.BookAppointmentAsync(command.PatientId,command.DoctorId,command.AppointmentDate,command.AppointmentTime, It.IsAny<CancellationToken>(), It.IsAny<int?>(), It.IsAny<decimal?>()))
                .ReturnsAsync(appointmentEntity);

            // 2. إعداد الـ Repositories لتعيد بيانات الطبيب والمريض
            _mockUnitOfWork.Setup(u => u.DoctorsRepository.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Doctor { Id = doctorId, FullName = "Dr. Smith" });

            _mockUnitOfWork.Setup(u => u.PatientsRepository.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Patient { Id = patientId, FullName = "John Doe" });

            // 3. إعداد الـ Mapper
            _mockMapper.Setup(m => m.Map<AppointmentDTO>(appointmentEntity)).Returns(expectedDto);

            // 4. إعداد الحفظ
            _mockUnitOfWork.Setup(u => u.SaveAsync()).ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockAppointmentService.Verify(s => s.BookAppointmentAsync(command.PatientId, command.DoctorId, command.AppointmentDate, command.AppointmentTime, It.IsAny<CancellationToken>(), It.IsAny<int?>(), It.IsAny<decimal?>()), Times.Once);


            Assert.True(result.Succeeded);
            Assert.Equal(HttpStatusCode.Created, result.StatusCode);

        }

        [Theory]
        [InlineData("Doctor is not available at this time", "No se pudo")]
        [InlineData("Patient already has an appointment", "No se pudo")]
        [InlineData("Doctor is on vacation", "No se pudo")]
        public async Task Handle_InValidCommand_ShouldReturnBadRequest(string exceptionMessage, string expectedInResponse)
        {
            // Arrange
            var command = new BookAppointmentCommand
            {
                DoctorId = 1,
                PatientId = 1,
                AppointmentDate = DateTime.Now.AddDays(1)
            };

            _mockUnitOfWork.Setup(u => u.PatientsRepository.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new Patient { Id = 1 });

            // محاكاة رمي استثناء برسائل مختلفة بناءً على الـ InlineData
            _mockAppointmentService
                .Setup(s => s.BookAppointmentAsync(command.PatientId, command.DoctorId, command.AppointmentDate, command.AppointmentTime, It.IsAny<CancellationToken>(), It.IsAny<int?>(), It.IsAny<decimal?>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            // 1. التأكد أن العملية فشلت
            Assert.False(result.Succeeded);

            // 2. التأكد أن رسالة الخطأ تحتوي على الجزء المطلوب (Case-Insensitive check)
            Assert.Contains(expectedInResponse.ToLower(), result.Message.ToLower());

            // 3. التأكد من حماية قاعدة البيانات: عدم استدعاء الحفظ عند حدوث خطأ في الخدمة
            _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_ServiceReturnsNull_ShouldReturnBadRequest()
        {
            // Arrange
            var command = new BookAppointmentCommand { PatientId = 999 };

            // محاكاة إرجاع null عند البحث عن المريض
            _mockUnitOfWork.Setup(u => u.PatientsRepository.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Patient)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("No se pudo agendar la cita. Inténtelo nuevamente.", result.Message);

            _mockAppointmentService.Verify(
                s => s.BookAppointmentAsync(
                    command.PatientId,
                    command.DoctorId,
                    command.AppointmentDate,
                    command.AppointmentTime,
                    It.IsAny<CancellationToken>(), It.IsAny<int?>(), It.IsAny<decimal?>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_StaffWithCreatePermission_CanBookForPatient()
        {
            _mockCurrentUserService.Setup(s => s.IsAdmin).Returns(false);
            _mockCurrentUserService
                .Setup(s => s.HasPermission("agendar-cita.create"))
                .Returns(true);

            var command = new BookAppointmentCommand
            {
                PatientId = 7,
                DoctorId = 3,
                AppointmentDate = DateTime.Today.AddDays(1),
                AppointmentTime = new TimeSpan(15, 0, 0)
            };
            var appointment = new Appointment
            {
                Id = 101,
                PatientId = command.PatientId,
                DoctorId = command.DoctorId,
                AppointmentDate = command.AppointmentDate.Add(command.AppointmentTime)
            };

            _mockAppointmentService
                .Setup(s => s.BookAppointmentAsync(
                    command.PatientId,
                    command.DoctorId,
                    command.AppointmentDate,
                    command.AppointmentTime,
                    It.IsAny<CancellationToken>(), It.IsAny<int?>(), It.IsAny<decimal?>()))
                .ReturnsAsync(appointment);
            _mockMapper.Setup(m => m.Map<AppointmentDTO>(appointment))
                .Returns(new AppointmentDTO { Id = appointment.Id });

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.Succeeded);
            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        }

        [Fact]
        public async Task Handle_StaffWithoutCreatePermission_IsDenied()
        {
            _mockCurrentUserService.Setup(s => s.IsAdmin).Returns(false);
            _mockCurrentUserService
                .Setup(s => s.HasPermission("agendar-cita.create"))
                .Returns(false);

            var command = new BookAppointmentCommand
            {
                PatientId = 7,
                DoctorId = 3,
                AppointmentDate = DateTime.Today.AddDays(1),
                AppointmentTime = new TimeSpan(15, 0, 0)
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.False(result.Succeeded);
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
            _mockAppointmentService.Verify(
                s => s.BookAppointmentAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>(), It.IsAny<int?>(), It.IsAny<decimal?>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_PatientCannotBookForAnotherPatient()
        {
            _mockCurrentUserService.Setup(s => s.IsAdmin).Returns(false);
            _mockCurrentUserService.Setup(s => s.PatientId).Returns(4);

            var command = new BookAppointmentCommand
            {
                PatientId = 7,
                DoctorId = 3,
                AppointmentDate = DateTime.Today.AddDays(1),
                AppointmentTime = new TimeSpan(15, 0, 0)
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.False(result.Succeeded);
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task Handle_PatientCanBookForOwnProfile()
        {
            _mockCurrentUserService.Setup(s => s.IsAdmin).Returns(false);
            _mockCurrentUserService.Setup(s => s.PatientId).Returns(4);

            var command = new BookAppointmentCommand
            {
                PatientId = 4,
                DoctorId = 3,
                AppointmentDate = DateTime.Today.AddDays(1),
                AppointmentTime = new TimeSpan(15, 0, 0)
            };
            var appointment = new Appointment
            {
                Id = 102,
                PatientId = command.PatientId,
                DoctorId = command.DoctorId,
                AppointmentDate = command.AppointmentDate.Add(command.AppointmentTime)
            };
            _mockAppointmentService
                .Setup(s => s.BookAppointmentAsync(
                    command.PatientId,
                    command.DoctorId,
                    command.AppointmentDate,
                    command.AppointmentTime,
                    It.IsAny<CancellationToken>(), It.IsAny<int?>(), It.IsAny<decimal?>()))
                .ReturnsAsync(appointment);
            _mockMapper.Setup(m => m.Map<AppointmentDTO>(appointment))
                .Returns(new AppointmentDTO { Id = appointment.Id });

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.Succeeded);
            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        }

        [Fact]
        public async Task Handle_SlotAlreadyBooked_ReturnsClearBadRequest()
        {
            var command = new BookAppointmentCommand
            {
                PatientId = 4,
                DoctorId = 3,
                AppointmentDate = DateTime.Today.AddDays(1),
                AppointmentTime = new TimeSpan(15, 0, 0)
            };
            _mockAppointmentService
                .Setup(s => s.BookAppointmentAsync(
                    command.PatientId,
                    command.DoctorId,
                    command.AppointmentDate,
                    command.AppointmentTime,
                    It.IsAny<CancellationToken>(), It.IsAny<int?>(), It.IsAny<decimal?>()))
                .ThrowsAsync(new SlotAlreadyBookedException("slot unavailable"));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.False(result.Succeeded);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Contains("ya no está disponible", result.Message);
        }
    }
}
