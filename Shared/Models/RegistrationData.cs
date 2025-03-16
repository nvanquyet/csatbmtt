namespace Shared.Models;

public class RegistrationData(string? phoneNumber, string rsaPublicKey)
{
    public string? PhoneNumber { get; set; } = phoneNumber;
    public string RsaPublicKey { get; set; } = rsaPublicKey;
}