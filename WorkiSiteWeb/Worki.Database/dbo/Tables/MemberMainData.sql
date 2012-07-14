﻿CREATE TABLE [dbo].[MemberMainData] (
    [RelationId]     INT            IDENTITY (1, 1) NOT NULL,
    [MemberId]       INT            NOT NULL,
    [Civility]       INT            NOT NULL,
    [FirstName]      NVARCHAR (256) NOT NULL,
    [LastName]       NVARCHAR (256) NOT NULL,
    [Avatar]         NVARCHAR (256) NULL,
    [CompanyName]    NVARCHAR (256) NULL,
	[Address]        NVARCHAR (256)  NULL,
    [City]           NVARCHAR (50)  NULL,
    [PostalCode]     NVARCHAR (10)  NULL,
    [Country]        NVARCHAR (256) NULL,
    [WorkCity]       NVARCHAR (256) NULL,
    [WorkPostalCode] NVARCHAR (10)  NULL,
    [WorkCountry]    NVARCHAR (256) NULL,
    [Profile]        INT            NOT NULL,
    [BirthDate]      DATETIME       NULL,
    [PhoneNumber]    NVARCHAR (50)  NULL,
    [Description]    NVARCHAR (256) NULL,
    [FavoritePlaces] NVARCHAR (256) NULL,
    [Facebook]       NVARCHAR (256) NULL,
    [Twitter]        NVARCHAR (256) NULL,
    [Linkedin]       NVARCHAR (256) NULL,
    [Viadeo]         NVARCHAR (256) NULL,
	[Website]        NVARCHAR (256) NULL,
    [PaymentAddress] NVARCHAR (256) NULL, 
	[SiretNumber]    NVARCHAR (256) NULL, 
	[TaxNumber]      NVARCHAR (256) NULL, 
	[TaxRate]        decimal(18,2)  NOT NULL DEFAULT 0, 
	[BOStatus]       INT            NOT NULL DEFAULT 0,
    [ApiKey]		 NVARCHAR(256)	NULL, 
    [Token] NVARCHAR(256) NULL, 
    CONSTRAINT [PK_MemberMainData] PRIMARY KEY CLUSTERED ([MemberId] ASC),
    CONSTRAINT [FK_MemberMainData_Member] FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Member] ([MemberId]) ON DELETE CASCADE ON UPDATE NO ACTION
);

