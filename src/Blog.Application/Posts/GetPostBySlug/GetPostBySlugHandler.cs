namespace Blog.Application.Posts.GetPostBySlug;

using Blog.Domain.Interfaces;

public class GetPostBySlugHandler(IPostRepository postRepository)
{
    public async Task<PostDetailDto?> HandleAsync(
        GetPostBySlugQuery query, 
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query.Slug))
        {
            return null;
        }

        var post = await postRepository.GetBySlugAsync(query.Slug, ct);

        if (post is null)
        {
            return null;
        }

        return new PostDetailDto(
            Id: post.Id,
            Title: post.Title,
            Slug: post.Slug,
            Summary: post.Summary,
            ContentMd: post.ContentMd,
            CoverImage: post.CoverImage,
            ReadingTimeMinutes: post.ReadingTimeMinutes,
            PublishedAt: post.PublishedAt,
            AuthorName: post.Author.Name,
            CategoryName: post.Category.Name,
            CategorySlug: post.Category.Slug,
            Tags: post.Tags.Select(t => t.Name).ToList()
        );
    }
}