using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using TinyUrl.Api.Models;

namespace TinyUrl.Api.Data
{
    //public class AppDbContext : DbContext
    //{
    //    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    //    {
    //    }
    //    public DbSet<Url> Urls { get; set; }
    //}

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Url> Urls { get; set; }
        
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);
        //    // Seed data
        //    modelBuilder.Entity<Url>().HasData(
        //        new Url
        //        {
        //            Id = 1,
        //            OriginalUrl = "https://www.example.com",
        //            ShortCode = "abc123",
        //            IsPrivate = false,
        //            Clicks = 0,
        //            CreatedAt = DateTime.UtcNow
        //        },
        //        new Url
        //        {
        //            Id = 2,
        //            OriginalUrl = "https://www.google.com",
        //            ShortCode = "def456",
        //            IsPrivate = true,
        //            Clicks = 0,
        //            CreatedAt = DateTime.UtcNow
        //        }
        //    );
        //}
    }

}