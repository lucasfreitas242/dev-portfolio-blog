using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blog.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPostTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "posts",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "TranslationGroupId",
                table: "posts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_posts_Status_Language_PublishedAt",
                table: "posts",
                columns: new[] { "Status", "Language", "PublishedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_posts_TranslationGroupId",
                table: "posts",
                column: "TranslationGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_posts_Status_Language_PublishedAt",
                table: "posts");

            migrationBuilder.DropIndex(
                name: "IX_posts_TranslationGroupId",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "TranslationGroupId",
                table: "posts");
        }
    }
}
