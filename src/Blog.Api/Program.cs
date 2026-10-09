using Blog.Api.Data;
using Blog.Api.Data.Repositories;
using Blog.Api.Endpoints;
using Blog.Application;
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
builder.Services.AddApplicationServices();

// Swagger / OpenAPI para documentação interativa
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPostEndpoints();

app.UseHttpsRedirection();

app.Run();