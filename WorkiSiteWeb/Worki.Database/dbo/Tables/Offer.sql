CREATE TABLE [dbo].[Offer]
(
	[Id] INT NOT NULL  IDENTITY, 
    [LocalisationId] INT NOT NULL, 
	[Type] INT NOT NULL, 
    [Name] NVARCHAR(256) NOT NULL DEFAULT '', 
    [Capacity] INT NOT NULL DEFAULT 0, 
    [Price] DECIMAL(18, 2) NOT NULL DEFAULT 0, 
    [Period] INT NOT NULL DEFAULT 0, 
    [IsOnline] BIT NOT NULL DEFAULT 1, 
	[IsBookable] BIT NOT NULL DEFAULT 0,
	[IsQuotable] BIT NOT NULL DEFAULT 0, 
    [PaymentType] INT NOT NULL DEFAULT 0, 
    [Currency] INT NOT NULL DEFAULT 0, 
    CONSTRAINT [FK_Offer_Localisation] FOREIGN KEY (LocalisationId) REFERENCES [dbo].[Localisation] ([ID]) ON DELETE CASCADE ON UPDATE NO ACTION, 
    CONSTRAINT [PK_Offer] PRIMARY KEY ([Id])
)
