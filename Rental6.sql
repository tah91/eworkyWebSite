/*
   mardi 23 août 201118:51:51
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
ALTER TABLE dbo.Rental
	DROP CONSTRAINT FK_Rental_Member
GO
ALTER TABLE dbo.Member SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.Member', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.Member', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.Member', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.Rental
	DROP CONSTRAINT DF_Rental_Reference
GO
ALTER TABLE dbo.Rental
	DROP CONSTRAINT DF_Rental_Type
GO
ALTER TABLE dbo.Rental
	DROP CONSTRAINT DF_Rental_AvailableNow
GO
ALTER TABLE dbo.Rental
	DROP CONSTRAINT DF_Rental_LeaseType
GO
ALTER TABLE dbo.Rental
	DROP CONSTRAINT DF_Rental_Rate
GO
ALTER TABLE dbo.Rental
	DROP CONSTRAINT DF_Rental_Charges
GO
ALTER TABLE dbo.Rental
	DROP CONSTRAINT DF_Rental_Energy
GO
ALTER TABLE dbo.Rental
	DROP CONSTRAINT DF_Rental_GreenHouse
GO
ALTER TABLE dbo.Rental
	DROP CONSTRAINT DF_Rental_HeatingType
GO
CREATE TABLE dbo.Tmp_Rental
	(
	Id int NOT NULL IDENTITY (1, 1),
	MemberId int NOT NULL,
	Reference nvarchar(256) NULL,
	Type int NOT NULL,
	Address nvarchar(256) NOT NULL,
	PostalCode nvarchar(10) NOT NULL,
	City nvarchar(256) NOT NULL,
	Country nvarchar(256) NOT NULL,
	Latitude float(53) NOT NULL,
	Longitude float(53) NOT NULL,
	AvailableDate datetime NULL,
	AvailableNow bit NOT NULL,
	LeaseType int NOT NULL,
	Rate int NOT NULL,
	Charges int NOT NULL,
	Surface int NOT NULL,
	Description nvarchar(MAX) NOT NULL,
	Energy int NOT NULL,
	GreenHouse int NOT NULL,
	HeatingType int NOT NULL,
	TimeStamp datetime NOT NULL,
	CreationDate datetime NOT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_Rental SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_Rental ADD CONSTRAINT
	DF_Rental_Reference DEFAULT ('') FOR Reference
GO
ALTER TABLE dbo.Tmp_Rental ADD CONSTRAINT
	DF_Rental_Type DEFAULT ((0)) FOR Type
GO
ALTER TABLE dbo.Tmp_Rental ADD CONSTRAINT
	DF_Rental_AvailableNow DEFAULT ((0)) FOR AvailableNow
GO
ALTER TABLE dbo.Tmp_Rental ADD CONSTRAINT
	DF_Rental_LeaseType DEFAULT ((0)) FOR LeaseType
GO
ALTER TABLE dbo.Tmp_Rental ADD CONSTRAINT
	DF_Rental_Rate DEFAULT ((0)) FOR Rate
GO
ALTER TABLE dbo.Tmp_Rental ADD CONSTRAINT
	DF_Rental_Charges DEFAULT ((0)) FOR Charges
GO
ALTER TABLE dbo.Tmp_Rental ADD CONSTRAINT
	DF_Rental_Energy DEFAULT ((0)) FOR Energy
GO
ALTER TABLE dbo.Tmp_Rental ADD CONSTRAINT
	DF_Rental_GreenHouse DEFAULT ((0)) FOR GreenHouse
GO
ALTER TABLE dbo.Tmp_Rental ADD CONSTRAINT
	DF_Rental_HeatingType DEFAULT ((0)) FOR HeatingType
GO
SET IDENTITY_INSERT dbo.Tmp_Rental ON
GO
IF EXISTS(SELECT * FROM dbo.Rental)
	 EXEC('INSERT INTO dbo.Tmp_Rental (Id, MemberId, Reference, Type, Address, PostalCode, City, Country, Latitude, Longitude, AvailableDate, AvailableNow, LeaseType, Rate, Charges, Surface, Description, Energy, GreenHouse, HeatingType, TimeStamp)
		SELECT Id, MemberId, Reference, Type, Address, PostalCode, City, Country, Latitude, Longitude, AvailableDate, AvailableNow, LeaseType, Rate, Charges, Surface, Description, Energy, GreenHouse, HeatingType, TimeStamp FROM dbo.Rental WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_Rental OFF
GO
ALTER TABLE dbo.RentalAccess
	DROP CONSTRAINT FK_RentalAccess_Rental
GO
ALTER TABLE dbo.RentalFeature
	DROP CONSTRAINT FK_RentalFeature_Rental
GO
ALTER TABLE dbo.RentalFile
	DROP CONSTRAINT FK_RentalFile_Rental
GO
DROP TABLE dbo.Rental
GO
EXECUTE sp_rename N'dbo.Tmp_Rental', N'Rental', 'OBJECT' 
GO
ALTER TABLE dbo.Rental ADD CONSTRAINT
	PK_Rental PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.Rental ADD CONSTRAINT
	FK_Rental_Member FOREIGN KEY
	(
	MemberId
	) REFERENCES dbo.Member
	(
	MemberId
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
COMMIT
select Has_Perms_By_Name(N'dbo.Rental', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.Rental', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.Rental', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
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
select Has_Perms_By_Name(N'dbo.RentalFile', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.RentalFile', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.RentalFile', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.RentalFeature ADD CONSTRAINT
	FK_RentalFeature_Rental FOREIGN KEY
	(
	RentalId
	) REFERENCES dbo.Rental
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.RentalFeature SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.RentalFeature', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.RentalFeature', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.RentalFeature', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.RentalAccess ADD CONSTRAINT
	FK_RentalAccess_Rental FOREIGN KEY
	(
	RentalId
	) REFERENCES dbo.Rental
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.RentalAccess SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.RentalAccess', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.RentalAccess', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.RentalAccess', 'Object', 'CONTROL') as Contr_Per 