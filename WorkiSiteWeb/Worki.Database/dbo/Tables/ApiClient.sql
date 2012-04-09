CREATE TABLE [dbo].[ApiClient]
(
	[Id]				INT IDENTITY(1,1)	NOT NULL,
	[ClientIdentifier]	NVARCHAR(50)		NOT NULL,
	[ClientSecret]		NVARCHAR(50)		NULL,
	[Callback]			NVARCHAR(4000)		NULL,
	[Name]				NVARCHAR(4000)		NOT NULL,
	[ClientType]		INT					NOT NULL,
	CONSTRAINT [PK_ApiClient] PRIMARY KEY ([Id])
)
