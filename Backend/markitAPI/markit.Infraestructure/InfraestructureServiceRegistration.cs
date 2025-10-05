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
using static markit.Application.Helpers.GeneralConstant.Configuration;
using Microsoft.AspNetCore.Authentication;
using markit.Application.Contracts.Google;
using markit.Infraestructure.Security.Services.Google;

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
                .AddHangfire(configuration);
            return services;
        }

        public static IServiceCollection AddDataBasePersistence(this IServiceCollection services, IConfiguration configuration)
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

        public static IServiceCollection AddMeiliSearchPersistence(this IServiceCollection services, IConfiguration configuration)
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

        public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind configuration data classes
            var jwtSettings = new JwtSettings();
            services.Configure<JwtSettings>(configuration.GetSection(JWT_SECTION_NAME));
            configuration.Bind(JWT_SECTION_NAME, jwtSettings);

            var googleAuthSettings = new GoogleAuthSettings();
            services.Configure<GoogleAuthSettings>(configuration.GetSection(GOOGLE_AUTH_SECTION_NAME));
            configuration.Bind(GOOGLE_AUTH_SECTION_NAME, googleAuthSettings);

            var spaSettings = new SpaSettings();
            services.Configure<SpaSettings>(configuration.GetSection(SPA_SECTION_NAME));
            configuration.Bind(SPA_SECTION_NAME, spaSettings);

            // Identity configuration
            services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<MarkitDbContext>()
                .AddDefaultTokenProviders();

            // Inject authentication services
            services.AddTransient<IAppUserService, AppUserService>();
            services.AddTransient<IJwtService, JwtService>();
            services.AddTransient<ILoginService, LoginService>();
            services.AddTransient<IExternalLoginService, ExternalLoginService>();
            services.AddTransient<IGoogleApiService, GoogleApiService>();

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
            })
            // Add external login providers
            .AddGoogle(options =>
            {
                options.ClientId = googleAuthSettings.ClientId;
                options.ClientSecret = googleAuthSettings.ClientSecret;
                options.AccessType = "offline";
                options.SaveTokens = true;
                options.AccessDeniedPath = "/api/external-login/access-denied";
                options.Scope.Add("https://www.googleapis.com/auth/userinfo.email");
                options.Scope.Add("https://www.googleapis.com/auth/userinfo.profile");
                options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
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

            return services;
        }

        public static IServiceCollection AddHangfire(this IServiceCollection services, IConfiguration configuration)
        {
            string connString = configuration.GetConnectionString(CONN_STRING_SECTION_NAME) ?? "";
            services.AddHangfire(config => config.UseSqlServerStorage(connString));
            services.AddHangfireServer();
            return services;
        }
    }
}
