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

        var translations = await postRepository.GetTranslationsByGroupIdAsync(post.TranslationGroupId, ct);
        var availableTranslations = translations
            .Where(t => t.Id != post.Id)
            .Select(t => new PostTranslationDto(t.Language, t.Slug))
            .ToList();

        return new PostDetailDto(
            Id: post.Id,
            Title: post.Title,
            Slug: post.Slug,
            Language: post.Language,
            TranslationGroupId: post.TranslationGroupId,
            Summary: post.Summary,
            ContentMd: post.ContentMd,
            CoverImage: post.CoverImage,
            ReadingTimeMinutes: post.ReadingTimeMinutes,
            PublishedAt: post.PublishedAt,
            AuthorName: post.Author.Name,
            CategoryName: post.Category.Name,
            CategorySlug: post.Category.Slug,
            Tags: post.Tags.Select(t => t.Name).ToList(),
            AvailableTranslations: availableTranslations
        );
    }
}