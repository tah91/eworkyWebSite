CREATE TABLE [dbo].[MemberBookingLog] (
    [Id]              INT            NOT NULL IDENTITY,
    [MemberBookingId] INT            NOT NULL,
    [Event]           NVARCHAR (100) NOT NULL,
    [CreatedDate] DATETIME NOT NULL, 
    CONSTRAINT [PK_MemberBookingLog] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MemberBookingLog_MemberBooking] FOREIGN KEY ([MemberBookingId]) REFERENCES [dbo].[MemberBooking] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION
);

