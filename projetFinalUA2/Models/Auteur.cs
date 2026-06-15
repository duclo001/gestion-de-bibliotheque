using projetFinalUA2.Models;
using System.ComponentModel.DataAnnotations;

namespace projetFinalUA2.Models;
/*** Auteur : [Votre nom]
 * Date de création : [Date de création]
 * Description : Modèle représentant un auteur dans l'application de gestion de livres.
 */
public sealed class Auteur
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nom { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Prenom { get; set; }

    [MaxLength(200)]
    public string? Email { get; set; }

    public DateTime DateCreationUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Livre> Livres { get; set; } = new List<Livre>();
}