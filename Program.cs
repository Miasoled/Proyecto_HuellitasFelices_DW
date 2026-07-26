using HuellitasFelices.Data;
using HuellitasFelices.Services;
using HuellitasFelices.Services.PaymentGateway;
using HuellitasFelices.Settings;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

// Permite enviar DateTime con Kind=Unspecified a PostgreSQL (legacy behavior)
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Registrar DbContext con PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar Identity con MFA y lockout
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Contraseña
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;

    // Confirmación
    options.SignIn.RequireConfirmedAccount = true;

    // Lockout por intentos fallidos
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;

    // MFA
    options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddRoles<IdentityRole>();

// Autenticación externa con Google
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
        var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
        {
            options.ClientId = googleClientId;
            options.ClientSecret = googleClientSecret;
            options.CallbackPath = "/signin-google";
        }
    });

// Configurar rutas de login
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.FromMinutes(5);
});

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailSender, GmailEmailSender>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHostedService<EmailWorker>();

// ── Sesión distribuida + Data Protection (Redis en Docker Swarm) ──
var redisUrl = builder.Configuration["REDIS_URL"];
if (!string.IsNullOrEmpty(redisUrl))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisUrl;
        options.InstanceName = "Huellitas_";
    });

    // Data Protection compartido entre réplicas (clave para MFA/TOTP)
    builder.Services.AddDataProtection()
        .PersistKeysToStackExchangeRedis(
            ConnectionMultiplexer.Connect(redisUrl), "Huellitas-DataProtection-Keys")
        .SetApplicationName("HuellitasFelices");
}
else
{
    builder.Services.AddDistributedMemoryCache();

    var dataProtectionKeysPath = builder.Configuration["DataProtection:KeysPath"];
    if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath))
    {
        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
            .SetApplicationName("HuellitasFelices");
    }
}
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<ICarritoService, CarritoService>();
builder.Services.AddScoped<IAccountService, AccountService>();

// ── IA / Ollama ──────────────────────────────────────────────────────
builder.Services.Configure<AiSettings>(builder.Configuration.GetSection("AI"));
builder.Services.AddHttpClient<IAIService, OllamaService>(client =>
{
    var settings = builder.Configuration.GetSection("AI").Get<AiSettings>() ?? new AiSettings();
    client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
});
builder.Services.AddScoped<IContextProviderService, ContextProviderService>();

// ── Pasarelas de pago ───────────────────────────────────────────────
builder.Services.Configure<PaymentSettings>(builder.Configuration.GetSection("PaymentSettings"));
builder.Services.Configure<PayPhoneSettings>(builder.Configuration.GetSection("PayPhone"));
builder.Services.AddHttpClient<IPaymentGateway, PayPalPaymentGateway>();
builder.Services.AddHttpClient<IPaymentGateway, PayPhonePaymentGateway>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddHostedService<PaymentExpirationWorker>();

// MVC
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapStaticAssets();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Ejecutar seeder
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (app.Configuration.GetValue<bool>("Database:AutoMigrate"))
        await context.Database.MigrateAsync();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await SeedData.Initialize(context, userManager, roleManager);
    await CargaMasivaService.GenerarDatos(context);
}

app.Run();
