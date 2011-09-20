CREATE TABLE [dbo].[RentalFeature] (
    [Id]        INT IDENTITY (1, 1) NOT NULL,
    [RentalId]  INT NOT NULL,
    [FeatureId] INT CONSTRAINT [DF_RentalFeature_FeatureId] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_RentalFeature] PRIMARY KEY CLUSTERED ([Id] ASC, [RentalId] ASC),
    CONSTRAINT [FK_RentalFeature_Rental] FOREIGN KEY ([RentalId]) REFERENCES [dbo].[Rental] ([Id]) ON DELETE CASCADE ON UPDATE NO ACTION
);

