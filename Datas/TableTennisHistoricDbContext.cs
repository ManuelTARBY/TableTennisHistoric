using Microsoft.EntityFrameworkCore;
using TableTennisHistoric.Models;

namespace TableTennisHistoric.Datas
{
    public class TableTennisHistoricDbContext: DbContext
    {
        public TableTennisHistoricDbContext(DbContextOptions<TableTennisHistoricDbContext> options)
            : base(options)
        {
        }

        public DbSet<Player> Player { get; set; }
        public DbSet<Club> Club { get; set; }
        public DbSet<Competition> Competition { get; set; }
        public DbSet<Championship> Championship { get; set; }
        public DbSet<ChampionshipFormat> ChampionshipFormat { get; set; }
        public DbSet<ChampionshipTeam> ChampionshipTeam { get; set; }
        public DbSet<ChampionshipMatch> ChampionshipMatch { get; set; }
        public DbSet<Team> Team { get; set; }
        public DbSet<Season> Season { get; set; }
        public DbSet<CompetitionCoefficient> CompetitionCoefficient { get; set; }
        public DbSet<PlayerSeason> PlayerSeason { get; set; }
        public DbSet<TableTennisMatch> TableTennisMatch { get; set; }
        public DbSet<Stage> Stage { get; set; }
        public DbSet<MatchSet> MatchSet { get; set; }

        // Optionnel : configuration fine
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PlayerSeason>()
                .Property(p => p.Category)
                .HasConversion<string>();

            // Configurations spécifiques
            modelBuilder.Entity<TableTennisMatch>()
                .Property(m => m.Result)
                .HasConversion<string>();

            // Configuration de l'entité
            modelBuilder.Entity<TableTennisMatch>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.Result)
                      .HasConversion<string>();
            });

            modelBuilder.Entity<TableTennisMatch>()
                .Property(m => m.Date_match)
                .HasColumnType("date");

            modelBuilder.Entity<Season>()
               .Property(s => s.Start_date)
               .HasColumnType("date");

            modelBuilder.Entity<Season>()
               .Property(s => s.End_date)
               .HasColumnType("date");
        }
    }
}
