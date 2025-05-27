using ParkShare.Application.DTOs.Auth; // Assuming DTOs will be in ParkShare.Application.DTOs
using System.Threading.Tasks;

namespace ParkShare.Application.Interfaces;

public interface IAuthService
{
    Task<UserDto> GetOrCreateUserAsync(GoogleUserInfoDto googleUserInfo);
    string GenerateJwtToken(UserDto user);
}
