using Data.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Service.AuthService;
using Service.AuthService.AuthServiceExtension;
using Service.AuthService.Options;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

//database
builder.Services.AddDbContext<AuthDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionStrng"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 10,
                maxRetryDelay: TimeSpan.FromSeconds(1),
                errorNumbersToAdd: null);
        });
});




//add Auth
string Issuer = builder.Configuration.GetSection("AuthServiceOptions").GetValue<string>("Issuer")!;
string Audience = builder.Configuration.GetSection("AuthServiceOptions").GetValue<string>("Audience")!;
string SecretKey = builder.Configuration.GetSection("AuthServiceOptions").GetValue<string>("SecretKey")!;
builder.Services.AddAuth(Issuer, Audience, SecretKey);

builder.Services.Configure<AuthServiceOptions>(builder.Configuration.GetSection("AuthServiceOptions"));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireEditor", policy => policy.RequireRole("Editor", "Admin"));
    options.AddPolicy("RequireGod", policy => policy.RequireRole("God"));
});


//services
builder.Services.AddTransient<AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
