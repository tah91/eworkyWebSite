CREATE TABLE [dbo].[Member] (
    [MemberId]                               INT            IDENTITY (1, 1) NOT NULL,
    [Username]                               NVARCHAR (256) NOT NULL,
    [Password]                               NVARCHAR (128) NOT NULL,
    [PasswordSalt]                           NVARCHAR (128) NOT NULL,
    [Email]                                  NVARCHAR (256) NULL,
    [EmailKey]                               NVARCHAR (128) NULL,
    [PasswordQuestion]                       NVARCHAR (256) NULL,
    [PasswordAnswer]                         NVARCHAR (256) NULL,
    [IsApproved]                             BIT            NOT NULL,
    [IsLockedOut]                            BIT            NOT NULL,
    [CreatedDate]                            DATETIME       NOT NULL,
    [LastActivityDate]                       DATETIME       NOT NULL,
    [LastLoginDate]                          DATETIME       NOT NULL,
    [LastPasswordChangedDate]                DATETIME       NOT NULL,
    [LastLockoutDate]                        DATETIME       NOT NULL,
    [FailedPasswordAttemptCount]             INT            NOT NULL,
    [FailedPasswordAttemptWindowStart]       DATETIME       NOT NULL,
    [FailedPasswordAnswerAttemptCount]       INT            NOT NULL,
    [FailedPasswordAnswerAttemptWindowStart] DATETIME       NOT NULL,
    [Comment]                                NTEXT          NULL,
    CONSTRAINT [PK_Member] PRIMARY KEY CLUSTERED ([MemberId] ASC)
);

