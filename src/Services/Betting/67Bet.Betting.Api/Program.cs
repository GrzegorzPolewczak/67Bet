using _67Bet.Betting.Application;
using _67Bet.Betting.Infrastructure;
using _67Bet.Betting.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

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

// Seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<_67Bet.Betting.Infrastructure.Persistence.BettingDbContext>();
    if (!context.Events.Any())
    {
        var footballSport = new _67Bet.Betting.Domain.Entities.Sport("Football");
        var basketballSport = new _67Bet.Betting.Domain.Entities.Sport("Basketball");
        var mmaSport = new _67Bet.Betting.Domain.Entities.Sport("MMA");
        context.AddRange(footballSport, basketballSport, mmaSport);

        // Football
        var event1 = new _67Bet.Betting.Domain.Entities.Event("Real Madrid vs Barcelona", footballSport.Id, "La Liga", DateTime.Now.AddHours(5));
        var market1 = new _67Bet.Betting.Domain.Entities.Market(event1.Id, "Match Result");
        market1.AddOutcome("1", 2.15m);
        market1.AddOutcome("X", 3.40m);
        market1.AddOutcome("2", 3.10m);
        
        var event2 = new _67Bet.Betting.Domain.Entities.Event("Manchester City vs Arsenal", footballSport.Id, "Premier League", DateTime.Now.AddHours(2));
        var market2 = new _67Bet.Betting.Domain.Entities.Market(event2.Id, "Match Result");
        market2.AddOutcome("1", 1.85m);
        market2.AddOutcome("X", 3.75m);
        market2.AddOutcome("2", 4.20m);

        // Basketball
        var event4 = new _67Bet.Betting.Domain.Entities.Event("Los Angeles Lakers vs Boston Celtics", basketballSport.Id, "NBA", DateTime.Now.AddHours(10));
        var market4 = new _67Bet.Betting.Domain.Entities.Market(event4.Id, "Match Winner (Incl. OT)");
        market4.AddOutcome("Lakers", 1.95m);
        market4.AddOutcome("Celtics", 1.85m);

        var event5 = new _67Bet.Betting.Domain.Entities.Event("Golden State Warriors vs Phoenix Suns", basketballSport.Id, "NBA", DateTime.Now.AddHours(14));
        var market5 = new _67Bet.Betting.Domain.Entities.Market(event5.Id, "Match Winner (Incl. OT)");
        market5.AddOutcome("Warriors", 1.65m);
        market5.AddOutcome("Suns", 2.25m);

        // MMA
        var event6 = new _67Bet.Betting.Domain.Entities.Event("Jon Jones vs Tom Aspinall", mmaSport.Id, "UFC 309", DateTime.Now.AddDays(2));
        var market6 = new _67Bet.Betting.Domain.Entities.Market(event6.Id, "Fight Result");
        market6.AddOutcome("Jones", 1.75m);
        market6.AddOutcome("Aspinall", 2.10m);

        var event7 = new _67Bet.Betting.Domain.Entities.Event("Islam Makhachev vs Ciryl Gane", mmaSport.Id, "UFC Fight Night", DateTime.Now.AddDays(14));
        var market7 = new _67Bet.Betting.Domain.Entities.Market(event7.Id, "Fight Result");
        market7.AddOutcome("Makhachev", 1.45m);
        market7.AddOutcome("Gane", 2.80m);

        context.Events.AddRange(event1, event2, event4, event5, event6, event7);
        context.AddRange(market1, market2, market4, market5, market6, market7);

        context.SaveChanges();
    }
}

app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Commented for local HTTP debugging

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
