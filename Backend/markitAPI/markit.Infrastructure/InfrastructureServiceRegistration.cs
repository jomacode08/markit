using markit.Application.Contracts.Authentication;
using markit.Application.Contracts.Authentication.Demo;
using markit.Application.Contracts.Authentication.ExternalLogin;
using markit.Application.Contracts.GitHub;
using markit.Application.Contracts.Google;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Contracts.Settings;
using markit.Application.Helpers;
using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.GitHub;
using markit.Application.Models.Authentication.Google;
using markit.Application.Models.Settings;
using markit.Application.Models.Settings.RateLimiting;
using markit.Infrastructure.Persistence.EF;
using markit.Infrastructure.Repositories;
using markit.Infrastructure.Repositories.Common;
using markit.Infrastructure.Security.Services;
using markit.Infrastructure.Security.Services.Demo;
using markit.Infrastructure.Security.Services.ExternalLogin;
using markit.Infrastructure.Security.Services.GitHub;
using markit.Infrastructure.Security.Services.Google;
using markit.Infrastructure.Security.Services.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using static markit.Application.Helpers.GeneralConstant;
using static markit.Application.Helpers.GeneralConstant.Configuration;

namespace markit.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            services
                .AddDataBasePersistence(configuration)
                .AddAuthentication(configuration)
                .AddAuthorization(configuration)
                .AddRateLimiter(configuration);
            return services;
        }

        private static IServiceCollection AddDataBasePersistence(this IServiceCollection services, IConfiguration configuration)
        {
            string connString = configuration.GetConnectionString(CONN_STRING_SECTION_NAME) ?? "";

            // Bind UserDefaultSettings configuration data
            var userDefaultSettings = new UserDefaultSettings();
            services.Configure<UserDefaultSettings>(configuration.GetSection(USER_DEFAULT_SECTION_NAME));
            configuration.Bind(USER_DEFAULT_SECTION_NAME, userDefaultSettings);

            // Database connection
            services.AddDbContext<MarkitDbContext>(
                options => options
                    .UseNpgsql(connString, o => o.SetPostgresVersion(17, 0))
                    .UseSnakeCaseNamingConvention()
                    .ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning))
            );

            services.AddHttpContextAccessor();
            services.AddTransient<SessionService>();
            services.AddScoped<ISettingsService, SettingsService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IAsyncRepository<>), typeof(BaseRepository<>));

            return services;
        }

        private static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind configuration data classes
            var jwtSettings = new JwtSettings();
            services.Configure<JwtSettings>(configuration.GetSection(JWT_SECTION_NAME));
            configuration.Bind(JWT_SECTION_NAME, jwtSettings);

            var googleAuthSettings = new GoogleAuthSettings();
            services.Configure<GoogleAuthSettings>(configuration.GetSection(GOOGLE_AUTH_SECTION_NAME));
            configuration.Bind(GOOGLE_AUTH_SECTION_NAME, googleAuthSettings);

            var gitHubAuthSettings = new GitHubAuthSettings();
            services.Configure<GitHubAuthSettings>(configuration.GetSection(GITHUB_AUTH_SECTION_NAME));
            configuration.Bind(GITHUB_AUTH_SECTION_NAME, gitHubAuthSettings);

            var spaSettings = new SpaSettings();
            services.Configure<SpaSettings>(configuration.GetSection(SPA_SECTION_NAME));
            configuration.Bind(SPA_SECTION_NAME, spaSettings);

            // Identity configuration
            services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<MarkitDbContext>()
                .AddDefaultTokenProviders();

            // Inject authentication services
            services.AddScoped<IAppUserService, AppUserService>();
            services.AddScoped<IDemoService, DemoService>();
            services.AddScoped<IOnboardingSeedService, OnboardingSeedService>();
            services.AddScoped<IGoogleApiService, GoogleApiService>();
            services.AddScoped<IGitHubApiService, GitHubApiService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<IExternalIdentifierService, ExternalIdentifierService>();
            services.AddScoped<IExternalTokenService, ExternalTokenService>();
            services.AddTransient<IExternalLoginService, ExternalLoginService>();

            // Configurate authentication
            AuthenticationBuilder authBuilder = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        ctx.Request.Cookies.TryGetValue(GeneralConstant.Token.ACCESS_TOKEN_NAME, out string? accessToken);
                        if (!string.IsNullOrEmpty(accessToken))
                            ctx.Token = accessToken;
                        return Task.CompletedTask;
                    }
                };
            });

            // Add external login providers
            if (googleAuthSettings.IsConfigured())
                authBuilder.AddGoogle(options => {
                    options.ClientId = googleAuthSettings.ClientId;
                    options.ClientSecret = googleAuthSettings.ClientSecret;
                    options.AccessType = "offline";
                    options.SaveTokens = true;
                    options.AccessDeniedPath = "/auth/external/access-denied";
                    options.Scope.Add("https://www.googleapis.com/auth/userinfo.email");
                    options.Scope.Add("https://www.googleapis.com/auth/userinfo.profile");
                    options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
                });

            if (gitHubAuthSettings.IsConfigured())
                authBuilder.AddGitHub(options => {
                    options.ClientId = gitHubAuthSettings.ClientId;
                    options.ClientSecret = gitHubAuthSettings.ClientSecret;
                    options.SaveTokens = true;
                    options.AccessDeniedPath = "/auth/external/access-denied";
                    options.Scope.Add("read:user");
                    options.Scope.Add("user:email");
                });

            // Password configuration
            services.Configure<IdentityOptions>(options => {
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
            });

            // Configure login provider clients for httpClient
            services.AddHttpClient<GoogleApiService>();
            services.AddHttpClient<GitHubApiService>();

            return services;
        }

        private static IServiceCollection AddAuthorization(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthorizationBuilder()
                .AddPolicy(AuthorizationPolicies.ADMIN_ONLY, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole([Role.ADMIN_NAME]);
                })
                .AddPolicy(AuthorizationPolicies.CAN_ESCALATE, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole([Role.ADMIN_NAME, Role.GENERAL_NAME]);
                })
                .AddPolicy(AuthorizationPolicies.DEMO_ONLY, policy =>
                    policy.RequireRole([Role.GUEST_NAME])
                );
            return services;
        }

        private static IServiceCollection AddRateLimiter(this IServiceCollection services, IConfiguration configuration)
        {
            const string UNKNOWN_PARTITION_KEY = "unknown";
            const string SHARED_RESOURCE_PARTITION_KEY = "shared-api-resource";

            RateLimitingOptions rateLimitingOptions = new();
            services.Configure<RateLimitingOptions>(configuration.GetSection(RATE_LIMITING));
            configuration.Bind(RATE_LIMITING, rateLimitingOptions);
            ValidateRateLimitingOptions(rateLimitingOptions);

            VolumeControl volumeControl = rateLimitingOptions.VolumeControl;
            ConcurrencyControl concurrency = rateLimitingOptions.ConcurrencyControl;
            DemoLoginQuota demoLoginQuota = rateLimitingOptions.DemoLoginQuota;
            services.AddRateLimiter(options =>
            {
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "text/plain";
                    await context.HttpContext.Response.WriteAsync("Too many requests, please try again later.", token);
                };

                options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                    // First Filter: Volume Control
                    PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>                    
                        RateLimitPartition.GetSlidingWindowLimiter(
                            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? UNKNOWN_PARTITION_KEY,
                            factory: _ => new SlidingWindowRateLimiterOptions
                            {
                                PermitLimit = volumeControl.PermitLimit,
                                Window = TimeSpan.FromMinutes(volumeControl.WindowMinutes),
                                SegmentsPerWindow = volumeControl.WindowSegments,
                                QueueLimit = 0
                            }
                        )
                    ),
                    // Second Filter: Concurrency
                    PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                        RateLimitPartition.GetConcurrencyLimiter(
                            partitionKey: SHARED_RESOURCE_PARTITION_KEY,
                            factory: _ => new ConcurrencyLimiterOptions
                            {
                                PermitLimit = concurrency.PermitLimit,
                                QueueLimit = concurrency.QueueLimit
                            }
                        )
                    )
                );

                options.AddPolicy(RateLimiterPolicies.DEMO_LOGIN_QUOTA, httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? UNKNOWN_PARTITION_KEY,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = demoLoginQuota.PermitLimit,
                            Window = TimeSpan.FromHours(demoLoginQuota.WindowHours),
                            QueueLimit = 0,
                        }
                    )
                );
            });
            return services;
        }

        private static void ValidateRateLimitingOptions(RateLimitingOptions options)
        {
            if (options.VolumeControl.PermitLimit <= 0 ||
                options.VolumeControl.WindowMinutes <= 0 ||
                options.VolumeControl.WindowSegments <= 0)
            {
                throw new InvalidOperationException(ConstructRateLimitingError(VolumeControl.SectionName));
            }

            if (options.DemoLoginQuota.PermitLimit <= 0 ||
                options.DemoLoginQuota.WindowHours <= 0)
            {
                throw new InvalidOperationException(ConstructRateLimitingError(DemoLoginQuota.SectionName));
            }

            if (options.ConcurrencyControl.PermitLimit <= 0 ||
                options.ConcurrencyControl.QueueLimit < 0)
            {
                throw new InvalidOperationException(ConstructRateLimitingError(ConcurrencyControl.SectionName));
            }
        }

        private static string ConstructRateLimitingError(string sectionName) => $"Rate limiting configuration section '{RATE_LIMITING}:{sectionName}' is missing or has invalid values.";
    }
}
