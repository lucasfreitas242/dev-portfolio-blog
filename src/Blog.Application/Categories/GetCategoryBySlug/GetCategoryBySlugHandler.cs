namespace Blog.Application.Categories.GetCategoryBySlug;

using Blog.Application.Exceptions;
using Blog.Domain.Interfaces;

public class GetCategoryBySlugHandler(ICategoryRepository categoryRepository)
{
    public async Task<CategoryDetailDto> HandleAsync(
        GetCategoryBySlugQuery query, 
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query.Slug))
        {
            throw new ValidationException("Slug", "Slug cannot be empty.");
        }

        var category = await categoryRepository.GetBySlugAsync(query.Slug, ct);
        if (category is null)
        {
            throw new NotFoundException("Category", query.Slug);
        }

        var postCount = await categoryRepository.CountPublishedPostsAsync(category.Id, ct);

        return new CategoryDetailDto(
            Id: category.Id,
            Name: category.Name,
            Slug: category.Slug,
            IsFeatured: category.IsFeatured,
            PostCount: postCount
        );
    }
}
