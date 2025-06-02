using FluentValidation;
using markit.Application.Common.Behaviours;
using markit.Application.Common.Helpers.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace markit.Application
{
    /// <summary>
    /// Clase estatica para la creación del contenedor Dependency Injection
    /// </summary>
    public static class ApplicationServiceRegistration
    {
        /// <summary>
        /// Método estático de extensión del tipo IServiceCollection
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Behaviours
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            // Helper services
            services.AddTransient<CollectionItemService>();

            return services;
        }
    }
}
