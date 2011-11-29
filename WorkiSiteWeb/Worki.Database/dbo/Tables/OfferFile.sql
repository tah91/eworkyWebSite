CREATE TABLE [dbo].[OfferFile]
(
	[Id] INT NOT NULL  IDENTITY, 
    [OfferId] INT NOT NULL, 
    [FileName] NVARCHAR(256) NOT NULL, 
    [IsDefault] BIT NOT NULL DEFAULT 0, 
	CONSTRAINT [FK_OfferFile_Offer] FOREIGN KEY (OfferId) REFERENCES [dbo].[Offer] ([Id]) ON DELETE CASCADE ON UPDATE NO ACTION, 
    CONSTRAINT [PK_OfferFile] PRIMARY KEY ([Id], [OfferId])
)
