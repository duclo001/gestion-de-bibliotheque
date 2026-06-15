IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260328010517_InitialSchema'
)
BEGIN
    CREATE TABLE [Auteurs] (
        [Id] int NOT NULL IDENTITY,
        [Nom] nvarchar(100) NOT NULL,
        [Prenom] nvarchar(100) NULL,
        [Email] nvarchar(200) NULL,
        [DateCreationUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_Auteurs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260328010517_InitialSchema'
)
BEGIN
    CREATE TABLE [Categories] (
        [Id] int NOT NULL IDENTITY,
        [Nom] nvarchar(80) NOT NULL,
        [Description] nvarchar(250) NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260328010517_InitialSchema'
)
BEGIN
    CREATE TABLE [Editeurs] (
        [Id] int NOT NULL IDENTITY,
        [Nom] nvarchar(150) NOT NULL,
        [Telephone] nvarchar(20) NULL,
        [SiteWeb] nvarchar(200) NULL,
        [DateCreationUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_Editeurs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260328010517_InitialSchema'
)
BEGIN
    CREATE TABLE [Livres] (
        [Id] int NOT NULL IDENTITY,
        [Titre] nvarchar(200) NOT NULL,
        [Isbn] nvarchar(13) NOT NULL,
        [DatePublication] datetime2 NOT NULL,
        [AuteurId] int NOT NULL,
        [EditeurId] int NOT NULL,
        CONSTRAINT [PK_Livres] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Livres_Auteurs_AuteurId] FOREIGN KEY ([AuteurId]) REFERENCES [Auteurs] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Livres_Editeurs_EditeurId] FOREIGN KEY ([EditeurId]) REFERENCES [Editeurs] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260328010517_InitialSchema'
)
BEGIN
    CREATE TABLE [FicheDetails] (
        [Id] int NOT NULL IDENTITY,
        [LivreId] int NOT NULL,
        [Resume] nvarchar(2000) NULL,
        [Langue] nvarchar(50) NULL,
        [NombrePages] int NULL,
        [DateCreationUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_FicheDetails] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FicheDetails_Livres_LivreId] FOREIGN KEY ([LivreId]) REFERENCES [Livres] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260328010517_InitialSchema'
)
BEGIN
    CREATE TABLE [LivreCategories] (
        [LivreId] int NOT NULL,
        [CategorieId] int NOT NULL,
        [DateAssociationUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_LivreCategories] PRIMARY KEY ([LivreId], [CategorieId]),
        CONSTRAINT [FK_LivreCategories_Categories_CategorieId] FOREIGN KEY ([CategorieId]) REFERENCES [Categories] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_LivreCategories_Livres_LivreId] FOREIGN KEY ([LivreId]) REFERENCES [Livres] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260328010517_InitialSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Categories_Nom] ON [Categories] ([Nom]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260328010517_InitialSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Editeurs_Nom] ON [Editeurs] ([Nom]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260328010517_InitialSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_FicheDetails_LivreId] ON [FicheDetails] ([LivreId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260328010517_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_LivreCategories_CategorieId] ON [LivreCategories] ([CategorieId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260328010517_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_Livres_AuteurId] ON [Livres] ([AuteurId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260328010517_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_Livres_EditeurId] ON [Livres] ([EditeurId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260328010517_InitialSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Livres_Isbn] ON [Livres] ([Isbn]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260328010517_InitialSchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260328010517_InitialSchema', N'9.0.0');
END;

COMMIT;
GO

