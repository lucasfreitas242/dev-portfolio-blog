namespace Blog.Application.Categories.CreateCategory;

using Blog.Application.Exceptions;
using Blog.Domain.Common;
using Blog.Domain.Entities;
using Blog.Domain.Interfaces;

public class CreateCategoryHandler(ICategoryRepository categoryRepository)
{
    public async Task<CategoryCreatedDto> HandleAsync(
        CreateCategoryCommand command,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ValidationException("Name", "Category name cannot be empty.");
        }

        var slug = string.IsNullOrWhiteSpace(command.Slug)
            ? SlugHelper.Generate(command.Name)
            : SlugHelper.Generate(command.Slug);

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ValidationException("Slug", "Could not generate a valid slug from the provided name or slug.");
        }

        if (await categoryRepository.ExistsBySlugAsync(slug, ct))
        {
            throw new ConflictException($"A category with slug '{slug}' already exists.");
        }

        var category = Category.Create(command.Name.Trim(), slug, command.IsFeatured);

        await categoryRepository.AddAsync(category, ct);

        return new CategoryCreatedDto(
            Id: category.Id,
            Name: category.Name,
            Slug: category.Slug,
            IsFeatured: category.IsFeatured
        );
    }
}
