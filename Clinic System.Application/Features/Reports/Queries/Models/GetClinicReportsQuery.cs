using Clinic_System.Application.DTOs.Reports;
using MediatR;

namespace Clinic_System.Application.Features.Reports.Queries.Models;

public class GetClinicReportsQuery : IRequest<Response<ClinicReportsDTO>>
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
