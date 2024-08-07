using Microsoft.EntityFrameworkCore;
using PostAppService.Models;

namespace PostAppService.Data
{
    public class PostAppServiceContext : DbContext
    {
        public PostAppServiceContext(DbContextOptions<PostAppServiceContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
    }
}
