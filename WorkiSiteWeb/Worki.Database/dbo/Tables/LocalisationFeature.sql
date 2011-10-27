CREATE TABLE [dbo].[LocalisationFeature] (
    [ID]             INT IDENTITY (1, 1) NOT NULL,
    [LocalisationID] INT NOT NULL,
    [FeatureID]      INT NOT NULL,
    [OfferID]        INT CONSTRAINT [DF_LocalisationFeature_OfferID] DEFAULT ((0)) NOT NULL,
    [StringValue] NVARCHAR(256) NULL, 
    [DecimalValue] DECIMAL(18, 2) NULL, 
    CONSTRAINT [PK_LocalisationFeature] PRIMARY KEY CLUSTERED ([ID] ASC, [LocalisationID] ASC),
    CONSTRAINT [FK_LocalisationFeature_Localisation] FOREIGN KEY ([LocalisationID]) REFERENCES [dbo].[Localisation] ([ID]) ON DELETE CASCADE ON UPDATE NO ACTION
);

