using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Settings;
using markit.Domain.Common;
using markit.Domain.Entities;
using markit.Infrastructure.Persistence.EF.Configurations;
using markit.Infrastructure.Security.Configurations;
using markit.Infrastructure.Security.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace markit.Infrastructure.Persistence.EF
{
    public class MarkitDbContext : IdentityDbContext<AppUser>
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

        public DbSet<AppUser> User { get; set; }
        public DbSet<SystemConfig> SystemConfigs { get; set; }
        public DbSet<Collection> Collections { get; set; }
        public DbSet<Block> Blocks { get; set; }
        public DbSet<Mark> Marks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new RoleConfiguration());
            builder.ApplyConfiguration(new CollectionConfiguration());
            builder.ApplyConfiguration(new MarkConfiguration());
            builder.ApplyConfiguration(new BlockConfiguration());
            builder.ApplyConfiguration(new SystemConfigConfiguration());

            ChangeNameSchemas(builder);
            AddQueryFilters(builder);
        }

        /// <summary>
        /// Método que se ejecutara antes de Agregar/Editar un record en la bd.
        /// Permite setear las propiedades de auditoria contenidas en el BaseModel.
        /// </summary>
        /// <param name="cancellationToken"></param>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            string? userIdentification = _sessionService.GetIdentity();

            foreach (var entry in ChangeTracker.Entries<BaseModel>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                    {
                        entry.Entity.CreatedDate = DateTime.UtcNow;
                        entry.Entity.Enable = true;
                        entry.Entity.CreatedBy = userIdentification ?? "System";
                        break;
                    }

                    case EntityState.Modified:
                    {
                        entry.Entity.UpdatedDate = DateTime.UtcNow;
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
            builder.Entity<AppUser>().ToTable("users", securitySchema);
            builder.Entity<SystemConfig>().ToTable("system_configs", securitySchema);
            builder.Entity<IdentityRole>().ToTable("roles", securitySchema);
            builder.Entity<IdentityRoleClaim<string>>().ToTable("role_claims", securitySchema);
            builder.Entity<IdentityUserRole<string>>().ToTable("user_roles", securitySchema);
            builder.Entity<IdentityUserClaim<string>>().ToTable("user_claims", securitySchema);
            builder.Entity<IdentityUserLogin<string>>().ToTable("user_logins", securitySchema);
            builder.Entity<IdentityUserToken<string>>().ToTable("user_tokens", securitySchema);
        }

        private static void AddQueryFilters(ModelBuilder builder) {
            builder.Entity<Mark>().HasQueryFilter(m => m.Enable);
            builder.Entity<Block>().HasQueryFilter(m => m.Enable);
            builder.Entity<Collection>().HasQueryFilter(m => m.Enable);
        }
    }
}
