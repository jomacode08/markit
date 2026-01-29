using markit.Application.Contracts.Persistence.Common;
using markit.Application.Helpers;
using markit.Application.Models.Authentication;
using markit.Infraestructure.Persistence.EF;
using markit.Infraestructure.Repositorys.Common;
using markit.Infraestructure.Repositorys;
using markit.Infraestructure.Security.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using markit.Application.Models.Authentication.Google;
using markit.Application.Contracts.Authentication;
using markit.Application.Models.Authentication.AppUser;
using Microsoft.EntityFrameworkCore.Diagnostics;
using markit.Application.Models.Authentication.MeiliSearch;
using markit.Infraestructure.Repositorys.MeiliSearch;
using markit.Application.Contracts.MeiliSearch;
using markit.Application.Models.MeiliSearch.Documents;
using Hangfire;
using markit.Infraestructure.Persistence.MeiliSearch.Services;
using Microsoft.AspNetCore.Authentication;
using markit.Application.Contracts.Google;
using markit.Infraestructure.Security.Services.Google;
using markit.Infraestructure.Security.Services.ExternalLogin;
using markit.Application.Contracts.Authentication.ExternalLogin;
using markit.Application.Models.Authentication.GitHub;
using markit.Application.Contracts.GitHub;
using markit.Infraestructure.Security.Services.GitHub;
using markit.Application.Models.Settings;
using static markit.Application.Helpers.GeneralConstant.Configuration;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Infraestructure
{
    public static class InfraestructureServiceRegistration
    {
        public static IServiceCollection AddInfraestructureServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            services
                .AddDataBasePersistence(configuration)
                .AddMeiliSearchPersistence(configuration)
                .AddAuthentication(configuration)
                .AddAuthorization(configuration)
                .AddHangfire(configuration);
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
                    .UseSqlServer(connString)
                    .ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning))
            );

            services.AddHttpContextAccessor();
            services.AddTransient<SessionService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IAsyncRepository<>), typeof(BaseRepository<>));

            return services;
        }

        private static IServiceCollection AddMeiliSearchPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind auth settings configuration data
            var meiliSearchAuthSettings = new MeiliSearchAuthSettings();
            services.Configure<MeiliSearchAuthSettings>(configuration.GetSection(MEILISEARCH_SECTION_NAME));
            configuration.Bind(MEILISEARCH_SECTION_NAME, meiliSearchAuthSettings);

            // Inject repositories
            services.AddScoped<IDocumentRepository<CollectionDocument>, DocumentRepository<CollectionDocument>>(provider =>
            {
                return new DocumentRepository<CollectionDocument>(meiliSearchAuthSettings, GeneralConstant.MeiliSearch.COLLECTION_INDEX_UID);
            });
            services.AddScoped<IDocumentRepository<MarkDocument>, DocumentRepository<MarkDocument>>(provider =>
            {
                return new DocumentRepository<MarkDocument>(meiliSearchAuthSettings, GeneralConstant.MeiliSearch.MARK_INDEX_UID);
            });

            // Inject background job service
            services.AddTransient(typeof(IDocumentJobService<>), typeof(DocumentJobService<>));

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

            var githubAuthSettings = new GitHubAuthSettings();
            services.Configure<GitHubAuthSettings>(configuration.GetSection(GITHUB_AUTH_SECTION_NAME));
            configuration.Bind(GITHUB_AUTH_SECTION_NAME, githubAuthSettings);

            var spaSettings = new SpaSettings();
            services.Configure<SpaSettings>(configuration.GetSection(SPA_SECTION_NAME));
            configuration.Bind(SPA_SECTION_NAME, spaSettings);

            // Identity configuration
            services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<MarkitDbContext>()
                .AddDefaultTokenProviders();

            // Inject authentication services
            services.AddScoped<IAppUserService, AppUserService>();
            services.AddScoped<IGoogleApiService, GoogleApiService>();
            services.AddScoped<IGitHubApiService, GitHubApiService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<IExternalIdentifierService, ExternalIdentifierService>();
            services.AddScoped<IExternalTokenService, ExternalTokenService>();
            services.AddTransient<IExternalLoginService, ExternalLoginService>();

            // Configurate authentication
            services.AddAuthentication(options =>
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
            })
            // Add external login providers
            .AddGoogle(options =>
            {
                options.ClientId = googleAuthSettings.ClientId;
                options.ClientSecret = googleAuthSettings.ClientSecret;
                options.AccessType = "offline";
                options.SaveTokens = true;
                options.AccessDeniedPath = "/auth/external/access-denied";
                options.Scope.Add("https://www.googleapis.com/auth/userinfo.email");
                options.Scope.Add("https://www.googleapis.com/auth/userinfo.profile");
                options.Scope.Add("https://www.googleapis.com/auth/calendar.events.readonly");
                options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
            })
            .AddGitHub(options =>
            {
                options.ClientId = githubAuthSettings.ClientId;
                options.ClientSecret = githubAuthSettings.ClientSecret;
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
            });

            // Configure login provider clients for httpClient
            services.AddHttpClient<GoogleApiService>();
            services.AddHttpClient<GitHubApiService>();

            return services;
        }

        private static IServiceCollection AddHangfire(this IServiceCollection services, IConfiguration configuration)
        {
            string connString = configuration.GetConnectionString(CONN_STRING_SECTION_NAME) ?? "";
            services.AddHangfire(config => config.UseSqlServerStorage(connString));
            services.AddHangfireServer();
            return services;
        }

        private static IServiceCollection AddAuthorization(this IServiceCollection services, IConfiguration configuration)
        {
            DemoSettings demoSettings = new();
            services.Configure<DemoSettings>(configuration.GetSection(DEMO_SECTION_NAME));
            configuration.Bind(DEMO_SECTION_NAME, demoSettings);

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
                    policy.RequireAssertion(context => demoSettings.Enabled)
                );
            return services;
        }
    }
}
