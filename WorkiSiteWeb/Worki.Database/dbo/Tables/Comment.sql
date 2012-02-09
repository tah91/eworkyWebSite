CREATE TABLE [dbo].[Comment] (
    [ID]             INT            IDENTITY (1, 1) NOT NULL,
    [LocalisationID] INT            NOT NULL,
    [PostUserID]     INT            CONSTRAINT [DF_Comment_PostUserID] DEFAULT ((0)) NULL,
    [Date]           DATETIME       NOT NULL,
    [Post]           NVARCHAR (MAX) NULL,
	[PostEn]         NVARCHAR (MAX) NULL,
	[PostEs]         NVARCHAR (MAX) NULL,
    [Rating]         INT            NOT NULL,
    [RatingPrice]    INT            CONSTRAINT [DF_Comment_RatingPrice] DEFAULT ((-1)) NOT NULL,
    [RatingWifi]     INT            CONSTRAINT [DF_Comment_RatingWifi] DEFAULT ((-1)) NOT NULL,
    [RatingDispo]    INT            CONSTRAINT [DF_Comment_RatingDispo] DEFAULT ((-1)) NOT NULL,
    [RatingWelcome]  INT            CONSTRAINT [DF_Comment_RatingWelcome] DEFAULT ((-1)) NOT NULL,
    CONSTRAINT [PK_Comment] PRIMARY KEY CLUSTERED ([ID] ASC, [LocalisationID] ASC),
    CONSTRAINT [FK_Comment_Localisation] FOREIGN KEY ([LocalisationID]) REFERENCES [dbo].[Localisation] ([ID]) ON DELETE CASCADE ON UPDATE NO ACTION,
    CONSTRAINT [FK_Comment_Member] FOREIGN KEY ([PostUserID]) REFERENCES [dbo].[Member] ([MemberId]) ON DELETE SET NULL ON UPDATE NO ACTION
);

