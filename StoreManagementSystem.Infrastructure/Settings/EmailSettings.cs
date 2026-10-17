namespace StoreManagementSystem.Infrastructure.Settings;

public class EmailSettings
{
    public string Host { get; set; } = "smtp.gmail.com";

    public int Port { get; set; } = 587;

    public string SenderName { get; set; } =
        "Store Management System";

    public string SenderEmail { get; set; } = string.Empty;

    public string AppPassword { get; set; } = string.Empty;
}