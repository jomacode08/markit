using markit.Application.Models.Authentication;
using markit.Domain.Common;
using markit.Infraestructure.Security.Configurations;
using markit.Infraestructure.Security.Models;
using markit.Infraestructure.Security.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace markit.Infraestructure.Persistence.EF
{
    public class MarkitDbContext : IdentityDbContext<User>
    {
        private readonly UserDefaultSettings _userDefaultSettings;
        private readonly SessionService _sessionService;

        public MarkitDbContext
        (
            DbContextOptions<MarkitDbContext> options,
            SessionService sessionService,
            IOptions<UserDefaultSettings> userDefaultSettings
        ) : base(options)
        {
            _sessionService = sessionService;
            _userDefaultSettings = userDefaultSettings.Value;
        }

        public DbSet<User> User { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new UserConfiguration(_userDefaultSettings));
            builder.ApplyConfiguration(new RoleConfiguration());
            builder.ApplyConfiguration(new UserRoleConfiguration(_userDefaultSettings));

            ChangeNameSchemas(builder);
        }

        /// <summary>
        /// Método que se ejecutara antes de Agregar/Editar un record en la bd.
        /// Permite setear las propiedades de auditoria contenidas en el BaseModel.
        /// </summary>
        /// <param name="cancellationToken"></param>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            string? userIdentification = _sessionService.GetSessionUserIdentification();

            foreach (var entry in ChangeTracker.Entries<BaseModel>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                    {
                        entry.Entity.CreatedDate = DateTime.Now;
                        entry.Entity.Enable = true;
                        entry.Entity.CreatedBy = userIdentification ?? "System";
                        break;
                    }

                    case EntityState.Modified:
                    {
                        entry.Entity.UpdatedDate = DateTime.Now;
                        entry.Entity.UpdatedBy = userIdentification ?? "System";
                        break;
                    }
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        private static void ChangeNameSchemas(ModelBuilder builder)
        {
            const string securitySchema = "security";
            builder.Entity<User>().ToTable("Users", securitySchema);
            builder.Entity<IdentityRole>().ToTable("Roles", securitySchema);
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims", securitySchema);
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles", securitySchema);
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims", securitySchema);
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins", securitySchema);
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens", securitySchema);
        }
    }
}
