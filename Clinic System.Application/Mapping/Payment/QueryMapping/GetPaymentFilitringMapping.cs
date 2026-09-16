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
                .ForMember(dest => dest.Kind, opt => opt.MapFrom(src => src.Kind.ToString()))
                .ForMember(dest => dest.KindDisplay, opt => opt.MapFrom(src => BillingCopy.ReceiptKind(src.Kind)))
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
                .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Subtotal))
                .ForMember(dest => dest.SubtotalDisplay, opt => opt.MapFrom(src => Money.Format(src.Subtotal)))
                .ForMember(dest => dest.DiscountAmount, opt => opt.MapFrom(src => src.DiscountAmount))
                .ForMember(dest => dest.DiscountAmountDisplay, opt => opt.MapFrom(src => Money.Format(src.DiscountAmount)))
                .ForMember(dest => dest.AmountCollected, opt => opt.MapFrom(src => src.AmountCollected))
                .ForMember(dest => dest.AmountCollectedDisplay, opt => opt.MapFrom(src => Money.Format(src.AmountCollected)))
                .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => src.Balance))
                .ForMember(dest => dest.BalanceDisplay, opt => opt.MapFrom(src => Money.Format(src.Balance)))
                .ForMember(dest => dest.BalanceRaw, opt => opt.MapFrom(src => Money.ToInput(src.Balance)))
                .ForMember(dest => dest.TreatmentPlanId, opt => opt.MapFrom(src => src.TreatmentPlanId))
                .ForMember(dest => dest.TreatmentPlanTitle, opt => opt.MapFrom(src => src.TreatmentPlan != null ? src.TreatmentPlan.Title : null))

                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src =>
                    src.Patient != null ? src.Patient.FullName : src.Appointment.Patient.FullName))
                .ForMember(dest => dest.PatientId, opt => opt.MapFrom(src => src.PatientId))
                .ForMember(dest => dest.PatientPhone, opt => opt.MapFrom(src =>
                    src.Patient != null
                        ? (src.Patient.MobilePhone ?? src.Patient.Phone)
                        : (src.Appointment.Patient.MobilePhone ?? src.Appointment.Patient.Phone)))
                .ForMember(dest => dest.PatientNationalId, opt => opt.MapFrom(src =>
                    src.Patient != null ? src.Patient.NationalId : src.Appointment.Patient.NationalId))

                .ForMember(dest => dest.DoctorId, opt => opt.MapFrom(src => src.Appointment != null ? src.Appointment.DoctorId : 0))
                .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src =>
                    src.Appointment != null ? src.Appointment.Doctor.FullName : "—"))
                .ForMember(dest => dest.DoctorSpecialization, opt => opt.MapFrom(src =>
                    src.Appointment != null ? src.Appointment.Doctor.Specialization : null))
                .ForMember(dest => dest.AppointmentId, opt => opt.MapFrom(src => src.AppointmentId ?? 0))
                .ForMember(dest => dest.AppointmentDate, opt => opt.MapFrom(src =>
                    src.Appointment != null ? src.Appointment.AppointmentDate.ToString("yyyy-MM-dd HH:mm") : null))

                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod.ToString()))
                .ForMember(dest => dest.PaymentMethodDisplay, opt => opt.MapFrom(src => BillingCopy.Method(src.PaymentMethod)))
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.PaymentStatus.ToString()))
                .ForMember(dest => dest.PaymentStatusDisplay, opt => opt.MapFrom(src => BillingCopy.Status(src.PaymentStatus)));
        }
    }
}
