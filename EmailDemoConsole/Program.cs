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