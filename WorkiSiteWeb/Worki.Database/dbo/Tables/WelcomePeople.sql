CREATE TABLE [dbo].[WelcomePeople] (
    [Id]                  INT            IDENTITY (1, 1) NOT NULL,
    [MemberId]            INT            CONSTRAINT [DF_WelcomePeople_MemberId] DEFAULT ((0)) NOT NULL,
    [LocalisationId]      INT            CONSTRAINT [DF_WelcomePeople_LocalisationId] DEFAULT ((0)) NOT NULL,
    [Description]         NVARCHAR (256) NOT NULL,
    [LocalisationPicture] NVARCHAR (256) NULL,
    [Online] BIT NOT NULL DEFAULT 1, 
    CONSTRAINT [PK_WelcomePeople] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_WelcomePeople_Localisation] FOREIGN KEY ([LocalisationId]) REFERENCES [dbo].[Localisation] ([ID]) ON DELETE CASCADE ON UPDATE NO ACTION,
    CONSTRAINT [FK_WelcomePeople_Member] FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Member] ([MemberId]) ON DELETE CASCADE ON UPDATE NO ACTION
);

