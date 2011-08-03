/*
   dimanche 31 juillet 201116:03:04
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
ALTER TABLE dbo.Localisation
	DROP CONSTRAINT FK_Localisation_Member
GO
ALTER TABLE dbo.Member SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.Member', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.Member', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.Member', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.Localisation
	DROP CONSTRAINT DF_Localisation_TypeValue
GO
ALTER TABLE dbo.Localisation
	DROP CONSTRAINT DF_Localisation_CodePostal
GO
ALTER TABLE dbo.Localisation
	DROP CONSTRAINT DF_Localisation_Ville
GO
ALTER TABLE dbo.Localisation
	DROP CONSTRAINT DF_Localisation_Country
GO
ALTER TABLE dbo.Localisation
	DROP CONSTRAINT DF_Localisation_PhoneNumber
GO
ALTER TABLE dbo.Localisation
	DROP CONSTRAINT DF_Localisation_Mail
GO
ALTER TABLE dbo.Localisation
	DROP CONSTRAINT DF_Localisation_Fax
GO
ALTER TABLE dbo.Localisation
	DROP CONSTRAINT DF_Localisation_WebSite
GO
ALTER TABLE dbo.Localisation
	DROP CONSTRAINT DF_Localisation_OwnerID
GO
CREATE TABLE dbo.Tmp_Localisation
	(
	ID int NOT NULL IDENTITY (1, 1),
	Name nvarchar(256) NOT NULL,
	TypeValue int NOT NULL,
	Adress nvarchar(256) NOT NULL,
	PostalCode nvarchar(10) NOT NULL,
	City nvarchar(256) NOT NULL,
	Country nvarchar(256) NOT NULL,
	PhoneNumber nvarchar(50) NULL,
	Mail nvarchar(256) NULL,
	Fax nvarchar(50) NULL,
	WebSite nvarchar(256) NULL,
	Description nvarchar(MAX) NULL,
	Latitude float(53) NOT NULL,
	Longitude float(53) NOT NULL,
	OwnerID int NULL,
	Bookable bit NOT NULL,
	MonOpen smalldatetime NULL,
	MonClose smalldatetime NULL,
	MonOpen2 smalldatetime NULL,
	MonClose2 smalldatetime NULL,
	TueOpen smalldatetime NULL,
	TueClose smalldatetime NULL,
	TueOpen2 smalldatetime NULL,
	TueClose2 smalldatetime NULL,
	WedOpen smalldatetime NULL,
	WedClose smalldatetime NULL,
	WedOpen2 smalldatetime NULL,
	WedClose2 smalldatetime NULL,
	ThuOpen smalldatetime NULL,
	ThuClose smalldatetime NULL,
	ThuOpen2 smalldatetime NULL,
	ThuClose2 smalldatetime NULL,
	FriOpen smalldatetime NULL,
	FriClose smalldatetime NULL,
	FriOpen2 smalldatetime NULL,
	FriClose2 smalldatetime NULL,
	SatOpen smalldatetime NULL,
	SatClose smalldatetime NULL,
	SatOpen2 smalldatetime NULL,
	SatClose2 smalldatetime NULL,
	SunOpen smalldatetime NULL,
	SunClose smalldatetime NULL,
	SunOpen2 smalldatetime NULL,
	SunClose2 smalldatetime NULL,
	PublicTransportation nvarchar(256) NULL,
	Station nvarchar(256) NULL,
	RoadAccess nvarchar(256) NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_Localisation SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_Localisation ADD CONSTRAINT
	DF_Localisation_TypeValue DEFAULT ((0)) FOR TypeValue
GO
ALTER TABLE dbo.Tmp_Localisation ADD CONSTRAINT
	DF_Localisation_CodePostal DEFAULT ('') FOR PostalCode
GO
ALTER TABLE dbo.Tmp_Localisation ADD CONSTRAINT
	DF_Localisation_Ville DEFAULT ('') FOR City
GO
ALTER TABLE dbo.Tmp_Localisation ADD CONSTRAINT
	DF_Localisation_Country DEFAULT ('') FOR Country
GO
ALTER TABLE dbo.Tmp_Localisation ADD CONSTRAINT
	DF_Localisation_PhoneNumber DEFAULT ('') FOR PhoneNumber
GO
ALTER TABLE dbo.Tmp_Localisation ADD CONSTRAINT
	DF_Localisation_Mail DEFAULT ('') FOR Mail
GO
ALTER TABLE dbo.Tmp_Localisation ADD CONSTRAINT
	DF_Localisation_Fax DEFAULT ('') FOR Fax
GO
ALTER TABLE dbo.Tmp_Localisation ADD CONSTRAINT
	DF_Localisation_WebSite DEFAULT ('') FOR WebSite
GO
ALTER TABLE dbo.Tmp_Localisation ADD CONSTRAINT
	DF_Localisation_OwnerID DEFAULT ((0)) FOR OwnerID
GO
ALTER TABLE dbo.Tmp_Localisation ADD CONSTRAINT
	DF_Localisation_Bookable DEFAULT 0 FOR Bookable
GO
SET IDENTITY_INSERT dbo.Tmp_Localisation ON
GO
IF EXISTS(SELECT * FROM dbo.Localisation)
	 EXEC('INSERT INTO dbo.Tmp_Localisation (ID, Name, TypeValue, Adress, PostalCode, City, Country, PhoneNumber, Mail, Fax, WebSite, Description, Latitude, Longitude, OwnerID, MonOpen, MonClose, MonOpen2, MonClose2, TueOpen, TueClose, TueOpen2, TueClose2, WedOpen, WedClose, WedOpen2, WedClose2, ThuOpen, ThuClose, ThuOpen2, ThuClose2, FriOpen, FriClose, FriOpen2, FriClose2, SatOpen, SatClose, SatOpen2, SatClose2, SunOpen, SunClose, SunOpen2, SunClose2, PublicTransportation, Station, RoadAccess)
		SELECT ID, Name, TypeValue, Adress, PostalCode, City, Country, PhoneNumber, Mail, Fax, WebSite, Description, Latitude, Longitude, OwnerID, MonOpen, MonClose, MonOpen2, MonClose2, TueOpen, TueClose, TueOpen2, TueClose2, WedOpen, WedClose, WedOpen2, WedClose2, ThuOpen, ThuClose, ThuOpen2, ThuClose2, FriOpen, FriClose, FriOpen2, FriClose2, SatOpen, SatClose, SatOpen2, SatClose2, SunOpen, SunClose, SunOpen2, SunClose2, PublicTransportation, Station, RoadAccess FROM dbo.Localisation WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_Localisation OFF
GO
ALTER TABLE dbo.WelcomePeople
	DROP CONSTRAINT FK_WelcomePeople_Localisation
GO
ALTER TABLE dbo.MainLocalisation
	DROP CONSTRAINT FK_MainLocalisation_Localisation
GO
ALTER TABLE dbo.LocalisationFeature
	DROP CONSTRAINT FK_LocalisationFeature_Localisation
GO
ALTER TABLE dbo.LocalisationData
	DROP CONSTRAINT FK_LocalisationData_Localisation
GO
ALTER TABLE dbo.Comment
	DROP CONSTRAINT FK_Comment_Localisation
GO
ALTER TABLE dbo.LocalisationFile
	DROP CONSTRAINT FK_LocalisationFile_Localisation
GO
ALTER TABLE dbo.FavoriteLocalisation
	DROP CONSTRAINT FK_FavoriteLocalisation_Localisation
GO
ALTER TABLE dbo.MemberEdition
	DROP CONSTRAINT FK_MemberEdition_Localisation
GO
DROP TABLE dbo.Localisation
GO
EXECUTE sp_rename N'dbo.Tmp_Localisation', N'Localisation', 'OBJECT' 
GO
ALTER TABLE dbo.Localisation ADD CONSTRAINT
	PK_Localisation PRIMARY KEY CLUSTERED 
	(
	ID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.Localisation ADD CONSTRAINT
	FK_Localisation_Member FOREIGN KEY
	(
	OwnerID
	) REFERENCES dbo.Member
	(
	MemberId
	) ON UPDATE  NO ACTION 
	 ON DELETE  SET NULL 
	
GO
COMMIT
select Has_Perms_By_Name(N'dbo.Localisation', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.Localisation', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.Localisation', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.MemberEdition ADD CONSTRAINT
	FK_MemberEdition_Localisation FOREIGN KEY
	(
	LocalisationId
	) REFERENCES dbo.Localisation
	(
	ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.MemberEdition SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.MemberEdition', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.MemberEdition', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.MemberEdition', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.FavoriteLocalisation ADD CONSTRAINT
	FK_FavoriteLocalisation_Localisation FOREIGN KEY
	(
	LocalisationId
	) REFERENCES dbo.Localisation
	(
	ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.FavoriteLocalisation SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.FavoriteLocalisation', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.FavoriteLocalisation', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.FavoriteLocalisation', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.LocalisationFile ADD CONSTRAINT
	FK_LocalisationFile_Localisation FOREIGN KEY
	(
	LocalisationID
	) REFERENCES dbo.Localisation
	(
	ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.LocalisationFile SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.LocalisationFile', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.LocalisationFile', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.LocalisationFile', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.Comment ADD CONSTRAINT
	FK_Comment_Localisation FOREIGN KEY
	(
	LocalisationID
	) REFERENCES dbo.Localisation
	(
	ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.Comment SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.Comment', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.Comment', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.Comment', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.LocalisationData ADD CONSTRAINT
	FK_LocalisationData_Localisation FOREIGN KEY
	(
	LocalisationID
	) REFERENCES dbo.Localisation
	(
	ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.LocalisationData SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.LocalisationData', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.LocalisationData', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.LocalisationData', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.LocalisationFeature ADD CONSTRAINT
	FK_LocalisationFeature_Localisation FOREIGN KEY
	(
	LocalisationID
	) REFERENCES dbo.Localisation
	(
	ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.LocalisationFeature SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.LocalisationFeature', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.LocalisationFeature', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.LocalisationFeature', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.MainLocalisation ADD CONSTRAINT
	FK_MainLocalisation_Localisation FOREIGN KEY
	(
	LocalisationID
	) REFERENCES dbo.Localisation
	(
	ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.MainLocalisation SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.MainLocalisation', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.MainLocalisation', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.MainLocalisation', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.WelcomePeople ADD CONSTRAINT
	FK_WelcomePeople_Localisation FOREIGN KEY
	(
	LocalisationId
	) REFERENCES dbo.Localisation
	(
	ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.WelcomePeople SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.WelcomePeople', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.WelcomePeople', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.WelcomePeople', 'Object', 'CONTROL') as Contr_Per 