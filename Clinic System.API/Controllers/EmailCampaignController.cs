using Clinic_System.Application.DTOs.EmailCampaigns;
using ApiCampaignList = Clinic_System.Application.Common.Bases.Response<System.Collections.Generic.List<Clinic_System.Application.DTOs.EmailCampaigns.EmailCampaignListItemDTO>>;
using ApiCampaign = Clinic_System.Application.Common.Bases.Response<Clinic_System.Application.DTOs.EmailCampaigns.EmailCampaignDetailDTO>;
using ApiAudience = Clinic_System.Application.Common.Bases.Response<Clinic_System.Application.DTOs.EmailCampaigns.EmailCampaignAudienceDTO>;

namespace Clinic_System.API.Controllers;

[Route("api/email-campaigns")]
[ApiController]
[Authorize]
public class EmailCampaignController : AppControllerBase
{
    private readonly IEmailCampaignService _campaigns;

    public EmailCampaignController(IMediator mediator, IEmailCampaignService campaigns) : base(mediator)
    {
        _campaigns = campaigns;
    }

    [HttpGet("audience")]
    [Authorize(Policy = "campanas.view")]
    public async Task<IActionResult> GetAudience(CancellationToken cancellationToken)
    {
        var data = await _campaigns.GetAudienceAsync(cancellationToken);
        return NewResult(Ok(data, "Pacientes con correo para campañas."));
    }

    [HttpGet]
    [Authorize(Policy = "campanas.view")]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var data = await _campaigns.ListAsync(cancellationToken);
        return NewResult(OkList(data, "Campañas de correo."));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "campanas.view")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var data = await _campaigns.GetAsync(id, cancellationToken);
        if (data == null)
            return NewResult(Fail("No se encontró la campaña."));
        return NewResult(Ok(data, "Campaña de correo."));
    }

    [HttpPost]
    [Authorize(Policy = "campanas.create")]
    public async Task<IActionResult> Create([FromBody] CreateEmailCampaignDTO request, CancellationToken cancellationToken)
    {
        var (data, error) = await _campaigns.CreateAsync(request, cancellationToken);
        if (error != null || data == null)
            return NewResult(Fail(error ?? "No se pudo crear la campaña."));
        return NewResult(Ok(data, "Campaña creada."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "campanas.edit")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateEmailCampaignDTO request, CancellationToken cancellationToken)
    {
        var (data, error) = await _campaigns.UpdateDraftAsync(id, request, cancellationToken);
        if (error != null || data == null)
            return NewResult(Fail(error ?? "No se pudo guardar la campaña."));
        return NewResult(Ok(data, "Campaña actualizada."));
    }

    [HttpPost("{id:int}/start")]
    [Authorize(Policy = "campanas.edit")]
    public async Task<IActionResult> Start(int id, CancellationToken cancellationToken)
    {
        var (data, error) = await _campaigns.StartAsync(id, cancellationToken);
        if (error != null || data == null)
            return NewResult(Fail(error ?? "No se pudo iniciar la campaña."));
        return NewResult(Ok(data, "Campaña en curso. Se enviarán 15 correos cada 15 minutos."));
    }

    [HttpPost("{id:int}/pause")]
    [Authorize(Policy = "campanas.edit")]
    public async Task<IActionResult> Pause(int id, CancellationToken cancellationToken)
    {
        var (data, error) = await _campaigns.PauseAsync(id, cancellationToken);
        if (error != null || data == null)
            return NewResult(Fail(error ?? "No se pudo pausar la campaña."));
        return NewResult(Ok(data, "Campaña pausada."));
    }

    [HttpPost("{id:int}/resume")]
    [Authorize(Policy = "campanas.edit")]
    public async Task<IActionResult> Resume(int id, CancellationToken cancellationToken)
    {
        var (data, error) = await _campaigns.ResumeAsync(id, cancellationToken);
        if (error != null || data == null)
            return NewResult(Fail(error ?? "No se pudo reanudar la campaña."));
        return NewResult(Ok(data, "Campaña reanudada."));
    }

    [HttpPost("{id:int}/cancel")]
    [Authorize(Policy = "campanas.edit")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        var (data, error) = await _campaigns.CancelAsync(id, cancellationToken);
        if (error != null || data == null)
            return NewResult(Fail(error ?? "No se pudo cancelar la campaña."));
        return NewResult(Ok(data, "Campaña cancelada."));
    }

    private static ApiAudience Ok(EmailCampaignAudienceDTO data, string message) => new()
    {
        Succeeded = true,
        StatusCode = HttpStatusCode.OK,
        Data = data,
        Message = message
    };

    private static ApiCampaign Ok(EmailCampaignDetailDTO data, string message) => new()
    {
        Succeeded = true,
        StatusCode = HttpStatusCode.OK,
        Data = data,
        Message = message
    };

    private static ApiCampaignList OkList(List<EmailCampaignListItemDTO> data, string message) => new()
    {
        Succeeded = true,
        StatusCode = HttpStatusCode.OK,
        Data = data,
        Message = message
    };

    private static ApiCampaign Fail(string message) => new()
    {
        Succeeded = false,
        StatusCode = HttpStatusCode.BadRequest,
        Message = message
    };
}
