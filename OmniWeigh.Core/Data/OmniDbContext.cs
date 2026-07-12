using Microsoft.EntityFrameworkCore;
using OmniWeigh.Core.Models;

namespace OmniWeigh.Core.Data
{
    public class OmniDbContext : DbContext
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Weighing> Weighings { get; set; }
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

            // Configuration des relations pour garantir l'intégrité des données
            modelBuilder.Entity<Weighing>()
                .HasOne(w => w.Client)
                .WithMany(c => c.Weighings)
                .HasForeignKey(w => w.ClientId)
                .OnDelete(DeleteBehavior.Restrict); // Empêche de supprimer un client s'il a des pesées associées

            modelBuilder.Entity<Weighing>()
                .HasOne(w => w.Product)
                .WithMany(p => p.Weighings)
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // Empêche de supprimer un produit s'il a des pesées associées
        }
    }
}
