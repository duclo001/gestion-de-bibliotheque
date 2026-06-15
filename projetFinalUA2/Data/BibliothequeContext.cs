using projetFinalUA2.Models;
using Microsoft.EntityFrameworkCore;

namespace projetFinalUA2.Data;
/*** Classe de contexte pour la bibliothèque, gérant les entités et les relations.
 */
public sealed class BibliothequeContext : DbContext
{
    public DbSet<Auteur> Auteurs { get; set; }
    public DbSet<Livre> Livres { get; set; }
    public DbSet<FicheDetail> FicheDetails { get; set; }
    public DbSet<Categorie> Categories { get; set; }
    public DbSet<LivreCategorie> LivreCategories { get; set; }
    public DbSet<Editeur> Editeurs { get; set; }

    // Configuration de la connexion à la base de données SQL Server
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            @"Server=localhost;Database=projetFinalUA2BibliothequeDb;Trusted_Connection=True;TrustServerCertificate=True;");
    }

    // Configuration des entités et de leurs relations via Fluent API
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigurerLivre(modelBuilder);
        ConfigurerRelationLivreFicheDetail(modelBuilder);
        ConfigurerRelationEditeurLivres(modelBuilder);
        ConfigurerRelationLivresCategories(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }
    // Fluent API obligatoire : Configuration de l'entité Livre
    private static void ConfigurerLivre(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Livre>()
            .HasIndex(x => x.Isbn)
            .IsUnique();

        modelBuilder.Entity<Livre>()
            .Property(x => x.Isbn)
            .HasMaxLength(13)
            .IsRequired();

        modelBuilder.Entity<Livre>()
            .Property(x => x.Titre)
            .HasMaxLength(200)
            .IsRequired();
    }

    // Fluent API obligatoire : 1-1 Livre ↔ FicheDetail
    private static void ConfigurerRelationLivreFicheDetail(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Livre>()
            .HasOne(x => x.FicheDetail)
            .WithOne(x => x.Livre)
            .HasForeignKey<FicheDetail>(x => x.LivreId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FicheDetail>()
            .HasIndex(x => x.LivreId)
            .IsUnique();
    }

    // Fluent API obligatoire : 1-N Editeur → Livres
    private static void ConfigurerRelationEditeurLivres(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Livre>()
            .HasOne(x => x.Editeur)
            .WithMany(x => x.Livres)
            .HasForeignKey(x => x.EditeurId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Editeur>()
            .Property(x => x.Nom)
            .HasMaxLength(150)
            .IsRequired();

        modelBuilder.Entity<Editeur>()
            .HasIndex(x => x.Nom)
            .IsUnique();
    }

    // Fluent API obligatoire : N-M Livres ↔ Categories via LivreCategorie
    private static void ConfigurerRelationLivresCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LivreCategorie>()
            .HasKey(x => new { x.LivreId, x.CategorieId });

        modelBuilder.Entity<LivreCategorie>()
            .HasOne(x => x.Livre)
            .WithMany(x => x.LivreCategories)
            .HasForeignKey(x => x.LivreId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LivreCategorie>()
            .HasOne(x => x.Categorie)
            .WithMany(x => x.LivreCategories)
            .HasForeignKey(x => x.CategorieId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Categorie>()
            .Property(x => x.Nom)
            .HasMaxLength(80)
            .IsRequired();

        modelBuilder.Entity<Categorie>()
            .HasIndex(x => x.Nom)
            .IsUnique();
    }
}