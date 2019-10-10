IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UserLoginPersona' and TABLE_SCHEMA='Ident')
BEGIN
	IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TempPersona')
		DROP TABLE TempPersona
	IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TempUserLoginPersona') 
		DROP TABLE TempUserLoginPersona
	IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TempFieldValue')
		DROP TABLE TempFieldValue

	CREATE TABLE TempUserLoginPersona(
		[UserLoginPersonaId] [bigint] IDENTITY(1,1) NOT NULL,
		[UserLoginId] [bigint] NOT NULL,
		[StatusTypeId] [int] NOT NULL,
		[OrganizationPartyId] [bigint] NOT NULL,
		[PrimaryOrganization] [bit] NOT NULL,
		[FromDate] [datetime] NOT NULL,
		[ThruDate] [datetime] NULL,
		[StatusThruDate] [datetime] NULL)

	DECLARE @SQL VARCHAR(MAX)
	SET @sql = 
	'INSERT INTO  TempUserLoginPersona
		([UserLoginId]
		,[StatusTypeId]
		,[OrganizationPartyId]
		,[PrimaryOrganization]
		,[FromDate]
		,[ThruDate]
		,[StatusThruDate])
     SELECT
		 UL.[UserId]
		,UL.[StatusId]
		,P.[OrganizationPartyId]
		,''True''
		,UL.[FromDate]
		,UL.[ThruDate]
		,UL.[StatusThruDate]
	FROM
		Ident.UserLogin UL
	INNER JOIN Person.Persona P ON P.UserId = UL.UserId
	'

	EXEC (@SQL)
	CREATE TABLE TempPersona(
		[PersonaId] [bigint] NOT NULL,
		[UserLoginPersonaId] [bigint] NOT NULL,
		[PersonPartyId] [bigint] NOT NULL  )

	SET @SQL = 
	'INSERT INTO TempPersona
           ([PersonaId]
			,[UserLoginPersonaId]
			,[PersonPartyId])
	SELECT
		 [PersonaId]
		,ULP.UserLoginPersonaId
		,P.[PersonPartyId]
	FROM
		Person.Persona P
	INNER JOIN
		Ident.UserLogin UL ON P.UserID = UL.UserId
	INNER JOIN TempUserLoginPersona ULP ON UL.UserId = ULP.UserLoginId
	'
	EXEC (@SQL)

	CREATE TABLE TempFieldValue(
		[FieldValueId] [bigint] NOT NULL,
		[UserLoginId] [bigint] NOT NULL)

		SET @SQL=
	'INSERT INTO TempFieldValue(
		 FieldValueId
		,UserLoginId)
	SELECT 
		 FieldValueId
		,UserLoginId
	FROM
		CustomField.FieldValue
	'
	EXEC (@SQL)


END