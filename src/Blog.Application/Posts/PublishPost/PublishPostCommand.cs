namespace Blog.Application.Posts.PublishPost;

public record PublishPostCommand(Guid Id);

public record PostPublishedDto(
    Guid Id,
    string Slug,
    string Status,
    DateTime? PublishedAt);
