CREATE TABLE [dbo].[LocalisationFile] (
    [ID]             INT            IDENTITY (1, 1) NOT NULL,
    [LocalisationID] INT            NOT NULL,
    [FileName]       NVARCHAR (256) NOT NULL,
    [IsDefault]      BIT            NOT NULL,
    [IsLogo]         BIT            NOT NULL,
    CONSTRAINT [PK_LocalisationFile] PRIMARY KEY CLUSTERED ([ID] ASC, [LocalisationID] ASC),
    CONSTRAINT [FK_LocalisationFile_Localisation] FOREIGN KEY ([LocalisationID]) REFERENCES [dbo].[Localisation] ([ID]) ON DELETE CASCADE ON UPDATE NO ACTION
);

