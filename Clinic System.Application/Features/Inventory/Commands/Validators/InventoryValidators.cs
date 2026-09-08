using Clinic_System.Application.Features.Inventory.Commands.Models;

namespace Clinic_System.Application.Features.Inventory.Commands.Validators
{
    public class CreateInventoryItemValidator : AbstractValidator<CreateInventoryItemCommand>
    {
        public CreateInventoryItemValidator()
        {
            RuleFor(x => x.Sku).NotEmpty().MaximumLength(80);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Category).NotEmpty().MaximumLength(80);
            RuleFor(x => x.Unit).NotEmpty().MaximumLength(20);
            RuleFor(x => x.MinimumStock).GreaterThanOrEqualTo(0);
            RuleFor(x => x.InitialQuantity).GreaterThanOrEqualTo(0);
        }
    }

    public class UpdateInventoryItemValidator : AbstractValidator<UpdateInventoryItemCommand>
    {
        public UpdateInventoryItemValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.Sku).NotEmpty().MaximumLength(80);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Category).NotEmpty().MaximumLength(80);
            RuleFor(x => x.Unit).NotEmpty().MaximumLength(20);
            RuleFor(x => x.MinimumStock).GreaterThanOrEqualTo(0);
        }
    }

    public class RegisterStockEntryValidator : AbstractValidator<RegisterStockEntryCommand>
    {
        public RegisterStockEntryValidator()
        {
            RuleFor(x => x.InventoryItemId).GreaterThan(0);
            RuleFor(x => x.Quantity).GreaterThan(0);
            RuleFor(x => x.Reason).MaximumLength(200);
            RuleFor(x => x.Notes).MaximumLength(500);
        }
    }

    public class RegisterStockAdjustmentValidator : AbstractValidator<RegisterStockAdjustmentCommand>
    {
        public RegisterStockAdjustmentValidator()
        {
            RuleFor(x => x.InventoryItemId).GreaterThan(0);
            RuleFor(x => x.NewQuantityOnHand).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Reason).MaximumLength(200);
            RuleFor(x => x.Notes).MaximumLength(500);
        }
    }

    public class ReplaceProcedureBomValidator : AbstractValidator<ReplaceProcedureBomCommand>
    {
        public ReplaceProcedureBomValidator()
        {
            RuleFor(x => x.TreatmentProcedureId).GreaterThan(0);
            RuleForEach(x => x.Materials).ChildRules(m =>
            {
                m.RuleFor(p => p.InventoryItemId).GreaterThan(0);
                m.RuleFor(p => p.DefaultQuantity).GreaterThan(0);
            });
        }
    }

    public class ConfirmMaterialConsumptionValidator : AbstractValidator<ConfirmMaterialConsumptionCommand>
    {
        public ConfirmMaterialConsumptionValidator()
        {
            RuleFor(x => x.DentalTreatmentId).GreaterThan(0);
            RuleForEach(x => x.Lines).ChildRules(l =>
            {
                l.RuleFor(p => p.InventoryItemId).GreaterThan(0);
                l.RuleFor(p => p.Quantity).GreaterThanOrEqualTo(0);
            });
        }
    }

    public class ReplaceMaterialConsumptionValidator : AbstractValidator<ReplaceMaterialConsumptionCommand>
    {
        public ReplaceMaterialConsumptionValidator()
        {
            RuleFor(x => x.DentalTreatmentId).GreaterThan(0);
            RuleFor(x => x.Lines).NotNull();
            RuleForEach(x => x.Lines).ChildRules(l =>
            {
                l.RuleFor(p => p.InventoryItemId).GreaterThan(0);
                l.RuleFor(p => p.Quantity).GreaterThanOrEqualTo(0);
            });
        }
    }
}
