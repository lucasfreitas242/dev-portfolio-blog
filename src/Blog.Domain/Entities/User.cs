namespace Blog.Domain.Entities;

public class User
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public ICollection<Post> Posts { get; private set; } = [];

    protected User() { }

    public User(string email, string passwordHash, string name)
    {
        Email = email;
        PasswordHash = passwordHash;
        Name = name;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }
}