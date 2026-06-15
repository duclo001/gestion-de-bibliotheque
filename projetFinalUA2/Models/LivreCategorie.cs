using projetFinalUA2.Models;

namespace projetFinalUA2.Models;
/*** Auteur : [Votre nom]
 * Date de création : [Date de création]
 * Description : Modèle représentant la relation entre un livre et une catégorie dans l'application de gestion de livres.
 */
public sealed class LivreCategorie
{
    public int LivreId { get; set; }
    public Livre Livre { get; set; } = null!;

    public int CategorieId { get; set; }
    public Categorie Categorie { get; set; } = null!;

    public DateTime DateAssociationUtc { get; set; } = DateTime.UtcNow;
}