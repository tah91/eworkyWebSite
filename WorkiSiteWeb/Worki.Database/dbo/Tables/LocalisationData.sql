CREATE TABLE [dbo].[LocalisationData] (
    [ID]             INT             IDENTITY (1, 1) NOT NULL,
    [LocalisationID] INT             NOT NULL,
    [CoffeePrice]    DECIMAL (18, 2) NULL,
    CONSTRAINT [PK_LocalisationData] PRIMARY KEY CLUSTERED ([LocalisationID] ASC),
    CONSTRAINT [FK_LocalisationData_Localisation] FOREIGN KEY ([LocalisationID]) REFERENCES [dbo].[Localisation] ([ID]) ON DELETE CASCADE ON UPDATE NO ACTION
);

