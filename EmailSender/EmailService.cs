using EmailSender.Dtos;
using EmailSender.Interfaces;
using FluentEmail.Core;
using FluentEmail.Core.Models;

namespace EmailSender;

public class EmailService(IFluentEmail fluentEmail) : IEmailService
{
  public async Task<bool> SendEmailAsync(EmailRequest emailRequest)
  {
    try
    {
      var email = fluentEmail
        .To(emailRequest.To)
        .Subject(emailRequest.Subject)
        .Body(emailRequest.Body);

      SendResponse result;

      // If there is an attachment, add it to the email and send the email.
      if (emailRequest.Attachment is not null && !string.IsNullOrWhiteSpace(emailRequest.AttachmentName))
      {
        using var stream = new MemoryStream(emailRequest.Attachment);
        email.Attach(new Attachment
        {
          Data = stream,
          Filename = emailRequest.AttachmentName
        });
        result = await email.SendAsync();
        return result.Successful;
      }

      // There is no attachment. Just send the email.
      result = await email.SendAsync();
      return result.Successful;
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex.Message);
      throw;
    }
  }
}