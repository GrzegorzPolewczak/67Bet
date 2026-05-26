using System;

namespace _67Bet.Identity.Application.DTOs;

public record UserDto(Guid Id, string Username, string Email, string Role, bool IsKycVerified);
