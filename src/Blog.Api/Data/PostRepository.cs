namespace Blog.Api.Data.Repositories;

using Blog.Domain.Entities;
using Blog.Domain.Enums;
using Blog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

public class PostRepository(AppDbContext db) : IPostRepository
{
    public async Task<IReadOnlyList<Post>> GetPublishedPostsAsync(
        int page,
        int pageSize,
        string? categorySlug = null,
        string? language = null,
        CancellationToken ct = default)
    {
        var query = db.Posts
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.Status == PostStatus.Published);

        if (!string.IsNullOrWhiteSpace(categorySlug))
        {
            query = query.Where(p => p.Category.Slug == categorySlug.Trim().ToLowerInvariant());
        }

        if (!string.IsNullOrWhiteSpace(language))
        {
            query = query.Where(p => p.Language == language.Trim());
        }

        return await query
            .OrderByDescending(p => p.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> CountPublishedPostsAsync(
        string? categorySlug = null,
        string? language = null,
        CancellationToken ct = default)
    {
        var query = db.Posts
            .AsNoTracking()
            .Where(p => p.Status == PostStatus.Published);

        if (!string.IsNullOrWhiteSpace(categorySlug))
        {
            query = query.Where(p => p.Category.Slug == categorySlug.Trim().ToLowerInvariant());
        }

        if (!string.IsNullOrWhiteSpace(language))
        {
            query = query.Where(p => p.Language == language.Trim());
        }

        return await query.CountAsync(ct);
    }

    public async Task<Post?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await db.Posts
            .AsNoTracking()
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Slug == slug.Trim().ToLowerInvariant() && p.Status == PostStatus.Published, ct);
    }

    public async Task<IReadOnlyList<Post>> GetTranslationsByGroupIdAsync(
        Guid translationGroupId, 
        CancellationToken ct = default)
    {
        return await db.Posts
            .AsNoTracking()
            .Where(p => p.TranslationGroupId == translationGroupId && p.Status == PostStatus.Published)
            .ToListAsync(ct);
    }

    public async Task<Post?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Posts
            .Include(p => p.Category)
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task AddAsync(Post post, CancellationToken ct = default)
    {
        await db.Posts.AddAsync(post, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Post post, CancellationToken ct = default)
    {
        db.Posts.Update(post);
        await db.SaveChangesAsync(ct);
    }
}