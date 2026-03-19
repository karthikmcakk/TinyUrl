using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using TinyUrl.Api.Data;
using TinyUrl.Api.Models;
using TinyUrl.Api.Services;
using Azure.Identity;

try
{
    var builder = WebApplication.CreateBuilder(args);

    var home = Environment.GetEnvironmentVariable("HOME") ?? "D:\\home";
#if RELEASE
    var dbPath = Path.Combine(home, "data", "tinyurl.db");
#else
    var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "tinyurl.db");
#endif

    Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

    builder.Services.AddDbContext<AppDbContext>(options =>
      {
          options.UseSqlite($"Data Source={dbPath}");
      });


    builder.Services.AddScoped<ShortCodeService>();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();


    // Configure CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DefaultPolicy", policy =>
        {
            policy.WithOrigins(
                "http://localhost:4200",             // Angular dev
                "https://ashy-grass-0dcc17500.1.azurestaticapps.net/" // production URL
            )
            .AllowAnyMethod()
            .AllowAnyHeader();
        });
    });

    var app = builder.Build();

    // Ensure database is created and apply pending migrations if any
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var db = services.GetRequiredService<AppDbContext>();
            var pending = db.Database.GetPendingMigrations();
            if (pending != null && pending.Any())
            {
                db.Database.Migrate();
                Console.WriteLine("Applied pending EF Core migrations.");
            }
            else
            {
                // If there are no migrations, ensure the database exists for the model
                db.Database.EnsureCreated();
                Console.WriteLine("Ensured database is created.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while creating or migrating the database: {ex.Message}");
            throw;
        }
    }
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    app.UseCors("DefaultPolicy");


    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }


    app.MapGet("/", () => "Tiny URL API Running");
    #region Main endpoint code
    app.MapPost("/api/urls", async (AppDbContext db, ShortCodeService shortCodeService, Url request) =>
    {
        if (!Uri.IsWellFormedUriString(request.OriginalUrl, UriKind.Absolute))
        {
            return Results.BadRequest("Invalid URL format.");
        }
        // check the original Url already exists
        var existingUrl = await db.Urls.FirstOrDefaultAsync(uu => uu.OriginalUrl == request.OriginalUrl);
        if (existingUrl != null)
        {
            return Results.Ok(new
            {
                existingUrl.Id,
                existingUrl.ShortCode,
                message = "URL already shortened",
            });
        }
        // Generate unique shortcode
        string shortCode = string.Empty;
        do
        {
            shortCode = shortCodeService.GenerateShortCode();
        }
        while (await db.Urls.AnyAsync(u => u.ShortCode == shortCode));
        var url = new Url
        {
            OriginalUrl = request.OriginalUrl,
            ShortCode = shortCode,
            IsPrivate = request.IsPrivate
            //message = "Short URL created successfully",
        };
        db.Urls.Add(url);
        await db.SaveChangesAsync();
        return Results.Ok(new { url.Id, url.ShortCode });
    });
    #endregion

    #region Redirct url from DB
    app.MapGet("/{code}", async (string code, AppDbContext db) =>
    {
        var url = await db.Urls.FirstOrDefaultAsync(x => x.ShortCode == code);

        if (url == null)
            return Results.NotFound();

        url.Clicks++;
        await db.SaveChangesAsync();

        return Results.Redirect(url.OriginalUrl);
    });
    #endregion

    #region Get all url
    app.MapGet("/api/urls", async (AppDbContext db) =>
    {
        var urls = await db.Urls
        .OrderByDescending(u => u.CreatedAt)
        .ToListAsync();
        return Results.Ok(urls);
    });
    #endregion

    #region GetSingleShortcode Details
    app.MapGet("api/urls/{Code}", async (string code, AppDbContext db) =>
    {
        var url = await db.Urls.FirstOrDefaultAsync(u => u.ShortCode == code);

        if (url == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(url);
    });

    #endregion

    #region Delete the shotcode
    app.MapDelete("api/urls/{code}", async (string code, AppDbContext db) =>
    {
        var url = await db.Urls.FirstOrDefaultAsync(u => u.ShortCode == code);
        if (url == null)
        {
            return Results.NotFound("URL not Found");
        }
        db.Urls.Remove(url);
        await db.SaveChangesAsync();

        return Results.Ok("Url deleted successfully");

    });
    #endregion

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}