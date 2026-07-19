using Microsoft.EntityFrameworkCore;
using OmniWeigh.Core.Models;

namespace OmniWeigh.Core.Data
{
    public class OmniDbContext : DbContext
    {
        public DbSet<Company> Companies { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<WeighingSession> WeighingSessions { get; set; }
        public DbSet<WeighingHistory> WeighingHistories { get; set; }
        public DbSet<SequenceTracker> SequenceTrackers { get; set; }
        public DbSet<AppSetting> AppSettings { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // On isole la base de données dans le dossier LocalApplicationData de la machine
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string omniWeighFolderPath = Path.Combine(appDataFolder, "OmniWeigh");

            // Crée le dossier s'il n'existe pas encore sur le PC du client
            Directory.CreateDirectory(omniWeighFolderPath);

            string databasePath = Path.Combine(omniWeighFolderPath, "omniweigh.db");

            // Trace the actual DB file used so it's easy to debug mismatches with external tools
            System.Diagnostics.Debug.WriteLine($"OmniDbContext: using database at {databasePath}");

            // Connexion native à SQLite
            optionsBuilder.UseSqlite($"Data Source={databasePath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration de WeighingHistory
            modelBuilder.Entity<WeighingHistory>(entity =>
            {
                entity.ToTable("WeighingHistory");
                
                entity.HasOne(w => w.Product)
                      .WithMany(p => p.WeighingHistories)
                      .HasForeignKey(w => w.ProductId)
                      .OnDelete(DeleteBehavior.Restrict); // Empêche de supprimer un produit s'il a des pesées associées
                      
                entity.HasOne(w => w.Session)
                      .WithMany(s => s.HistoryRecords)
                      .HasForeignKey(w => w.SessionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuration de Document
            modelBuilder.Entity<Document>()
                .HasOne(d => d.Client)
                .WithMany(c => c.Documents)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Restrict); // Empêche de supprimer un client s'il a des documents associés

            // Configuration de SequenceTracker
            modelBuilder.Entity<SequenceTracker>(entity =>
            {
                entity.HasIndex(e => e.EntityType).IsUnique();
            });

            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.Property(v => v.Registration).IsRequired();
                entity.Property(v => v.Type).IsRequired();
            });
        }
    }
}
