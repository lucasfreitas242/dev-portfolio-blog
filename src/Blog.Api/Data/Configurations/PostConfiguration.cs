namespace Blog.Api.Data.Configurations;

using Blog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("posts");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Slug)
            .HasMaxLength(250)
            .IsRequired();

        builder.HasIndex(p => p.Slug)
            .IsUnique();

        builder.Property(p => p.Language)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(p => p.TranslationGroupId)
            .IsRequired();

        builder.HasIndex(p => p.TranslationGroupId);

        builder.Property(p => p.Summary)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(p => p.ContentMd)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(p => p.CoverImage)
            .HasMaxLength(500);

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(p => new { p.Status, p.PublishedAt });
        builder.HasIndex(p => new { p.Status, p.Language, p.PublishedAt });

        builder.HasOne(p => p.Author)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Posts)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Tags)
            .WithMany(t => t.Posts)
            .UsingEntity(j => j.ToTable("post_tags"));
    }
}