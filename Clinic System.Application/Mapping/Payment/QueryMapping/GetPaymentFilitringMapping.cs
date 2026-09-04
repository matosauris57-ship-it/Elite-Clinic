using Clinic_System.Application.Common;
using Clinic_System.Core.Finance;

namespace Clinic_System.Application.Mapping.Payments
{
    public partial class PaymentProfile
    {
        public void GetPaymentFilitringMapping()
        {
            CreateMap<PaymentReceipt, PaymentReceiptDTO>()
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod.ToString()))
                .ForMember(dest => dest.PaymentMethodDisplay, opt => opt.MapFrom(src => BillingCopy.Method(src.PaymentMethod)))
                .ForMember(dest => dest.AmountDisplay, opt => opt.MapFrom(src => Money.Format(src.Amount)))
                .ForMember(dest => dest.PaidAt, opt => opt.MapFrom(src => src.PaidAt.ToString("yyyy-MM-dd HH:mm")));

            CreateMap<Payment, PaymentDetailsDTO>()
                .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(src =>
                    (src.PaymentDate ?? src.CreatedAt).ToString("yyyy-MM-dd HH:mm")))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.AdditionalNotes ?? "N/A"))

                .ForMember(dest => dest.PaymentId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.InvoiceTotal))
                .ForMember(dest => dest.AmountDisplay, opt => opt.MapFrom(src => Money.Format(src.InvoiceTotal)))
                .ForMember(dest => dest.AmountRaw, opt => opt.MapFrom(src => Money.ToInput(src.InvoiceTotal)))
                .ForMember(dest => dest.AmountCollected, opt => opt.MapFrom(src => src.AmountCollected))
                .ForMember(dest => dest.AmountCollectedDisplay, opt => opt.MapFrom(src => Money.Format(src.AmountCollected)))
                .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => src.Balance))
                .ForMember(dest => dest.BalanceDisplay, opt => opt.MapFrom(src => Money.Format(src.Balance)))
                .ForMember(dest => dest.BalanceRaw, opt => opt.MapFrom(src => Money.ToInput(src.Balance)))

                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Appointment.Patient.FullName))
                .ForMember(dest => dest.PatientId, opt => opt.MapFrom(src => src.Appointment.PatientId))
                .ForMember(dest => dest.PatientPhone, opt => opt.MapFrom(src =>
                    src.Appointment.Patient.MobilePhone ?? src.Appointment.Patient.Phone))
                .ForMember(dest => dest.PatientNationalId, opt => opt.MapFrom(src => src.Appointment.Patient.NationalId))

                .ForMember(dest => dest.DoctorId, opt => opt.MapFrom(src => src.Appointment.DoctorId))
                .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Appointment.Doctor.FullName))
                .ForMember(dest => dest.DoctorSpecialization, opt => opt.MapFrom(src => src.Appointment.Doctor.Specialization))
                .ForMember(dest => dest.AppointmentDate, opt => opt.MapFrom(src =>
                    src.Appointment.AppointmentDate.ToString("yyyy-MM-dd HH:mm")))

                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod.ToString()))
                .ForMember(dest => dest.PaymentMethodDisplay, opt => opt.MapFrom(src => BillingCopy.Method(src.PaymentMethod)))
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.PaymentStatus.ToString()))
                .ForMember(dest => dest.PaymentStatusDisplay, opt => opt.MapFrom(src => BillingCopy.Status(src.PaymentStatus)));
        }
    }
}
