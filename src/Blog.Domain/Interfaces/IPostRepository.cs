namespace Blog.Domain.Interfaces;

using Blog.Domain.Entities;

public interface IPostRepository
{
    Task<IReadOnlyList<Post>> GetPublishedPostsAsync(
        int page, 
        int pageSize, 
        string? categorySlug = null, 
        CancellationToken ct = default);

    Task<int> CountPublishedPostsAsync(
        string? categorySlug = null, 
        CancellationToken ct = default);

    Task<Post?> GetBySlugAsync(
        string slug, 
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
}