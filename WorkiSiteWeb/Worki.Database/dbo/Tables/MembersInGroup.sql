CREATE TABLE [dbo].[MembersInGroup] (
    [RelationId] INT IDENTITY (1, 1) NOT NULL,
    [MemberId]   INT NOT NULL,
    [GroupId]    INT NOT NULL,
    CONSTRAINT [PK_MembersInGroup] PRIMARY KEY CLUSTERED ([RelationId] ASC),
    CONSTRAINT [FK_MembersInGroup_Group] FOREIGN KEY ([GroupId]) REFERENCES [dbo].[Group] ([GroupId]) ON DELETE CASCADE ON UPDATE NO ACTION,
    CONSTRAINT [FK_MembersInGroup_Member] FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Member] ([MemberId]) ON DELETE CASCADE ON UPDATE NO ACTION
);

