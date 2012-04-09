CREATE TABLE [dbo].[ApiNonce]
(
	[Context]	NVARCHAR(4000) NOT NULL,
	[Code]		NVARCHAR(4000) NOT NULL,
	[Timestamp] DATETIME NOT NULL, 
    CONSTRAINT [PK_ApiNonce] PRIMARY KEY ([Context], [Code], [Timestamp])
)
