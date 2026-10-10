namespace Blog.Application.Posts.UpdatePost;

using Blog.Application.Exceptions;
using Blog.Domain.Common;
using Blog.Domain.Interfaces;

public class UpdatePostHandler(
    IPostRepository postRepository,
    ICategoryRepository categoryRepository)
{
    public async Task<PostUpdatedDto> HandleAsync(
        UpdatePostCommand command, 
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

        var post = await postRepository.GetByIdAsync(command.Id, ct);
        if (post is null || post.Status == Blog.Domain.Enums.PostStatus.Archived)
        {
            throw new NotFoundException("Post", command.Id);
        }

        Guid categoryId = post.CategoryId;
        string categoryName = post.Category.Name;
        string categorySlug = post.Category.Slug;

        if (command.CategoryId.HasValue && command.CategoryId.Value != Guid.Empty)
        {
            var category = await categoryRepository.GetByIdAsync(command.CategoryId.Value, ct);
            if (category is null)
            {
                throw new NotFoundException("Category", command.CategoryId.Value);
            }
            categoryId = category.Id;
            categoryName = category.Name;
            categorySlug = category.Slug;
        }
        else if (!string.IsNullOrWhiteSpace(command.CategorySlug))
        {
            var normalizedCatSlug = SlugHelper.Generate(command.CategorySlug);
            var category = await categoryRepository.GetBySlugAsync(normalizedCatSlug, ct);
            if (category is null)
            {
                throw new NotFoundException("Category", command.CategorySlug);
            }
            categoryId = category.Id;
            categoryName = category.Name;
            categorySlug = category.Slug;
        }

        string targetSlug = post.Slug;
        if (!string.IsNullOrWhiteSpace(command.Slug))
        {
            var newSlug = SlugHelper.Generate(command.Slug);
            if (string.IsNullOrWhiteSpace(newSlug))
            {
                throw new ValidationException("Slug", "Could not generate a valid slug from the provided slug.");
            }

            if (newSlug != post.Slug)
            {
                if (await postRepository.ExistsBySlugAsync(newSlug, post.Id, ct))
                {
                    throw new ConflictException($"A post with slug '{newSlug}' already exists.");
                }
                targetSlug = newSlug;
            }
        }

        if (command.Tags is not null)
        {
            var tags = await postRepository.GetOrCreateTagsAsync(command.Tags, ct);
            post.Tags.Clear();
            foreach (var tag in tags)
            {
                post.Tags.Add(tag);
            }
        }

        post.Update(
            title: command.Title.Trim(),
            summary: command.Summary.Trim(),
            contentMd: command.ContentMd,
            categoryId: categoryId,
            slug: targetSlug,
            coverImage: command.CoverImage?.Trim()
        );

        await postRepository.UpdateAsync(post, ct);

        return new PostUpdatedDto(
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
            PublishedAt: post.PublishedAt,
            CategoryName: categoryName,
            CategorySlug: categorySlug,
            Tags: post.Tags.Select(t => t.Name).ToList()
        );
    }
}
