namespace Clinic_System.Application.DTOs.Payment
{
    public class PaymentDetailsDTO
    {
        public int PaymentId { get; set; }
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } // مهم عشان التقرير يكون مقروء
        public string? PatientPhone { get; set; }
        public string? PatientNationalId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string? DoctorSpecialization { get; set; }
        public string? AppointmentDate { get; set; }

        public decimal Amount { get; set; }
        public string AmountDisplay { get; set; } = string.Empty;
        public string AmountRaw { get; set; } = string.Empty;
        public decimal AmountCollected { get; set; }
        public string AmountCollectedDisplay { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string BalanceDisplay { get; set; } = string.Empty;
        public string BalanceRaw { get; set; } = string.Empty;
        public string PaymentMethod { get; set; }
        public string PaymentMethodDisplay { get; set; } = string.Empty;
        public string PaymentStatus { get; set; }
        public string PaymentStatusDisplay { get; set; } = string.Empty;
        public string PaymentDate { get; set; } // Formatted Date
        public string? Notes { get; set; }
        public List<PaymentReceiptDTO> Receipts { get; set; } = [];
    }
}
