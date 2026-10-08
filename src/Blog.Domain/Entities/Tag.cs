namespace Blog.Domain.Entities;

public class Tag
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;

    public ICollection<Post> Posts { get; private set; } = [];

    protected Tag() { }

    public Tag(string name, string slug)
    {
        Name = name;
        Slug = slug;
    }
}