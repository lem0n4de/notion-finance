using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Notion.Client;
using NotionFinance;
using NotionFinance.Data;
using NotionFinance.Exceptions;
using NotionFinance.Services;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Debug()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Debug());

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlite($"Data Source={builder.Configuration["Sqlite:Users"]}"));

builder.Services.AddHostedService<NotionAutoUpdateService>();
builder.Services.AddScoped<INotionClient>(provider =>
{
    var httpContextAccessor = provider.GetService<IHttpContextAccessor>();
    if (httpContextAccessor == null) // Not in a request
    {
        
    }

    var emailClaim = httpContextAccessor!.HttpContext!.User.FindFirst(ClaimTypes.Email);
    if (emailClaim == null) throw new Exception(Messages.InvalidUser);
    var userDbContext = provider.GetService<UserDbContext>();
    var user = userDbContext!.Users.First(x => x.Email == emailClaim.Value);
    if (user.NotionAccessToken != null)
        return NotionClientFactory.Create(new ClientOptions() {AuthToken = user.NotionAccessToken});
    throw new NotionAccountNotConnectedException();
});
builder.Services.AddScoped<INotionService, NotionService>();
builder.Services.AddTransient<ICryptocurrencyService, CoinGeckoService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateLifetime = true,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // configure SwaggerDoc and others

    // add JWT Authentication
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter JWT Bearer token **_only_**",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer", // must be lower case
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {securityScheme, new string[] { }}
    });
});

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