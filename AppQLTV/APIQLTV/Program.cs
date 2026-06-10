using APIQLTV.Models;
using APIQLTV.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// OpenAPI mặc định của .NET
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Tránh lỗi vòng lặp tham chiếu khi generate Swagger
    c.CustomSchemaIds(type => type.FullName);
});
builder.Services.AddMemoryCache();

// ===== DATABASE MYSQL =====
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// ===== CORS CHO BLAZOR FE & ANGULAR =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins(
                "https://localhost:7028",
                "http://localhost:5246",
                "http://localhost:4200"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("LibrarianOrAdmin", policy =>
        policy.RequireRole("Librarian", "Admin"));
});

// ===== JWT AUTHENTICATION =====
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
    };
})
.AddCookie("ExternalCookie")
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration["GoogleAuth:ClientId"]!;
    options.ClientSecret = builder.Configuration["GoogleAuth:ClientSecret"]!;
    options.CallbackPath = "/signin-google";
    options.SignInScheme = "ExternalCookie";
    options.Scope.Add("email");
    options.Scope.Add("profile");

    options.Events.OnRedirectToAuthorizationEndpoint = context =>
    {
        var redirectUri = context.RedirectUri;
        if (!redirectUri.Contains("prompt="))
        {
            redirectUri += redirectUri.Contains("?") ? "&prompt=select_account" : "?prompt=select_account";
        }
        context.Response.Redirect(redirectUri);
        return Task.CompletedTask;
    };
});

builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<LibrarySettingService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting(); // Quan trọng: đặt trước UseCors

app.UseCors("AllowBlazorClient"); // Chỉ một policy duy nhất, bỏ "AllowAngular"

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();