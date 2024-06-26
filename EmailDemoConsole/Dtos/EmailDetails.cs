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