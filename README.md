# FluentEmail Demo

## Step 1: Initial Project Setup

1. Create a new folder where you want to store your .NET solution and all projects associated with it. For this demo, I have created a folder **FluentMailDemo-Public** at the location **C:\GH\FluentEmailDemo-Public**.
2. Open **VSCode**, then navigate to **File -> Open Folder** and select the folder you created in the above step.
3. Open the terminal in **VSCode** by either right-clicking on the **Explorer** panel on the left of **VSCode** and then selecting **Open in Integrated Terminal** or by using the keyboard shortcut **ctrl + `**.
4. Create a new .NET solution named **FluentEmailDemo** in the **C:\GH\FluentEmailDemo-Public** folder by entering the following command in terminal:  

    ```cmd
    dotnet new sln
    ```

    This should create a solution file named **FluentEmailDemo-Public.sln** in the folder.
5. Now, we want to create 3 new projects named **EmailApi**, **EmailDemoConsole**, and **EmailSender**. Let's do this by executing the following commands in the terminal:

    ```cmd
    dotnet new webapi --output EmailApi --use-controllers
    dotnet new console --output EmailDemoConsole
    dotnet new classlib --output EmailSender
    ```

   You should now see 3 folders in **Explorer** named **EmailApi**, **EmailDemoConsole**, and **EmailSender** for the 3 projects your created respectively.
6. Expand the **EmailSender** folder under **Explorer**, then delete the default **Class1.cs** file.
7. Now, it is time to add these 3 projects to our solution. Execute the following three commands to do this:

    ```cmd
    dotnet sln add .\EmailApi\EmailApi.csproj
    dotnet sln add .\EmailDemoConsole\EmailDemoConsole.csproj
    dotnet sln add .\EmailSender\EmailSender.csproj 
    ```

8. Before we proceed, let's make sure the solution builds successfully. To do this, run the following command:

    ```cmd
    dotnet build
    ```

    Please make sure that the build succeeds without any Errors before moving further.

## Step 2: Breating life into the EmailSender

In this demo, we will be using [FluentEmail](https://github.com/lukencode/FluentEmail), an email library for .NET, and we will be using SMTP to send the emails. In order to do this, you need to install the following two nuget packages:

- FluentEmail.Core
- FluentEmail.Smtp

We also need to install the **Microsoft.Extensions.Configuration** nuget package so that we can load configurations from the **appsettings.json** file in our **EmailApi** project.
  
To install all the required packages, navigate to the **EmailSender** folder using the **Integrated Terminal**, then run the following commands:

```cmd
dotnet add package FluentEmail.Core
dotnet add package FluentEmail.Smtp
dotnet add package Microsoft.Extensions.Configuration
```

Once both nuget packages are installed, create a folder named **Dtos** in the **EmailSender** project, then create a new class named **EmailRequest.cs** in this new folder. Now, paste the following code snippet in the **EmailRequest.cs** class:

```cs
namespace EmailSender.Dtos;

public class EmailRequest
{
  public string To { get; set; } = string.Empty;
  public string Subject { get; set; } = string.Empty;
  public string Body { get; set; } = string.Empty;
  public string? AttachmentName { get; set; }
  public byte[]? Attachment { get; set; }
}
```

The next step is to add an **IEmailService** interface and its implementation. Let's begin by creating a folder named **Interfaces** in the **EmailSender** project and then adding a new file named **IEmailService.cs** in it. Once you have created this file, add the following code snippet in the **IEmailService.cs** interface:

```cs
using EmailSender.Dtos;

namespace EmailSender.Interfaces;

public interface IEmailService
{
  Task<bool> SendEmailAsync(EmailRequest emailRequest);
}
```

Now, let's implement the **IEmailService** interface. To do this, add a new class named **EmailService.cs** in the **EmailSender** project and add the below code to it:

```cs
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
```

Finally, we need to add an extension method for **IServiceCollection** which allows us to add **FluentEmail** and our newly created IEmailService to the dependency injection container. Let's begin by adding a new folder named **Extensions** in the **EmailSender** project and creating a class named **ServiceCollectionExtensions.cs** in it. Add the following code to this new class:

```cs
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
```

That's it. Our **EmailSender** is implemented! Before we proceed to the next step, let's make sure that the solution still builds successfully by running the following command in the terminal:

```cmd
dotnet build
```

## Step 3: Implementing the EmailApi

Let's start off by adding references to **EmailDemoConsole** and **EmailSender** projects to the **EmailApi** project.
This can be done by executing the following command in the terminal (make sure you navigate to **EmailApi** folder via terminal before executing this command):

```cmd
dotnet add reference ..\EmailDemoConsole\EmailDemoConsole.csproj ..\EmailSender\EmailSender.csproj
```

> Make sure that you see the following message in the terminal before you proceed:  
`Reference ..\EmailDemoConsole\EmailDemoConsole.csproj added to the project.`  
`Reference ..\EmailSender\EmailSender.csproj added to the project.`

Now, we need to clean up the boilerplate code from **EmailApi**. To do this, navigate to **Program.cs** file in the **EmailApi** project, then replace the code in it with the snippet below:

```cs
using EmailSender.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddEmailService(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

```

You can now delete the **WeatherForecast.cs** and **WeatherForecastController.cs** files. You should be able to find them under the **EmailApi\\** and **EmailApi\\Controllers\\** folders respectively.

Once all boilerplate code is removed, navigate to **EmailDemoConsole** project and create a folder named **Dtos** in it. Add a file named **EmailDetails.cs** to this folder with the following contents:

```cs
namespace EmailDemoConsole.Dtos
{
  public class EmailDetails
  {
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? AttachmentName { get; set; }
    public string? Attachment { get; set; }
  }
}
```

Now, in the **EmailApi** project, add a new file named **FluentEmailController.cs** under **Controllers** and add the code below to it:

```cs
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
```

Finally, we need to add configurations for FluentEmail to the **appsettings.json** file and store sensitive information like *Username* and *Password* that will be used to authenticate with the SMTP server to the secret storage.

First, open the **appsettings.json** file in the **EmailApi** project and append the following string to it (make sure you update the **SenderName** and **SenderEmail** with your information):

```json
"EmailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "SenderName": "Luke Skywalker",
    "SenderEmail": "luke.skywalker@gmail.com"
}
```

Next, enable secret storage for your project by executing the following command from the terminal (make sure you navigate to EmailApi folder via terminal before executing this command):

```cmd
dotnet user-secrets init
```

Finally, add the username and password to the secret storage by executing the following two commands (make sure you update the **username** and **password** with your credentials):

```cmd
dotnet user-secrets set "EmailSettings:Username" "luke.skywalker@gmail.com"
dotnet user-secrets set "EmailSettings:Password" "May-the-Force-be-with-you" 
```

> **NOTE:** If you are using an email service like Gmail, you will need to create app password and use that password to avoid compromising your credentials. Please visit [Sign in with app passwords](https://support.google.com/mail/answer/185833?sjid=17203575874250778219-NC#) in order to create app passwords.

With that final step, our **EmailApi** is now ready for use.

## Step 4: Using EmailApi in the EmailDemoConsole app

Open **Program.cs** in the **EmailDemoConsole** project, then add the following code to it:

```cs
using System.Net.Http.Json;
using EmailDemoConsole.Dtos;

