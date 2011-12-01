CREATE TABLE [dbo].[MemberQuotationTransaction] 
(
    [Id]				INT           IDENTITY (1, 1) NOT NULL,
    [MemberQuotationId] INT			  NOT NULL,
    [Amount]			MONEY         NOT NULL,
    [PaymentType]		INT			  NOT NULL,
    [StatusId]			INT           NOT NULL,
    [TransactionId]		NVARCHAR (50) NULL,
    [CreatedDate]		DATETIME      NOT NULL,
    [UpdatedDate]		DATETIME      NULL,
    [RequestId]			NVARCHAR (50) NOT NULL,
    CONSTRAINT [FK_QuotationTransaction_MemberQuotation] FOREIGN KEY ([MemberQuotationId]) REFERENCES [dbo].[MemberQuotation] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT [PK_QuotationTransaction] PRIMARY KEY ([Id])
);

