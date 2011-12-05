CREATE TABLE [dbo].[MemberQuotation]
(
	[Id]            INT     IDENTITY (1, 1) NOT NULL,
    [MemberId]		INT NOT NULL, 
    [OfferId]		INT NOT NULL, 
    [Surface]		DECIMAL(18, 2) NOT NULL DEFAULT 0, 
    [Message]		NVARCHAR(MAX) NULL,
    [StatusId]		INT NOT NULL DEFAULT 0, 
    [Response]		NVARCHAR(MAX) NULL, 
    [VisitNeeded]	BIT NOT NULL DEFAULT 0, 
    CONSTRAINT [FK_MemberQuotation_Offer] FOREIGN KEY ([OfferId]) REFERENCES [dbo].[Offer] ([Id]) ON DELETE CASCADE ON UPDATE NO ACTION,
    CONSTRAINT [FK_MemberQuotation_Member] FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Member] ([MemberId]) ON DELETE CASCADE ON UPDATE NO ACTION, 
    CONSTRAINT [PK_MemberQuotation] PRIMARY KEY ([Id])
)
