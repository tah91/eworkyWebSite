CREATE TABLE [dbo].[ApiClientAuthorization]
(
	[Id]				INT IDENTITY(1,1)	NOT NULL,
	[CreatedOnUtc]		DATETIME			NOT NULL,
	[ApiClientId]		INT					NOT NULL,
	[MemberId]			INT					NULL,
	[Scope]				NVARCHAR(max)		NULL,
	[ExpirationDateUtc] DATETIME			NULL,
	CONSTRAINT [FK_ApiClientAuthorization_ApiClient] FOREIGN KEY ([ApiClientId]) REFERENCES [dbo].[ApiClient] ([Id]) ON DELETE CASCADE ON UPDATE NO ACTION,
    CONSTRAINT [FK_ApiClientAuthorization_Member] FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Member] ([MemberId]) ON DELETE CASCADE ON UPDATE NO ACTION, 
    CONSTRAINT [PK_ApiClientAuthorization] PRIMARY KEY ([Id])
)
