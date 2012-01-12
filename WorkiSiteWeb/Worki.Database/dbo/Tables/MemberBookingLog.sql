CREATE TABLE [dbo].[MemberBookingLog] (
    [Id]              INT            NOT NULL IDENTITY,
    [MemberBookingId] INT            NOT NULL,
	[LoggerId]		  INT            NOT NULL DEFAULT 0,
    [Event]           NVARCHAR (100) NOT NULL,
    [CreatedDate] DATETIME NOT NULL, 
    [EventType] INT NOT NULL DEFAULT 0, 
    [Read] BIT NOT NULL DEFAULT 0, 
    CONSTRAINT [PK_MemberBookingLog] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MemberBookingLog_MemberBooking] FOREIGN KEY ([MemberBookingId]) REFERENCES [dbo].[MemberBooking] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION
);

