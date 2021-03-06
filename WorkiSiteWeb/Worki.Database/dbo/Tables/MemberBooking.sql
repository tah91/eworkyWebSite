﻿CREATE TABLE [dbo].[MemberBooking] (
    [Id]				INT            IDENTITY (1, 1) NOT NULL,
    [MemberId]			INT            NOT NULL,
    [OfferId]			INT            NOT NULL,
	[InvoiceNumberId]	INT            NOT NULL DEFAULT 0,
    [FromDate]			DATETIME       NOT NULL,
    [ToDate]			DATETIME       NOT NULL, 
    [Message]			NVARCHAR (MAX) NULL,
    [Price]				DECIMAL(18, 2)            DEFAULT (0) NOT NULL,
    [StatusId]			INT NOT NULL   DEFAULT (0),
    [Response]			NVARCHAR(MAX)  NULL, 
    [TimeUnits]			INT NOT NULL DEFAULT 0, 
    [TimeType]			INT NOT NULL DEFAULT 0, 
    [PeriodType]		INT NOT NULL DEFAULT 0, 
    [PaymentType]		INT NOT NULL DEFAULT 0, 
    CONSTRAINT [FK_MemberBooking_Offer] FOREIGN KEY ([OfferId]) REFERENCES [dbo].[Offer] ([Id]) ON DELETE CASCADE ON UPDATE NO ACTION,
    CONSTRAINT [FK_MemberBooking_Member] FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Member] ([MemberId]) ON DELETE CASCADE ON UPDATE NO ACTION, 
	CONSTRAINT [FK_MemberBooking_InvoiceNumber] FOREIGN KEY ([InvoiceNumberId]) REFERENCES [dbo].[InvoiceNumber] ([Id]) ON DELETE CASCADE ON UPDATE NO ACTION, 
    CONSTRAINT [PK_MemberBooking] PRIMARY KEY ([Id])
);

