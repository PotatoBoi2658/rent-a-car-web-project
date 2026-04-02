/// <summary>
///forgot password email send settings
/// <summary>
namespace rent_a_car.Services
{
    public class SmtpOptions
    {
        public string Host { get; set; } = null!;
        public int Port { get; set; } = 587;
        public bool UseSsl { get; set; } = true;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string SenderEmail { get; set; } = null!;
        public string? SenderName { get; set; }
    }
}