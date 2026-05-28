using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StreamingAPI.Data;
using StreamingAPI.Docs;
using StreamingAPI.Middlewares;
using StreamingAPI.Models;
using StreamingAPI.Repositories;
using StreamingAPI.Services;
using Microsoft.Extensions.FileProviders;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container
builder.Services.AddOpenApi("v1");
builder.Services.AddControllers();
builder.Services.AddAuthorization();

// Configurar DbContext - usando SQLite (Banco local)
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=Streaming.db";

builder.Services.AddDbContext<StreamingContext>(options =>
    options.UseSqlite(connectionString));

// Registrar Repository
builder.Services.AddScoped<PlaylistRepository>();
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<ConteudoRepository>();
builder.Services.AddScoped<CriadorRepository>();
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddSingleton<TokenService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<StreamingContext>();
    context.Database.Migrate();
}

// Garantir que a pasta Media exista
var mediaPath = Path.Combine(app.Environment.ContentRootPath, "Media");
if (!Directory.Exists(mediaPath)) Directory.CreateDirectory(mediaPath);

// Configurar para servir arquivos estáticos da pasta Media
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(mediaPath),
    RequestPath = "/Media"
});

app.MapOpenApi("/swagger/{documentName}/swagger.json").AllowAnonymous();
app.MapGet("/swagger", () => Results.Content(ApiDocsHtml.SwaggerUi(), "text/html")).AllowAnonymous();
app.MapGet("/swagger/index.html", () => Results.Content(ApiDocsHtml.SwaggerUi(), "text/html")).AllowAnonymous();
app.MapGet("/docs/frontend", () => Results.Content(ApiDocsHtml.FrontendGuide(), "text/html")).AllowAnonymous();

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAll");
app.UseMiddleware<TokenValidationMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();
