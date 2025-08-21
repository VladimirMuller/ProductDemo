using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductDemo;
using ProductDemo.Models;
using ProductDemo.Repositories;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddMvc();
builder.Services.AddControllersWithViews();

builder.Services.AddCors(option =>
{
    option.AddPolicy("AllowClientJSApp",
            policy => policy.WithOrigins("http://localhost:5173") // TS/JS/React/Angular/VUE app URL
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
        );
});

builder.Services.AddDbContextFactory<ApplicationDbContext>(
       //options => options.UseSqlServer(
       //    @"Server=(localdb)\mssqllocaldb;Database=Test;ConnectRetryCount=0")
       );

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors("AllowClientJSApp");
//app.UseAuthentication();
//app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// http://localhost:5136/swagger/index.html
// http://localhost:5136/swagger/v1/swagger.json
