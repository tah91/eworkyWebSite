CREATE TABLE [dbo].[InvoiceItem]
(
	[Id]				INT	IDENTITY (1, 1) NOT NULL,
	[InvoiceId]			INT				NOT NULL,
	[Description]		NVARCHAR (256)	NULL,
	[Price]				DECIMAL(18, 2)	NOT NULL DEFAULT(0),
	[Quantity]			INT				NOT NULL DEFAULT(0),
	CONSTRAINT [FK_InvoiceItem_Invoice] FOREIGN KEY ([InvoiceId]) REFERENCES [dbo].[Invoice] ([Id]) ON DELETE CASCADE ON UPDATE NO ACTION,
    CONSTRAINT [PK_InvoiceItem] PRIMARY KEY ([Id])
)
