namespace Blog.Domain.Entities;

public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsFeatured { get; private set; }

    public ICollection<Post> Posts { get; set; } = new List<Post>();

    protected Category() { }

    public Category(string name, string slug, bool isFeatured)
    {
        Name = name;
        Slug = slug;
        IsFeatured = isFeatured;
    }
}
