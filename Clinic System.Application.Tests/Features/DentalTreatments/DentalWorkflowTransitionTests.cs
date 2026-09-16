using Clinic_System.Application.Features.DentalTreatments.Commands.Models;
using Clinic_System.Application.Features.DentalTreatments.Commands.Validators;

namespace Clinic_System.Application.Tests.Features.DentalTreatments;

public class DentalWorkflowTransitionTests
{
    [Fact]
    public void Treatment_RequiresStartBeforeCompletion()
    {
        var treatment = new DentalTreatment();

        var act = () => treatment.Complete();

        act.Should().Throw<InvalidOperationException>();
        treatment.Start();
        treatment.Complete();
        treatment.Status.Should().Be(DentalTreatmentStatus.Completed);
    }

    [Fact]
    public void Treatment_CannotRestartOrCancelAfterCompletion()
    {
        var treatment = new DentalTreatment();
        treatment.Start();
        treatment.Complete();

        ((Action)treatment.Start).Should().Throw<InvalidOperationException>();
        ((Action)(() => treatment.Cancel())).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Plan_FollowsDraftIssuedAcceptedCompletedTransition()
    {
        var plan = new TreatmentPlan();

        plan.Issue();
        plan.Status.Should().Be(TreatmentPlanStatus.Issued);
        plan.Approve("Ana Pérez");
        plan.Complete();

        plan.Status.Should().Be(TreatmentPlanStatus.Completed);
        plan.AcceptedByName.Should().Be("Ana Pérez");
        ((Action)(() => plan.Reject())).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Plan_BillableLines_ApplyDiscountToLastItems()
    {
        var plan = new TreatmentPlan
        {
            DiscountAmount = 100,
            Items =
            [
                new PlanItem { ProcedureName = "Corona", Quantity = 1, UnitPrice = 400 },
                new PlanItem { ProcedureName = "Limpieza", Quantity = 1, UnitPrice = 100 }
            ]
        };

        var lines = plan.ToBillableLines();

        lines.Should().HaveCount(2);
        lines.Sum(x => x.UnitPrice * x.Quantity).Should().Be(400);
        plan.FinalAmount.Should().Be(400);
    }

    [Fact]
    public void CompletionValidator_RejectsInvalidClinicalResultEnums()
    {
        var validator = new CompleteDentalTreatmentValidator();
        var command = new CompleteDentalTreatmentCommand
        {
            TreatmentId = 1,
            ClinicalResult = new DentalTreatmentClinicalResultInput
            {
                Surface = (ToothSurface)99,
                Condition = (ToothCondition)99
            }
        };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ClinicalResult!.Surface);
        result.ShouldHaveValidationErrorFor(x => x.ClinicalResult!.Condition);
    }

    [Fact]
    public async Task CreateTreatment_AddsSeparateTreatmentTimelineEvent()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        var patients = new Mock<IPatientRepository>();
        var treatments = new Mock<IDentalTreatmentRepository>();
        var events = new Mock<IDentalClinicalEventRepository>();
        patients.Setup(x => x.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Patient { Id = 7, FullName = "Paciente" });
        unitOfWork.SetupGet(x => x.PatientsRepository).Returns(patients.Object);
        unitOfWork.SetupGet(x => x.DentalTreatmentsRepository).Returns(treatments.Object);
        unitOfWork.SetupGet(x => x.DentalClinicalEventsRepository).Returns(events.Object);
        var service = new DentalTreatmentService(unitOfWork.Object);

        await service.CreateAsync(
            7, "Limpieza", 100, null, null, null, null, null, null, "user-1", CancellationToken.None);

        events.Verify(x => x.AddAsync(
            It.Is<DentalClinicalEvent>(e =>
                e.PatientId == 7 &&
                e.Type == DentalClinicalEventType.Treatment &&
                e.Title == "Tratamiento creado" &&
                e.RecordedByUserId == "user-1"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Appointment_StartConsultation_ThenComplete_DoesNotRequireInvoice()
    {
        var appointment = new Appointment { Status = AppointmentStatus.Pending };
        appointment.StartConsultation();
        appointment.Status.Should().Be(AppointmentStatus.InProgress);
        appointment.Complete();
        appointment.Status.Should().Be(AppointmentStatus.Completed);
    }

    [Fact]
    public void PlanApprove_LeavesEachItemPendingUntilAccepted()
    {
        var restoration = new PlanItem { ProcedureName = "Restauración 16", Quantity = 1, UnitPrice = 3500 };
        var cleaning = new PlanItem { ProcedureName = "Limpieza", Quantity = 1, UnitPrice = 2000 };
        var plan = new TreatmentPlan { Items = [restoration, cleaning] };

        plan.Approve("Paciente");

        restoration.AcceptanceStatus.Should().Be(PlanItemAcceptanceStatus.Pending);
        cleaning.AcceptanceStatus.Should().Be(PlanItemAcceptanceStatus.Pending);

        restoration.Accept();
        cleaning.Reject();

        restoration.AcceptanceStatus.Should().Be(PlanItemAcceptanceStatus.Approved);
        restoration.ExecutionStatus.Should().Be(PlanItemExecutionStatus.Pending);
        cleaning.AcceptanceStatus.Should().Be(PlanItemAcceptanceStatus.Rejected);
        plan.BillableCompletedItems.Should().BeEmpty();

        restoration.MarkCompleted();
        plan.UnbilledCompletedAmount.Should().Be(3500);
    }
}
