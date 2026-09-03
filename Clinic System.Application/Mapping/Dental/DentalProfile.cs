namespace Clinic_System.Application.Mapping.Dental
{
    public partial class DentalProfile : Profile
    {
        public DentalProfile()
        {
            DentalHistoryMapping();
            ToothRecordMapping();
            DentalTreatmentMapping();
            TreatmentPlanMapping();
            InvoiceLineMapping();
            TreatmentProcedureMapping();
            DentalTreatmentListItemMapping();
            CreateMap<ToothChartEntry, ToothChartEntryDTO>();
            CreateMap<DentalClinicalEvent, DentalClinicalEventDTO>()
                .ForMember(d => d.RecordedByUserName, o => o.Ignore());
        }
    }
}
