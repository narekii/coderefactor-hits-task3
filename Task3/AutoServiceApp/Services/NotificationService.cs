namespace AutoServiceApp.Services;

public class NotificationService
{
    public List<string> Log { get; private set; } = new();

    public void Notify(string type, string phone, string email, string title, string message)
    {
        if (type == "sms" || type == "both")
        {
            Log.Add($"SMS {DateTime.Now:g} -> {phone}: {message}");
        }

        if (type == "email" || type == "both")
        {
            Log.Add($"EMAIL {DateTime.Now:g} -> {email}: {title} {message}");
        }
    }
}
