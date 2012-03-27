CREATE TABLE [dbo].[WelcomePeople] (
    [Id]                  INT            IDENTITY (1, 1) NOT NULL,
	[OfferId]			  INT		     NOT NULL DEFAULT 0, 
    [Description]         NVARCHAR (256) NOT NULL DEFAULT '',
    [LocalisationPicture] NVARCHAR (256) NULL,
	[SiteVersion]		  INT		     NOT NULL DEFAULT 0, 
    [Online]			  BIT NOT NULL DEFAULT 1, 
    CONSTRAINT [PK_WelcomePeople] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_WelcomePeople_Offer] FOREIGN KEY ([OfferId]) REFERENCES [dbo].[Offer] ([Id]) ON DELETE CASCADE ON UPDATE NO ACTION
);

