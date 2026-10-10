namespace Blog.Domain.Interfaces;

using Blog.Domain.Entities;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default);
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<bool> ExistsBySlugAsync(string slug, CancellationToken ct = default);
    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);
    Task<int> CountPublishedPostsAsync(Guid categoryId, CancellationToken ct = default);
    Task<Dictionary<Guid, int>> GetPublishedPostCountsAsync(CancellationToken ct = default);
    Task AddAsync(Category category, CancellationToken ct = default);
}
