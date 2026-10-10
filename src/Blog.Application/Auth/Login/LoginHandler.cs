namespace Blog.Application.Auth.Login;

using Blog.Application.Common;
using Blog.Application.Exceptions;
using Blog.Domain.Interfaces;

public class LoginHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator)
{
    public async Task<LoginResultDto> HandleAsync(
        LoginCommand command, 
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
        {
            throw new ValidationException("Email", "Email cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(command.Password))
        {
            throw new ValidationException("Password", "Password cannot be empty.");
        }

        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        var user = await userRepository.GetByEmailAsync(normalizedEmail, ct);

        if (user is null || !passwordHasher.VerifyPassword(command.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var (token, expiresAt) = jwtTokenGenerator.GenerateToken(user);

        return new LoginResultDto(
            Token: token,
            ExpiresAt: expiresAt,
            Email: user.Email,
            Name: user.Name
        );
    }
}
