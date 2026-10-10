namespace Blog.Application.Posts.GetDraftById;

using Blog.Application.Exceptions;
using Blog.Domain.Interfaces;

public class GetDraftByIdHandler(IPostRepository postRepository)
{
    public async Task<DraftDetailDto> HandleAsync(
        GetDraftByIdQuery query, 
        CancellationToken ct = default)
    {
        var post = await postRepository.GetDraftByIdAsync(query.Id, ct);
        if (post is null)
        {
            throw new NotFoundException("Draft", query.Id);
        }

        return new DraftDetailDto(
            Id: post.Id,
            Title: post.Title,
            Slug: post.Slug,
            Language: post.Language,
            TranslationGroupId: post.TranslationGroupId,
            Summary: post.Summary,
            ContentMd: post.ContentMd,
            CoverImage: post.CoverImage,
            ReadingTimeMinutes: post.ReadingTimeMinutes,
            Status: post.Status.ToString(),
            CreatedAt: post.CreatedAt,
            UpdatedAt: post.UpdatedAt,
            AuthorName: post.Author.Name,
            CategoryName: post.Category.Name,
            CategorySlug: post.Category.Slug,
            Tags: post.Tags.Select(t => t.Name).ToList()
        );
    }
}
