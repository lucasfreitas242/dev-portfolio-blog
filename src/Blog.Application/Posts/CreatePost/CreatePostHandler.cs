namespace Blog.Application.Posts.CreatePost;

using Blog.Application.Exceptions;
using Blog.Domain.Common;
using Blog.Domain.Entities;
using Blog.Domain.Interfaces;

public class CreatePostHandler(IPostRepository postRepository)
{
    public async Task<PostCreatedDto> HandleAsync(
        CreatePostCommand command, 
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
        {
            throw new ValidationException("Title", "Title cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(command.Summary))
        {
            throw new ValidationException("Summary", "Summary cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(command.ContentMd))
        {
            throw new ValidationException("ContentMd", "Content cannot be empty.");
        }

        var slug = string.IsNullOrWhiteSpace(command.Slug)
            ? SlugHelper.Generate(command.Title)
            : SlugHelper.Generate(command.Slug);

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ValidationException("Slug", "Could not generate a valid slug from the provided title or slug.");
        }

        if (await postRepository.ExistsBySlugAsync(slug, ct))
        {
            throw new ConflictException($"A post with slug '{slug}' already exists.");
        }

        if (!await postRepository.CategoryExistsAsync(command.CategoryId, ct))
        {
            throw new NotFoundException("Category", command.CategoryId);
        }

        var language = string.IsNullOrWhiteSpace(command.Language) 
            ? "en-US" 
            : command.Language.Trim();

        var translationGroupId = command.TranslationGroupId ?? Guid.NewGuid();

        if (command.TranslationGroupId.HasValue)
        {
            if (!await postRepository.TranslationGroupExistsAsync(command.TranslationGroupId.Value, ct))
            {
                throw new NotFoundException("TranslationGroup", command.TranslationGroupId.Value);
            }

            if (await postRepository.ExistsByTranslationGroupAndLanguageAsync(command.TranslationGroupId.Value, language, ct))
            {
                throw new ConflictException($"A post in language '{language}' already exists within translation group '{command.TranslationGroupId.Value}'.");
            }
        }

        var authorId = command.AuthorId ?? await postRepository.GetDefaultAuthorIdAsync(ct);
        if (authorId == Guid.Empty)
        {
            throw new ValidationException("AuthorId", "Author identifier could not be resolved.");
        }

        var tags = new List<Tag>();
        if (command.Tags is { Count: > 0 })
        {
            tags = await postRepository.GetOrCreateTagsAsync(command.Tags, ct);
        }

        var post = Post.Create(
            title: command.Title.Trim(),
            slug: slug,
            summary: command.Summary.Trim(),
            contentMd: command.ContentMd,
            categoryId: command.CategoryId,
            authorId: authorId,
            language: language,
            translationGroupId: translationGroupId,
            coverImage: command.CoverImage?.Trim()
        );

        foreach (var tag in tags)
        {
            post.Tags.Add(tag);
        }

        if (command.PublishNow)
        {
            post.Publish();
        }

        await postRepository.AddAsync(post, ct);

        return new PostCreatedDto(
            Id: post.Id,
            Slug: post.Slug,
            Language: post.Language,
            TranslationGroupId: post.TranslationGroupId,
            Status: post.Status.ToString(),
            CreatedAt: post.CreatedAt,
            PublishedAt: post.PublishedAt
        );
    }
}
