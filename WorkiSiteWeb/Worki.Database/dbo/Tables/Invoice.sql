CREATE TABLE [dbo].[Invoice]
(
	[Id]				INT	IDENTITY (1, 1) NOT NULL,
	[LocalisationId]	INT		NOT NULL,
	[MemberId]			INT		NOT NULL,
	[InvoiceNumberId]	INT		NOT NULL DEFAULT(0),
	[Title]				NVARCHAR (256) NULL,
	[Currency]			INT		NOT NULL DEFAULT(0),
	[PaymentType]		INT		NOT NULL DEFAULT(0),
	[TaxRate]			DECIMAL(18, 2) NOT NULL DEFAULT (0), 
    [CreationDate]		DATETIME NOT NULL, 
    CONSTRAINT [FK_Invoice_Member] FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Member] ([MemberId]) ON DELETE CASCADE ON UPDATE NO ACTION,
    CONSTRAINT [FK_Invoice_Localisation] FOREIGN KEY ([LocalisationId]) REFERENCES [dbo].[Localisation] ([ID]) ON DELETE CASCADE ON UPDATE NO ACTION, 
	CONSTRAINT [FK_Invoice_InvoiceNumber] FOREIGN KEY ([InvoiceNumberId]) REFERENCES [dbo].[InvoiceNumber] ([Id]) ON DELETE CASCADE ON UPDATE NO ACTION, 
    CONSTRAINT [PK_Invoice] PRIMARY KEY ([Id])
)
