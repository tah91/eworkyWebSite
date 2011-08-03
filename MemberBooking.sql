/*
   dimanche 31 juillet 201116:19:02
   User: 
   Server: .\SQLEXPRESS
   Database: 
   Application: 
*/

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Member SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.Member', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.Member', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.Member', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.Localisation SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.Localisation', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.Localisation', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.Localisation', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
CREATE TABLE dbo.MemberBooking
	(
	Id int NOT NULL IDENTITY (1, 1),
	MemberId int NOT NULL,
	LocalisationId int NOT NULL,
	Offer int NOT NULL,
	FromDate datetime NOT NULL,
	ToDate datetime NOT NULL,
	Message nvarchar(MAX) NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.MemberBooking ADD CONSTRAINT
	PK_MemberBooking PRIMARY KEY CLUSTERED 
	(
	Id,
	MemberId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.MemberBooking ADD CONSTRAINT
	FK_MemberBooking_Localisation FOREIGN KEY
	(
	LocalisationId
	) REFERENCES dbo.Localisation
	(
	ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.MemberBooking ADD CONSTRAINT
	FK_MemberBooking_Member FOREIGN KEY
	(
	MemberId
	) REFERENCES dbo.Member
	(
	MemberId
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.MemberBooking SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.MemberBooking', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.MemberBooking', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.MemberBooking', 'Object', 'CONTROL') as Contr_Per 