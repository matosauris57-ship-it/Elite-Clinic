namespace DentalCare.Admin.Models;

public class ApiResponse<T>
{
    public bool Succeeded { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
    public T? Data { get; set; }
}

public class LoginRequest
{
    public string EmailOrUserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool ForAdminPanel { get; set; }
}

public class LoginResponse
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string ExpiresAt { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public List<string> Permissions { get; set; } = [];
}

public class RefreshTokenRequest
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public class JwtAuthResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string ExpiresAt { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = [];
}

public class AppointmentStats
{
    public int TotalAppointments { get; set; }
    public int Completed { get; set; }
    public int Pending { get; set; }
    public int Rescheduled { get; set; }
    public int Confirmed { get; set; }
    public int Cancelled { get; set; }
    public int NoShow { get; set; }
}

public class ClinicSchedule
{
    public TimeSpan OpenTime { get; set; } = new(12, 0, 0);
    public TimeSpan CloseTime { get; set; } = new(22, 0, 0);
    public int SlotDurationMinutes { get; set; } = 15;
    public List<int> WorkingDays { get; set; } = [0, 1, 2, 3, 4, 5, 6];

    public bool IsOpenOn(DateTime date) => WorkingDays.Contains((int)date.DayOfWeek);
}

public class ClinicEmailSettingsRequest
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string FromEmail { get; set; } = string.Empty;
    public string SmtpUser { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string? Password { get; set; }
    public bool PasswordConfigured { get; set; }
    public bool IsConfigured { get; set; }
}

public class DailyRevenue
{
    public decimal TotalRevenue { get; set; }
    public string TotalRevenueDisplay { get; set; } = string.Empty;
    public decimal CashTotal { get; set; }
    public string CashTotalDisplay { get; set; } = string.Empty;
    public decimal InstaPayTotal { get; set; }
    public string InstaPayTotalDisplay { get; set; } = string.Empty;
    public decimal CardTotal { get; set; }
    public string CardTotalDisplay { get; set; } = string.Empty;
    public decimal CashAndInstaPayTotal { get; set; }
    public string CashAndInstaPayTotalDisplay { get; set; } = string.Empty;
    public int TotalTransactions { get; set; }
    public string? ReportDate { get; set; }
}

public class PatientListItem
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string? MobilePhone { get; set; }
    public string? Email { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string ApplicationUserId { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class PaymentListItem
{
    public int PaymentId { get; set; }
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string? PatientPhone { get; set; }
    public string? PatientNationalId { get; set; }
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
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
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentMethodDisplay { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string PaymentStatusDisplay { get; set; } = string.Empty;
    public string PaymentDate { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<PaymentReceiptItem> Receipts { get; set; } = [];
}

public class PaymentReceiptItem
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string AmountDisplay { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentMethodDisplay { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string PaidAt { get; set; } = string.Empty;
}

public class PaymentUpdateResult
{
    public int PaymentId { get; set; }
    public decimal AmountPaid { get; set; }
    public string AmountPaidDisplay { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string PaymentDate { get; set; } = string.Empty;
    public string? AdditionalNotes { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
}

public class UpdatePaymentRequest
{
    public int PaymentId { get; set; }
    public decimal? Amount { get; set; }
    public string? AmountInput { get; set; }
    public int? PaymentMethod { get; set; }
    public string? Notes { get; set; }
}

public class InvoiceLineItem
{
    public int Id { get; set; }
    public int PaymentId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? ToothNumber { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string UnitPriceDisplay { get; set; } = string.Empty;
    public decimal LineTotal { get; set; }
    public string LineTotalDisplay { get; set; } = string.Empty;
    public string MetaDisplay { get; set; } = string.Empty;
    public int? DentalTreatmentId { get; set; }
}

public class AddInvoiceLinesRequest
{
    public int PaymentId { get; set; }
    public List<InvoiceLineInputItem> Lines { get; set; } = [];
}

public class InvoiceLineInputItem
{
    public string Description { get; set; } = string.Empty;
    public int? ToothNumber { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public string? UnitPriceInput { get; set; }
    public int? DentalTreatmentId { get; set; }
}

public class BillingPaymentFilters
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? PatientId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Method { get; set; }
    public string? Status { get; set; }
    public string? Search { get; set; }
}

public class CollectPaymentRequest
{
    public int PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public decimal? Amount { get; set; }
    public string? AmountInput { get; set; }
}

public class PaymentReasonRequest
{
    public string? Reason { get; set; }
}

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

public class PermissionModuleItem
{
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public List<string> Actions { get; set; } = [];
}

public class PermissionCatalogResponse
{
    public List<PermissionModuleItem> Modules { get; set; } = [];
    public List<string> AllPermissions { get; set; } = [];
}

public class RoleListItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    public int PermissionCount { get; set; }
}

public class RolePermissionsResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    public List<string> Permissions { get; set; } = [];
}

public class ManagedUserItem
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public string? LinkedName { get; set; }
    public List<string> Roles { get; set; } = [];
    public bool IsLockedOut { get; set; }
    public bool EmailConfirmed { get; set; }
}

public class ManagedUserListResponse
{
    public List<ManagedUserItem> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class CreateStaffUserRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public List<string> RoleNames { get; set; } = [];
}

public class AssignUserRolesRequest
{
    public List<string> RoleNames { get; set; } = [];
}

public class SetUserLockoutRequest
{
    public bool LockoutEnabled { get; set; }
}

public class CreateRoleRequest
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateRolePermissionsRequest
{
    public List<string> Permissions { get; set; } = [];
}
