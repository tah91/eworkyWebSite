CREATE TABLE [dbo].[MemberBookingLog] (
    [Id]              INT            NOT NULL,
    [MemberBookingId] INT            NOT NULL,
    [MemberId]        INT            NOT NULL,
    [LocalisationId]  INT            NOT NULL,
    [OfferId]         INT            NOT NULL,
    [Event]           NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_MemberBookingLog] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MemberBookingLog_MemberBooking] FOREIGN KEY ([MemberBookingId], [MemberId], [LocalisationId], [OfferId]) REFERENCES [dbo].[MemberBooking] ([Id], [MemberId], [LocalisationId], [OfferId]) ON DELETE NO ACTION ON UPDATE NO ACTION
);

