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
using markit.Application.Models.Autentication;
using Microsoft.AspNetCore.Identity;
using markit.Infraestructure.Security.Models;
using markit.Application.Contracts.Autentication;
using markit.Infraestructure.Autentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using markit.Application.Contracts.Authentication.Google;
using markit.Infraestructure.Security.Services.Google;
using markit.Application.Models.Authentication.Google;

namespace markit.Infraestructure
{
    public static class InfraestructureServiceRegistration
    {
        private static readonly string jwtSectionName = GeneralConstant.Configuration.jwtSectionName;
        private static readonly string connStringSectionName = GeneralConstant.Configuration.connStringSectionName;
        private static readonly string userDefaultSectionName = GeneralConstant.Configuration.userDefaultSectionName;
        private static readonly string googleAuthSectionName = GeneralConstant.Configuration.googleAuthSectionName;


        public static IServiceCollection AddInfraestructureServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            services
                .AddPersistence(configuration)
                .AddAuthentication(configuration);
            return services;
        }

        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
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

            // Inyección del Service de autentificación
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IGoogleAuthenticationService, GoogleAuthenticationService>();

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
    }
}
