using CinemaxAPI.Data;
using CinemaxAPI.Mappings;
using CinemaxAPI.Middlewares;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Repositories;
using CinemaxAPI.Repositories.Impl;
using CinemaxAPI.Services;
using CinemaxAPI.Services.Impl;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using sib_api_v3_sdk.Client;

var builder = WebApplication.CreateBuilder(args);

// add cors policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClientWebsite", policy =>
    {
        policy.WithOrigins(builder.Configuration["CinemaxClients:Website"] ?? "http://localhost:3001")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();

    });
});

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

// configure Brevo API client
Configuration.Default.ApiKey.Add("api-key", builder.Configuration["BrevoApi:ApiKey"]);

// declare dependency injections
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// add AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfiles));

// add db context
builder.Services.AddDbContext<CinemaxServerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CinemaxConnectionString")));

// register Identity services
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredUniqueChars = 1;

    options.SignIn.RequireConfirmedEmail = true;
}).AddEntityFrameworkStores<CinemaxServerDbContext>()
    .AddDefaultTokenProviders();

// add JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"] ?? "default_secret_key")),
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandler>();
app.UseHttpsRedirection();
app.UseCors("AllowClientWebsite");
app.UseAuthentication();
app.UseAuthorization();

// serving static files
//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "Images")),
//    RequestPath = "/Images"
//});


app.MapControllers();

app.Run();
