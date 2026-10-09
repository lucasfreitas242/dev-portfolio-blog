namespace Blog.Api.Data;

using Blog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        if (await context.Posts.AnyAsync())
        {
            return;
        }

        var author = new User(
            email: "contato@lucasefreitas.dev",
            passwordHash: "AQAAAAIAAYagAAAAEPw...", // Hash provisório para desenvolvimento
            name: "Lucas Freitas"
        );

        var categoryArch = new Category(name: "Architecture", slug: "architecture", isFeatured: false);
        var categoryDotNet = new Category(name: ".NET", slug: "dotnet", isFeatured: false);

        var tagCsharp = new Tag(name: "C#", slug: "csharp");
        var tagCleanArch = new Tag(name: "Clean Architecture", slug: "clean-architecture");

        var cleanArchGroupId = Guid.NewGuid();

        var post1 = Post.Create(
            title: "Building Clean Architecture with .NET 8",
            slug: "building-clean-architecture-dotnet-8",
            summary: "A practical guide on structuring scalable APIs using 3-layer Clean Architecture and Vertical Feature Slicing.",
            contentMd: "# Clean Architecture in .NET 8\n\nPragmatic design prevents accidental complexity...",
            categoryId: categoryArch.Id,
            authorId: author.Id,
            language: "en-US",
            translationGroupId: cleanArchGroupId
        );
        post1.Publish();
        post1.Tags.Add(tagCleanArch);

        var post1Pt = Post.Create(
            title: "Construindo Clean Architecture com .NET 8",
            slug: "construindo-clean-architecture-dotnet-8",
            summary: "Um guia prático sobre como estruturar APIs escaláveis usando Clean Architecture em 3 camadas e Vertical Slicing.",
            contentMd: "# Clean Architecture no .NET 8\n\nDesign pragmático evita complexidade acidental...",
            categoryId: categoryArch.Id,
            authorId: author.Id,
            language: "pt-BR",
            translationGroupId: cleanArchGroupId
        );
        post1Pt.Publish();
        post1Pt.Tags.Add(tagCleanArch);

        var post2 = Post.Create(
            title: "EF Core 8 Performance Insights",
            slug: "ef-core-8-performance-insights",
            summary: "Analyzing generated queries, indexing strategies, and split queries with PostgreSQL.",
            contentMd: "# EF Core 8 and PostgreSQL\n\nNever trust your ORM blindly...",
            categoryId: categoryDotNet.Id,
            authorId: author.Id,
            language: "en-US"
        );
        post2.Publish();
        post2.Tags.Add(tagCsharp);

        await context.Users.AddAsync(author);
        await context.Categories.AddRangeAsync(categoryArch, categoryDotNet);
        await context.Tags.AddRangeAsync(tagCsharp, tagCleanArch);
        await context.Posts.AddRangeAsync(post1, post1Pt, post2);

        await context.SaveChangesAsync();
    }
}