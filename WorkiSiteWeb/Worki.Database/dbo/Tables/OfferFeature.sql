CREATE TABLE [dbo].[OfferFeature]
(
	[Id] INT NOT NULL IDENTITY , 
    [OfferId] INT NOT NULL, 
    [FeatureId] INT NOT NULL, 
    [StringValue] NVARCHAR(256) NULL, 
    [DecimalValue] DECIMAL(18, 2) NULL, 
	CONSTRAINT [FK_OfferFeature_Offer] FOREIGN KEY (OfferId) REFERENCES [dbo].[Offer] ([Id]) ON DELETE CASCADE ON UPDATE NO ACTION, 
    CONSTRAINT [PK_OfferFeature] PRIMARY KEY ([OfferId], [Id])
)
