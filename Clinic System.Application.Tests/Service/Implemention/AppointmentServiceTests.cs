namespace Clinic_System.Application.Tests.Service.Implemention
{
    public class AppointmentServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IDistributedLockService> _mockLock;
        private readonly Mock<IPaymentService> _mockPaymentService;
        private readonly Mock<IMedicalRecordService> _mockMedicalRecordService;
        private readonly Mock<IAppointmentRepository> _mockAppointmentRepository;
        private readonly Mock<ILogger<AppointmentService>> _mockLogger;
        private readonly Mock<IClinicOperatingHoursService> _mockHours;
        private readonly Mock<IMessagePublisher> _mockPublisher;
        private readonly AppointmentService _appointmentService;


        public AppointmentServiceTests()
        {
            _mockHours = new Mock<IClinicOperatingHoursService>();
            _mockHours.Setup(h => h.GetAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ClinicOperatingHours
                {
                    OpenTime = new TimeSpan(12, 0, 0),
                    CloseTime = new TimeSpan(22, 0, 0),
                    SlotDurationMinutes = 15,
                    WorkingDays = [0, 1, 2, 3, 4, 5, 6]
                });
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLock = new Mock<IDistributedLockService>();
            _mockLock.Setup(l => l.AcquireLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync(true);
            _mockAppointmentRepository = new Mock<IAppointmentRepository>();
            _mockPaymentService = new Mock<IPaymentService>();
            _mockMedicalRecordService = new Mock<IMedicalRecordService>();
            _mockLogger = new Mock<ILogger<AppointmentService>>();
            _mockPublisher = new Mock<IMessagePublisher>();
            _mockUnitOfWork.SetupGet(u => u.AppointmentsRepository).Returns(_mockAppointmentRepository.Object);
            _appointmentService = new AppointmentService(_mockPublisher.Object,_mockUnitOfWork.Object, _mockPaymentService.Object,
               _mockMedicalRecordService.Object, _mockLogger.Object,_mockHours.Object,_mockLock.Object);
        }

        [Fact]
        public async Task BookAppointmentAsync_SlotAvailable_SavesAndReturnsAppointment()
        {
            // Arrange
            var command = new BookAppointmentCommand
            {
                DoctorId = 1,
                PatientId = 1,
                AppointmentDate = DateTime.Today.AddDays(1),
                AppointmentTime = new TimeSpan(12, 0, 0) // 12:00 PM
            };

            _mockAppointmentRepository
                .Setup(r => r.GetBookedAppointmentsAsync(
                    command.DoctorId,
                    command.AppointmentDate,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Appointment>()); // No booked appointments
       
            _mockUnitOfWork
                .Setup(u => u.SaveAsync())
                .ReturnsAsync(1);

            _mockAppointmentRepository
                .Setup(r => r.GetAppointmentWithDetailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Appointment
                {
                    Id = 1,
                    DoctorId = 1,
                    PatientId = 1,
                    Status = AppointmentStatus.Pending,
                    Patient = new Patient { FullName = "Paciente", ApplicationUserId = "u1" },
                    Doctor = new Doctor { FullName = "Doctor", Specialization = "General" }
                });

            // Act
            var result = await _appointmentService.BookAppointmentAsync(command.PatientId, command.DoctorId, command.AppointmentDate, command.AppointmentTime, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(AppointmentStatus.Pending);

            _mockAppointmentRepository.Verify(r => r.AddAsync(
                 It.Is<Appointment>(a => a.DoctorId == command.DoctorId),
                 It.IsAny<CancellationToken>()), Times.Once);
           
            _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task BookAppointmentAsync_SaveFails_ThrowsException()
        {
            // Arrange
            var command = new BookAppointmentCommand
            {
                DoctorId = 1,
                PatientId = 1,
                AppointmentDate = DateTime.Today.AddDays(1),
                AppointmentTime = new TimeSpan(12, 0, 0) // 12:00 PM
            };

            _mockAppointmentRepository
                .Setup(r => r.GetBookedAppointmentsAsync(
                    command.DoctorId,
                    command.AppointmentDate,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Appointment>()); // No booked appointments

            _mockUnitOfWork
                .Setup(u => u.SaveAsync())
                .ReturnsAsync(0);

            // Act

            // Assert
            await Assert.ThrowsAsync<DatabaseSaveException>(async () =>
            {
                await _appointmentService.BookAppointmentAsync(command.PatientId,command.DoctorId
                    ,command.AppointmentDate,command.AppointmentTime, CancellationToken.None);
            });


            _mockAppointmentRepository.Verify(r => r.AddAsync(
                 It.IsAny<Appointment>(),
                 It.IsAny<CancellationToken>()), Times.Once); // التأكد من استدعاء AddAsync

            _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Once); // التأكد من استدعاء SaveAsync
        }

        [Fact]
        public async Task BookAppointmentAsync_SlotAlreadyBooked_ThrowsException()
        {
            // Arrange
            var command = new BookAppointmentCommand
            {
                DoctorId = 1,
                PatientId = 1,
                AppointmentDate = DateTime.Today.AddDays(1),
                AppointmentTime = new TimeSpan(12, 0, 0) // 12:00 PM
            };

            var bookedAppointments = new List<Appointment>
            {
                new Appointment { 
                    AppointmentDate = command.AppointmentDate.Date.Add(command.AppointmentTime),
                    DoctorId = command.DoctorId
                } // Slot already booked
            };

            _mockAppointmentRepository
                .Setup(r => r.GetBookedAppointmentsAsync(
                    command.DoctorId,
                    command.AppointmentDate,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookedAppointments);

            // Act & Assert
            await Assert.ThrowsAsync<SlotAlreadyBookedException>(() => _appointmentService.BookAppointmentAsync(command.PatientId, command.DoctorId
                    , command.AppointmentDate, command.AppointmentTime, CancellationToken.None));

            _mockAppointmentRepository.Verify(r => r.AddAsync(
                 It.IsAny<Appointment>(),
                 It.IsAny<CancellationToken>()), Times.Never); // التأكد من عدم استدعاء AddAsync

            _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Never); // التأكد من عدم استدعاء SaveAsync
        }

        [Fact]
        public async Task GetAvailableSlotsAsync_OneSlotBooked_ReturnsCorrectAvailableSlots()
        {
            // Arrange
            int doctorId = 1;
            DateTime date = DateTime.Today;
            var bookedAppointments = new List<Appointment>
            {
                new Appointment { AppointmentDate = date.AddHours(12).AddMinutes(15) }, // 12:15 PM
            };


            _mockAppointmentRepository.Setup(u => u
                .GetBookedAppointmentsAsync(doctorId, date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookedAppointments);

            // المواعيد المتوقعة (بداية من 12:15:00، لأن 12:00:00 محجوز)
            var expectedStartTime = new TimeSpan(12, 0, 0);
            var expectedEndTime = new TimeSpan(22, 0, 0);

            // Act
            var availableSlots = await _appointmentService.GetAvailableSlotsAsync(doctorId, date);

            // Assert
            // 1. التأكد من أن الموعد المحجوز قد أُزيل
            availableSlots.Should().NotContain(new TimeSpan(12, 15, 0));

            // 2. التأكد من أن القائمة تبدأ بالموعد التالي (12:15:00)
            availableSlots.First().Should().Be(expectedStartTime);

            // 3. التأكد من أن عدد الفترات صحيح: 
            // عدد الفترات الممكنة بين 12:00 و 22:00 (10 ساعات) هو 40 فترة (10 * 4).
            // إذا أزلنا فترة واحدة، يجب أن يكون العدد 39.
            availableSlots.Count.Should().Be(39);
        }

        [Fact]
        public async Task GetAvailableSlotsAsync_ClosedDay_ReturnsEmpty()
        {
            _mockHours.Setup(h => h.GetAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ClinicOperatingHours
                {
                    OpenTime = new TimeSpan(8, 0, 0),
                    CloseTime = new TimeSpan(17, 0, 0),
                    SlotDurationMinutes = 30,
                    WorkingDays = [(int)DayOfWeek.Monday]
                });

            var sunday = new DateTime(2026, 9, 6);
            _mockAppointmentRepository.Setup(u => u
                .GetBookedAppointmentsAsync(1, sunday, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Appointment>());

            var slots = await _appointmentService.GetAvailableSlotsAsync(1, sunday);

            slots.Should().BeEmpty();
        }
    }
}
