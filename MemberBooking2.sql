/*
   mercredi 3 août 201110:51:46
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
ALTER TABLE dbo.MemberBooking ADD
	Handled bit NOT NULL CONSTRAINT DF_MemberBooking_Handled DEFAULT 0,
	Confirmed bit NOT NULL CONSTRAINT DF_MemberBooking_Confirmed DEFAULT 0,
	Price int NOT NULL CONSTRAINT DF_MemberBooking_Price DEFAULT 0
GO
ALTER TABLE dbo.MemberBooking SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.MemberBooking', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.MemberBooking', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.MemberBooking', 'Object', 'CONTROL') as Contr_Per 