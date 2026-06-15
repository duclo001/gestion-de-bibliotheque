using projetFinalUA2.Models;
using System.ComponentModel.DataAnnotations;

namespace projetFinalUA2.Models;
/*** Auteur : [Votre nom]
 * Date de création : [Date de création]
 * Description : Modèle représentant les détails d'une fiche dans l'application de gestion de livres.
 */
public sealed class FicheDetail
{
    public int Id { get; set; }

    // FK 1-1 (sera unique via Fluent API)
    public int LivreId { get; set; }

    [MaxLength(2000)]
    public string? Resume { get; set; }

    [MaxLength(50)]
    public string? Langue { get; set; }

    public int? NombrePages { get; set; }

    public DateTime DateCreationUtc { get; set; } = DateTime.UtcNow;

    public Livre Livre { get; set; } = null!;
}