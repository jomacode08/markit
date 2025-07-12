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
using markit.Application.Contracts.Authentication.Google;
using markit.Infraestructure.Security.Services.Google;
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

namespace markit.Infraestructure
{
    public static class InfraestructureServiceRegistration
    {
        private static readonly string jwtSectionName = GeneralConstant.Configuration.jwtSectionName;
        private static readonly string connStringSectionName = GeneralConstant.Configuration.connStringSectionName;
        private static readonly string userDefaultSectionName = GeneralConstant.Configuration.userDefaultSectionName;
        private static readonly string googleAuthSectionName = GeneralConstant.Configuration.googleAuthSectionName;
        private static readonly string meiliSearchSectionName = GeneralConstant.Configuration.meiliSearchSettingsName;

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
            string connString = configuration.GetConnectionString(connStringSectionName) ?? "";

            // Mapear clase UserDefaultSettings contra la configuración incluida en AppSettings.json
            var userDefaultSettings = new UserDefaultSettings();
            services.Configure<UserDefaultSettings>(configuration.GetSection(userDefaultSectionName));
            configuration.Bind(userDefaultSectionName, userDefaultSettings);

            // Conexión Base de datos
            services.AddDbContext<MarkitDbContext>(
                options => options
                    .UseSqlServer(connString)
                    .ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning))
            );

            // Agregar Context Accesor usado en el Session Service
            services.AddHttpContextAccessor();

            // Inyección del servicio de sesión
            services.AddTransient<SessionService>();

            // Inyección de UnitOfWork y Repositorio genérico
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IAsyncRepository<>), typeof(BaseRepository<>));

            return services;
        }

        public static IServiceCollection AddMeiliSearchPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind auth settings configuration data
            var meiliSearchAuthSettings = new MeiliSearchAuthSettings();
            services.Configure<MeiliSearchAuthSettings>(configuration.GetSection(meiliSearchSectionName));
            configuration.Bind(meiliSearchSectionName, meiliSearchAuthSettings);

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
            // Mapear clase JwtSettings, GoogleAuthSettings contra la configuración incluida en AppSettings.json
            var jwtSettings = new JwtSettings();
            services.Configure<JwtSettings>(configuration.GetSection(jwtSectionName));
            configuration.Bind(jwtSectionName, jwtSettings);

            var googleAuthSettings = new GoogleAuthSettings();
            services.Configure<GoogleAuthSettings>(configuration.GetSection(googleAuthSectionName));
            configuration.Bind(googleAuthSectionName, googleAuthSettings);

            // Configurar Identity con la clase personalizada de Usuario
            services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<MarkitDbContext>()
                .AddDefaultTokenProviders();

            // Inyección de services de autentificación
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IGoogleAuthenticationService, GoogleAuthenticationService>();
            services.AddTransient<IAppUserService, AppUserService>();

            // Congigurar Autentificación
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
            });

            // Configuración de password
            services.Configure<IdentityOptions>(options => {
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
            });

            return services;
        }

        public static IServiceCollection AddHangfire(this IServiceCollection services, IConfiguration configuration)
        {
            string connString = configuration.GetConnectionString(connStringSectionName) ?? "";
            services.AddHangfire(config => config.UseSqlServerStorage(connString));
            services.AddHangfireServer();
            return services;
        }
    }
}
