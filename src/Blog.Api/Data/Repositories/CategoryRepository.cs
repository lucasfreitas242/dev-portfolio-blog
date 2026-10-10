namespace Blog.Api.Data.Repositories;

using Blog.Domain.Entities;
using Blog.Domain.Enums;
using Blog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

public class CategoryRepository(AppDbContext db) : ICategoryRepository
{
    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        return await db.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Slug == normalizedSlug, ct);
    }

    public async Task<bool> ExistsBySlugAsync(string slug, CancellationToken ct = default)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        return await db.Categories
            .AnyAsync(c => c.Slug == normalizedSlug, ct);
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Categories
            .AnyAsync(c => c.Id == id, ct);
    }

    public async Task<int> CountPublishedPostsAsync(Guid categoryId, CancellationToken ct = default)
    {
        return await db.Posts
            .AsNoTracking()
            .Where(p => p.CategoryId == categoryId && p.Status == PostStatus.Published)
            .CountAsync(ct);
    }

    public async Task<Dictionary<Guid, int>> GetPublishedPostCountsAsync(CancellationToken ct = default)
    {
        return await db.Posts
            .AsNoTracking()
            .Where(p => p.Status == PostStatus.Published)
            .GroupBy(p => p.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count, ct);
    }

    public async Task AddAsync(Category category, CancellationToken ct = default)
    {
        await db.Categories.AddAsync(category, ct);
        await db.SaveChangesAsync(ct);
    }
}
