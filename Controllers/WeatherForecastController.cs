using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

[ApiController]
[Route("webhooks/agiloft")]
public class WebhookController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(IHttpClientFactory httpClientFactory, ILogger<WebhookController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    // Verificação inicial (GET)
    [HttpGet]
    public IActionResult VerifyWebhook()
    {
        if (Request.Headers.TryGetValue("Verification-Code", out var verificationCode))
        {
            Response.Headers.Append("Verification-Code", verificationCode);
            return Content(verificationCode.ToString(), "text/plain", Encoding.UTF8);
        }

        return Ok("Verification-Code header missing.");
    }

    // Notificação de webhook (POST)
    [HttpPost]
    public async Task<IActionResult> HandleWebhook([FromBody] SaCasePayload chamado)
    {
        // Envia para o Teams
        var mensagem = new
        {
            text = $"📢 Novo chamado criado ou editado:\n\n" +
                   $"**ID:** {chamado.Id}\n",
            V = $"**Titulo:** {chamado.Summary}\n"

        };

        var client = _httpClientFactory.CreateClient();
        var content = new StringContent(JsonSerializer.Serialize(mensagem), Encoding.UTF8, "application/json");

        var teamsUrl = "https://americatecnologia.webhook.office.com/webhookb2/6dce263f-8388-4429-b486-60a3b79b005f@1822a948-a93d-4f58-8515-9b94a3f89105/IncomingWebhook/0317ee0a94d741cf8622d0abc33d0e85/177cf279-905a-4ace-bf4e-abd76d27a537/V2UiKPeASWPs07KpfEWfz2LcbKk1CIsWg6bs5vRedwkAo1\r\n";
        var response = await client.PostAsync(teamsUrl, content);

        if (response.IsSuccessStatusCode)
        {
            // Verificação implícita do Agiloft — devolve o mesmo Verification-Code
            if (Request.Headers.TryGetValue("Verification-Code", out var verificationCode))
            {
                return Content(verificationCode.ToString(), "text/plain", Encoding.UTF8);
            }

            // Caso não tenha verification code (pouco provável), responde padrão
            return Ok(new { message = "Chamado enviado ao Teams!" });
        }

        _logger.LogError("Erro ao enviar para o Teams: {StatusCode}", response.StatusCode);
        return StatusCode((int)response.StatusCode, "Falha ao enviar para o Teams.");
    }
}
