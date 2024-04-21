using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using EmailSender.Interfaces;

namespace EmailSender.Extensions;

public static class ServiceCollectionExtensions
{
  public static void AddEmailService(this IServiceCollection services, IConfiguration configuration)
  {
    // Retrieves email settings from appsettings.json and secrets.json files
    var emailSettings = configuration.GetSection("EmailSettings");
    var defaultFromEmail = emailSettings["SenderEmail"];
    var defaultDisplayName = emailSettings["SenderName"];
    var host = emailSettings["Host"];
    var portString = emailSettings["Port"];
    var port = int.Parse(portString!);
    var userName = emailSettings["Username"];
    var password = emailSettings["Password"];

    // Adds FluentEmail services to DI container
    services.AddFluentEmail(defaultFromEmail, defaultDisplayName)
        .AddSmtpSender(host, port, userName, password);

    services.AddScoped<IEmailService, EmailService>();
  }
}