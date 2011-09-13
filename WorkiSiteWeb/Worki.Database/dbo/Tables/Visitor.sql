CREATE TABLE [dbo].[Visitor] (
    [Id]      INT            IDENTITY (1, 1) NOT NULL,
    [Email]   NVARCHAR (100) NOT NULL,
    [IsValid] BIT            CONSTRAINT [DF_Visitor_IsValid] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Visitor] PRIMARY KEY CLUSTERED ([Id] ASC)
);

