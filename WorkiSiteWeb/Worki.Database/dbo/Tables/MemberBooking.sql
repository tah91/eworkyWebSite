CREATE TABLE [dbo].[MemberBooking] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [MemberId]       INT            NOT NULL,
    [OfferId]        INT            NOT NULL,
    [FromDate]       DATETIME       NOT NULL,
    [ToDate]         DATETIME       NOT NULL, 
    [Message]        NVARCHAR (MAX) NULL,
    [Price]          INT            DEFAULT (0) NOT NULL,
    [StatusId]	     INT NOT NULL   DEFAULT (0),
    [Response]       NVARCHAR(MAX)  NULL, 
    CONSTRAINT [FK_MemberBooking_Offer] FOREIGN KEY ([OfferId]) REFERENCES [dbo].[Offer] ([Id]) ON DELETE CASCADE ON UPDATE NO ACTION,
    CONSTRAINT [FK_MemberBooking_Member] FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Member] ([MemberId]) ON DELETE CASCADE ON UPDATE NO ACTION, 
    CONSTRAINT [PK_MemberBooking] PRIMARY KEY ([Id])
);

