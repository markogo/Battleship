using System;
using System.IO;
using System.Linq;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DAL
{
    public class AppDbContext : DbContext
    {
        public DbSet<Game> Games { get; set; } = null!;
        public DbSet<GameOption> GameOptions { get; set; } = null!;
        public DbSet<Player> Players { get; set; } = null!;
        public DbSet<GameOptionShip> GameOptionShips { get; set; } = null!;
        public DbSet<Ship> Ships { get; set; } = null!;
        public DbSet<GameShip> GameShips { get; set; } = null!;
        public DbSet<PlayerBoardState> PlayerBoardStates { get; set; } = null!;
        public DbSet<GameSave> GameSaves { get; set; } = null!;


        private static readonly ILoggerFactory _loggerFactory = LoggerFactory.Create(
            builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Information)
                    .AddConsole();
            }
        );

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.
                Entity<GameOptionShip>()
                .HasIndex(g => new
                {
                    g.ShipId,
                    g.GameOptionId
                })
                .IsUnique();

            modelBuilder
                .Entity<Player>()
                .HasOne<Game>()
                .WithOne(x => x.Player1!)
                .HasForeignKey<Game>(x => x.Player1Id);
            
            modelBuilder
                .Entity<Player>()
                .HasOne<Game>()
                .WithOne(x => x.Player2!)
                .HasForeignKey<Game>(x => x.Player2Id);

            foreach (var relationship in modelBuilder.Model
                .GetEntityTypes()
                .Where(e => !e.IsOwned())
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder
                .EnableSensitiveDataLogging()
                .UseSqlite($"DataSource={Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../Battleship.db"))}");
        }
    }
    
}