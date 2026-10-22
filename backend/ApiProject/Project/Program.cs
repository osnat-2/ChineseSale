using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Project;
using Project.Bll;
using Project.Bll.Interfaces;
using Project.Dal;
using Project.Dal.Interfaces;
using Project.Json;
using Project.Middlewares;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ����� Serilog ������ ����� ������ ���
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(AppContext.BaseDirectory, "Logs", "log-.txt"),
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    options.SerializerSettings.Converters.Add(new DateOnlyJsonConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Add JWT Bearer authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
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
            new string[] { }
        }
    });
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be configured.");
}

builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(connectionString));

// Dependency Injection Scopes
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<ICardDal, CardDal>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryDal, CategoryDal>();
builder.Services.AddScoped<ILotteryService, LotteryService>();
builder.Services.AddScoped<ILotteryDal, LotteryDal>();
builder.Services.AddScoped<IPresentService, PresentService>();
builder.Services.AddScoped<IPresentDal, PresentDal>();
builder.Services.AddSingleton<PresentImageStorage>();
builder.Services.AddScoped<PresentResponseMapper>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IRoleDal, RoleDal>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserDal, UserDal>();
builder.Services.AddScoped<IWinnerService, WinnerService>();
builder.Services.AddScoped<IWinnerDal, WinnerDal>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Configure JWTSettings
builder.Services.Configure<JWTSettings>(builder.Configuration.GetSection("JWTSettings"));

// Configure JWT authentication
var jwtSection = builder.Configuration.GetSection("JWTSettings");
var jwtSettings = jwtSection.Get<JWTSettings>();
if (jwtSettings == null ||
    string.IsNullOrWhiteSpace(jwtSettings.SecretKey) ||
    Encoding.UTF8.GetByteCount(jwtSettings.SecretKey) < 18 ||
    string.IsNullOrWhiteSpace(jwtSettings.Issuer) ||
    string.IsNullOrWhiteSpace(jwtSettings.Audience) ||
    jwtSettings.ExpirationInMinutes <= 0)
{
    throw new InvalidOperationException(
        "JWTSettings must include a secret of at least 32 bytes, issuer, audience, and positive expiration.");
}

var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
    });

var allowedHosts = builder.Configuration["AllowedHosts"]?
    .Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? [];
if (allowedHosts.Length == 0 || allowedHosts.Any(host => host.Contains('*')))
{
    throw new InvalidOperationException("AllowedHosts must contain explicit host names for this environment.");
}

// var allowedOrigins = builder.Configuration
//     .GetSection("Cors:AllowedOrigins")
//     .Get<string[]>() ?? [];
// if (allowedOrigins.Length == 0 ||
//     allowedOrigins.Any(origin =>
//         !Uri.TryCreate(origin, UriKind.Absolute, out var uri) ||
//         (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
//         !string.Equals(origin, uri.GetLeftPart(UriPartial.Authority), StringComparison.OrdinalIgnoreCase)))
// {
//     throw new InvalidOperationException("Cors:AllowedOrigins must contain explicit HTTP or HTTPS origins.");
// }

builder.Services.AddCors(options =>
{
    options.AddPolicy("MyAllowSpecificOrigins",
        policy => policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    await scope.ServiceProvider
        .GetRequiredService<IRoleService>()
        .SeedDefaultRolesAsync();
}

// Configure the HTTP request pipeline.
app.UseRequestLogging();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// Enable CORS middleware
app.UseCors("MyAllowSpecificOrigins");
app.UseHttpsRedirection();
app.UseStaticFiles();


app.UseAuthentication(); // 1. ���� ������� �� ������
app.UseAuthorization();

app.MapControllers();

app.Run();