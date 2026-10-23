using System.Reflection;
using System.Text.Json;
using AcademicApi.Converters;
using AcademicApi.Data;
using AcademicApi.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' tidak ditemukan.");

builder.Services.AddDbContext<AcademicDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new TrimmedStringJsonConverter());
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = CreateValidationErrorResponse;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Migration diterapkan otomatis agar API cukup dijalankan dengan `dotnet run`.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AcademicDbContext>();
    db.Database.Migrate();
}

// Semua error tak terduga tetap dikembalikan dalam format JSON yang konsisten.
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Terjadi kesalahan yang tidak terduga.");

        if (!context.Response.HasStarted)
        {
            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ErrorResponse
            {
                Message = "Terjadi kesalahan pada server."
            });
        }
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

static BadRequestObjectResult CreateValidationErrorResponse(ActionContext context)
{
    var errors = context.ModelState
        .Where(entry => entry.Value?.Errors.Count > 0)
        .ToDictionary(
            entry => ToCamelCasePath(entry.Key),
            entry => entry.Value!.Errors
                .Select(ToIndonesianMessage)
                .Distinct()
                .ToArray());

    return new BadRequestObjectResult(new ErrorResponse
    {
        Message = "Data tidak valid.",
        Errors = errors
    });
}

static string ToCamelCasePath(string key)
{
    if (string.IsNullOrWhiteSpace(key) || key == "$")
    {
        return "body";
    }

    return string.Join(
        '.',
        key.Split('.', StringSplitOptions.RemoveEmptyEntries)
            .Select(JsonNamingPolicy.CamelCase.ConvertName));
}

static string ToIndonesianMessage(Microsoft.AspNetCore.Mvc.ModelBinding.ModelError error)
{
    if (error.Exception is JsonException ||
        error.ErrorMessage.Contains("JSON", StringComparison.OrdinalIgnoreCase))
    {
        return "Format data JSON tidak valid.";
    }

    if (error.ErrorMessage.Contains("non-empty request body", StringComparison.OrdinalIgnoreCase))
    {
        return "Body request wajib diisi.";
    }

    if (error.ErrorMessage.Contains("request field is required", StringComparison.OrdinalIgnoreCase))
    {
        return "Data request wajib diisi.";
    }

    return string.IsNullOrWhiteSpace(error.ErrorMessage)
        ? "Data tidak valid."
        : error.ErrorMessage;
}

public partial class Program
{
}
