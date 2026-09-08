using Clinic_System.Application.DTOs.Reports;
using Clinic_System.Application.Features.Reports.Queries.Models;
using Clinic_System.Application.Service.Interface;

namespace Clinic_System.Application.Features.Reports.Queries.Handlers;

public class GetClinicReportsQueryHandler : AppRequestHandler<GetClinicReportsQuery, ClinicReportsDTO>
{
    private readonly IClinicReportService _reports;

    public GetClinicReportsQueryHandler(
        ICurrentUserService currentUserService,
        IClinicReportService reports) : base(currentUserService)
    {
        _reports = reports;
    }

    public override async Task<Response<ClinicReportsDTO>> Handle(
        GetClinicReportsQuery request,
        CancellationToken cancellationToken)
    {
        var to = (request.ToDate ?? DateTime.Today).Date;
        var from = (request.FromDate ?? to.AddDays(-30)).Date;

        try
        {
            var data = await _reports.GetReportsAsync(from, to, cancellationToken);
            return Success(data);
        }
        catch (ValidationException ex)
        {
            return BadRequest<ClinicReportsDTO>(ex.Message);
        }
    }
}
