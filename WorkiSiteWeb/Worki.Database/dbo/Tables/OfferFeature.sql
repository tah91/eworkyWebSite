CREATE TABLE [dbo].[OfferFeature]
(
	[Id] INT NOT NULL IDENTITY , 
    [OfferId] INT NOT NULL, 
	[LocalisationId] INT NOT NULL, 
    [FeatureId] INT NOT NULL, 
    [StringValue] NVARCHAR(256) NULL, 
    [DecimalValue] DECIMAL(18, 2) NULL, 
    CONSTRAINT [PK_OfferFeature] PRIMARY KEY CLUSTERED ([Id] ASC, [OfferId] ASC, [LocalisationId] ASC), 
	CONSTRAINT [FK_OfferFeature_Offer] FOREIGN KEY (OfferId,LocalisationId) REFERENCES [dbo].[Offer] ([Id],[LocalisationId]) ON DELETE CASCADE ON UPDATE NO ACTION
)
