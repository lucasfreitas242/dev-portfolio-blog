namespace Blog.Domain.Interfaces;

using Blog.Domain.Entities;

public interface IPostRepository
{
    Task<IReadOnlyList<Post>> GetPublishedPostsAsync(
        int page, 
        int pageSize, 
        string? categorySlug = null, 
        string? language = null,
        CancellationToken ct = default);

    Task<int> CountPublishedPostsAsync(
        string? categorySlug = null, 
        string? language = null,
        CancellationToken ct = default);

    Task<Post?> GetBySlugAsync(
        string slug, 
        CancellationToken ct = default);

    Task<IReadOnlyList<Post>> GetTranslationsByGroupIdAsync(
        Guid translationGroupId,
        CancellationToken ct = default);

    Task<Post?> GetByIdAsync(
        Guid id, 
        CancellationToken ct = default);

    Task AddAsync(
        Post post, 
        CancellationToken ct = default);

    Task UpdateAsync(
        Post post, 
        CancellationToken ct = default);

    Task<bool> ExistsBySlugAsync(
        string slug, 
        CancellationToken ct = default);

    Task<bool> ExistsByTranslationGroupAndLanguageAsync(
        Guid translationGroupId, 
        string language, 
        CancellationToken ct = default);

    Task<bool> TranslationGroupExistsAsync(
        Guid translationGroupId, 
        CancellationToken ct = default);

    Task<bool> CategoryExistsAsync(
        Guid categoryId, 
        CancellationToken ct = default);

    Task<Guid> GetDefaultAuthorIdAsync(
        CancellationToken ct = default);

    Task<List<Tag>> GetOrCreateTagsAsync(
        IEnumerable<string> tagNames, 
        CancellationToken ct = default);
}