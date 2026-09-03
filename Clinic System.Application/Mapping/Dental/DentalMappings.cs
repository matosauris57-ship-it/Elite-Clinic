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
                .ForMember(d => d.LineTotal, o => o.MapFrom(s => s.LineTotal));

            CreateMap<TreatmentPlan, TreatmentPlanDTO>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.TotalAmount, o => o.MapFrom(s => s.TotalAmount))
                .ForMember(d => d.FinalAmount, o => o.MapFrom(s => s.FinalAmount));
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
                .ForMember(d => d.PriceDisplay, o => o.MapFrom(s => Money.Format(s.Price)))
                .ForMember(d => d.PriceRaw, o => o.MapFrom(s => Money.ToInput(s.Price)))
                .ForMember(d => d.PriceRangeDisplay, o => o.Ignore())
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
