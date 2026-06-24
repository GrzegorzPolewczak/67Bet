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
builder.Services.AddHostedService<_67Bet.Betting.Api.Services.EventSettlementBackgroundWorker>();

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
        var existingAchievements = context.Achievements.ToList();
        if (existingAchievements.Any(a => a.Name.Contains("Debiutant") || a.Name.Contains("Snajper") || a.Name.Contains("Rekordzista"))
            || !existingAchievements.Any(a => a.Type == _67Bet.Betting.Domain.Enums.AchievementType.PlinkoRounds)
            || existingAchievements.Any(a => a.Description.Contains("$")))
        {
            context.Achievements.RemoveRange(existingAchievements);
            var userAchievements = context.UserAchievements.ToList();
            context.UserAchievements.RemoveRange(userAchievements);
            context.SaveChanges();
            existingAchievements.Clear();
        }

        if (!context.Achievements.Any())
        {
            context.Achievements.AddRange(
                // TotalBets (First Bets)
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("First Bets (Bronze)", "Place 25 bets", _67Bet.Betting.Domain.Enums.AchievementType.TotalBets, 25, "icon-bet-25"),
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("First Bets (Silver)", "Place 100 bets", _67Bet.Betting.Domain.Enums.AchievementType.TotalBets, 100, "icon-bet-100"),
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("First Bets (Gold)", "Place 250 bets", _67Bet.Betting.Domain.Enums.AchievementType.TotalBets, 250, "icon-bet-250"),
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("First Bets (Diamond)", "Place 750 bets", _67Bet.Betting.Domain.Enums.AchievementType.TotalBets, 750, "icon-bet-750"),

                // HighOdds (Sniper)
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("Sniper (Bronze)", "Win a bet with odds at least 2.0", _67Bet.Betting.Domain.Enums.AchievementType.HighOdds, 2.0m, "icon-odds-2"),
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("Sniper (Silver)", "Win a bet with odds at least 5.0", _67Bet.Betting.Domain.Enums.AchievementType.HighOdds, 5.0m, "icon-odds-5"),
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("Sniper (Gold)", "Win a bet with odds at least 10.0", _67Bet.Betting.Domain.Enums.AchievementType.HighOdds, 10.0m, "icon-odds-10"),
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("Sniper (Diamond)", "Win a bet with odds at least 50.0", _67Bet.Betting.Domain.Enums.AchievementType.HighOdds, 50.0m, "icon-odds-50"),

                // TotalWinnings (High Roller)
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("High Roller (Bronze)", "Reach total winnings of 100 PLN", _67Bet.Betting.Domain.Enums.AchievementType.TotalWinnings, 100, "icon-win-100"),
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("High Roller (Silver)", "Reach total winnings of 500 PLN", _67Bet.Betting.Domain.Enums.AchievementType.TotalWinnings, 500, "icon-win-500"),
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("High Roller (Gold)", "Reach total winnings of 2500 PLN", _67Bet.Betting.Domain.Enums.AchievementType.TotalWinnings, 2500, "icon-win-2500"),
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("High Roller (Diamond)", "Reach total winnings of 10000 PLN", _67Bet.Betting.Domain.Enums.AchievementType.TotalWinnings, 10000, "icon-win-10000"),

                // LoginStreak (Daily Bettor)
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("Daily Bettor (Bronze)", "Log in for 3 consecutive days", _67Bet.Betting.Domain.Enums.AchievementType.LoginStreak, 3, "icon-streak-3"),
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("Daily Bettor (Silver)", "Log in for 7 consecutive days", _67Bet.Betting.Domain.Enums.AchievementType.LoginStreak, 7, "icon-streak-7"),
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("Daily Bettor (Gold)", "Log in for 14 consecutive days", _67Bet.Betting.Domain.Enums.AchievementType.LoginStreak, 14, "icon-streak-14"),
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("Daily Bettor (Diamond)", "Log in for 30 consecutive days", _67Bet.Betting.Domain.Enums.AchievementType.LoginStreak, 30, "icon-streak-30"),

                // PlinkoRounds (Plinko Master)
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("Plinko Master (Bronze)", "Play 25 Plinko rounds", _67Bet.Betting.Domain.Enums.AchievementType.PlinkoRounds, 25, "icon-plinko-25"),
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("Plinko Master (Silver)", "Play 100 Plinko rounds", _67Bet.Betting.Domain.Enums.AchievementType.PlinkoRounds, 100, "icon-plinko-100"),
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("Plinko Master (Gold)", "Play 250 Plinko rounds", _67Bet.Betting.Domain.Enums.AchievementType.PlinkoRounds, 250, "icon-plinko-250"),
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("Plinko Master (Diamond)", "Play 750 Plinko rounds", _67Bet.Betting.Domain.Enums.AchievementType.PlinkoRounds, 750, "icon-plinko-750"),

                // RouletteSpins (Wheel Spinner)
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("Wheel Spinner (Bronze)", "Spin the Roulette wheel 25 times", _67Bet.Betting.Domain.Enums.AchievementType.RouletteSpins, 25, "icon-roulette-25"),
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("Wheel Spinner (Silver)", "Spin the Roulette wheel 100 times", _67Bet.Betting.Domain.Enums.AchievementType.RouletteSpins, 100, "icon-roulette-100"),
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("Wheel Spinner (Gold)", "Spin the Roulette wheel 250 times", _67Bet.Betting.Domain.Enums.AchievementType.RouletteSpins, 250, "icon-roulette-250"),
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("Wheel Spinner (Diamond)", "Spin the Roulette wheel 750 times", _67Bet.Betting.Domain.Enums.AchievementType.RouletteSpins, 750, "icon-roulette-750"),

                // GreenRoulette (Lucky Zero)
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("Lucky Zero", "Land on green (0) in Roulette", _67Bet.Betting.Domain.Enums.AchievementType.GreenRoulette, 1, "icon-green-roulette"),

                // KycVerification (Verified Bettor)
                new _67Bet.Betting.Domain.Entities.Gamification.Achievement("Verified Bettor", "Verify your account identity (KYC)", _67Bet.Betting.Domain.Enums.AchievementType.KycVerification, 1, "icon-verified")
            );
            context.SaveChanges();
            Console.WriteLine("Successfully seeded initial English multi-stage achievements.");
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
