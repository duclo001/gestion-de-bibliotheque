using projetFinalUA2.Models;
using System.ComponentModel.DataAnnotations;

namespace projetFinalUA2.Models;
/*** Auteur : [Votre nom]
 * Date de création : [Date de création]
 * Description : Modèle représentant une catégorie dans l'application de gestion de livres.
 */
public sealed class Categorie
{
    public int Id { get; set; }

    [Required]
    [MaxLength(80)]
    public string Nom { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Description { get; set; }

    public ICollection<LivreCategorie> LivreCategories { get; set; } = new List<LivreCategorie>();
}