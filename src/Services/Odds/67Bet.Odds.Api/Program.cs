using _67Bet.Odds.Application;
using _67Bet.Odds.Infrastructure;
using _67Bet.Odds.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();
builder.Services.AddSingleton<_67Bet.Odds.Api.Services.ILiveMatchSimulator, _67Bet.Odds.Api.Services.SoccerSimulator>();
builder.Services.AddSingleton<_67Bet.Odds.Api.Services.ILiveMatchSimulator, _67Bet.Odds.Api.Services.BasketballSimulator>();
builder.Services.AddSingleton<_67Bet.Odds.Api.Services.ILiveMatchSimulator, _67Bet.Odds.Api.Services.EsportSimulator>();
builder.Services.AddSingleton<_67Bet.Odds.Api.Services.ILiveMatchSimulator, _67Bet.Odds.Api.Services.TennisSimulator>();
builder.Services.AddSingleton<_67Bet.Odds.Api.Services.ILiveMatchSimulator, _67Bet.Odds.Api.Services.DefaultSimulator>();
builder.Services.AddSingleton<_67Bet.Odds.Api.Services.MatchSimulatorFactory>();
builder.Services.AddHostedService<_67Bet.Odds.Api.Services.LiveTrackerBackgroundService>();
builder.Services.AddHostedService<_67Bet.Odds.Api.Services.OddsSyncBackgroundService>();

// Configure CORS (Oryginalna podwójna konfiguracja)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configure JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "67Bet Odds API", Version = "v1" });

    // Add JWT support to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Register layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<_67Bet.Odds.Infrastructure.Persistence.OddsDbContext>();
        context.Database.Migrate();
        Console.WriteLine("Odds database migrated successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error migrating odds database: {ex.Message}");
    }
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "67Bet Odds API v1");
    c.RoutePrefix = string.Empty;
});

// app.UseHttpsRedirection(); // Commented for local HTTP debugging

app.UseCors("AllowAll");
// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<_67Bet.Odds.Api.Hubs.LiveTrackerHub>("/api/liveTrackerHub");

app.Run();