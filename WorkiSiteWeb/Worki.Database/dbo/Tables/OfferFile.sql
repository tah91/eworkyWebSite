CREATE TABLE [dbo].[OfferFile]
(
	[Id] INT NOT NULL  IDENTITY, 
    [OfferId] INT NOT NULL, 
	[LocalisationId] INT NOT NULL, 
    [FileName] NVARCHAR(256) NOT NULL, 
    [IsDefault] BIT NOT NULL DEFAULT 0,
	CONSTRAINT [PK_OfferFile] PRIMARY KEY CLUSTERED ([Id] ASC, [OfferId] ASC, [LocalisationId] ASC), 
	CONSTRAINT [FK_OfferFile_Offer] FOREIGN KEY (OfferId,LocalisationId) REFERENCES [dbo].[Offer] ([Id],[LocalisationId]) ON DELETE CASCADE ON UPDATE NO ACTION
)
