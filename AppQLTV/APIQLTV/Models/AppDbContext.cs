using APIQLTV.Models;
using Microsoft.EntityFrameworkCore;
using APIQLTV.Models.Books;
namespace APIQLTV.Models
{
    public class AppDbContext  : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<AppUser> Users { get; set; }
        public DbSet<Book> Books { get; set; }
    }
}
