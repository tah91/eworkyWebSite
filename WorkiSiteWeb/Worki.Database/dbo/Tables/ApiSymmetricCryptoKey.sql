CREATE TABLE [dbo].[ApiSymmetricCryptoKey]
(
	[Bucket]		NVARCHAR(4000) NOT NULL,
	[Handle]		NVARCHAR(4000) NOT NULL,
	[ExpiresUtc]	DATETIME NOT NULL,
	[Secret]		VARBINARY(8000) NOT NULL, 
    CONSTRAINT [PK_ApiSymmetricCryptoKey] PRIMARY KEY ([Bucket], [Handle]),
)
