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
    dotnet new webapi --output EmailApi
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

## Step 2: Implementing the EmailSender

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
  public string? Attachment { get; set; }
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
using System.Text;
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

      if (!string.IsNullOrWhiteSpace(emailRequest.Attachment) && !string.IsNullOrWhiteSpace(emailRequest.AttachmentName))
      {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(emailRequest.Attachment));
        email.Attach(new Attachment
        {
          Data = stream,
          Filename = emailRequest.AttachmentName
        });
        result = await email.SendAsync();
        return result.Successful;
      }

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
    var host = emailSettings["Host"];
    var portString = emailSettings["Port"];
    var port = int.Parse(portString!);
    var userName = emailSettings["Username"];
    var password = emailSettings["Password"];

    // Adds FluentEmail services to DI container
    services.AddFluentEmail(defaultFromEmail)
        .AddSmtpSender(host, port, userName, password);

    services.AddScoped<IEmailService, EmailService>();
  }
}
```

That's it. Our **EmailSender** is implemented! Before we proceed to the next step, let's make sure that the solution still builds successfully by running the following command:

```cmd
dotnet build
```
