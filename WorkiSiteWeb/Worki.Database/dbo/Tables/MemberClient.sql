CREATE TABLE [dbo].[MemberClient]
(
	[Id]		INT NOT NULL IDENTITY, 
    [MemberId]	INT NOT NULL, 
    [ClientId]	INT NOT NULL,
	CONSTRAINT [FK_MemberClient_Member] FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Member] ([MemberId]) ON DELETE CASCADE ON UPDATE NO ACTION,
	CONSTRAINT [FK_MemberClient_MemberClient] FOREIGN KEY ([ClientId]) REFERENCES [dbo].[Member] ([MemberId]),
    CONSTRAINT [PK_MemberClient] PRIMARY KEY ([MemberId],[ClientId])
)
