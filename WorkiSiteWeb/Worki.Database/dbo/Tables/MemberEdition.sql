CREATE TABLE [dbo].[MemberEdition] (
    [Id]               INT      IDENTITY (1, 1) NOT NULL,
    [MemberId]         INT      NOT NULL,
    [LocalisationId]   INT      NOT NULL,
    [ModificationDate] DATETIME NOT NULL,
    [ModificationType] INT      CONSTRAINT [DF_MemberEdition_ModificationType] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [FK_MemberEdition_Localisation] FOREIGN KEY ([LocalisationId]) REFERENCES [dbo].[Localisation] ([ID]) ON DELETE CASCADE ON UPDATE NO ACTION,
    CONSTRAINT [FK_MemberEdition_Member] FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Member] ([MemberId]) ON DELETE CASCADE ON UPDATE NO ACTION, 
    CONSTRAINT [PK_MemberEdition] PRIMARY KEY ([Id])
);

