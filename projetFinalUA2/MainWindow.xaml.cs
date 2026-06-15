using MahApps.Metro.Controls;
using Microsoft.EntityFrameworkCore;
using projetFinalUA2.Data;
using projetFinalUA2.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
/***Ce projet final pour l'UA2 est une application de gestion de bibliothèque développée en WPF avec Entity Framework Core pour la gestion des données.
 * L'application permet de gérer les auteurs, les livres, les catégories et les éditeurs, avec des fonctionnalités de recherche multicritère, et des opérations CRUD complètes.
 * Le code est structuré pour être clair et maintenable, avec une séparation logique des différentes parties (chargement des données, gestion des événements, opérations CRUD).
 * Les commentaires détaillés expliquent les choix de conception et les bonnes pratiques appliquées tout au long du code.
 */

namespace projetFinalUA2
{
    /*** Cette classe est volontairement très longue et complète pour servir de référence d'exemple sur la gestion d'une interface WPF avec Entity Framework Core.
     * Elle contient les méthodes de chargement des données, les handlers d'événements pour les interactions utilisateur, et les opérations CRUD pour chaque entité.
     * Les commentaires détaillés expliquent les choix de conception et les bonnes pratiques appliquées.
     */
    public partial class MainWindow : MetroWindow
    {
        private readonly BibliothequeContext _context = new();

        private Auteur? _auteurSelectionne;
        private Livre? _livreSelectionne;
        private Categorie? _categorieSelectionnee;
        private Editeur? _editeurSelectionne;

        public MainWindow()
        {
            InitializeComponent();
        }

        // Pour éviter les problèmes de contexte partagé entre threads, on charge tout au démarrage et on rafraîchit les données après chaque opération.
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await RechargerToutAsync();
            await ChargerFiltresLivresAsync();
            await ChargerFormLivreAsync();

            MettreStatusCrud("Données chargées.");
        }
        // Bouton de menu pour recharger les données depuis la base.
        private async void MenuRecharger_Click(object sender, RoutedEventArgs e)
        {
            await RechargerToutAsync();
            await ChargerFiltresLivresAsync();
            await ChargerFormLivreAsync();

            MettreStatusCrud("Données rechargées.");
        }

        // Bouton de menu pour quitter l'application.
        private void MenuQuitter_Click(object sender, RoutedEventArgs e) => Close();
        
          //Méthodes utilitaires pour afficher les statuts dans la barre de status.
        private async Task RechargerToutAsync()
        {
            await ChargerAuteursAsync();
            await ChargerLivresAsync();
            await ChargerCategoriesAsync();
            await ChargerEditeursAsync();

            MettreStatusRecherche(string.Empty);
        }

        // Méthodes pour charger les données dans les DataGrids. On utilise AsNoTracking pour optimiser la lecture seule.
        private async Task ChargerAuteursAsync()
        {
            var auteurs = await _context.Auteurs
                .AsNoTracking()
                .OrderBy(x => x.Nom)
                .ThenBy(x => x.Prenom)
                .ToListAsync();

            GridAuteurs.ItemsSource = auteurs;
            MettreStatusRecherche($"Auteurs: {auteurs.Count}");
        }
        // Pour les livres, on inclut les données liées (Auteur, Editeur, FicheDetail, Catégories)
        // pour éviter les problèmes de chargement différé dans l'interface.
        private async Task ChargerLivresAsync()
        {
            var livres = await _context.Livres
                .Include(x => x.Auteur)
                .Include(x => x.Editeur)
                .Include(x => x.FicheDetail)
                .Include(x => x.LivreCategories)
                    .ThenInclude(x => x.Categorie)
                .AsNoTracking()
                .OrderBy(x => x.Titre)
                .ToListAsync();

            GridLivres.ItemsSource = livres;
        }


        // Pour les catégories et éditeurs, on charge simplement la liste triée pour les DataGrids.
        private async Task ChargerCategoriesAsync()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(x => x.Nom)
                .ToListAsync();

