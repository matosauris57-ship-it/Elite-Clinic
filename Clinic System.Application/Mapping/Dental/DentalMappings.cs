using Clinic_System.Core.Finance;

namespace Clinic_System.Application.Mapping.Dental
{
    public partial class DentalProfile
    {
        public void DentalHistoryMapping()
        {
            CreateMap<DentalHistory, DentalHistoryDTO>();
        }

        public void ToothRecordMapping()
        {
            CreateMap<ToothRecord, ToothRecordDTO>()
                .ForMember(d => d.DiagnosisCondition, o => o.MapFrom(s => s.DiagnosisCondition.ToString()))
                .ForMember(d => d.TreatmentCondition, o => o.MapFrom(s => s.TreatmentCondition.HasValue ? s.TreatmentCondition.Value.ToString() : null));
        }

        public void DentalTreatmentMapping()
        {
            CreateMap<DentalTreatment, DentalTreatmentDTO>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));
        }

        public void TreatmentPlanMapping()
        {
            CreateMap<PlanItem, PlanItemDTO>()
                .ForMember(d => d.LineTotal, o => o.MapFrom(s => s.LineTotal))
                .ForMember(d => d.UnitPriceDisplay, o => o.MapFrom(s => Money.Format(s.UnitPrice)))
                .ForMember(d => d.LineTotalDisplay, o => o.MapFrom(s => Money.Format(s.LineTotal)))
                .ForMember(d => d.AcceptanceStatus, o => o.MapFrom(s => s.AcceptanceStatus.ToString()))
                .ForMember(d => d.ExecutionStatus, o => o.MapFrom(s => s.ExecutionStatus.ToString()));

            CreateMap<TreatmentPlan, TreatmentPlanDTO>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.TotalAmount, o => o.MapFrom(s => s.TotalAmount))
                .ForMember(d => d.FinalAmount, o => o.MapFrom(s => s.FinalAmount))
                .ForMember(d => d.TotalAmountDisplay, o => o.MapFrom(s => Money.Format(s.TotalAmount)))
                .ForMember(d => d.DiscountAmountDisplay, o => o.MapFrom(s => Money.Format(s.DiscountAmount)))
                .ForMember(d => d.FinalAmountDisplay, o => o.MapFrom(s => Money.Format(s.FinalAmount)))
                .ForMember(d => d.CanInvoice, o => o.MapFrom(s => s.CanInvoice))
                .ForMember(d => d.AmountBilled, o => o.MapFrom(s => s.AmountBilled))
                .ForMember(d => d.AmountBilledDisplay, o => o.MapFrom(s => Money.Format(s.AmountBilled)))
                .ForMember(d => d.AmountCollectedOnAccount, o => o.MapFrom(s => s.AmountCollectedOnAccount))
                .ForMember(d => d.AmountCollectedOnAccountDisplay, o => o.MapFrom(s => Money.Format(s.AmountCollectedOnAccount)))
                .ForMember(d => d.RemainingToBill, o => o.MapFrom(s => s.RemainingToBill))
                .ForMember(d => d.RemainingToBillDisplay, o => o.MapFrom(s => Money.Format(s.RemainingToBill)))
                .ForMember(d => d.RemainingToCollect, o => o.MapFrom(s => s.RemainingToCollect))
                .ForMember(d => d.RemainingToCollectDisplay, o => o.MapFrom(s => Money.Format(s.RemainingToCollect)))
                .ForMember(d => d.UnbilledCompletedAmount, o => o.MapFrom(s => s.UnbilledCompletedAmount))
                .ForMember(d => d.UnbilledCompletedAmountDisplay, o => o.MapFrom(s => Money.Format(s.UnbilledCompletedAmount)))
                .ForMember(d => d.InvoicePaymentIds, o => o.MapFrom(s => s.InvoicePaymentIds.ToList()))
                .ForMember(d => d.PatientName, o => o.MapFrom(s => s.Patient != null ? s.Patient.FullName : string.Empty))
                .ForMember(d => d.PatientPhone, o => o.MapFrom(s =>
                    s.Patient != null ? (s.Patient.MobilePhone ?? s.Patient.Phone) : null))
                .ForMember(d => d.PatientNationalId, o => o.MapFrom(s =>
                    s.Patient != null ? s.Patient.NationalId : null));
        }

        public void InvoiceLineMapping()
        {
            CreateMap<InvoiceLine, InvoiceLineDTO>()
                .ForMember(d => d.LineTotal, o => o.MapFrom(s => s.LineTotal))
                .ForMember(d => d.UnitPriceDisplay, o => o.MapFrom(s => Money.Format(s.UnitPrice)))
                .ForMember(d => d.LineTotalDisplay, o => o.MapFrom(s => Money.Format(s.LineTotal)))
                .ForMember(d => d.MetaDisplay, o => o.MapFrom(s => FormatInvoiceLineMeta(s)));
        }

        public void TreatmentProcedureMapping()
        {
            CreateMap<TreatmentProcedure, TreatmentProcedureDTO>()
                .ForMember(d => d.PricingMode, o => o.MapFrom(s => s.PricingMode.ToString()))
                .ForMember(d => d.PriceDisplay, o => o.MapFrom(s => Money.Format(s.Price)))
                .ForMember(d => d.PriceRaw, o => o.MapFrom(s => Money.ToInput(s.Price)))
                .ForMember(d => d.PriceRangeDisplay, o => o.Ignore())
                .ForMember(d => d.PricingModeDisplay, o => o.Ignore())
                .ForMember(d => d.DoctorPrices, o => o.Ignore());
        }

        public void DentalTreatmentListItemMapping()
        {
            CreateMap<DentalTreatment, DentalTreatmentListItemDTO>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.PatientName, o => o.MapFrom(s => s.Patient.FullName));
        }

        private static string FormatInvoiceLineMeta(InvoiceLine line)
        {
            var parts = new List<string> { $"{line.Quantity} × {Money.Format(line.UnitPrice)}" };
            if (line.ToothNumber.HasValue)
                parts.Add($"Pieza {line.ToothNumber.Value}");
            if (line.DentalTreatmentId.HasValue)
                parts.Add($"Tratamiento #{line.DentalTreatmentId.Value}");
            return string.Join(" · ", parts);
        }
    }
}
