using projetFinalUA2.Models;
using System.ComponentModel.DataAnnotations;

namespace projetFinalUA2.Models;
/*** Auteur : [Votre nom]
 * Date de création : [Date de création]
 * Description : Modèle représentant un éditeur dans l'application de gestion de livres.
 */
public sealed class Editeur
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Nom { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Telephone { get; set; }

    [MaxLength(200)]
    public string? SiteWeb { get; set; }

    public DateTime DateCreationUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Livre> Livres { get; set; } = new List<Livre>();
}