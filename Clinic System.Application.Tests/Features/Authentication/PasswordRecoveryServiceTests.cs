using Clinic_System.Application.DTOs;
using Clinic_System.Application.DTOs.Authentications;

namespace Clinic_System.Application.Tests.Features.Authentication;

public class PasswordRecoveryServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IPasswordRecoveryRequestRepository> _repo = new();
    private readonly Mock<IIdentityService> _identity = new();
    private readonly Mock<INotificationsService> _notifications = new();
    private readonly PasswordRecoveryService _service;

    public PasswordRecoveryServiceTests()
    {
        _unitOfWork.SetupGet(u => u.PasswordRecoveryRequestsRepository).Returns(_repo.Object);
        _unitOfWork.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _service = new PasswordRecoveryService(_unitOfWork.Object, _identity.Object, _notifications.Object);
    }

    [Fact]
    public async Task SubmitAsync_CreatesRequestAndNotifiesStaff()
    {
        _repo.Setup(r => r.FindRecentPendingAsync("ANA@CLINIC.COM", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PasswordRecoveryRequest?)null);
        _identity.Setup(i => i.FindAccountForRecoveryAsync("ana@clinic.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PasswordRecoveryAccountMatch("u1", "ana@clinic.com", "ana", "Ana Maria", "Médico"));
        _repo.Setup(r => r.AddAsync(It.IsAny<PasswordRecoveryRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.SubmitAsync("ana@clinic.com", "No recuerdo la clave");

        result.UserMatched.Should().BeTrue();
        result.UserDisplayName.Should().Be("Ana Maria");
        result.Status.Should().Be(PasswordRecoveryStatus.Pending);
        _notifications.Verify(n => n.SendToGroupAsync("Admins", It.Is<NotificationDTO>(d => d.NotificationType == "PasswordRecoveryRequested")), Times.Once);
        _notifications.Verify(n => n.SendToGroupAsync("PasswordRecoveryStaff", It.IsAny<NotificationDTO>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAsync_ReusesPendingDuplicate()
    {
        var existing = new PasswordRecoveryRequest { Id = 4, Identifier = "ana@clinic.com", Status = PasswordRecoveryStatus.Pending };
        _repo.Setup(r => r.FindRecentPendingAsync("ANA@CLINIC.COM", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _service.SubmitAsync("ana@clinic.com", null);

        result.Id.Should().Be(4);
        _repo.Verify(r => r.AddAsync(It.IsAny<PasswordRecoveryRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        _notifications.Verify(n => n.SendToGroupAsync(It.IsAny<string>(), It.IsAny<NotificationDTO>()), Times.Never);
    }
}
