namespace Clinic_System.Application.Tests.Features.Patients.QueriesTests.HandlersTests
{
    public class PatientListPagingQueryHandlerTests
    {
        private readonly Mock<IPatientService> _mockPatientService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPaymentRepository> _mockPayments;
        private readonly Mock<ILogger<PatientListPagingQueryHandler>> _mockLogger;
        private readonly PatientListPagingQueryHandler _handler;

        public PatientListPagingQueryHandlerTests()
        {
            _mockPatientService = new Mock<IPatientService>();
            _mockMapper = new Mock<IMapper>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockPayments = new Mock<IPaymentRepository>();
            _mockLogger = new Mock<ILogger<PatientListPagingQueryHandler>>();

            _mockUnitOfWork.SetupGet(u => u.PaymentsRepository).Returns(_mockPayments.Object);
            _mockPayments
                .Setup(p => p.GetOutstandingBalancesByPatientAsync(It.IsAny<IEnumerable<int>?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<int, decimal>());

            _handler = new PatientListPagingQueryHandler(
                _mockPatientService.Object,
                _mockMapper.Object,
                _mockUnitOfWork.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnsPagedResult()
        {
            var request = new GetPatientListPagingQuery { PageNumber = 1, PageSize = 10, Status = "all" };
            var patients = new PagedResult<Patient>(
                new List<Patient>
                {
                    new() { Id = 1, FullName = "Ana Perez" },
                    new() { Id = 2, FullName = "Luis Gomez" }
                },
                totalCount: 2,
                currentPage: 1,
                pageSize: 10);

            _mockPatientService
                .Setup(s => s.GetPatientsListPagingAsync(1, 10, null, "all", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(patients);
            _mockMapper.Setup(m => m.Map<List<GetPatientListDTO>>(It.IsAny<List<Patient>>()))
                .Returns(
                [
                    new GetPatientListDTO { Id = 1, FullName = "Ana Perez" },
                    new GetPatientListDTO { Id = 2, FullName = "Luis Gomez" }
                ]);

            var response = await _handler.Handle(request, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.NotNull(response.Data);
            Assert.Equal(2, response.Data.TotalCount);
            Assert.Equal(2, response.Data.Items.Count());
        }

        [Fact]
        public async Task Handle_NoPatientsFound_ReturnsEmptySuccess()
        {
            var request = new GetPatientListPagingQuery { PageNumber = 1, PageSize = 10 };
            var patients = new PagedResult<Patient>([], totalCount: 0, currentPage: 1, pageSize: 10);

            _mockPatientService
                .Setup(s => s.GetPatientsListPagingAsync(1, 10, null, "all", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(patients);
            _mockMapper.Setup(m => m.Map<List<GetPatientListDTO>>(It.IsAny<List<Patient>>()))
                .Returns([]);

            var response = await _handler.Handle(request, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.NotNull(response.Data);
            Assert.Empty(response.Data.Items);
            Assert.Equal(0, response.Data.TotalCount);
        }

        [Fact]
        public async Task Handle_WithSearch_PassesSearchToService()
        {
            var request = new GetPatientListPagingQuery
            {
                PageNumber = 1,
                PageSize = 20,
                Status = "active",
                Search = "  Ana  "
            };
            var patients = new PagedResult<Patient>([], 0, 1, 20);

            _mockPatientService
                .Setup(s => s.GetPatientsListPagingAsync(1, 20, "Ana", "active", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(patients);
            _mockMapper.Setup(m => m.Map<List<GetPatientListDTO>>(It.IsAny<List<Patient>>()))
                .Returns([]);

            var response = await _handler.Handle(request, CancellationToken.None);

            Assert.True(response.Succeeded);
            _mockPatientService.Verify(
                s => s.GetPatientsListPagingAsync(1, 20, "Ana", "active", null, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
