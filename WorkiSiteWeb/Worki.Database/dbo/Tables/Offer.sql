CREATE TABLE [dbo].[Offer]
(
	[Id] INT NOT NULL  IDENTITY, 
    [LocalisationId] INT NOT NULL, 
	[Type] INT NOT NULL, 
    [Name] NVARCHAR(256) NOT NULL DEFAULT '', 
    [Capacity] INT NOT NULL DEFAULT 0, 
    [Price] DECIMAL(18, 2) NOT NULL DEFAULT 0, 
    [Period] INT NOT NULL DEFAULT 0, 
    [IsOffline] BIT NOT NULL DEFAULT 0, 
    CONSTRAINT [PK_Offer] PRIMARY KEY CLUSTERED ([Id] ASC, [LocalisationId] ASC), 
	CONSTRAINT [FK_Offer_Localisation] FOREIGN KEY (LocalisationId) REFERENCES [dbo].[Localisation] ([ID]) ON DELETE CASCADE ON UPDATE NO ACTION
)
