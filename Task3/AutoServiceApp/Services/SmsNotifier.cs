namespace AutoServiceApp.Services;

public class SmsNotifier
{
    public List<string> SentMessages { get; set; } = new();

    public void SendSms(string phone, string text)
    {
        SentMessages.Add($"SMS {DateTime.Now:g} -> {phone}: {text}");
    }
}
