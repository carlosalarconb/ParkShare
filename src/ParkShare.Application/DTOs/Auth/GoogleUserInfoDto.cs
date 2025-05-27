namespace ParkShare.Application.DTOs.Auth;

public class GoogleUserInfoDto
{
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string GoogleId { get; set; } // Store Google's unique ID
}
