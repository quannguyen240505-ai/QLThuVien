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
        public DbSet<LibrarySetting> LibrarySettings { get; set; }
        public DbSet<Reader> Readers { get; set; }
        public DbSet<BorrowTicket> BorrowTickets { get; set; }
        public DbSet<BorrowDetail> BorrowDetails { get; set; }
    }
}