            GridCategories.ItemsSource = categories;
        }

        // Pour les éditeurs, même chose que pour les catégories : liste simple triée.
        private async Task ChargerEditeursAsync()
        {
            var editeurs = await _context.Editeurs
                .AsNoTracking()
                .OrderBy(x => x.Nom)
                .ToListAsync();

            GridEditeurs.ItemsSource = editeurs;
        }

        // =========================
        // RECHERCHE MULTICRITERE LIVRES
        // =========================
        /** La recherche est effectuée côté base de données en construisant dynamiquement la requête LINQ en fonction des filtres saisis.
         * On utilise AsNoTracking pour optimiser la lecture seule, et on inclut les données liées nécessaires à l'affichage.
         * Le nombre de filtres appliqués est comptabilisé pour l'afficher dans le status.
         * Un bouton de reset permet de tout réinitialiser facilement.
         */
        private async Task ChargerFiltresLivresAsync()
        {
            var auteurs = await _context.Auteurs
                .AsNoTracking()
                .OrderBy(x => x.Nom)
                .ToListAsync();

            CboLivreAuteur.ItemsSource = auteurs;
            CboLivreAuteur.SelectedItem = null;

            var editeurs = await _context.Editeurs
                .AsNoTracking()
                .OrderBy(x => x.Nom)
                .ToListAsync();

            CboLivreEditeur.ItemsSource = editeurs;
            CboLivreEditeur.SelectedItem = null;

            var categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(x => x.Nom)
                .ToListAsync();

            CboLivreCategorie.ItemsSource = categories;
            CboLivreCategorie.SelectedItem = null;

            TxtLivreTitre.Text = string.Empty;
            DpLivreDateDebut.SelectedDate = null;
            DpLivreDateFin.SelectedDate = null;
        }

        // Handler du bouton de recherche qui lance la méthode de recherche asynchrone.
        private async void BtnLivreRechercher_Click(object sender, RoutedEventArgs e) => await RechercherLivresAsync();

        // Handler du bouton de reset qui réinitialise tous les filtres et recharge la liste complète des livres.
        private async void BtnLivreReset_Click(object sender, RoutedEventArgs e)
        {
            TxtLivreTitre.Text = string.Empty;
            CboLivreAuteur.SelectedItem = null;
            CboLivreEditeur.SelectedItem = null;
            CboLivreCategorie.SelectedItem = null;
            DpLivreDateDebut.SelectedDate = null;
            DpLivreDateFin.SelectedDate = null;

            await ChargerLivresAsync();
            MettreStatusCrud("Filtres réinitialisés.");
            MettreStatusRecherche("Recherche: 0 filtre");
        }

        // Méthode de recherche qui construit dynamiquement la requête en fonction des filtres saisis, et affiche les résultats dans le DataGrid.
        private async Task RechercherLivresAsync()
        {
            var titre = TxtLivreTitre.Text.Trim();
            int? auteurId = CboLivreAuteur.SelectedValue as int?;
            int? editeurId = CboLivreEditeur.SelectedValue as int?;
            int? categorieId = CboLivreCategorie.SelectedValue as int?;
            DateTime? dateDebut = DpLivreDateDebut.SelectedDate;
            DateTime? dateFin = DpLivreDateFin.SelectedDate;

            if (dateDebut.HasValue && dateFin.HasValue && dateDebut > dateFin)
            {
                MettreStatusCrud("Intervalle de dates invalide : début > fin.");
                return;
            }

            IQueryable<Livre> query = _context.Livres
                .AsNoTracking()
                .Include(x => x.Auteur)
                .Include(x => x.Editeur)
                .Include(x => x.LivreCategories)
                    .ThenInclude(x => x.Categorie);

            int nbFiltres = 0;

            if (!string.IsNullOrWhiteSpace(titre))
            {
                nbFiltres++;
                query = query.Where(x => x.Titre.Contains(titre));
            }

            if (auteurId.HasValue)
            {
                nbFiltres++;
                query = query.Where(x => x.AuteurId == auteurId.Value);
            }

            if (editeurId.HasValue)
            {
                nbFiltres++;
                query = query.Where(x => x.EditeurId == editeurId.Value);
            }

            if (categorieId.HasValue)
            {
                nbFiltres++;
                query = query.Where(x => x.LivreCategories.Any(lc => lc.CategorieId == categorieId.Value));
            }

            if (dateDebut.HasValue)
            {
                nbFiltres++;
                query = query.Where(x => x.DatePublication >= dateDebut.Value);
            }

            if (dateFin.HasValue)
            {
                nbFiltres++;
                query = query.Where(x => x.DatePublication <= dateFin.Value);
            }

            var resultats = await query
                .OrderBy(x => x.Titre)
                .ToListAsync();

            GridLivres.ItemsSource = resultats;

            MettreStatusCrud("Recherche effectuée.");
            MettreStatusRecherche($"Résultats: {resultats.Count} | Filtres: {nbFiltres}");
        }

        // =========================
        // AUTEURS - CRUD
        // =========================
        /*** La gestion des auteurs inclut les opérations de création, modification et suppression, avec des vérifications d'intégrité (doublons) et de validation (nom obligatoire).
         * Lors de la sélection d'un auteur dans le DataGrid, les détails sont affichés dans le formulaire de droite pour modification.
         * Le bouton de suppression est activé uniquement lorsqu'un auteur est sélectionné, et supprime également les livres associés pour maintenir l'intégrité.
         * Les méthodes sont asynchrones pour éviter de bloquer l'interface lors des opérations sur la base de données.
         */
        private void GridAuteurs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _auteurSelectionne = GridAuteurs.SelectedItem as Auteur;

            if (_auteurSelectionne is null)
            {
                ViderFormAuteur();
                MettreStatusCrud("Aucun auteur sélectionné.");
                return;
            }

            TxtAuteurNom.Text = _auteurSelectionne.Nom;
            TxtAuteurPrenom.Text = _auteurSelectionne.Prenom ?? string.Empty;
            TxtAuteurEmail.Text = _auteurSelectionne.Email ?? string.Empty;

            BtnAuteurSupprimer.IsEnabled = true;
            MettreStatusCrud($"Auteur sélectionné: {_auteurSelectionne.Nom}");
        }

        // Handler du bouton "Nouveau" qui désélectionne tout dans le DataGrid et vide le formulaire pour permettre la saisie d'un nouvel auteur.
        private void BtnAuteurNouveau_Click(object sender, RoutedEventArgs e)
        {
            GridAuteurs.SelectedItem = null;
            ViderFormAuteur();
            MettreStatusCrud("Saisie nouvel auteur.");
        }

        // Handler du bouton "Enregistrer" qui gère à la fois la création d'un nouvel auteur et la modification d'un auteur existant,
        // avec des vérifications de doublons et de validation.
        private async void BtnAuteurEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            var nom = TxtAuteurNom.Text.Trim();
            var prenom = TxtAuteurPrenom.Text.Trim();
            var email = TxtAuteurEmail.Text.Trim();

            if (string.IsNullOrWhiteSpace(nom))
            {
                MettreStatusCrud("Nom obligatoire.");
                return;
            }

            if (_auteurSelectionne is null)
            {
                bool existeDeja = await _context.Auteurs.AnyAsync(x =>
                    x.Nom == nom
                    && (x.Prenom ?? string.Empty) == prenom
                    && (x.Email ?? string.Empty) == email);

                if (existeDeja)
                {
                    MettreStatusCrud("Doublon détecté : auteur déjà présent.");
                    return;
                }

                _context.Auteurs.Add(new Auteur
                {
                    Nom = nom,
                    Prenom = string.IsNullOrWhiteSpace(prenom) ? null : prenom,
                    Email = string.IsNullOrWhiteSpace(email) ? null : email,
                    DateCreationUtc = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await ChargerAuteursAsync();

                ViderFormAuteur();
                MettreStatusCrud("Auteur ajouté.");
                return;
            }

            var auteurDb = await _context.Auteurs.FirstOrDefaultAsync(x => x.Id == _auteurSelectionne.Id);
            if (auteurDb is null)
            {
                MettreStatusCrud("Modification impossible : auteur introuvable.");
                await ChargerAuteursAsync();
                return;
            }

            bool doublon = await _context.Auteurs.AnyAsync(x =>
                x.Id != auteurDb.Id
                && x.Nom == nom
                && (x.Prenom ?? string.Empty) == prenom
                && (x.Email ?? string.Empty) == email);

            if (doublon)
            {
                MettreStatusCrud("Modification refusée : doublon détecté.");
                return;
            }

            auteurDb.Nom = nom;
            auteurDb.Prenom = string.IsNullOrWhiteSpace(prenom) ? null : prenom;
            auteurDb.Email = string.IsNullOrWhiteSpace(email) ? null : email;

            await _context.SaveChangesAsync();
            await ChargerAuteursAsync();

            MettreStatusCrud("Auteur modifié.");
        }

        // Handler du bouton "Supprimer" qui vérifie d'abord que l'auteur existe toujours en base,
        // puis supprime les livres associés avant de supprimer l'auteur lui-même pour maintenir l'intégrité.
        private async void BtnAuteurSupprimer_Click(object sender, RoutedEventArgs e)
        {
            if (_auteurSelectionne is null)
            {
                MettreStatusCrud("Suppression impossible : aucun auteur sélectionné.");
                return;
            }

            var auteurDb = await _context.Auteurs.FirstOrDefaultAsync(x => x.Id == _auteurSelectionne.Id);
            if (auteurDb is null)
            {
                MettreStatusCrud("Suppression impossible : auteur introuvable.");
                await ChargerAuteursAsync();
                return;
            }

            var livresAuteur = await _context.Livres.Where(x => x.AuteurId == auteurDb.Id).ToListAsync();
            if (livresAuteur.Count > 0)
            {
                _context.Livres.RemoveRange(livresAuteur);
            }

            _context.Auteurs.Remove(auteurDb);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                MettreStatusCrud($"Erreur suppression (intégrité): {ex.GetBaseException().Message}");
                return;
            }

            await RechargerToutAsync();
            await ChargerFiltresLivresAsync();
            await ChargerFormLivreAsync();

            GridAuteurs.SelectedItem = null;
            ViderFormAuteur();
            MettreStatusCrud("Auteur supprimé (et livres associés supprimés).");
        }

        // Méthode utilitaire pour vider le formulaire d'auteur et désactiver le bouton de suppression.
        private void ViderFormAuteur()
        {
            TxtAuteurNom.Text = string.Empty;
            TxtAuteurPrenom.Text = string.Empty;
            TxtAuteurEmail.Text = string.Empty;
            BtnAuteurSupprimer.IsEnabled = false;
        }

        // =========================
        // CATEGORIES - CRUD + intégrité (supprime liens LivreCategorie)
        // =========================
        /***Cette section gère les catégories avec des opérations CRUD similaires à celles des auteurs, mais avec une attention particulière à l'intégrité des données.
         * Lors de la suppression d'une catégorie, tous les liens avec les livres (LivreCategorie) sont également supprimés pour éviter les références orphelines.
         * Les vérifications de doublons et de validation sont également présentes pour garantir la qualité des données.
         * Les méthodes sont asynchrones pour maintenir une interface réactive lors des opérations sur la base de données.
         */
        private void GridCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _categorieSelectionnee = GridCategories.SelectedItem as Categorie;

            if (_categorieSelectionnee is null)
            {
                ViderFormCategorie();
                MettreStatusCrud("Aucune catégorie sélectionnée.");
                return;
            }

            TxtCategorieNom.Text = _categorieSelectionnee.Nom;
            TxtCategorieDescription.Text = _categorieSelectionnee.Description ?? string.Empty;

            BtnCategorieSupprimer.IsEnabled = true;
            MettreStatusCrud($"Catégorie sélectionnée: {_categorieSelectionnee.Nom}");
        }

        // Handler du bouton "Nouveau" qui désélectionne tout dans le DataGrid et vide le formulaire pour permettre la saisie d'une nouvelle catégorie.
        private void BtnCategorieNouveau_Click(object sender, RoutedEventArgs e)
        {
            GridCategories.SelectedItem = null;
            ViderFormCategorie();
            MettreStatusCrud("Saisie nouvelle catégorie.");
        }
        // Handler du bouton "Enregistrer" qui gère à la fois la création d'une nouvelle catégorie et la modification d'une catégorie existante,
        // avec des vérifications de doublons et de validation.
        private async void BtnCategorieEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            var nom = TxtCategorieNom.Text.Trim();
            var description = TxtCategorieDescription.Text.Trim();

            if (string.IsNullOrWhiteSpace(nom))
            {
                MettreStatusCrud("Nom de catégorie obligatoire.");
                return;
            }

            if (_categorieSelectionnee is null)
            {
                bool existeDeja = await _context.Categories.AnyAsync(x => x.Nom == nom);
                if (existeDeja)
                {
                    MettreStatusCrud("Doublon détecté : catégorie déjà présente.");
                    return;
                }

                _context.Categories.Add(new Categorie
                {
                    Nom = nom,
                    Description = string.IsNullOrWhiteSpace(description) ? null : description
                });

                await _context.SaveChangesAsync();

                await RechargerToutAsync();
                await ChargerFiltresLivresAsync();
                await ChargerFormLivreAsync();

                ViderFormCategorie();
                MettreStatusCrud("Catégorie ajoutée.");
                return;
            }

            var categorieDb = await _context.Categories.FirstOrDefaultAsync(x => x.Id == _categorieSelectionnee.Id);
            if (categorieDb is null)
            {
                MettreStatusCrud("Modification impossible : catégorie introuvable.");
                await ChargerCategoriesAsync();
                return;
            }

            bool doublon = await _context.Categories.AnyAsync(x => x.Id != categorieDb.Id && x.Nom == nom);
            if (doublon)
            {
                MettreStatusCrud("Modification refusée : doublon détecté.");
                return;
            }

            categorieDb.Nom = nom;
            categorieDb.Description = string.IsNullOrWhiteSpace(description) ? null : description;

            await _context.SaveChangesAsync();

            await RechargerToutAsync();
            await ChargerFiltresLivresAsync();
            await ChargerFormLivreAsync();

            MettreStatusCrud("Catégorie modifiée.");
        }

        // Handler du bouton "Supprimer" qui vérifie d'abord que la catégorie existe toujours en base,
        // puis supprime les liens avec les livres avant de supprimer la catégorie elle-même pour maintenir l'intégrité.
        private async void BtnCategorieSupprimer_Click(object sender, RoutedEventArgs e)
        {
            if (_categorieSelectionnee is null)
            {
                MettreStatusCrud("Suppression impossible : aucune catégorie sélectionnée.");
                return;
            }

            var categorieDb = await _context.Categories.FirstOrDefaultAsync(x => x.Id == _categorieSelectionnee.Id);
            if (categorieDb is null)
            {
                MettreStatusCrud("Suppression impossible : catégorie introuvable.");
                await ChargerCategoriesAsync();
                return;
            }

            var liens = await _context.LivreCategories
                .Where(x => x.CategorieId == categorieDb.Id)
                .ToListAsync();

            if (liens.Count > 0)
            {
                _context.LivreCategories.RemoveRange(liens);
            }

            _context.Categories.Remove(categorieDb);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                MettreStatusCrud($"Erreur suppression (intégrité): {ex.GetBaseException().Message}");
                return;
            }

            await RechargerToutAsync();
            await ChargerFiltresLivresAsync();
            await ChargerFormLivreAsync();

            GridCategories.SelectedItem = null;
            ViderFormCategorie();
            MettreStatusCrud("Catégorie supprimée (liens LivreCatégorie supprimés).");
        }

        // Méthode utilitaire pour vider le formulaire de catégorie et désactiver le bouton de suppression.
        private void ViderFormCategorie()
        {
            TxtCategorieNom.Text = string.Empty;
            TxtCategorieDescription.Text = string.Empty;
            BtnCategorieSupprimer.IsEnabled = false;
        }

        // =========================
        // EDITEURS - CRUD + intégrité (supprime d'abord les livres)
        // =========================
        /***Cette section gère les éditeurs avec des opérations CRUD similaires à celles des auteurs, mais avec une attention particulière à l'intégrité des données.
            * Lors de la suppression d'un éditeur, tous les livres associés sont également supprimés pour éviter les références orphelines.
            * Les vérifications de doublons et de validation sont également présentes pour garantir la qualité des données.
            * Les méthodes sont asynchrones pour maintenir une interface réactive lors des opérations sur la base de données.
            */
        private void GridEditeurs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _editeurSelectionne = GridEditeurs.SelectedItem as Editeur;

            if (_editeurSelectionne is null)
            {
                ViderFormEditeur();
                MettreStatusCrud("Aucun éditeur sélectionné.");
                return;
            }

            TxtEditeurNom.Text = _editeurSelectionne.Nom;
            TxtEditeurTelephone.Text = _editeurSelectionne.Telephone ?? string.Empty;
            TxtEditeurSiteWeb.Text = _editeurSelectionne.SiteWeb ?? string.Empty;

            BtnEditeurSupprimer.IsEnabled = true;
            MettreStatusCrud($"Éditeur sélectionné: {_editeurSelectionne.Nom}");
        }

        // Handler du bouton "Nouveau" qui désélectionne tout dans le DataGrid et vide le formulaire pour permettre la saisie d'un nouvel éditeur.
        private void BtnEditeurNouveau_Click(object sender, RoutedEventArgs e)
        {
            GridEditeurs.SelectedItem = null;
            ViderFormEditeur();
            MettreStatusCrud("Saisie nouvel éditeur.");
        }

        // Handler du bouton "Enregistrer" qui gère à la fois la création d'un nouvel éditeur et la modification d'un éditeur existant,
        // avec des vérifications de doublons et de validation.
        private async void BtnEditeurEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            var nom = TxtEditeurNom.Text.Trim();
            var telephone = TxtEditeurTelephone.Text.Trim();
            var siteWeb = TxtEditeurSiteWeb.Text.Trim();

            if (string.IsNullOrWhiteSpace(nom))
            {
                MettreStatusCrud("Nom éditeur obligatoire.");
                return;
            }

            if (_editeurSelectionne is null)
            {
                bool existeDeja = await _context.Editeurs.AnyAsync(x => x.Nom == nom);
                if (existeDeja)
                {
                    MettreStatusCrud("Doublon détecté : éditeur déjà présent.");
                    return;
                }

                _context.Editeurs.Add(new Editeur
                {
                    Nom = nom,
                    Telephone = string.IsNullOrWhiteSpace(telephone) ? null : telephone,
                    SiteWeb = string.IsNullOrWhiteSpace(siteWeb) ? null : siteWeb,
                    DateCreationUtc = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                await RechargerToutAsync();
                await ChargerFiltresLivresAsync();
                await ChargerFormLivreAsync();

                ViderFormEditeur();
                MettreStatusCrud("Éditeur ajouté.");
                return;
            }

            var editeurDb = await _context.Editeurs.FirstOrDefaultAsync(x => x.Id == _editeurSelectionne.Id);
            if (editeurDb is null)
            {
                MettreStatusCrud("Modification impossible : éditeur introuvable.");
                await ChargerEditeursAsync();
                return;
            }

            bool doublon = await _context.Editeurs.AnyAsync(x => x.Id != editeurDb.Id && x.Nom == nom);
            if (doublon)
            {
                MettreStatusCrud("Modification refusée : doublon détecté.");
                return;
            }

            editeurDb.Nom = nom;
            editeurDb.Telephone = string.IsNullOrWhiteSpace(telephone) ? null : telephone;
            editeurDb.SiteWeb = string.IsNullOrWhiteSpace(siteWeb) ? null : siteWeb;

            await _context.SaveChangesAsync();

            await RechargerToutAsync();
            await ChargerFiltresLivresAsync();
            await ChargerFormLivreAsync();

            MettreStatusCrud("Éditeur modifié.");
        }

        // Handler du bouton "Supprimer" qui vérifie d'abord que l'éditeur existe toujours en base,
        // puis supprime les livres associés avant de supprimer l'éditeur lui-même pour maintenir l'intégrité.
        private async void BtnEditeurSupprimer_Click(object sender, RoutedEventArgs e)
        {
            if (_editeurSelectionne is null)
            {
                MettreStatusCrud("Suppression impossible : aucun éditeur sélectionné.");
                return;
            }

            var editeurDb = await _context.Editeurs.FirstOrDefaultAsync(x => x.Id == _editeurSelectionne.Id);
            if (editeurDb is null)
            {
                MettreStatusCrud("Suppression impossible : éditeur introuvable.");
                await ChargerEditeursAsync();
                return;
            }

            var livres = await _context.Livres.Where(x => x.EditeurId == editeurDb.Id).ToListAsync();
            if (livres.Count > 0)
            {
                _context.Livres.RemoveRange(livres);
            }

            _context.Editeurs.Remove(editeurDb);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                MettreStatusCrud($"Erreur suppression (intégrité): {ex.GetBaseException().Message}");
                return;
            }

            await RechargerToutAsync();
            await ChargerFiltresLivresAsync();
            await ChargerFormLivreAsync();

            GridEditeurs.SelectedItem = null;
            ViderFormEditeur();
            MettreStatusCrud("Éditeur supprimé (et livres associés supprimés).");
        }

        private void ViderFormEditeur()
        {
            TxtEditeurNom.Text = string.Empty;
            TxtEditeurTelephone.Text = string.Empty;
            TxtEditeurSiteWeb.Text = string.Empty;
            BtnEditeurSupprimer.IsEnabled = false;
        }

        // =========================
        // LIVRES - CRUD + intégrité (ISBN unique, FicheDetail 1-1, Catégories N-M)
        // =========================
        /***Cette section gère les livres avec des opérations CRUD complètes, en prenant en compte les spécificités de l'entité Livre :
         * pour la création et la modification, on vérifie que le titre et l'ISBN sont présents, 
         * que l'auteur et l'éditeur sont sélectionnés, et que l'ISBN est unique.
         * De plus, chaque livre peut avoir une fiche détaillée (1-1) et plusieurs catégories (N-M).
         ***/

        private async Task ChargerFormLivreAsync()
        {
            var auteurs = await _context.Auteurs.AsNoTracking().OrderBy(x => x.Nom).ToListAsync();
            CboLivreAuteurCrud.ItemsSource = auteurs;
            CboLivreAuteurCrud.SelectedItem = null;

            var editeurs = await _context.Editeurs.AsNoTracking().OrderBy(x => x.Nom).ToListAsync();
            CboLivreEditeurCrud.ItemsSource = editeurs;
            CboLivreEditeurCrud.SelectedItem = null;

            var categories = await _context.Categories.AsNoTracking().OrderBy(x => x.Nom).ToListAsync();
            LstLivreCategories.ItemsSource = categories;
            LstLivreCategories.SelectedItems.Clear();

            ViderFormLivre();
        }

        // Handler de sélection d'un livre dans le DataGrid qui affiche les détails du livre dans le formulaire de droite pour modification.
        // On remplit les champs du formulaire avec les données du livre sélectionné, y compris les données liées (Auteur, Editeur, FicheDetail, Catégories).
        private void GridLivres_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _livreSelectionne = GridLivres.SelectedItem as Livre;

            if (_livreSelectionne is null)
            {
                ViderFormLivre();
                BtnLivreSupprimer.IsEnabled = false;
                MettreStatusCrud("Aucun livre sélectionné.");
                return;
            }

            TxtLivreTitreCrud.Text = _livreSelectionne.Titre;
            TxtLivreIsbn.Text = _livreSelectionne.Isbn;
            DpLivrePublication.SelectedDate = _livreSelectionne.DatePublication;

            CboLivreAuteurCrud.SelectedValue = _livreSelectionne.AuteurId;
            CboLivreEditeurCrud.SelectedValue = _livreSelectionne.EditeurId;

            TxtFicheResume.Text = _livreSelectionne.FicheDetail?.Resume ?? string.Empty;
            TxtFicheLangue.Text = _livreSelectionne.FicheDetail?.Langue ?? string.Empty;
            TxtFichePages.Text = _livreSelectionne.FicheDetail?.NombrePages?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

            LstLivreCategories.SelectedItems.Clear();
            var sourceCats = LstLivreCategories.ItemsSource as IEnumerable<Categorie>;
            if (sourceCats is not null)
            {
                foreach (var lc in _livreSelectionne.LivreCategories)
                {
                    var cat = sourceCats.FirstOrDefault(x => x.Id == lc.CategorieId);
                    if (cat is not null)
                    {
                        LstLivreCategories.SelectedItems.Add(cat);
                    }
                }
            }

            BtnLivreSupprimer.IsEnabled = true;
            MettreStatusCrud($"Livre sélectionné: {_livreSelectionne.Titre}");
        }

        // Handler du bouton "Nouveau" qui désélectionne tout dans le DataGrid et vide le formulaire pour permettre la saisie d'un nouveau livre.
        private void BtnLivreNouveau_Click(object sender, RoutedEventArgs e)
        {
            GridLivres.SelectedItem = null;
            ViderFormLivre();
            MettreStatusCrud("Saisie nouveau livre.");
        }

        // Handler du bouton "Enregistrer" qui gère à la fois la création d'un nouveau livre et la modification d'un livre existant,
        // avec des vérifications de validation (titre, ISBN, auteur, éditeur) et d'intégrité (ISBN unique).
        private async void BtnLivreEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            var titre = TxtLivreTitreCrud.Text.Trim();
            var isbn = TxtLivreIsbn.Text.Trim();

            if (string.IsNullOrWhiteSpace(titre) || string.IsNullOrWhiteSpace(isbn))
            {
                MettreStatusCrud("Titre et ISBN obligatoires.");
                return;
            }

            if (CboLivreAuteurCrud.SelectedValue is not int auteurId)
            {
                MettreStatusCrud("Auteur obligatoire.");
                return;
            }

            if (CboLivreEditeurCrud.SelectedValue is not int editeurId)
            {
                MettreStatusCrud("Éditeur obligatoire.");
                return;
            }

            var datePublication = DpLivrePublication.SelectedDate ?? DateTime.Today;

            int? pages = null;
            var pagesText = TxtFichePages.Text.Trim();
            if (!string.IsNullOrWhiteSpace(pagesText))
            {
                if (!int.TryParse(pagesText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var pagesParsed) || pagesParsed < 0)
                {
                    MettreStatusCrud("Nombre de pages invalide.");
                    return;
                }

                pages = pagesParsed;
            }

            var resume = TxtFicheResume.Text.Trim();
            var langue = TxtFicheLangue.Text.Trim();

            var categoriesSelectionnees = LstLivreCategories.SelectedItems
                .Cast<Categorie>()
                .Select(x => x.Id)
                .ToList();

            if (_livreSelectionne is null)
            {
                bool isbnExiste = await _context.Livres.AnyAsync(x => x.Isbn == isbn);
                if (isbnExiste)
                {
                    MettreStatusCrud("Doublon détecté : ISBN déjà présent.");
                    return;
                }

                var livre = new Livre
                {
                    Titre = titre,
                    Isbn = isbn,
                    DatePublication = datePublication,
                    AuteurId = auteurId,
                    EditeurId = editeurId
                };

                if (!string.IsNullOrWhiteSpace(resume) || !string.IsNullOrWhiteSpace(langue) || pages.HasValue)
                {
                    livre.FicheDetail = new FicheDetail
                    {
                        Resume = string.IsNullOrWhiteSpace(resume) ? null : resume,
                        Langue = string.IsNullOrWhiteSpace(langue) ? null : langue,
                        NombrePages = pages
                    };
                }

                foreach (var catId in categoriesSelectionnees)
                {
                    livre.LivreCategories.Add(new LivreCategorie
                    {
                        CategorieId = catId,
                        DateAssociationUtc = DateTime.UtcNow
                    });
                }

                _context.Livres.Add(livre);
                await _context.SaveChangesAsync();

                await RechargerToutAsync();
                await ChargerFiltresLivresAsync();
                await ChargerFormLivreAsync();

                MettreStatusCrud("Livre ajouté.");
                return;
            }

            var livreDb = await _context.Livres
                .Include(x => x.FicheDetail)
                .Include(x => x.LivreCategories)
                .FirstOrDefaultAsync(x => x.Id == _livreSelectionne.Id);

            if (livreDb is null)
            {
                MettreStatusCrud("Modification impossible : livre introuvable.");
                await ChargerLivresAsync();
                return;
            }

            bool isbnDoublon = await _context.Livres.AnyAsync(x => x.Id != livreDb.Id && x.Isbn == isbn);
            if (isbnDoublon)
            {
                MettreStatusCrud("Modification refusée : ISBN déjà utilisé.");
                return;
            }

            livreDb.Titre = titre;
            livreDb.Isbn = isbn;
            livreDb.DatePublication = datePublication;
            livreDb.AuteurId = auteurId;
            livreDb.EditeurId = editeurId;

            if (string.IsNullOrWhiteSpace(resume) && string.IsNullOrWhiteSpace(langue) && !pages.HasValue)
            {
                if (livreDb.FicheDetail is not null)
                {
                    _context.FicheDetails.Remove(livreDb.FicheDetail);
                    livreDb.FicheDetail = null;
                }
            }
            else
            {
                if (livreDb.FicheDetail is null)
                {
                    livreDb.FicheDetail = new FicheDetail();
                }

                livreDb.FicheDetail.Resume = string.IsNullOrWhiteSpace(resume) ? null : resume;
                livreDb.FicheDetail.Langue = string.IsNullOrWhiteSpace(langue) ? null : langue;
                livreDb.FicheDetail.NombrePages = pages;
            }

            // Sync N-M catégories
            var existantes = livreDb.LivreCategories.Select(x => x.CategorieId).ToHashSet();
            var nouvelles = categoriesSelectionnees.ToHashSet();

            var aSupprimer = livreDb.LivreCategories.Where(x => !nouvelles.Contains(x.CategorieId)).ToList();
            if (aSupprimer.Count > 0)
            {
                _context.LivreCategories.RemoveRange(aSupprimer);
            }

            var aAjouter = nouvelles.Where(id => !existantes.Contains(id)).ToList();
            foreach (var catId in aAjouter)
            {
                livreDb.LivreCategories.Add(new LivreCategorie
                {
                    LivreId = livreDb.Id,
                    CategorieId = catId,
                    DateAssociationUtc = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            await RechargerToutAsync();
            await ChargerFiltresLivresAsync();
            await ChargerFormLivreAsync();

            MettreStatusCrud("Livre modifié.");
        }

        // Handler du bouton "Supprimer" qui vérifie d'abord que le livre existe toujours en base,
        // puis supprime les associations avec les catégories avant de supprimer le livre lui-même.
        private async void BtnLivreSupprimer_Click(object sender, RoutedEventArgs e)
        {
            if (_livreSelectionne is null)
            {
                MettreStatusCrud("Suppression impossible : aucun livre sélectionné.");
                return;
            }

            var livreDb = await _context.Livres
                .Include(x => x.LivreCategories)
                .FirstOrDefaultAsync(x => x.Id == _livreSelectionne.Id);

            if (livreDb is null)
            {
                MettreStatusCrud("Suppression impossible : livre introuvable.");
                await ChargerLivresAsync();
                return;
            }

            if (livreDb.LivreCategories.Count > 0)
            {
                _context.LivreCategories.RemoveRange(livreDb.LivreCategories);
            }

            _context.Livres.Remove(livreDb);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                MettreStatusCrud($"Erreur suppression (intégrité): {ex.GetBaseException().Message}");
                return;
            }

            await RechargerToutAsync();
            await ChargerFiltresLivresAsync();
            await ChargerFormLivreAsync();

            GridLivres.SelectedItem = null;
            ViderFormLivre();
            MettreStatusCrud("Livre supprimé.");
        }

        // Méthode utilitaire pour vider le formulaire de livre et désactiver le bouton de suppression.
        // Les champs liés à la fiche détaillée et aux catégories sont également réinitialisés.
        private void ViderFormLivre()
        {
            TxtLivreTitreCrud.Text = string.Empty;
            TxtLivreIsbn.Text = string.Empty;
            DpLivrePublication.SelectedDate = null;

            CboLivreAuteurCrud.SelectedItem = null;
            CboLivreEditeurCrud.SelectedItem = null;

            TxtFicheResume.Text = string.Empty;
            TxtFicheLangue.Text = string.Empty;
            TxtFichePages.Text = string.Empty;

            if (LstLivreCategories.ItemsSource is not null)
            {
                LstLivreCategories.SelectedItems.Clear();
            }

            BtnLivreSupprimer.IsEnabled = false;
        }

        // =========================
        // Helpers StatusBar
        // =========================

        // Méthodes utilitaires pour afficher des messages dans la StatusBar,
        // séparant les messages liés aux opérations CRUD et à la recherche pour plus de clarté.
        private void MettreStatusCrud(string message) => TxtStatusCrud.Text = message;

        // Méthode utilitaire pour afficher les messages liés à la recherche dans la StatusBar.
        private void MettreStatusRecherche(string message) => TxtStatusRecherche.Text = message;
    }
}