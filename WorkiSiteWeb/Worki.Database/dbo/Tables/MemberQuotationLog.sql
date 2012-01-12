CREATE TABLE [dbo].[MemberQuotationLog]
(
	[Id]				INT				NOT NULL IDENTITY,
    [MemberQuotationId] INT				NOT NULL,
	[LoggerId]		    INT             NOT NULL DEFAULT 0,
    [Event]				NVARCHAR (100)	NOT NULL,
    [CreatedDate]		DATETIME		NOT NULL, 
    [EventType]			INT				NOT NULL DEFAULT 0, 
    [Read]				INT				NOT NULL DEFAULT 0, 
    CONSTRAINT [PK_MemberQuotationLog] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MemberQuotationLog_MemberQuotation] FOREIGN KEY ([MemberQuotationId]) REFERENCES [dbo].[MemberQuotation] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION
)

