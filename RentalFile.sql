/*
   mercredi 17 août 201119:01:05
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
ALTER TABLE dbo.Rental SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.Rental', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.Rental', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.Rental', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
CREATE TABLE dbo.RentalFile
	(
	Id int NOT NULL IDENTITY (1, 1),
	RentalId int NOT NULL,
	FileName nvarchar(256) NOT NULL,
	IsDefault bit NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.RentalFile ADD CONSTRAINT
	DF_RentalFile_IsDefault DEFAULT 0 FOR IsDefault
GO
ALTER TABLE dbo.RentalFile ADD CONSTRAINT
	PK_RentalFile PRIMARY KEY CLUSTERED 
	(
	Id,
	RentalId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.RentalFile ADD CONSTRAINT
	FK_RentalFile_Rental FOREIGN KEY
	(
	RentalId
	) REFERENCES dbo.Rental
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.RentalFile SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.RentalFile', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.RentalFile', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.RentalFile', 'Object', 'CONTROL') as Contr_Per 