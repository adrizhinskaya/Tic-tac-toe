using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using Tic_tac_toe.Entities;

namespace Tic_tac_toe.Data
{
    public class AppDbContext : IdentityDbContext<Player>
    {
        public DbSet<Game> Games { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Player>()
                .HasOne(p => p.Game)
                .WithMany(g => g.Players)
                .HasForeignKey(p => p.GameId);

            builder.Entity<Game>()
                .HasOne(g => g.Player1)
                .WithOne()
                .HasForeignKey<Game>(g => g.Player1Id);

            builder.Entity<Game>()
                .HasOne(g => g.Player2)
                .WithOne()
                .HasForeignKey<Game>(g => g.Player2Id);

            builder.Entity<Game>()
                .HasOne(g => g.CurrentPlayer)
                .WithOne()
                .HasForeignKey<Game>(g => g.CurrentPlayerId);
        }
    }
}
