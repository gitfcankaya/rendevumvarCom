using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using RendevumVar.Infrastructure.Data;
using RendevumVar.Infrastructure.Data.Seeders;
using RendevumVar.Infrastructure.Repositories;
using RendevumVar.Application.Interfaces;
using RendevumVar.Application.Services;
using RendevumVar.Core.Repositories;
using RendevumVar.API.Middleware;
using RendevumVar.API.Authorization;
using RendevumVar.Core.Constants;
using RendevumVar.Core.Configuration;
using Serilog;
using AspNetCoreRateLimit;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// Configure Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Configure options
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Configure Entity Framework Core with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
builder.Services.AddScoped(typeof(RendevumVar.Core.Repositories.IRepository<>), typeof(RendevumVar.Infrastructure.Repositories.Repository<>));
builder.Services.AddScoped<RendevumVar.Core.Repositories.ISubscriptionPlanRepository, RendevumVar.Infrastructure.Repositories.SubscriptionPlanRepository>();
builder.Services.AddScoped<RendevumVar.Core.Repositories.ITenantSubscriptionRepository, RendevumVar.Infrastructure.Repositories.TenantSubscriptionRepository>();
builder.Services.AddScoped<RendevumVar.Core.Repositories.IInvoiceRepository, RendevumVar.Infrastructure.Repositories.InvoiceRepository>();

// Phase 2: Staff Management Repositories
builder.Services.AddScoped<RendevumVar.Core.Repositories.IStaffRepository, RendevumVar.Infrastructure.Repositories.StaffRepository>();
builder.Services.AddScoped<RendevumVar.Core.Repositories.IRoleRepository, RendevumVar.Infrastructure.Repositories.RoleRepository>();
builder.Services.AddScoped<RendevumVar.Core.Repositories.IStaffScheduleRepository, RendevumVar.Infrastructure.Repositories.StaffScheduleRepository>();
builder.Services.AddScoped<RendevumVar.Core.Repositories.ITimeOffRequestRepository, RendevumVar.Infrastructure.Repositories.TimeOffRequestRepository>();

// Phase 3: Salon & Service Management Repositories
builder.Services.AddScoped<RendevumVar.Core.Repositories.ISalonRepository, RendevumVar.Infrastructure.Repositories.SalonRepository>();
builder.Services.AddScoped<RendevumVar.Core.Repositories.IServiceRepository, RendevumVar.Infrastructure.Repositories.ServiceRepository>();
builder.Services.AddScoped<RendevumVar.Core.Repositories.IServiceCategoryRepository, RendevumVar.Infrastructure.Repositories.ServiceCategoryRepository>();

// Phase 4: Appointment Management Repositories
builder.Services.AddScoped<RendevumVar.Core.Repositories.IAppointmentRepository, RendevumVar.Infrastructure.Repositories.AppointmentRepository>();

// Register application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IServiceCategoryService, ServiceCategoryService>();
builder.Services.AddScoped<RendevumVar.Application.Services.ISubscriptionService, RendevumVar.Application.Services.SubscriptionService>();

// Phase 2: Staff Management Services
builder.Services.AddScoped<RendevumVar.Application.Services.IEmailService, RendevumVar.Application.Services.EmailService>();
builder.Services.AddScoped<RendevumVar.Application.Services.IStaffService, RendevumVar.Application.Services.StaffService>();
builder.Services.AddScoped<RendevumVar.Application.Services.IScheduleService, RendevumVar.Application.Services.ScheduleService>();
builder.Services.AddScoped<RendevumVar.Application.Services.ITimeOffService, RendevumVar.Application.Services.TimeOffService>();

// Phase 3: Salon & Service Management Services
builder.Services.AddScoped<RendevumVar.Application.Interfaces.ISalonService, RendevumVar.Application.Services.SalonService>();
builder.Services.AddScoped<RendevumVar.Application.Interfaces.IImageService, RendevumVar.Application.Services.ImageService>();

// Phase 4: Appointment Management Services
builder.Services.AddScoped<RendevumVar.Application.Interfaces.IAppointmentService, RendevumVar.Application.Services.AppointmentService>();
builder.Services.AddScoped<RendevumVar.Application.Interfaces.IAvailabilityService, RendevumVar.Application.Services.AvailabilityService>();
builder.Services.AddScoped<RendevumVar.Application.Interfaces.INotificationService, RendevumVar.Application.Services.NotificationService>();

// Phase 5: Review & Ratings Services
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IReviewService, ReviewService>();

// Phase 10: Payment Services
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
// Configure payment gateway (FakePOS by default, can be changed to PayTR in production)
var paymentConfig = builder.Configuration.GetSection("PaymentConfiguration").Get<PaymentConfiguration>();
if (paymentConfig?.DefaultGateway == "PayTR")
{
    builder.Services.AddScoped<IPaymentGateway, PayTRGateway>();
    builder.Services.Configure<PayTRConfiguration>(builder.Configuration.GetSection("PayTRConfiguration"));
}
else
{
    builder.Services.AddScoped<IPaymentGateway, FakePaymentGateway>();
}
builder.Services.Configure<PaymentConfiguration>(builder.Configuration.GetSection("PaymentConfiguration"));
builder.Services.AddHttpClient("PayTR");

// Phase 11: Analytics Services
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

// Background Jobs
builder.Services.AddHostedService<RendevumVar.API.BackgroundJobs.AppointmentReminderJob>();


// Register background jobs
builder.Services.AddHostedService<RendevumVar.API.BackgroundJobs.TrialExpirationJob>();
builder.Services.AddHostedService<RendevumVar.API.BackgroundJobs.BillingCycleJob>();
builder.Services.AddHostedService<RendevumVar.API.BackgroundJobs.OverdueInvoiceJob>();

// Configure SignalR for real-time updates
builder.Services.AddSignalR();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// Configure Authorization with Permission-based policies
builder.Services.AddAuthorization(options =>
{
    // Add permission-based policies dynamically
    foreach (var permission in RendevumVar.Core.Constants.Permissions.GetAllPermissions())
    {
        options.AddPolicy(permission, policy =>
            policy.Requirements.Add(new RendevumVar.API.Authorization.PermissionRequirement(permission)));
    }
});

// Register authorization handler
builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, RendevumVar.API.Authorization.PermissionAuthorizationHandler>();


// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "RendevumVar API",
        Version = "v1",
        Description = "SaaS Salon Appointment System API"
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Required for SignalR
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RendevumVar API v1");
    });
}

app.UseCors("AllowFrontend");

// Use IP Rate Limiting
app.UseIpRateLimiting();

// Test database connection
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        await context.Database.CanConnectAsync();
        app.Logger.LogInformation("Database connection successful");
        app.Logger.LogInformation("Database setup completed");

        // Seed development data
        await RendevumVar.Infrastructure.Data.Seeders.DataSeeder.SeedAsync(context);
        app.Logger.LogInformation("Development seed data created");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Database connection failed");
    }
}

app.UseAuthentication();
app.UseAuthorization();

// Use tenant resolution middleware after authentication
app.UseTenantResolution();

// Use subscription enforcement middleware after authentication
app.UseSubscriptionEnforcement();

app.MapControllers();

// Map SignalR Hub
app.MapHub<RendevumVar.API.Hubs.AppointmentHub>("/hubs/appointments");

// Simple test endpoint
app.MapGet("/", () => "RendevumVar API is running!");
app.MapGet("/health", () => "Healthy");

app.Logger.LogInformation("RendevumVar API is starting...");

app.Run();
