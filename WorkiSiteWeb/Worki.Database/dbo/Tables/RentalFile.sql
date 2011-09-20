CREATE TABLE [dbo].[RentalFile] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [RentalId]  INT            NOT NULL,
    [FileName]  NVARCHAR (256) NOT NULL,
    [IsDefault] BIT            CONSTRAINT [DF_RentalFile_IsDefault] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_RentalFile] PRIMARY KEY CLUSTERED ([Id] ASC, [RentalId] ASC),
    CONSTRAINT [FK_RentalFile_Rental] FOREIGN KEY ([RentalId]) REFERENCES [dbo].[Rental] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION
);

