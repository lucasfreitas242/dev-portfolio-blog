namespace Blog.Application.Common;

using Blog.Domain.Entities;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}
