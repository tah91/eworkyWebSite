CREATE TABLE [dbo].[Transaction] (
    [Id]              INT           IDENTITY (1, 1) NOT NULL,
    [MemberBookingId] INT           NOT NULL,
    [MemberId]        INT           NOT NULL,
    [LocalisationId]  INT           NOT NULL,
    [OfferId]         INT           NOT NULL,
    [Amount]          MONEY         NOT NULL,
    [PaymentType]     NVARCHAR (50) NOT NULL,
    [StatusId]        INT           NOT NULL,
    [TransactionId]   NVARCHAR (50) NOT NULL,
    [CreatedDate]     DATETIME      NOT NULL,
    [UpdatedDate]     DATETIME      NULL,
    CONSTRAINT [PK_Transaction] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Transaction_MemberBooking] FOREIGN KEY ([MemberBookingId], [MemberId], [LocalisationId], [OfferId]) REFERENCES [dbo].[MemberBooking] ([Id], [MemberId], [LocalisationId], [OfferId]) ON DELETE NO ACTION ON UPDATE NO ACTION
);

