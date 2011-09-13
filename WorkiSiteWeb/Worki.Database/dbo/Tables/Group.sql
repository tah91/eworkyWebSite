CREATE TABLE [dbo].[Group] (
    [GroupId]     INT            IDENTITY (1, 1) NOT NULL,
    [Title]       NVARCHAR (256) NOT NULL,
    [Description] NVARCHAR (256) NULL,
    CONSTRAINT [PK_Group] PRIMARY KEY CLUSTERED ([GroupId] ASC)
);

