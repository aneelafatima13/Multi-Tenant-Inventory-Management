using BAL;
using DAL;
using Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Multi_Tenant_Inventory_Management_Web_API;

var builder = WebApplication.CreateBuilder(args);

// 1. Connection String (Make sure this is in your appsettings.json)
// 1. Get the string
var connectionString = builder.Configuration.GetConnectionString("Multi-Tenant-Inventory-Management-Db");

// 2. Register with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString,
        b => b.MigrationsAssembly("Multi_Tenant_Inventory_Management_Web_API"))); // Ensure migrations land in the API project

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantService, ApiTenantService>();
// 3. Register your N-Tier Layers
builder.Services.AddScoped<TenantDAL>();
builder.Services.AddScoped<TenantBAL>();

// 4. Add Services (Standard)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 5. Add CORS (Important: This allows your MVC project to talk to this API)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMVCApp",
        policy => policy.AllowAnyOrigin() // In production, limit this to your MVC URL
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 6. Use CORS Policy
app.UseCors("AllowMVCApp");

app.UseAuthorization();

app.MapControllers();

app.Run();