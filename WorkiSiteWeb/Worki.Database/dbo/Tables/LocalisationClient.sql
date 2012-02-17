CREATE TABLE [dbo].[LocalisationClient]
(
	[Id]				INT NOT NULL IDENTITY, 
    [LocalisationId]	INT NOT NULL, 
    [ClientId]			INT NOT NULL,
	CONSTRAINT [FK_LocalisationClient_Localisation] FOREIGN KEY ([LocalisationId]) REFERENCES [dbo].[Localisation] ([ID]) ON DELETE CASCADE ON UPDATE NO ACTION,
	CONSTRAINT [FK_LocalisationClient_Member] FOREIGN KEY ([ClientId]) REFERENCES [dbo].[Member] ([MemberId]) ON DELETE CASCADE ON UPDATE NO ACTION,
    CONSTRAINT [PK_LocalisationClient] PRIMARY KEY ([LocalisationId],[ClientId])
)

