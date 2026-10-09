using Blog.Api.Data;
using Blog.Api.Data.Repositories;
using Blog.Application.Posts.GetPostBySlug;
using Blog.Application.Posts.ListPublishedPosts;
using Blog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Conexão com o PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Injeção de Dependências
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ListPublishedPostsHandler>();
builder.Services.AddScoped<GetPostBySlugHandler>();

// Swagger / OpenAPI para documentação interativa
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Endpoint do Caso de Uso
app.MapGet("/api/v1/posts", async (
    [AsParameters] ListPublishedPostsQuery query,
    ListPublishedPostsHandler handler,
    CancellationToken ct) =>
{
    var result = await handler.HandleAsync(query, ct);
    return Results.Ok(result);
})
.WithName("ListPublishedPosts")
.WithTags("Posts")
.Produces<PagedResult<PostSummaryDto>>(StatusCodes.Status200OK);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Executa a migration e insere dados falsos de teste
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.SeedAsync(dbContext);
}

app.MapGet("/api/v1/posts/{slug}", async (
    string slug,
    GetPostBySlugHandler handler,
    CancellationToken ct) =>
{
    var post = await handler.HandleAsync(new GetPostBySlugQuery(slug), ct);
    return post is not null 
        ? Results.Ok(post) 
        : Results.NotFound();
})
.WithName("GetPostBySlug")
.WithTags("Posts")
.Produces<PostDetailDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

app.UseHttpsRedirection();

app.Run();