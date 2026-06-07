using _67Bet.Identity.Application.DTOs;
using _67Bet.Identity.Domain.Entities;

namespace _67Bet.Identity.Application.Mappings;

public static class MappingExtensions
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto(
            user.Id,
            user.Username,
            user.Email,
            user.UserRole.ToString(),
            user.IsKycVerified
        );
    }
}
