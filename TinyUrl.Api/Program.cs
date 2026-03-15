using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using TinyUrl.Api.Data;
using TinyUrl.Api.Models;
using TinyUrl.Api.Services;


var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseInMemoryDatabase("TinyUrlDb"));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=tinyurl.db"));
builder.Services.AddScoped<ShortCodeService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


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
    if(existingUrl != null)
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
    .OrderByDescending(u=>u.CreatedAt)
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
