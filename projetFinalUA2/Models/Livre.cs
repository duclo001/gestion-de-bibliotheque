using projetFinalUA2.Models;
using System.ComponentModel.DataAnnotations;

namespace projetFinalUA2.Models;
/*** Auteur : [Votre nom]
 * Date de création : [Date de création]
 * Description : Modèle représentant un livre dans l'application de gestion de livres.
 */
public sealed class Livre
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Titre { get; set; } = string.Empty;

    [Required]
    [MaxLength(13)]
    public string Isbn { get; set; } = string.Empty;

    public DateTime DatePublication { get; set; } = DateTime.Today;

    // 1-N Auteur (FK + nav). La relation est d�tect�e par convention.
    public int AuteurId { get; set; }
    public Auteur Auteur { get; set; } = null!;

    // 1-N Editeur (config Fluent demand�e c�t� contexte)
    public int EditeurId { get; set; }
    public Editeur Editeur { get; set; } = null!;

    // 1-1 FicheDetail (config Fluent demand�e)
    public FicheDetail? FicheDetail { get; set; }

    // N-M via table de jonction
    public ICollection<LivreCategorie> LivreCategories { get; set; } = new List<LivreCategorie>();
}