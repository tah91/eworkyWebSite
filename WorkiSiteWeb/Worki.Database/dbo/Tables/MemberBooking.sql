﻿CREATE TABLE [dbo].[MemberBooking] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [MemberId]       INT            NOT NULL,
    [LocalisationId] INT            NOT NULL,
    [Offer]          INT            NOT NULL,
    [FromDate]       DATETIME       NOT NULL,
    [ToDate]         DATETIME       NOT NULL,
    [Message]        NVARCHAR (MAX) NULL,
    [Handled]        BIT            CONSTRAINT [DF__MemberBoo__Handl__59FA5E80] DEFAULT ((0)) NOT NULL,
    [Confirmed]      BIT            CONSTRAINT [DF__MemberBoo__Confi__5AEE82B9] DEFAULT ((0)) NOT NULL,
    [Price]          INT            CONSTRAINT [DF__MemberBoo__Price__5BE2A6F2] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_MemberBooking] PRIMARY KEY CLUSTERED ([Id] ASC, [MemberId] ASC),
    CONSTRAINT [FK_MemberBooking_Localisation] FOREIGN KEY ([LocalisationId]) REFERENCES [dbo].[Localisation] ([ID]) ON DELETE CASCADE ON UPDATE NO ACTION,
    CONSTRAINT [FK_MemberBooking_Member] FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Member] ([MemberId]) ON DELETE CASCADE ON UPDATE NO ACTION
);
