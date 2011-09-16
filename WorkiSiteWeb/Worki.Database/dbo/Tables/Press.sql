CREATE TABLE [dbo].[Press]
(
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Date] [datetime] NOT NULL,
	[Title] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[Url] [nvarchar](256) NOT NULL
	CONSTRAINT [PK_Press] PRIMARY KEY CLUSTERED ([ID] ASC)
);
