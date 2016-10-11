using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using Carbon.Business;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Carbon.Services.IdentityServer
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private static readonly List<DbValidationError> _noErrors = new List<DbValidationError>();

        public ApplicationDbContext(AppSettings appSettings) 
            : base(appSettings.GetConnectionString("sql"))
        {                        
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Needed to ensure subclasses share the same table
            var user = modelBuilder.Entity<ApplicationUser>()
                .ToTable("AspNetUsers");
            user.HasMany(u => u.Roles).WithRequired().HasForeignKey(ur => ur.UserId);
            user.HasMany(u => u.Claims).WithRequired().HasForeignKey(uc => uc.UserId);
            user.HasMany(u => u.Logins).WithRequired().HasForeignKey(ul => ul.UserId);
            user.Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(256);
            
            user.Property(u => u.Email).HasMaxLength(256)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("EmailIndex") { IsUnique = true }));

            modelBuilder.Entity<IdentityUserRole>()
                .HasKey(r => new { r.UserId, r.RoleId })
                .ToTable("AspNetUserRoles");

            modelBuilder.Entity<IdentityUserLogin>()
                .HasKey(l => new { l.LoginProvider, l.ProviderKey, l.UserId })
                .ToTable("AspNetUserLogins");

            modelBuilder.Entity<IdentityUserClaim>()
                .ToTable("AspNetUserClaims");

            var role = modelBuilder.Entity<IdentityRole>()
                .ToTable("AspNetRoles");
            role.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("RoleNameIndex") { IsUnique = true }));
            role.HasMany(r => r.Users).WithRequired().HasForeignKey(ur => ur.RoleId);
        }

        protected override DbEntityValidationResult ValidateEntity(DbEntityEntry entityEntry, IDictionary<object, object> items)
        {
            if (entityEntry != null && entityEntry.State == EntityState.Added)
            {
                var user = entityEntry.Entity as ApplicationUser;
                if (user != null)
                {
                    //validated by user validator class
                    return new DbEntityValidationResult(entityEntry, _noErrors);
                }

                var errors = new List<DbValidationError>();
                var role = entityEntry.Entity as IdentityRole;
                if ((role != null) && Roles.Any(r => string.Equals(r.Name, role.Name)))
                {
                    errors.Add(new DbValidationError("Role", string.Format(CultureInfo.CurrentCulture, Strings.RoleAlreadyExists, role.Name)));
                }

                return new DbEntityValidationResult(entityEntry, errors);
            }
            return base.ValidateEntity(entityEntry, items);
        }

        public class DllDeployer
        {
            public static void M()
            {
                System.Data.Entity.SqlServer.SqlFunctions.Acos(1d);
            }
        }
    }
}