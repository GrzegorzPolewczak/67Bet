using _67Bet.Betting.Application;
using _67Bet.Betting.Infrastructure;
using _67Bet.Betting.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure CORS
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
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "67Bet Betting API", Version = "v1" });

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

// Migrate Database and Seed data
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<_67Bet.Betting.Infrastructure.Persistence.BettingDbContext>();
        // Auto-migrate database on startup
        context.Database.Migrate();
        Console.WriteLine("Database migrated successfully.");

        // Seed Virtual Racing Horses
        if (!context.Horses.Any())
        {
            context.Horses.AddRange(
                new _67Bet.Betting.Domain.Entities.VirtualRacing.Horse("Thunder", 8),
                new _67Bet.Betting.Domain.Entities.VirtualRacing.Horse("Lightning", 7),
                new _67Bet.Betting.Domain.Entities.VirtualRacing.Horse("Storm", 6),
                new _67Bet.Betting.Domain.Entities.VirtualRacing.Horse("Blizzard", 9),
                new _67Bet.Betting.Domain.Entities.VirtualRacing.Horse("Tornado", 5),
                new _67Bet.Betting.Domain.Entities.VirtualRacing.Horse("Hurricane", 8),
                new _67Bet.Betting.Domain.Entities.VirtualRacing.Horse("Avalanche", 7),
                new _67Bet.Betting.Domain.Entities.VirtualRacing.Horse("Typhoon", 8)
            );
            context.SaveChanges();
            Console.WriteLine("Successfully seeded initial virtual horses.");
        }

        // Seed Achievements
        if (!context.Achievements.Any())
        {
            context.Achievements.AddRange(
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("Debiutant Brąz", "Postaw 1 zakład", _67Bet.Betting.Domain.Enums.AchievementType.TotalBets, 1, "icon-bet-1"),
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("Debiutant Srebro", "Postaw 10 zakładów", _67Bet.Betting.Domain.Enums.AchievementType.TotalBets, 10, "icon-bet-10"),
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("Snajper", "Wygraj kupon z kursem min. 2.0", _67Bet.Betting.Domain.Enums.AchievementType.HighOdds, 2.0m, "icon-odds-2"),
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("Rekordzista", "Łączna suma wygranych 100 zł", _67Bet.Betting.Domain.Enums.AchievementType.TotalWinnings, 100, "icon-win-100")
            );
            context.SaveChanges();
            Console.WriteLine("Successfully seeded initial achievements.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error migrating or seeding data: {ex.Message}");
    }
}

app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "67Bet Betting API v1");
    c.RoutePrefix = string.Empty;
});

// // app.UseHttpsRedirection(); // Commented for local HTTP debugging

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
