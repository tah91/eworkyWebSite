CREATE TABLE [dbo].[OfferPrice]
(
	[Id] INT NOT NULL IDENTITY, 
    [OfferId] INT NOT NULL, 
    [Price] DECIMAL NOT NULL, 
    [PriceType] INT NOT NULL,
	CONSTRAINT [FK_OfferPrice_Offer] FOREIGN KEY (OfferId) REFERENCES [dbo].[Offer] ([Id]) ON DELETE CASCADE ON UPDATE NO ACTION,
	CONSTRAINT [PK_OfferPrice] PRIMARY KEY ([OfferId],[Id])
)
