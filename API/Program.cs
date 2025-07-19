using API.Configurations;
using Application.Common.Settings;
using Application.Helpers;
using Application.Interfaces;
using Application.Interfaces.Services; // Ensure this is the correct namespace for IAuthApplicationService
using Infrastructure.Authentication;
using Infrastructure.Authentication.Configuration;
using Infrastructure.Common;
using Infrastructure.Common.Interfaces;
using Infrastructure.Persistence.Interfaces;
using Infrastructure.Persistence.Repositroies;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Skype.Infrastructure.Common;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
            builder =>
            {
                builder.WithOrigins("http://localhost:4200")
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials(); // Required for SignalR with credentials
            });
});



builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<FileStorageSettings>(builder.Configuration.GetSection("FileStorageSettings"));
builder.Services.AddScoped<PasswordHasher>(); /// Add PasswordHasher as a service
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddMemoryCache(); // Add memory cache for online users
builder.Services.AddSingleton<Application.Interfaces.Services.IOnlineUserCacheService, Infrastructure.Services.OnlineUserCacheService>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Application Related Services

builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
// Register services via extension methods
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();
builder.Services.AddSwagger();
builder.Services.ConfigureAuthentication(builder.Configuration);



try
{
   
var app = builder.Build();


// Configure the HTTP request pipeline.
app.UseSwagger();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        options.RoutePrefix = string.Empty;  // Set Swagger UI to be served at root (optional)
    });
}

app.UseHttpsRedirection();

app.UseStaticFiles(); // Already added by default

app.UseRouting();

app.UseCors("AllowAngularApp");

//app.UseAuthentication();
app.UseAuthorization();
app.MapHub<ChatHub>("/chatHub"); // Your Hub endpoint
app.MapControllers();

app.Run();
    Log.Information("This is an information test message");
    Log.Warning("This is a warning test message");
    Log.Error("This is an error test message");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed");
}
finally
{
    Log.CloseAndFlush();
}
