using EmailDemoConsole.Dtos;
using EmailSender.Dtos;
using EmailSender.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EmailApi.Controllers;

[ApiController]
[Route("[controller]")]
public class FluentEmailController(ILogger<FluentEmailController> logger, IEmailService emailService) : ControllerBase
{
  [HttpPost("send")]
  public async Task<IActionResult> SendEmail([FromBody] EmailDetails emailDetails)
  {
    try
    {
      var emailRequest = new EmailRequest
      {
        To = emailDetails.To,
        Subject = emailDetails.Subject,
        Body = emailDetails.Body,
        Attachment = emailDetails.Attachment is not null
          ? Convert.FromBase64String(emailDetails.Attachment)
          : null,
        AttachmentName = emailDetails.AttachmentName
      };
      var response = await emailService.SendEmailAsync(emailRequest);
      return response ? Ok() : StatusCode(500);
    }
    catch (Exception ex)
    {
      logger.LogError(ex.Message);
      throw;
    }
  }
}
