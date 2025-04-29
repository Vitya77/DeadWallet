using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using DeadWallet.DAL;
using DeadWallet.DAL.Models;
using Microsoft.AspNetCore.Identity;
using DeadWallet.BLL.Services;
using DeadWallet.DAL.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DeadWallet.DAL.Interfaces;
using DeadWallet.BLL.Interfaces;
using Azure.Identity;


var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    var keyVaultUri = new Uri("https://deadwallet.vault.azure.net/");
    builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());

    var config = builder.Configuration;
    builder.Configuration["ConnectionStrings:DeadWallerContextDev"] = config["db-connection-string"];
    builder.Configuration["Email:From"] = config["EmailFrom"];
    builder.Configuration["Email:Password"] = config["EmailPassword"];
    builder.Configuration["Email:SmtpHost"] = config["SmtpHost"];
    builder.Configuration["Email:SmtpPort"] = config["SmtpPort"];
}

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddDbContext<DeadWalletContext>(options =>
{
    var prodConnectionString = builder.Configuration.GetConnectionString("DeadWallerContextProd");
    var devConnectionString = builder.Configuration.GetConnectionString("DeadWallerContextDev");

    if (!string.IsNullOrEmpty(prodConnectionString))
    {
        options.UseNpgsql(prodConnectionString);
    }
    else if (!string.IsNullOrEmpty(devConnectionString))
    {
        options.UseSqlServer(devConnectionString);
    }
    else
    {
        throw new InvalidOperationException("No valid connection string found. Please configure 'DeadWallerContextProd' or 'DeadWallerContextDev' in appsettings.json.");
    }
});


// Password hasher injection
builder.Services.AddScoped<IPasswordHasher<DeadWalletUser>, PasswordHasher<DeadWalletUser>>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IEmailOtpRepository, EmailOtpRepository>();

builder.Services.AddScoped<UserService, UserService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// JWT auth injection
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddCookie("Cookies", options =>
{
    options.LoginPath = "/Auth/Login";
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT:SecretKey string not found.")))
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.Use(async (context, next) =>
{
    var token = context.Request.Cookies["AuthToken"];
    if (!string.IsNullOrEmpty(token))
    {
        context.Request.Headers.Authorization = "Bearer " + token;
    }
    await next();
});

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "addTransaction",
    pattern: "Transaction/AddTransaction",
    defaults: new { controller = "Transaction", action = "AddTransaction" });

app.MapControllerRoute(
    name: "editTag",
    pattern: "Tag/Edit/{id}",
    defaults: new { controller = "Tag", action = "Edit" });

app.MapControllerRoute(
    name: "tags",
    pattern: "Tag/{action=Manage}",
    defaults: new { controller = "Tag" });



app.MapStaticAssets();
app.Run();