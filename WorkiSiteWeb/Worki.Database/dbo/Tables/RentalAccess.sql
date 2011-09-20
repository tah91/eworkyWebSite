CREATE TABLE [dbo].[RentalAccess] (
    [Id]       INT            IDENTITY (1, 1) NOT NULL,
    [RentalId] INT            NOT NULL,
    [Type]     INT            CONSTRAINT [DF_RentalAccess_Type] DEFAULT ((0)) NOT NULL,
    [Line]     NVARCHAR (10)  NOT NULL,
    [Station]  NVARCHAR (256) NOT NULL,
    CONSTRAINT [PK_RentalAccess] PRIMARY KEY CLUSTERED ([Id] ASC, [RentalId] ASC),
    CONSTRAINT [FK_RentalAccess_Rental] FOREIGN KEY ([RentalId]) REFERENCES [dbo].[Rental] ([Id]) ON DELETE CASCADE ON UPDATE NO ACTION
);

