using Application.Abstractions.Services;
using Application.DependencyInjection;
using Hangfire;
using Hangfire.PostgreSql;
using Infrastructure.DependencyInjection;
using Infrastructure.Identity.Models;
using Infrastructure.Persistence.Context;
using Integration.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.Reflection;
using System.Security.Claims;
using WebApi;
using WebApi.Hubs;
using WebApi.Middleware;
using WebApi.Services;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    // .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day) // Günlük dosyalara loglama
    // Daha gelişmiş yapılandırma için appsettings.json kullanılabilir (aşağıda)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    var corsSettings = builder.Configuration.GetSection("CorsSettings");
    var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>() ?? new string[0]; // Null kontrolü

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigins", // Politikaya bir isim veriyoruz
            policyBuilder =>
            {
                if (allowedOrigins.Any()) // Eğer appsettings'de origin tanımlanmışsa
                {
                    policyBuilder.WithOrigins(allowedOrigins)
                                 .AllowAnyHeader()
                                 .AllowAnyMethod();
                }
                else if (builder.Environment.IsDevelopment())
                {
                    policyBuilder.AllowAnyOrigin()
                                 .AllowAnyHeader()
                                 .AllowAnyMethod();
                }
            });
    });

    // --- Serilog'u ASP.NET Core loglama sistemine entegre et ---
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration) // appsettings.json'dan oku
        .ReadFrom.Services(services) // DI servislerini kullan (örn: IHttpContextAccessor)
        .Enrich.FromLogContext() // Log Context'ten gelen bilgileri ekle
        .WriteTo.Console()); // Konsola yaz (appsettings'de de olabilir)
                             // .WriteTo.File(...) // Dosyaya yaz (appsettings'de de olabilir)

    // --- HANGFIRE KAYITLARI ---
    builder.Services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("HangfireConnection")))
    );
    builder.Services.AddHangfireServer();

    builder.Services.AddHttpContextAccessor();

    #region OpenIddict + Authentication
    builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 6;
        options.User.RequireUniqueEmail = true;

        // OpenIddict'in kullanıcı bilgilerini (claim) yönetmesi için
        options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;
        options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;
        options.ClaimsIdentity.EmailClaimType = ClaimTypes.Email;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager<SignInManager<ApplicationUser>>()
    .AddDefaultTokenProviders();

    // --- OpenIddict (Auth Server + Validation) ---
    builder.Services.AddOpenIddict()
        // 1. Core
        .AddCore(options =>
        {
            options.UseEntityFrameworkCore()
                   .UseDbContext<ApplicationDbContext>()
                   .ReplaceDefaultEntities<Guid>();
        })
        // 2. Server (Token Basma)
        .AddServer(options =>
        {
            options.SetTokenEndpointUris("/connect/token"); // Token alacağımız endpoint

            // Password Flow (Mobil uygulama için)
            options.AllowPasswordFlow();
            options.AllowRefreshTokenFlow();

            //options.AddDevelopmentEncryptionCertificate() // Sadece Development için
            //       .AddDevelopmentSigningCertificate();  // Sadece Development için

            options.AddEphemeralEncryptionKey()
                    .AddEphemeralSigningKey();

            options.UseAspNetCore()
                   .EnableTokenEndpointPassthrough();
        })
        .AddValidation(options =>
        {
            options.UseLocalServer();
            options.UseAspNetCore();
        });

    // --- Authentication (API'yi Korumak için) ---
    // OpenIddict Validation'a ek olarak, API'mizin [Authorize]
    // attribute'unu tanıması için bunu ekliyoruz.
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = OpenIddict.Validation.AspNetCore.OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIddict.Validation.AspNetCore.OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    });
    #endregion

    builder.Services.AddAuthorization();
    builder.Services.AddHostedService<OpenIddictWorker>();
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddIntegrationServices(builder.Configuration);
    builder.Services.AddDistributedMemoryCache();

    #region Redis Info
    // Production için (Redis'e geçmek istersen):
    // 1. Microsoft.Extensions.Caching.StackExchangeRedis paketini ekle
    // 2. builder.Services.AddDistributedMemoryCache(); satırını comment'le
    // 3. builder.Services.AddStackExchangeRedisCache(options =>
    //    {
    //        options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
    //        options.InstanceName = "ChatApp_"; // Cache key'lerine ön ek
    //    });
    // 4. appsettings.json'a "RedisConnection": "localhost:6379" gibi bir connection string ekle.
    #endregion
    builder.Services.AddSignalR();

    builder.Services.AddScoped<INotificationService, SignalRNotificationService>();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "ChatApp API", Version = "v1" }); // İsteğe bağlı API başlığı

        var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }

        // Eğer Application katmanındaki DTO'larda da yorumlar varsa, onun XML dosyasını da ekleyebilirsiniz:
        var appXmlFilename = $"{typeof(Application.DependencyInjection.DependencyInjection).Assembly.GetName().Name}.xml";
        var appXmlPath = Path.Combine(AppContext.BaseDirectory, appXmlFilename);
        if (File.Exists(appXmlPath))
        {
            options.IncludeXmlComments(appXmlPath);
        }

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Lütfen 'Bearer ' kelimesini ekleyerek geçerli bir JWT token girin",
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

    builder.Services.AddTransient<GlobalExceptionHandlingMiddleware>();

    var app = builder.Build();

    //if (app.Environment.IsDevelopment())
    //{
    //    app.UseSwagger();
    //    app.UseSwaggerUI();
    //}
    app.UseDeveloperExceptionPage(); // Hata olursa tarayıcıda detaylı görelim (Çok Önemli!)
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ChatApp API V1");
        c.RoutePrefix = string.Empty; // Swagger'ı ana sayfada (root) açar
    });

    app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

    // 2. Loglamayı hemen başlat (Kimlik doğrulamadan red yiyenleri de görmek için)
    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseCors("AllowSpecificOrigins");

    // 3. SENİN TOKEN MIDDLEWARE'İN (Yeri Burası Doğru)
    app.Use(async (context, next) =>
    {
        var accessToken = context.Request.Query["access_token"];
        var path = context.Request.Path;
        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chathub"))
        {
            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                context.Request.Headers.Append("Authorization", "Bearer " + accessToken);
            }
        }
        await next();
    });

    // 4. Auth İşlemleri
    app.UseAuthentication();
    app.UseAuthorization();

    // 5. Endpointler
    app.MapControllers();
    app.MapHub<ChatHub>("/chathub");
    // Hangfire Dashboard'u /hangfire adresinde aktif et
    // TODO: Production'da buraya bir yetkilendirme filtresi eklenmelidir!
    // Örn: app.MapHangfireDashboard("/hangfire", new DashboardOptions
    // {
    //    Authorization = new [] { new MyHangfireAuthorizationFilter() }
    // });
    app.MapHangfireDashboard("/hangfire");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

