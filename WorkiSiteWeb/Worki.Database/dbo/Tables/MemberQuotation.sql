CREATE TABLE [dbo].[MemberQuotation]
(
	[Id] INT NOT NULL , 
    [MemberId] INT NOT NULL, 
    [LocalisationId] INT NOT NULL, 
    [OfferId] INT NOT NULL, 
    [Surface] DECIMAL(18, 2) NOT NULL DEFAULT 0, 
    [Message] NVARCHAR(MAX) NULL, 
    [Price] DECIMAL(18, 2) NOT NULL DEFAULT 0, 
    [Handled] BIT NOT NULL DEFAULT 0, 
    [Confirmed] BIT NOT NULL DEFAULT 0, 
    [Refused] BIT NOT NULL DEFAULT 0, 
	CONSTRAINT [PK_MemberQuotation] PRIMARY KEY CLUSTERED ([Id] ASC, [MemberId] ASC, [LocalisationId] ASC, [OfferId] ASC),
    CONSTRAINT [FK_MemberQuotation_Offer] FOREIGN KEY ([OfferId],[LocalisationId]) REFERENCES [dbo].[Offer] ([Id],[LocalisationId]) ON DELETE CASCADE ON UPDATE NO ACTION,
    CONSTRAINT [FK_MemberQuotation_Member] FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Member] ([MemberId]) ON DELETE CASCADE ON UPDATE NO ACTION
)