var emailDetails = GetEmailDetails();

if (emailDetails is not null)
{
  await SendEmailAsync(emailDetails);
}

// Accept email details from the user, then return an EmailDetails object.
static EmailDetails? GetEmailDetails()
{
  Console.Write("Please enter recipient email address: ");
  var recipient = Console.ReadLine();

  Console.Write("Please enter subject: ");
  var subject = Console.ReadLine();

  Console.Write("Please enter message: ");
  var message = Console.ReadLine();

  // Recipient, subject and message are mandatory. 
  if (string.IsNullOrWhiteSpace(recipient) || string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(message))
  {
    Console.WriteLine("Recipient, subject and message are required.");
    return null;
  }

  Console.Write("Please enter attachment path (optional): ");
  var attachmentPath = Console.ReadLine();

  Console.Write("Please enter attachment name (optional): ");
  var attachmentName = Console.ReadLine();

  var emailDetails = new EmailDetails
  {
    To = recipient,
    Subject = subject,
    Body = message
  };

  // If attachment path is provided and file exists, read the file and convert it to base64 string.
  if (!string.IsNullOrWhiteSpace(attachmentPath))
  {
    if (!File.Exists(attachmentPath))
    {
      Console.WriteLine("Attachment file not found.");
      return null;
    }

    try
    {
      var fileBytes = File.ReadAllBytes(attachmentPath);
      emailDetails.Attachment = Convert.ToBase64String(fileBytes);

      // If attachment name is not provided, use the file name.
      emailDetails.AttachmentName = !string.IsNullOrWhiteSpace(attachmentName)
      ? attachmentName
      : Path.GetFileName(attachmentPath) ?? "MyAttachment";
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error reading attachment file. {ex.Message}");
    }
  }

  return emailDetails;
}

static async Task SendEmailAsync(EmailDetails emailDetails)
{
  // URL of the API endpoint
  string apiUrl = "https://localhost:7012/FluentEmail/send";

  // Create an instance of HttpClient
  using var client = new HttpClient();

  try
  {
    Console.WriteLine("Sending email...");

    // Send the POST request
    var response = await client.PostAsJsonAsync(apiUrl, emailDetails);

    // Check if the request was successful
    if (response.IsSuccessStatusCode)
    {
      // Read the response content
      await response.Content.ReadAsStringAsync();
      Console.WriteLine("Email was successfully sent...");
      return;
    }
    Console.WriteLine("Email sending failed...");
  }
  catch (Exception ex)
  {
    Console.WriteLine("Error: " + ex.Message);
  }
}
```

> **Note**: Please make sure that you update `apiUrl` to match the URL of your **EmailApi**. Typically, it will be `https://localhost:<port-number>/FluentEmail/send`. So you just need to update the `<port-number>` to match that of your API.

Before you proceed, make sure that the project still build successfully by running the command below (make sure that you are in the **FluentMailDemo-Public** folder before you do so):

```cmd
dotnet build
```

Now, launch the **EmailApi** by executing the command below (assuming that you are still in the **FluentMailDemo-Public** folder):

```cmd
dotnet run --project .\EmailApi\EmailApi.csproj -lp https
```

Next, run the **EmailDemoConsole** app by running the command below (assuming that you are still in the **FluentMailDemo-Public** folder):

```cmd
dotnet run --project .\EmailDemoConsole\EmailDemoConsole.csproj
```

You will be asked to enter the following details:

- Mandatory fields:
  - Recipient Email Address (Multiple recipient emails can be added by separating them with semicolons `;`)
  - Email Subject
  - Email Message (body)
  
- Optional fields:
  - Attachment Path (Should be an absolute path like `C:\My-sample-attachment.txt`)
  - Attachment Name (file name will be used instead if no attachment name is provided)
  