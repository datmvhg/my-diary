using Entities;
using Microsoft.EntityFrameworkCore;

namespace DBConnect
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Note> Notes { get; set; } = null!;
        public DbSet<DiaryMoment> DiaryMoments { get; set; } = null!;
    }
}
