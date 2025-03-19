using Azure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Configuration;
using System.Data;
using System.Reflection.Metadata;
using ProductDemo.Models;

namespace ProductDemo;

public class ApplicationDbContext : DbContext
{
    //private readonly IConfiguration _configuration;
    //private IDbConnection DbConnection { get; }

    //public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)
    //    : base(options)
    //{
    //    this._configuration = configuration;
    //    DbConnection = new SqlConnection(this._configuration.GetConnectionString("DBContext"));
    //}

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("memory");

        //optionsBuilder.UseSqlServer(DbConnection.ConnectionString);

        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Product> Products { get; set; }
}
