CREATE TABLE [dbo].[FavoriteLocalisation] (
    [RelationId]     INT IDENTITY (1, 1) NOT NULL,
    [MemberId]       INT NOT NULL,
    [LocalisationId] INT NOT NULL,
    CONSTRAINT [PK_FavoriteLocalisation] PRIMARY KEY CLUSTERED ([RelationId] ASC, [MemberId] ASC),
    CONSTRAINT [FK_FavoriteLocalisation_Localisation] FOREIGN KEY ([LocalisationId]) REFERENCES [dbo].[Localisation] ([ID]) ON DELETE CASCADE ON UPDATE NO ACTION,
    CONSTRAINT [FK_FavoriteLocalisation_Member] FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Member] ([MemberId]) ON DELETE CASCADE ON UPDATE NO ACTION
);

