using APIQLTV.Models;
using Microsoft.EntityFrameworkCore;
namespace APIQLTV.Models
{
    public class AppDbContext  : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<AppUser> Users { get; set; }
    }
}
