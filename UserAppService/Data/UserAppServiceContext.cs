using Microsoft.EntityFrameworkCore;
using UserAppService.Models;
using UserService.Entities;

namespace UserAppService.Data
{
    public class UserAppServiceContext : DbContext
    {
        public UserAppServiceContext(DbContextOptions<UserAppServiceContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<IntegrationEvent> IntegrationEventOutbox { get; set; }

    }
}
