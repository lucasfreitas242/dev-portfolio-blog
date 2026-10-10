namespace Blog.Application.Auth.Login;

public record LoginCommand(string Email, string Password);

public record LoginResultDto(
    string Token,
    DateTime ExpiresAt,
    string Email,
    string Name);
