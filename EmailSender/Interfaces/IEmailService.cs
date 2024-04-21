using EmailSender.Dtos;

namespace EmailSender.Interfaces;

public interface IEmailService
{
  Task<bool> SendEmailAsync(EmailRequest emailRequest);
}