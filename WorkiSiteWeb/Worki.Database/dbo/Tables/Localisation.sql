﻿CREATE TABLE [dbo].[Localisation] (
    [ID]                   INT            IDENTITY (1, 1) NOT NULL,
    [Name]                 NVARCHAR (256) NOT NULL,
    [TypeValue]            INT            CONSTRAINT [DF__Localisat__TypeV__4D94879B] DEFAULT ((0)) NOT NULL,
	[CompanyName]		   NVARCHAR (256) NULL,
	[CompanyType]         INT            NOT NULL DEFAULT 0,
    [Adress]               NVARCHAR (256) NOT NULL,
    [PostalCode]           NVARCHAR (10)   NOT NULL,
    [City]                 NVARCHAR (256)  NOT NULL,
	[CountryId]            NVARCHAR (10)   NOT NULL DEFAULT '',
    [PhoneNumber]          NVARCHAR (50)   NULL,
    [Mail]                 NVARCHAR (256)  NULL,
    [Fax]                  NVARCHAR (50)   NULL,
    [WebSite]              NVARCHAR (256)  NULL,
	[Facebook]             NVARCHAR (256)  NULL,
	[Twitter]              NVARCHAR (256)  NULL,
    [Description]          NVARCHAR (MAX) NULL,
	[DescriptionEn]        NVARCHAR (MAX) NULL,
	[DescriptionEs]        NVARCHAR (MAX) NULL,
	[DescriptionDe]        NVARCHAR (MAX) NULL,
	[DescriptionNl]        NVARCHAR (MAX) NULL,
    [Latitude]             FLOAT (53)     NOT NULL,
    [Longitude]            FLOAT (53)     NOT NULL,
    [OwnerID]              INT            CONSTRAINT [DF__Localisat__Owner__5535A963] DEFAULT ((0)) NULL,
    [PublicTransportation] NVARCHAR (256) NULL,
    [Station]              NVARCHAR (256) NULL,
    [RoadAccess]           NVARCHAR (256) NULL, 
    [BookingCom]		   DECIMAL(18, 2) NOT NULL DEFAULT 0, 
    [QuotationPrice]	   DECIMAL(18, 2) NOT NULL DEFAULT 0, 
    [DirectlyReceiveQuotation] BIT NOT NULL DEFAULT 0, 
    CONSTRAINT [PK_Localisation] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_Localisation_Member] FOREIGN KEY ([OwnerID]) REFERENCES [dbo].[Member] ([MemberId]) ON DELETE NO ACTION ON UPDATE NO ACTION
);

