namespace Blog.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public bool IsFeatured { get; private set; }

    public ICollection<Post> Posts { get; private set; } = [];

    protected Category() { }

    public Category(string name, string slug, bool isFeatured = false)
    {
        Name = name;
        Slug = slug;
        IsFeatured = isFeatured;
    }
}
