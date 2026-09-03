namespace DentalCare.Admin.Services;

public class ClinicSettings
{
    public string Name { get; set; } = "DentalCare";
    public string DefaultCountryCode { get; set; } = "1";
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
}
