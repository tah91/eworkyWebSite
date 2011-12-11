CREATE TABLE [dbo].[MainLocalisation] (
    [ID]             INT IDENTITY (1, 1) NOT NULL,
    [LocalisationID] INT NOT NULL,
    [IsMain] BIT NOT NULL DEFAULT 0, 
    [IsOffline] BIT NOT NULL DEFAULT 0, 
    [HasSticker] BIT NOT NULL DEFAULT 0, 
    CONSTRAINT [PK_MainLocalisation] PRIMARY KEY CLUSTERED ([LocalisationID] ASC),
    CONSTRAINT [FK_MainLocalisation_Localisation] FOREIGN KEY ([LocalisationID]) REFERENCES [dbo].[Localisation] ([ID]) ON DELETE CASCADE ON UPDATE NO ACTION
);

