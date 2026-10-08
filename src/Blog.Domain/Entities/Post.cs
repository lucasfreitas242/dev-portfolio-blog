using Blog.Domain.Enums;

namespace Blog.Domain.Entities;

public class Post
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public string ContentMd { get; private set; } = string.Empty;
    public string? CoverImage { get; private set; }
    public PostStatus Status { get; private set; } = PostStatus.Draft;
    public int ReadingTimeMinutes { get; private set; } = 1;
    public DateTime? PublishedAt { get; private set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public Guid AuthorId { get; init; }
    public User Author { get; init; } = null!;

    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;

    public ICollection<Tag> Tags { get; private set; } = [];

    protected Post() { }

    public static Post Create(string title, string slug, string summary, string contentMd, Guid categoryId, Guid authorId, string? coverImage = null)
    {
        return new Post
        {
            Title = title,
            Slug = slug,
            Summary = summary,
            ContentMd = contentMd,
            CategoryId = categoryId,
            AuthorId = authorId,
            CoverImage = coverImage,
            ReadingTimeMinutes = CalculateReadingTime(contentMd)
        };
    }

    public void Publish()
    {
        Status = PostStatus.Published;
        PublishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    private static int CalculateReadingTime(string content)
    {
        var wordCount = content.Split([' ', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries).Length;
        return Math.Max(1, (int)Math.Ceiling(wordCount / 200.0));
    }
}
