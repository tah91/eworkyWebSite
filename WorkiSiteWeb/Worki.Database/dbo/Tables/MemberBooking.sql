CREATE TABLE [dbo].[MemberBooking] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [MemberId]       INT            NOT NULL,
	[LocalisationId] INT            NOT NULL,
    [OfferId]        INT            NOT NULL,
    [FromDate]       DATETIME       NOT NULL,
    [ToDate]         DATETIME       NOT NULL,
    [Message]        NVARCHAR (MAX) NULL,
    [Handled]        BIT            CONSTRAINT [DF__MemberBoo__Handl__59FA5E80] DEFAULT ((0)) NOT NULL,
    [Confirmed]      BIT            CONSTRAINT [DF__MemberBoo__Confi__5AEE82B9] DEFAULT ((0)) NOT NULL,
    [Price]          INT            CONSTRAINT [DF__MemberBoo__Price__5BE2A6F2] DEFAULT ((0)) NOT NULL,
    [Refused] BIT NOT NULL DEFAULT (0), 
    CONSTRAINT [PK_MemberBooking] PRIMARY KEY CLUSTERED ([Id] ASC, [MemberId] ASC, [LocalisationId] ASC, [OfferId] ASC),
    CONSTRAINT [FK_MemberBooking_Offer] FOREIGN KEY ([OfferId],[LocalisationId]) REFERENCES [dbo].[Offer] ([Id],[LocalisationId]) ON DELETE CASCADE ON UPDATE NO ACTION,
    CONSTRAINT [FK_MemberBooking_Member] FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Member] ([MemberId]) ON DELETE CASCADE ON UPDATE NO ACTION
);

