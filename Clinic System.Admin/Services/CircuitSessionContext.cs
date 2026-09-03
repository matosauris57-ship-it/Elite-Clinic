using System.Security.Claims;

namespace DentalCare.Admin.Services;

public class CircuitSessionContext
{
    public ClaimsPrincipal? User { get; set; }
}
