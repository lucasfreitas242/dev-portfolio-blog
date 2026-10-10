namespace Blog.Domain.Tests;

using Blog.Domain.Common;
using Blog.Domain.Entities;
using Blog.Domain.Enums;
using FluentAssertions;
using Xunit;

public class PostEntityTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldInitializeCorrectDefaults()
    {
        // Arrange
        var title = "Building Resilient APIs";
        var slug = "building-resilient-apis";
        var summary = "Practical patterns for resilient microservices.";
        var contentMd = "# Building Resilient APIs\n\nResilience patterns in .NET...";
        var categoryId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        // Act
        var post = Post.Create(
            title: title,
            slug: slug,
            summary: summary,
            contentMd: contentMd,
            categoryId: categoryId,
            authorId: authorId
        );

        // Assert
        post.Id.Should().NotBeEmpty();
        post.Title.Should().Be(title);
        post.Slug.Should().Be(slug);
        post.Summary.Should().Be(summary);
        post.ContentMd.Should().Be(contentMd);
        post.CategoryId.Should().Be(categoryId);
        post.AuthorId.Should().Be(authorId);
        post.Language.Should().Be("en-US");
        post.TranslationGroupId.Should().NotBeEmpty();
        post.Status.Should().Be(PostStatus.Draft);
        post.PublishedAt.Should().BeNull();
        post.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        post.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_WithExplicitTranslationGroupId_ShouldPreserveGroup()
    {
        // Arrange
        var groupId = Guid.NewGuid();

        // Act
        var post = Post.Create(
            title: "Portuguese Post",
            slug: "portuguese-post",
            summary: "Sumario",
            contentMd: "# Conteudo",
            categoryId: Guid.NewGuid(),
            authorId: Guid.NewGuid(),
            language: "pt-BR",
            translationGroupId: groupId
        );

        // Assert
        post.TranslationGroupId.Should().Be(groupId);
        post.Language.Should().Be("pt-BR");
    }

    [Fact]
    public void Publish_ShouldSetStatusToPublishedAndRecordPublishedAt()
    {
        // Arrange
        var post = Post.Create(
            title: "Draft Post",
            slug: "draft-post",
            summary: "Summary",
            contentMd: "Content",
            categoryId: Guid.NewGuid(),
            authorId: Guid.NewGuid()
        );

        // Act
        post.Publish();

        // Assert
        post.Status.Should().Be(PostStatus.Published);
        post.PublishedAt.Should().NotBeNull();
        post.PublishedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Archive_ShouldSetStatusToArchived()
    {
        // Arrange
        var post = Post.Create(
            title: "Active Post",
            slug: "active-post",
            summary: "Summary",
            contentMd: "Content",
            categoryId: Guid.NewGuid(),
            authorId: Guid.NewGuid()
        );

        // Act
        post.Archive();

        // Assert
        post.Status.Should().Be(PostStatus.Archived);
    }

    [Fact]
    public void Update_ShouldModifyFieldsAndRecalculateReadingTime()
    {
        // Arrange
        var post = Post.Create(
            title: "Initial Title",
            slug: "initial-title",
            summary: "Initial Summary",
            contentMd: "Short content",
            categoryId: Guid.NewGuid(),
            authorId: Guid.NewGuid()
        );

        var newCategoryId = Guid.NewGuid();
        var longContent = string.Join(" ", Enumerable.Repeat("word", 600)); // 600 words -> ~3 min

        // Act
        post.Update(
            title: "Updated Title",
            summary: "Updated Summary",
            contentMd: longContent,
            categoryId: newCategoryId,
            slug: "updated-title"
        );

        // Assert
        post.Title.Should().Be("Updated Title");
        post.Slug.Should().Be("updated-title");
        post.Summary.Should().Be("Updated Summary");
        post.CategoryId.Should().Be(newCategoryId);
        post.ReadingTimeMinutes.Should().Be(3);
    }
}

public class SlugHelperTests
{
    [Theory]
    [InlineData("Clean Architecture in .NET 8!", "clean-architecture-in-net-8")]
    [InlineData("Construindo APIs com C# e EF Core", "construindo-apis-com-c-e-ef-core")]
    [InlineData("  Espaços   múltiplos  e  hífens --  ", "espacos-multiplos-e-hifens")]
    [InlineData("Ações, Café & Códigos", "acoes-cafe-codigos")]
    public void Generate_ShouldReturnDeterministicNormalizedSlug(string input, string expected)
    {
        // Act
        var result = SlugHelper.Generate(input);

        // Assert
        result.Should().Be(expected);
    }
}
