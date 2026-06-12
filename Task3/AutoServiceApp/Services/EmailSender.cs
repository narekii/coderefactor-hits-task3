namespace AutoServiceApp.Services;

public class EmailSender
{
    public List<string> Log { get; set; } = new();

    public void Send(string email, string subject, string body)
    {
        Log.Add($"EMAIL {DateTime.Now:g} -> {email}: {subject} {body}");
    }
}
