using Microsoft.EntityFrameworkCore;
using TinyUrl.Api.Data;


var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseInMemoryDatabase("TinyUrlDb"));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=tinyurl.db"));

var app = builder.Build();


app.MapGet("/", () => "Tiny URL API Running");

app.Run();
