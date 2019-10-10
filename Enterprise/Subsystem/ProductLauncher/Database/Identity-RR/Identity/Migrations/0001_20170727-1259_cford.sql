-- <Migration ID="cd21093d-2afe-475a-af35-c782350891e7" />
GO

IF NOT EXISTS (SELECT * FROM master.dbo.syslogins WHERE loginname = N'identityserver')
CREATE LOGIN [identityserver] WITH PASSWORD = 'p@ssw0rd'
GO
CREATE USER [IdentityServer] FOR LOGIN [identityserver] WITH DEFAULT_SCHEMA=[Auth]
GO
PRINT N'Altering members of role db_datareader'
GO
EXEC sp_addrolemember N'db_datareader', N'IdentityServer'
GO
PRINT N'Altering members of role db_datawriter'
GO
EXEC sp_addrolemember N'db_datawriter', N'IdentityServer'
GO
PRINT N'Creating schemas'
GO
CREATE SCHEMA [Auth]
AUTHORIZATION [dbo]
GO
CREATE SCHEMA [Config]
AUTHORIZATION [dbo]
GO
CREATE SCHEMA [Enterprise]
AUTHORIZATION [dbo]
GO
CREATE SCHEMA [Ident]
AUTHORIZATION [dbo]
GO
CREATE SCHEMA [Person]
AUTHORIZATION [dbo]
GO
PRINT N'Creating types'
GO
CREATE TYPE [dbo].[Name] FROM nvarchar (50) NULL
GO
CREATE TYPE [dbo].[Phone] FROM nvarchar (25) NULL
GO
PRINT N'Creating [Enterprise].[ProductSettingType]'
GO
CREATE TABLE [Enterprise].[ProductSettingType]
(
[ProductSettingTypeId] [int] NOT NULL IDENTITY(1, 1),
[Name] [nvarchar] (50) NOT NULL,
[Description] [nvarchar] (100) NULL
)
GO
PRINT N'Creating primary key [PK_ProductSettingType] on [Enterprise].[ProductSettingType]'
GO
ALTER TABLE [Enterprise].[ProductSettingType] ADD CONSTRAINT [PK_ProductSettingType] PRIMARY KEY CLUSTERED  ([ProductSettingTypeId])
GO
PRINT N'Creating [dbo].[PrintError]'
GO

-- PrintError prints error information about the error that caused 
-- execution to jump to the CATCH block of a TRY...CATCH construct. 
-- Should be executed from within the scope of a CATCH block otherwise 
-- it will return without printing any error information.
CREATE PROCEDURE [dbo].[PrintError] 
AS
BEGIN
    SET NOCOUNT ON;

    -- Print error information. 
    PRINT 'Error ' + CONVERT(varchar(50), ERROR_NUMBER()) +
          ', Severity ' + CONVERT(varchar(5), ERROR_SEVERITY()) +
          ', State ' + CONVERT(varchar(5), ERROR_STATE()) + 
          ', Procedure ' + ISNULL(ERROR_PROCEDURE(), '-') + 
          ', Line ' + CONVERT(varchar(5), ERROR_LINE());
    PRINT ERROR_MESSAGE();
END;
GO
PRINT N'Creating [dbo].[ErrorLog]'
GO
CREATE TABLE [dbo].[ErrorLog]
(
[ErrorLogID] [int] NOT NULL IDENTITY(1, 1),
[ErrorTime] [datetime] NOT NULL CONSTRAINT [DF_ErrorLog_ErrorTime] DEFAULT (getdate()),
[UserName] [sys].[sysname] NOT NULL,
[ErrorNumber] [int] NOT NULL,
[ErrorSeverity] [int] NULL,
[ErrorState] [int] NULL,
[ErrorProcedure] [nvarchar] (126) NULL,
[ErrorLine] [int] NULL,
[ErrorMessage] [nvarchar] (4000) NOT NULL
)
GO
PRINT N'Creating primary key [PK_ErrorLog_ErrorLogID] on [dbo].[ErrorLog]'
GO
ALTER TABLE [dbo].[ErrorLog] ADD CONSTRAINT [PK_ErrorLog_ErrorLogID] PRIMARY KEY CLUSTERED  ([ErrorLogID])
GO
PRINT N'Creating [dbo].[LogError]'
GO

-- LogError logs error information in the ErrorLog table about the 
-- error that caused execution to jump to the CATCH block of a 
-- TRY...CATCH construct. This should be executed from within the scope 
-- of a CATCH block otherwise it will return without inserting error 
-- information. 
CREATE PROCEDURE [dbo].[LogError] 
    @ErrorLogID [int] = 0 OUTPUT -- contains the ErrorLogID of the row inserted
AS                               -- by LogError in the ErrorLog table
BEGIN
    SET NOCOUNT ON;

    -- Output parameter value of 0 indicates that error 
    -- information was not logged
    SET @ErrorLogID = 0;

    BEGIN TRY
        -- Return if there is no error information to log
        IF ERROR_NUMBER() IS NULL
            RETURN;

        -- Return if inside an uncommittable transaction.
        -- Data insertion/modification is not allowed when 
        -- a transaction is in an uncommittable state.
        IF XACT_STATE() = -1
        BEGIN
            PRINT 'Cannot log error since the current transaction is in an uncommittable state. ' 
                + 'Rollback the transaction before executing LogError in order to successfully log error information.';
            RETURN;
        END

        INSERT [dbo].[ErrorLog] 
            (
            [UserName], 
            [ErrorNumber], 
            [ErrorSeverity], 
            [ErrorState], 
            [ErrorProcedure], 
            [ErrorLine], 
            [ErrorMessage]
            ) 
        VALUES 
            (
            CONVERT(sysname, CURRENT_USER), 
            ERROR_NUMBER(),
            ERROR_SEVERITY(),
            ERROR_STATE(),
            ERROR_PROCEDURE(),
            ERROR_LINE(),
            ERROR_MESSAGE()
            );

        -- Pass back the ErrorLogID of the row inserted
        SET @ErrorLogID = @@IDENTITY;
    END TRY
    BEGIN CATCH
        PRINT 'An error occurred in stored procedure LogError: ';
        EXECUTE [dbo].[PrintError];
        RETURN -1;
    END CATCH
END;
GO
PRINT N'Creating [Enterprise].[CreateProductSettingType]'
GO
CREATE PROCEDURE [Enterprise].[CreateProductSettingType] (
    @ProductSettingTypeName VARCHAR(50),
    @ProductSettingTypeDescription VARCHAR(100),
	@ProductSettingTypeId INT OUTPUT
)
AS
BEGIN
	BEGIN TRY
        BEGIN TRANSACTION;
		INSERT INTO Enterprise.ProductSettingType (
			Name,
			Description
		)
		OUTPUT	Inserted.ProductSettingTypeId AS Id,
				'' AS ErrorMessage
		VALUES (
			@ProductSettingTypeName,
			@ProductSettingTypeDescription
		);

		SET @ProductSettingTypeId = SCOPE_IDENTITY();
		COMMIT;
	END TRY  
	BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
	END CATCH
END;
GO
PRINT N'Creating [Enterprise].[GeographicBoundaryType]'
GO
CREATE TABLE [Enterprise].[GeographicBoundaryType]
(
[GeographicBoundaryTypeId] [int] NOT NULL IDENTITY(1, 1),
[Name] [nvarchar] (50) NOT NULL
)
GO
PRINT N'Creating primary key [PK_GeographicBoundaryType] on [Enterprise].[GeographicBoundaryType]'
GO
ALTER TABLE [Enterprise].[GeographicBoundaryType] ADD CONSTRAINT [PK_GeographicBoundaryType] PRIMARY KEY CLUSTERED  ([GeographicBoundaryTypeId])
GO
PRINT N'Adding constraints to [Enterprise].[GeographicBoundaryType]'
GO
ALTER TABLE [Enterprise].[GeographicBoundaryType] ADD CONSTRAINT [AK_GeographicBoundaryType_Name] UNIQUE NONCLUSTERED  ([Name])
GO
PRINT N'Creating [Enterprise].[GeographicBoundary]'
GO
CREATE TABLE [Enterprise].[GeographicBoundary]
(
[GeographicBoundaryId] [int] NOT NULL IDENTITY(1, 1),
[GeographicBoundaryTypeId] [int] NOT NULL,
[Name] [nvarchar] (50) NOT NULL,
[GeographicBoundaryCode] [nvarchar] (50) NULL,
[Abbreviation] [nvarchar] (10) NULL
)
GO
PRINT N'Creating primary key [PK_GeographicBoundary] on [Enterprise].[GeographicBoundary]'
GO
ALTER TABLE [Enterprise].[GeographicBoundary] ADD CONSTRAINT [PK_GeographicBoundary] PRIMARY KEY CLUSTERED  ([GeographicBoundaryId])
GO
PRINT N'Adding constraints to [Enterprise].[GeographicBoundary]'
GO
ALTER TABLE [Enterprise].[GeographicBoundary] ADD CONSTRAINT [AK_GeographicBoundary_Name_GeographicBoundaryTypeId_GeographicBoundaryCode] UNIQUE NONCLUSTERED  ([Name], [GeographicBoundaryTypeId], [Abbreviation])
GO
PRINT N'Creating [Person].[CreateGeographicBoundary]'
GO
CREATE PROCEDURE [Person].[CreateGeographicBoundary] (
    @TypeName NVARCHAR(50),
    @Value NVARCHAR(50),
    @Code NVARCHAR(50),
    @Abbreviation NVARCHAR(10)
)
AS
BEGIN
	BEGIN TRY
	    BEGIN TRANSACTION; 

		DECLARE @GeographicBoundaryId INT,
			@GeographicBoundaryTypeId INT;
	
		SELECT	@GeographicBoundaryTypeId = GeographicBoundaryTypeId
		FROM	Enterprise.GeographicBoundaryType
		WHERE	[Name] = @TypeName;
	
		-- Check if the boundary already exists.
		-- Return the Boundary Id if it already exists.

		SELECT	@GeographicBoundaryId = GeographicBoundaryId
		FROM	Enterprise.GeographicBoundary
		WHERE	GeographicBoundaryTypeId = @GeographicBoundaryTypeId
		AND		[Name] = @Value;

		IF @GeographicBoundaryId IS NOT NULL
		BEGIN
			SELECT	@GeographicBoundaryId AS Id,
					'' AS ErrorMessage;
		END;
		ELSE
		BEGIN
			INSERT  INTO Enterprise.GeographicBoundary
			(
				GeographicBoundaryTypeId,
				Name,
				GeographicBoundaryCode,
				Abbreviation
			)
			OUTPUT	Inserted.GeographicBoundaryId AS Id,
					'' AS ErrorMessage
			VALUES
			(
				@GeographicBoundaryTypeId,
				@Value,
				@Code,
				@Abbreviation
			);
		END;

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				'' AS RealPageId,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
    END CATCH;
END;
GO
PRINT N'Creating [Enterprise].[Party]'
GO
CREATE TABLE [Enterprise].[Party]
(
[PartyId] [bigint] NOT NULL IDENTITY(1, 1),
[RealPageId] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_Party_rowguid] DEFAULT (newid()),
[CreateDate] [datetime] NOT NULL CONSTRAINT [DF_Party_CreateDate] DEFAULT (getutcdate())
)
GO
PRINT N'Creating primary key [PK_Party] on [Enterprise].[Party]'
GO
ALTER TABLE [Enterprise].[Party] ADD CONSTRAINT [PK_Party] PRIMARY KEY CLUSTERED  ([PartyId])
GO
PRINT N'Creating index [AK_Party_rowguid] on [Enterprise].[Party]'
GO
CREATE UNIQUE NONCLUSTERED INDEX [AK_Party_rowguid] ON [Enterprise].[Party] ([RealPageId])
GO
PRINT N'Creating [Enterprise].[DataImportMapping]'
GO
CREATE TABLE [Enterprise].[DataImportMapping]
(
[DataImportMappingId] [int] NOT NULL IDENTITY(1, 1),
[DataImportApplicationId] [int] NOT NULL,
[SourceId] [nvarchar] (100) NOT NULL,
[PartyId] [bigint] NOT NULL,
[DateCreated] [datetime] NOT NULL CONSTRAINT [df_DataImportMapping_DateCreated] DEFAULT (getutcdate())
)
GO
PRINT N'Creating primary key [PK_DataImportMapping] on [Enterprise].[DataImportMapping]'
GO
ALTER TABLE [Enterprise].[DataImportMapping] ADD CONSTRAINT [PK_DataImportMapping] PRIMARY KEY CLUSTERED  ([DataImportMappingId])
GO
PRINT N'Creating [Enterprise].[GetBlueBookIdByOrganization]'
GO
CREATE PROC [Enterprise].[GetBlueBookIdByOrganization]
@RealPageId UNIQUEIDENTIFIER
AS
BEGIN

    SELECT SourceId AS BlueBookId
	FROM Enterprise.DataImportMapping
	JOIN Enterprise.Party ON Party.PartyId = DataImportMapping.PartyId
	WHERE RealPageId = @RealPageId

END
GO
PRINT N'Creating [Enterprise].[StreetAddress]'
GO
CREATE TABLE [Enterprise].[StreetAddress]
(
[ContactMechanismID] [int] NOT NULL,
[StreetAddress1] [nvarchar] (50) NOT NULL,
[StreetAddress2] [nvarchar] (50) NULL,
[StreetAddress3] [nvarchar] (50) NULL
)
GO
PRINT N'Creating primary key [PK_StreetAddress] on [Enterprise].[StreetAddress]'
GO
ALTER TABLE [Enterprise].[StreetAddress] ADD CONSTRAINT [PK_StreetAddress] PRIMARY KEY CLUSTERED  ([ContactMechanismID])
GO
PRINT N'Creating [Person].[CreateStreetAddress]'
GO
-- Add an Street Address
CREATE PROCEDURE [Person].[CreateStreetAddress] (
    @ContactMechanismId INT,
    @StreetAddress1 NVARCHAR(50),
    @StreetAddress2 NVARCHAR(50) = NULL,
    @StreetAddress3 NVARCHAR(50) = NULL
)
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;

		INSERT  INTO Enterprise.StreetAddress
		(
			ContactMechanismID,
			StreetAddress1,
			StreetAddress2,
			StreetAddress3
		)
		VALUES
		(
			@ContactMechanismId,
			@StreetAddress1,
			@StreetAddress2,
			@StreetAddress3
		);

		SELECT	@ContactMechanismId AS Id,
                '' AS ErrorMessage

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
    END CATCH;
END
GO
PRINT N'Creating [Enterprise].[Organization]'
GO
CREATE TABLE [Enterprise].[Organization]
(
[PartyId] [bigint] NOT NULL,
[Name] [nvarchar] (50) NULL
)
GO
PRINT N'Creating primary key [PK_Organization] on [Enterprise].[Organization]'
GO
ALTER TABLE [Enterprise].[Organization] ADD CONSTRAINT [PK_Organization] PRIMARY KEY CLUSTERED  ([PartyId])
GO
PRINT N'Creating [Enterprise].[GetOrganization]'
GO
CREATE PROCEDURE [Enterprise].[GetOrganization]
	@RealPageId UNIQUEIDENTIFIER
AS
	BEGIN
		SELECT 
			o.PartyId ,
			o.Name ,
			RealPageId ,
			CreateDate
		FROM [Enterprise].Organization AS o  
			JOIN [Enterprise].Party ON Party.PartyId = o.PartyId  
		WHERE RealPageId = @RealPageId  
	END;
GO
PRINT N'Creating [Enterprise].[GetOrganizationByBlueBookId]'
GO
CREATE PROC [Enterprise].[GetOrganizationByBlueBookId]
@BlueBookId INT
AS
BEGIN
	DECLARE @RealPageId UNIQUEIDENTIFIER;

    SELECT @RealPageId = RealPageId
	FROM Enterprise.DataImportMapping
	JOIN Enterprise.Party ON Party.PartyId = DataImportMapping.PartyId
	WHERE SourceId = @BlueBookId

	EXEC Enterprise.GetOrganization @RealPageId

END
GO
PRINT N'Creating [Enterprise].[ContactMechanismUsage]'
GO
CREATE TABLE [Enterprise].[ContactMechanismUsage]
(
[ContactMechanismUsageID] [int] NOT NULL IDENTITY(1, 1),
[PartyContactMechanismID] [bigint] NOT NULL,
[ContactMechanismUsageTypeID] [int] NOT NULL
)
GO
PRINT N'Creating primary key [PK_ContactMechanismUsage] on [Enterprise].[ContactMechanismUsage]'
GO
ALTER TABLE [Enterprise].[ContactMechanismUsage] ADD CONSTRAINT [PK_ContactMechanismUsage] PRIMARY KEY CLUSTERED  ([ContactMechanismUsageID])
GO
PRINT N'Adding constraints to [Enterprise].[ContactMechanismUsage]'
GO
ALTER TABLE [Enterprise].[ContactMechanismUsage] ADD CONSTRAINT [AK_ContactMechanismUsage_PartyContactMechanismId_UsageId] UNIQUE NONCLUSTERED  ([PartyContactMechanismID], [ContactMechanismUsageTypeID])
GO
PRINT N'Creating [Person].[LinkUsageTypeToPartyContactMechanism]'
GO
CREATE PROCEDURE [Person].[LinkUsageTypeToPartyContactMechanism] (
    @PartyContactMechanismId INT ,
    @ContactMechanismUsageTypeId INT = 1
)
AS
BEGIN
	BEGIN TRY
        BEGIN TRANSACTION; 

		INSERT  INTO Enterprise.ContactMechanismUsage (
			PartyContactMechanismID ,
			ContactMechanismUsageTypeID
		)
		OUTPUT	Inserted.ContactMechanismUsageID AS Id,
				'' AS ErrorMessage
		VALUES (
			@PartyContactMechanismId,
			@ContactMechanismUsageTypeId
		);

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
    END CATCH;
END;
GO
PRINT N'Creating [Enterprise].[GetProductSettingType]'
GO
CREATE PROCEDURE [Enterprise].[GetProductSettingType]
    @Name VARCHAR(50) ,
    @ProductSettingTypeId INT OUTPUT
AS
    BEGIN
        SELECT @ProductSettingTypeId = ProductSettingTypeId
        FROM   Enterprise.ProductSettingType
        WHERE  [Name] = @Name;
    END;
GO
PRINT N'Creating [Enterprise].[PartyContactMechanism]'
GO
CREATE TABLE [Enterprise].[PartyContactMechanism]
(
[PartyContactMechanismId] [bigint] NOT NULL IDENTITY(1, 1),
[PartyId] [bigint] NOT NULL,
[ContactMechanismId] [int] NOT NULL,
[FromDate] [datetime] NOT NULL,
[ThruDate] [datetime] NULL
)
GO
PRINT N'Creating index [CLI_PartyContactMechanism_PartyId_ContactMechanismId_FromDate] on [Enterprise].[PartyContactMechanism]'
GO
CREATE CLUSTERED INDEX [CLI_PartyContactMechanism_PartyId_ContactMechanismId_FromDate] ON [Enterprise].[PartyContactMechanism] ([PartyId], [ContactMechanismId], [ThruDate])
GO
PRINT N'Creating primary key [PK_PartyContactMechanism] on [Enterprise].[PartyContactMechanism]'
GO
ALTER TABLE [Enterprise].[PartyContactMechanism] ADD CONSTRAINT [PK_PartyContactMechanism] PRIMARY KEY NONCLUSTERED  ([PartyContactMechanismId])
GO
PRINT N'Creating [Person].[LinkContactMechanismToParty]'
GO
CREATE PROCEDURE [Person].[LinkContactMechanismToParty] (
	@RealPageId UNIQUEIDENTIFIER,
	@ContactMechanismId INT,
	@FromDate DATETIME,
	@ThruDate DATETIME = NULL,
	@PartyContactMechanismId bigint = NULL
)
AS
BEGIN
    BEGIN TRY
        DECLARE @PartyID BIGINT;

        SELECT  @PartyID = p.PartyId
        FROM    Enterprise.Party p
        WHERE   p.RealPageId = @RealPageId;		

        BEGIN TRANSACTION; 	

		-- Check to see if we want to expire a current Contact Method
        IF ((@PartyContactMechanismId IS NOT NULL) AND (@PartyContactMechanismId > 0))
        BEGIN
            UPDATE  Enterprise.PartyContactMechanism
            SET     ThruDate = GETUTCDATE()
			OUTPUT  Inserted.PartyContactMechanismId AS Id ,
					@RealPageId AS RealPageId ,
					'' AS ErrorMessage
			WHERE   PartyContactMechanismId = @PartyContactMechanismId; 
        END;
        ELSE
        BEGIN
            INSERT  INTO Enterprise.PartyContactMechanism (
				PartyId ,
				ContactMechanismId ,
				FromDate ,
				ThruDate
            )
            OUTPUT  Inserted.PartyContactMechanismId AS Id ,
                    @RealPageId AS RealPageId ,
                    '' AS ErrorMessage
            VALUES  (
				@PartyID ,
				@ContactMechanismId ,
				@FromDate ,
				@ThruDate
			);
        END;

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id ,
                @RealPageId AS RealPageId ,
                ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
    END CATCH;
END;
GO
PRINT N'Creating [Enterprise].[CommunicationEventEmail]'
GO
CREATE TABLE [Enterprise].[CommunicationEventEmail]
(
[CommunicationEventEmailID] [bigint] NOT NULL IDENTITY(1, 1),
[CommunicationEmailTemplateID] [int] NOT NULL,
[ContactMechanismValidEmailID] [int] NOT NULL
)
GO
PRINT N'Creating primary key [PK_CommunicationEventEmail] on [Enterprise].[CommunicationEventEmail]'
GO
ALTER TABLE [Enterprise].[CommunicationEventEmail] ADD CONSTRAINT [PK_CommunicationEventEmail] PRIMARY KEY CLUSTERED  ([CommunicationEventEmailID])
GO
PRINT N'Creating [Enterprise].[LinkEmailTemplateToValidEmail]'
GO
CREATE PROCEDURE [Enterprise].[LinkEmailTemplateToValidEmail]
	@CommunicationEmailTemplateId INT,
	@ContactMechanismValidEmailID INT
AS
BEGIN
	INSERT INTO [Enterprise].CommunicationEventEmail (CommunicationEmailTemplateID, ContactMechanismValidEmailID)
	SELECT @CommunicationEmailTemplateId, @ContactMechanismValidEmailID
END;
GO
PRINT N'Creating [Enterprise].[ContactMechanism]'
GO
CREATE TABLE [Enterprise].[ContactMechanism]
(
[ContactMechanismID] [int] NOT NULL IDENTITY(1, 1)
)
GO
PRINT N'Creating primary key [PK_ContactMechanism] on [Enterprise].[ContactMechanism]'
GO
ALTER TABLE [Enterprise].[ContactMechanism] ADD CONSTRAINT [PK_ContactMechanism] PRIMARY KEY CLUSTERED  ([ContactMechanismID])
GO
PRINT N'Creating [Person].[CreateContactMechanism]'
GO
CREATE PROCEDURE [Person].[CreateContactMechanism]
	@ContactMechanismId INT OUTPUT
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION; 

	    INSERT	INTO Enterprise.ContactMechanism
		OUTPUT	Inserted.ContactMechanismID AS Id,
				'' AS ErrorMessage
            DEFAULT VALUES;
		SELECT @ContactMechanismId = SCOPE_IDENTITY();
        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
    END CATCH;
END;
GO
PRINT N'Creating [Enterprise].[GlobalProductConfiguration]'
GO
CREATE TABLE [Enterprise].[GlobalProductConfiguration]
(
[GlobalProductConfigurationId] [int] NOT NULL IDENTITY(1, 1),
[ConfigurationId] [int] NOT NULL,
[ProductId] [int] NOT NULL,
[FromDate] [datetime] NOT NULL CONSTRAINT [DF__GlobalPro__FromD__5555A4F4] DEFAULT (getutcdate()),
[ThruDate] [datetime] NULL
)
GO
PRINT N'Creating primary key [PK_GlobalProductConfiguration] on [Enterprise].[GlobalProductConfiguration]'
GO
ALTER TABLE [Enterprise].[GlobalProductConfiguration] ADD CONSTRAINT [PK_GlobalProductConfiguration] PRIMARY KEY CLUSTERED  ([GlobalProductConfigurationId])
GO
PRINT N'Adding constraints to [Enterprise].[GlobalProductConfiguration]'
GO
ALTER TABLE [Enterprise].[GlobalProductConfiguration] ADD CONSTRAINT [AK_GlobalProductConfiguration_ConfigurationId_ProductId_ThruDate] UNIQUE NONCLUSTERED  ([ConfigurationId], [ProductId], [ThruDate])
GO
PRINT N'Creating [Enterprise].[LinkGlobalConfigurationToProduct]'
GO
CREATE PROC [Enterprise].[LinkGlobalConfigurationToProduct]
@ConfigurationId INT,
@ProductId INT,
@FromDate DATETIME = NULL,
@ThruDate DATETIME = NULL
AS
BEGIN
    
	UPDATE Enterprise.GlobalProductConfiguration
	SET ThruDate = GETUTCDATE()
	WHERE ProductId = @ProductId
	AND (ThruDate >= GETUTCDATE() OR ThruDate IS NULL);

	INSERT INTO Enterprise.GlobalProductConfiguration (   ConfigurationId , ProductId , FromDate , ThruDate )
	VALUES (   @ConfigurationId , @ProductId , ISNULL(@FromDate, GETUTCDATE()) , @ThruDate )

END
GO
PRINT N'Creating [Person].[UpdateContactMechanismUsageForParty]'
GO
CREATE PROCEDURE [Person].[UpdateContactMechanismUsageForParty] (
    @PartyContactMechanismId INT ,
    @ContactMechanismUsageTypeId INT = 1
)
AS
BEGIN
	BEGIN TRY
        BEGIN TRANSACTION; 
        UPDATE  Enterprise.ContactMechanismUsage
        SET     ContactMechanismUsageTypeID = @ContactMechanismUsageTypeId
		OUTPUT	Inserted.ContactMechanismUsageID AS Id,
				'' AS ErrorMessage
        WHERE   PartyContactMechanismID = @PartyContactMechanismId; 
        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
    END CATCH;
END;
GO
PRINT N'Creating [Enterprise].[PersonaConfiguration]'
GO
CREATE TABLE [Enterprise].[PersonaConfiguration]
(
[PersonaConfigurationId] [bigint] NOT NULL IDENTITY(1, 1),
[PersonaId] [bigint] NOT NULL,
[ConfigurationId] [int] NOT NULL,
[ProductId] [int] NOT NULL,
[FromDate] [datetime] NOT NULL CONSTRAINT [DF__PersonaCo__FromD__0B5CAFEA] DEFAULT (getutcdate()),
[ThruDate] [datetime] NULL
)
GO
PRINT N'Creating primary key [PK_PersonaConfiguration] on [Enterprise].[PersonaConfiguration]'
GO
ALTER TABLE [Enterprise].[PersonaConfiguration] ADD CONSTRAINT [PK_PersonaConfiguration] PRIMARY KEY CLUSTERED  ([PersonaConfigurationId])
GO
PRINT N'Creating [Enterprise].[LinkProductConfigurationToPersona]'
GO
CREATE PROC [Enterprise].[LinkProductConfigurationToPersona]
    @PersonaId BIGINT ,
    @ConfigurationId INT ,
    @ProductId INT ,
    @FromDate DATETIME = NULL ,
    @ThruDate DATETIME = NULL
AS
    BEGIN

        DECLARE @NOW DATETIME = GETUTCDATE();

        IF @FromDate IS NULL
            SET @FromDate = GETUTCDATE();

        UPDATE PersonaConfiguration
        SET    ThruDate = @NOW
        FROM   Enterprise.PersonaConfiguration
        WHERE  ProductId = @ProductId
               AND (   ThruDate >= @NOW
                       OR ThruDate IS NULL
                   )
               AND PersonaId = @PersonaId;

        INSERT INTO Enterprise.PersonaConfiguration (   PersonaId , ConfigurationId , ProductId , FromDate , ThruDate )
        VALUES ( @PersonaId , @ConfigurationId , @ProductId , @FromDate , @ThruDate );

    END;
GO
PRINT N'Creating [Enterprise].[PartyRole]'
GO
CREATE TABLE [Enterprise].[PartyRole]
(
[PartyRoleId] [int] NOT NULL IDENTITY(1, 1),
[PartyId] [bigint] NOT NULL,
[RoleTypeId] [int] NOT NULL
)
GO
PRINT N'Creating primary key [PK_PartyRole] on [Enterprise].[PartyRole]'
GO
ALTER TABLE [Enterprise].[PartyRole] ADD CONSTRAINT [PK_PartyRole] PRIMARY KEY CLUSTERED  ([PartyRoleId])
GO
PRINT N'Adding constraints to [Enterprise].[PartyRole]'
GO
ALTER TABLE [Enterprise].[PartyRole] ADD CONSTRAINT [AK_PartyRole_PartyId_RoleTypeId] UNIQUE NONCLUSTERED  ([PartyId], [RoleTypeId])
GO
PRINT N'Creating [Person].[SetPartyRole]'
GO
CREATE PROCEDURE [Person].[SetPartyRole]
    @PartyId BIGINT ,
    @RoleTypeId INT
AS
    BEGIN
        BEGIN TRY
        -- Check if the ContactMechanism Exists, If it does, then update it.

            BEGIN TRANSACTION; 

            UPDATE  r
            SET     r.RoleTypeId = @RoleTypeId
            OUTPUT  Inserted.PartyRoleId AS Id ,
                    '' AS ErrorMessage
            FROM    Enterprise.PartyRole r
            WHERE   r.PartyId = @PartyId;

            IF @@ROWCOUNT = 0
                BEGIN

                    INSERT  INTO Enterprise.PartyRole
                            ( PartyId, RoleTypeId )
                    OUTPUT  Inserted.PartyRoleId AS Id, '' AS ErrorMessage
                    VALUES  ( @PartyId, @RoleTypeId );
	    
                END;

            COMMIT;
        END TRY
        BEGIN CATCH
            ROLLBACK;

            DECLARE @ErrorLogID INT;
            EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

            SELECT  0 AS Id ,
                    ErrorMessage
            FROM    dbo.ErrorLog
            WHERE   ErrorLogID = @ErrorLogID;
        END CATCH;
    END;
GO
PRINT N'Creating [Enterprise].[ProductConfiguration]'
GO
CREATE TABLE [Enterprise].[ProductConfiguration]
(
[ProductConfigurationId] [int] NOT NULL IDENTITY(1, 1),
[ConfigurationId] [int] NOT NULL,
[ProductSettingId] [int] NOT NULL,
[FromDate] [datetime] NOT NULL CONSTRAINT [DF__ProductCo__FromD__0C50D423] DEFAULT (getutcdate()),
[ThruDate] [datetime] NULL
)
GO
PRINT N'Creating primary key [PK_ProductConfiguration] on [Enterprise].[ProductConfiguration]'
GO
ALTER TABLE [Enterprise].[ProductConfiguration] ADD CONSTRAINT [PK_ProductConfiguration] PRIMARY KEY CLUSTERED  ([ProductConfigurationId])
GO
PRINT N'Creating [Enterprise].[LinkProductSettingToConfiguration]'
GO
CREATE PROCEDURE [Enterprise].[LinkProductSettingToConfiguration] (
	@ConfigurationId INT,
	@ProductSettingId INT,
	@FromDate DATETIME = NULL,
	@ThruDate DATETIME = NULL
)
AS
BEGIN
	DECLARE @UTCDATE datetime = GETUTCDATE();

	BEGIN TRY
        BEGIN TRANSACTION;
		UPDATE	Enterprise.ProductConfiguration
		SET		ThruDate = @UTCDATE
		WHERE	ConfigurationId = @ConfigurationId
		AND		ProductSettingId = @ProductSettingId
		AND		(ThruDate >= @UTCDATE OR ThruDate IS NULL)

		INSERT INTO Enterprise.ProductConfiguration (
			ConfigurationId,
			ProductSettingId,
			FromDate,
			ThruDate
		)
		OUTPUT	Inserted.ProductConfigurationId AS Id,
				'' AS ErrorMessage
		VALUES (
			@ConfigurationId,
			@ProductSettingId,
			ISNULL(@FromDate, @UTCDATE),
			@ThruDate
		)
		COMMIT;
	END TRY  
	BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
	END CATCH
END;
GO
PRINT N'Creating [Person].[GetPartyRole]'
GO
CREATE PROCEDURE [Person].[GetPartyRole]
	@PartyId BIGINT
AS
    BEGIN
        BEGIN TRY
        -- Check if the ContactMechanism Exists, If it does, then update it.

			SELECT PartyRoleId ,
                   PartyId ,
                   RoleTypeId
			FROM Enterprise.PartyRole
			WHERE PartyId = @PartyId

        END TRY
        BEGIN CATCH
            ROLLBACK;

            DECLARE @ErrorLogID INT;
            EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

            SELECT  0 AS Id ,
                    ErrorMessage
            FROM    dbo.ErrorLog
            WHERE   ErrorLogID = @ErrorLogID;
        END CATCH;
    END;
GO
PRINT N'Creating [Enterprise].[CommunicationEmailTemplate]'
GO
CREATE TABLE [Enterprise].[CommunicationEmailTemplate]
(
[CommunicationEmailTemplateID] [int] NOT NULL IDENTITY(1, 1),
[Subject] [nvarchar] (255) NOT NULL,
[Body] [nvarchar] (max) NOT NULL
)
GO
PRINT N'Creating primary key [PK_Enterprise.CommunicationEmailTemplate] on [Enterprise].[CommunicationEmailTemplate]'
GO
ALTER TABLE [Enterprise].[CommunicationEmailTemplate] ADD CONSTRAINT [PK_Enterprise.CommunicationEmailTemplate] PRIMARY KEY CLUSTERED  ([CommunicationEmailTemplateID])
GO
PRINT N'Creating [Enterprise].[ListCommunicationEmailTemplates]'
GO
CREATE PROCEDURE [Enterprise].[ListCommunicationEmailTemplates]
AS
BEGIN  
	SELECT	CommunicationEmailTemplateId,
			Subject,
			Body
	FROM	Enterprise.CommunicationEmailTemplate
END;
GO
PRINT N'Creating [Person].[Persona]'
GO
CREATE TABLE [Person].[Persona]
(
[PersonaId] [bigint] NOT NULL IDENTITY(1, 1),
[PersonPartyId] [bigint] NOT NULL,
[OrganizationPartyId] [bigint] NOT NULL,
[PersonaTypeId] [int] NOT NULL,
[PersonaEnvironmentTypeId] [int] NOT NULL CONSTRAINT [DF_Persona_PersonaEnvironmentTypeId] DEFAULT ((1)),
[FromDate] [datetime] NOT NULL CONSTRAINT [DF__Persona__FromDat__41049384] DEFAULT (getutcdate()),
[ThruDate] [datetime] NULL,
[IsDefault] [bit] NOT NULL CONSTRAINT [DF__Persona__IsDefau__41F8B7BD] DEFAULT ((0))
)
GO
PRINT N'Creating primary key [PK_Persona] on [Person].[Persona]'
GO
ALTER TABLE [Person].[Persona] ADD CONSTRAINT [PK_Persona] PRIMARY KEY CLUSTERED  ([PersonaId])
GO
PRINT N'Creating [Person].[CreatePersona]'
GO
CREATE PROCEDURE [Person].[CreatePersona]
    @PersonRealPageId UNIQUEIDENTIFIER,
    @OrganizationRealPageId UNIQUEIDENTIFIER,
    @PersonaTypeId INT,
    @PersonaEnvironmentTypeId INT,
    @FromDate DATETIME,
    @ThruDate DATETIME = NULL,
    @PersonaId BIGINT = NULL OUTPUT
AS
    BEGIN
		IF @FromDate IS NULL
			SELECT @FromDate = GETUTCDATE();

        BEGIN TRY
            DECLARE @PersonPartyId BIGINT,
                    @OrganizationPartyId BIGINT;

            -- Get the Party ID for a Person
            SELECT @PersonPartyId = PartyId
            FROM   Enterprise.Party
            WHERE  RealPageId = @PersonRealPageId;

            -- Get the Party ID for an Organization
            SELECT @OrganizationPartyId = PartyId
            FROM   Enterprise.Party
            WHERE  RealPageId = @OrganizationRealPageId;

            BEGIN TRANSACTION;

			-- Check if it exists
			SELECT	@PersonaId = PersonaId 
			FROM	Person.Persona
			WHERE	Persona.PersonPartyId = @PersonPartyId
			AND		Persona.OrganizationPartyId = @OrganizationPartyId
			AND		Persona.PersonaTypeId = @PersonaTypeId

			IF  @PersonaId IS NULL
				BEGIN
					INSERT INTO Person.Persona (
						PersonPartyId,
						OrganizationPartyId,
						PersonaTypeId,
						PersonaEnvironmentTypeId,
						FromDate,
						ThruDate
					)
					VALUES (
						@PersonPartyId,
						@OrganizationPartyId,
						@PersonaTypeId,
						@PersonaEnvironmentTypeId,
						@FromDate,
						@ThruDate
					);

					SET @PersonaId = SCOPE_IDENTITY()					
				END;

            SELECT	@PersonaId AS Id ,
					'' AS ErrorMessage
            COMMIT;
        END TRY
        BEGIN CATCH
            ROLLBACK;

            DECLARE @ErrorLogID INT;
            EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

            SELECT 0 AS Id,
                   '' AS RealPageId,
                   ErrorMessage
            FROM   dbo.ErrorLog
            WHERE  ErrorLogID = @ErrorLogID;
        END CATCH;
    END;
GO
PRINT N'Creating [Enterprise].[CommunicationEventPurposeType]'
GO
CREATE TABLE [Enterprise].[CommunicationEventPurposeType]
(
[CommunicationEventPurposeTypeID] [int] NOT NULL IDENTITY(1, 1),
[Description] [nvarchar] (50) NOT NULL
)
GO
PRINT N'Creating primary key [PK_Enterprise.CommunicationEventPurposeType] on [Enterprise].[CommunicationEventPurposeType]'
GO
ALTER TABLE [Enterprise].[CommunicationEventPurposeType] ADD CONSTRAINT [PK_Enterprise.CommunicationEventPurposeType] PRIMARY KEY CLUSTERED  ([CommunicationEventPurposeTypeID])
GO
PRINT N'Creating [Enterprise].[ListCommunicationEventPurposeTypes]'
GO
CREATE PROCEDURE [Enterprise].[ListCommunicationEventPurposeTypes]
AS
BEGIN
	SELECT	CommunicationEventPurposeTypeID,
			Description
	FROM [Enterprise].CommunicationEventPurposeType
END;
GO
PRINT N'Creating [Auth].[PasswordPolicy]'
GO
CREATE TABLE [Auth].[PasswordPolicy]
(
[PasswordPolicyId] [int] NOT NULL IDENTITY(1, 1),
[PortfolioId] [int] NOT NULL,
[MinimumLength] [tinyint] NOT NULL CONSTRAINT [DF_PasswordPolicy_MinimumLength] DEFAULT ((8)),
[MaximumLength] [tinyint] NOT NULL CONSTRAINT [DF_PasswordPolicy_MaximumLength] DEFAULT ((128)),
[MinimumLowercase] [tinyint] NOT NULL CONSTRAINT [DF_PasswordPolicy_MinimumLowercase] DEFAULT ((0)),
[MinimumUppercase] [tinyint] NOT NULL CONSTRAINT [DF_PasswordPolicy_MinimumUppercase] DEFAULT ((0)),
[MinimumNumeric] [tinyint] NOT NULL CONSTRAINT [DF_PasswordPolicy_MinimumNumeric] DEFAULT ((0)),
[MinimumSpecialCharacter] [tinyint] NOT NULL CONSTRAINT [DF_PasswordPolicy_MinimumSpecialCharacter] DEFAULT ((0)),
[AllowUsersToChangeOwnPassword] [bit] NOT NULL CONSTRAINT [DF_PasswordPolicy_AllowUsersToChangeOwnPassword] DEFAULT ((1)),
[EnablePasswordExpiration] [bit] NOT NULL CONSTRAINT [DF_PasswordPolicy_EnablePasswordExpiration] DEFAULT ((0)),
[PasswordExpirationPeriodInDays] [smallint] NULL,
[PreventPasswordReuse] [bit] NOT NULL CONSTRAINT [DF_PasswordPolicy_PreventPasswordReuse] DEFAULT ((0)),
[NumberOfPasswordsToRemember] [tinyint] NULL,
[UserId] [bigint] NOT NULL,
[SysStartDateTime] [datetime2] (0) GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_PasswordPolicy_SysStartDateTime] DEFAULT (sysutcdatetime()),
[SysEndDateTime] [datetime2] (0) GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_PasswordPolicy_SysEndDateTime] DEFAULT (sysutcdatetime()),
PERIOD FOR SYSTEM_TIME (SysStartDateTime, SysEndDateTime),
CONSTRAINT [PK_Auth.PasswordPolicy] PRIMARY KEY CLUSTERED  ([PasswordPolicyId])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [Auth].[PasswordPolicyHistory])
)
GO
PRINT N'Creating [Enterprise].[CommunicationEventPurposeUsage]'
GO
CREATE TABLE [Enterprise].[CommunicationEventPurposeUsage]
(
[CommunicationEventPurposeUsageID] [int] NOT NULL IDENTITY(1, 1),
[CommunicationEventID] [bigint] NOT NULL,
[CommunicationEventPurposeTypeID] [int] NOT NULL
)
GO
PRINT N'Creating primary key [PK_CommunicationEventPurposeUsage] on [Enterprise].[CommunicationEventPurposeUsage]'
GO
ALTER TABLE [Enterprise].[CommunicationEventPurposeUsage] ADD CONSTRAINT [PK_CommunicationEventPurposeUsage] PRIMARY KEY CLUSTERED  ([CommunicationEventPurposeUsageID])
GO
PRINT N'Creating [Enterprise].[ListCommunicationEventPurposeUsages]'
GO
CREATE PROCEDURE [Enterprise].[ListCommunicationEventPurposeUsages]
AS
BEGIN  
	SELECT	CommunicationEventPurposeUsageId,
			CommunicationEventId,
			CommunicationEventPurposeTypeId
	FROM	Enterprise.CommunicationEventPurposeUsage
END;
GO
PRINT N'Creating [Enterprise].[CommunicationEventRoleType]'
GO
CREATE TABLE [Enterprise].[CommunicationEventRoleType]
(
[CommunicationEventRoleTypeID] [int] NOT NULL IDENTITY(1, 1),
[Description] [nvarchar] (50) NOT NULL
)
GO
PRINT N'Creating primary key [PK_Enterprise.CommunicationEventRoleType] on [Enterprise].[CommunicationEventRoleType]'
GO
ALTER TABLE [Enterprise].[CommunicationEventRoleType] ADD CONSTRAINT [PK_Enterprise.CommunicationEventRoleType] PRIMARY KEY CLUSTERED  ([CommunicationEventRoleTypeID])
GO
PRINT N'Creating [Enterprise].[ListCommunicationEventRoleTypes]'
GO
CREATE PROCEDURE [Enterprise].[ListCommunicationEventRoleTypes]
AS
BEGIN
	SELECT	CommunicationEventRoleTypeID,
			Description
	FROM [Enterprise].CommunicationEventRoleType
END;
GO
PRINT N'Creating [Ident].[PasswordPolicy]'
GO
CREATE TABLE [Ident].[PasswordPolicy]
(
[PasswordPolicyId] [int] NOT NULL IDENTITY(1, 1),
[PartyId] [bigint] NOT NULL,
[MinimumLength] [tinyint] NOT NULL CONSTRAINT [DF_PasswordPolicy_MinimumLength] DEFAULT ((8)),
[MaximumLength] [tinyint] NOT NULL CONSTRAINT [DF_PasswordPolicy_MaximumLength] DEFAULT ((128)),
[MinimumLowercase] [tinyint] NOT NULL CONSTRAINT [DF_PasswordPolicy_MinimumLowercase] DEFAULT ((0)),
[MinimumUppercase] [tinyint] NOT NULL CONSTRAINT [DF_PasswordPolicy_MinimumUppercase] DEFAULT ((0)),
[MinimumNumeric] [tinyint] NOT NULL CONSTRAINT [DF_PasswordPolicy_MinimumNumeric] DEFAULT ((0)),
[MinimumSpecialCharacter] [tinyint] NOT NULL CONSTRAINT [DF_PasswordPolicy_MinimumSpecialCharacter] DEFAULT ((0)),
[AllowUsersToChangeOwnPassword] [bit] NOT NULL CONSTRAINT [DF_PasswordPolicy_AllowUsersToChangeOwnPassword] DEFAULT ((1)),
[EnablePasswordExpiration] [bit] NOT NULL CONSTRAINT [DF_PasswordPolicy_EnablePasswordExpiration] DEFAULT ((0)),
[PasswordExpirationPeriodInDays] [smallint] NULL,
[PreventPasswordReuse] [bit] NOT NULL CONSTRAINT [DF_PasswordPolicy_PreventPasswordReuse] DEFAULT ((0)),
[NumberOfPasswordsToRemember] [tinyint] NULL,
[UserId] [bigint] NOT NULL,
[SysStartDateTime] [datetime2] (0) GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_PasswordPolicy_SysStartDateTime] DEFAULT (sysutcdatetime()),
[SysEndDateTime] [datetime2] (0) GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_PasswordPolicy_SysEndDateTime] DEFAULT (sysutcdatetime()),
PERIOD FOR SYSTEM_TIME (SysStartDateTime, SysEndDateTime),
CONSTRAINT [PK_Ident.PasswordPolicy] PRIMARY KEY CLUSTERED  ([PasswordPolicyId])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [Ident].[PasswordPolicyHistory])
)
GO
PRINT N'Creating [Enterprise].[ContactMechanismType]'
GO
CREATE TABLE [Enterprise].[ContactMechanismType]
(
[ContactMechanismTypeID] [int] NOT NULL IDENTITY(1, 1),
[Description] [nvarchar] (50) NOT NULL
)
GO
PRINT N'Creating primary key [PK_ContactMechanismType] on [Enterprise].[ContactMechanismType]'
GO
ALTER TABLE [Enterprise].[ContactMechanismType] ADD CONSTRAINT [PK_ContactMechanismType] PRIMARY KEY CLUSTERED  ([ContactMechanismTypeID])
GO
PRINT N'Creating [Enterprise].[ListContactMechanismTypes]'
GO
CREATE PROCEDURE [Enterprise].[ListContactMechanismTypes]
AS
BEGIN
	SELECT	ContactMechanismTypeID,
			Description
	FROM [Enterprise].ContactMechanismType
END;
GO
PRINT N'Creating [Enterprise].[ContactMechanismValidEmail]'
GO
CREATE TABLE [Enterprise].[ContactMechanismValidEmail]
(
[ContactMechanismValidEmailID] [int] NOT NULL IDENTITY(1, 1),
[ContactMechanismTypeID] [int] NOT NULL
)
GO
PRINT N'Creating primary key [PK_ContactMechanismValidEmail] on [Enterprise].[ContactMechanismValidEmail]'
GO
ALTER TABLE [Enterprise].[ContactMechanismValidEmail] ADD CONSTRAINT [PK_ContactMechanismValidEmail] PRIMARY KEY CLUSTERED  ([ContactMechanismValidEmailID])
GO
PRINT N'Creating [Enterprise].[ListContactMechanismValidEmails]'
GO
CREATE PROCEDURE [Enterprise].[ListContactMechanismValidEmails]
AS
BEGIN  
	SELECT	ContactMechanismValidEmailId,
			ContactMechanismTypeId
	FROM	Enterprise.ContactMechanismValidEmail
END;
GO
PRINT N'Creating [Enterprise].[ProductSetting]'
GO
CREATE TABLE [Enterprise].[ProductSetting]
(
[ProductSettingId] [int] NOT NULL IDENTITY(1, 1),
[ProductId] [int] NOT NULL,
[ProductSettingTypeId] [int] NOT NULL,
[Value] [nvarchar] (1000) NOT NULL,
[FromDate] [datetime] NOT NULL CONSTRAINT [DF__ProductSe__FromD__078C1F06] DEFAULT (getutcdate()),
[ThruDate] [datetime] NULL
)
GO
PRINT N'Creating primary key [PK_ProductSetting] on [Enterprise].[ProductSetting]'
GO
ALTER TABLE [Enterprise].[ProductSetting] ADD CONSTRAINT [PK_ProductSetting] PRIMARY KEY CLUSTERED  ([ProductSettingId])
GO
PRINT N'Creating index [IX_ProductSetting_ProductId_ProductSettingTypeId] on [Enterprise].[ProductSetting]'
GO
CREATE NONCLUSTERED INDEX [IX_ProductSetting_ProductId_ProductSettingTypeId] ON [Enterprise].[ProductSetting] ([ProductId], [ProductSettingTypeId], [Value])
GO
PRINT N'Creating [Enterprise].[ListGlobalSettingsForProduct]'
GO
CREATE PROCEDURE [Enterprise].[ListGlobalSettingsForProduct]
    @ProductId INT
AS
    BEGIN

		DECLARE @NOW DATETIME = GETUTCDATE();

        SELECT	pc.ProductConfigurationId ,
				pst.Name ,
				ps.Value
        FROM	Enterprise.GlobalProductConfiguration gpc
				JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
				JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId
				JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
        WHERE  gpc.ProductId = @ProductId
				AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))
				AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))
				AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))
    END;
GO
PRINT N'Creating [Auth].[Tokens]'
GO
CREATE TABLE [Auth].[Tokens]
(
[TokenKey] [nvarchar] (128) NOT NULL,
[TokenType] [int] NOT NULL,
[ClientCode] [nvarchar] (200) NOT NULL,
[SubjectCode] [nvarchar] (200) NULL,
[JsonCode] [nvarchar] (3072) NULL,
[AuthCodeChallenge] [nvarchar] (250) NULL,
[AuthCodeChallengeMethod] [nvarchar] (50) NULL,
[Nonce] [nvarchar] (200) NULL,
[RedirectUri] [nvarchar] (2000) NULL,
[SessionId] [nvarchar] (200) NULL,
[IsOpenId] [bit] NULL,
[WasConsentShown] [bit] NULL,
[Expiry] [datetimeoffset] NOT NULL
)
GO
PRINT N'Creating primary key [PK_Tokens] on [Auth].[Tokens]'
GO
ALTER TABLE [Auth].[Tokens] ADD CONSTRAINT [PK_Tokens] PRIMARY KEY CLUSTERED  ([TokenKey], [TokenType])
GO
PRINT N'Creating [Auth].[GetToken]'
GO
CREATE PROCEDURE [Auth].[GetToken]
	@TokenKey		NVARCHAR (128),
	@TokenType INT 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT * FROM Auth.Tokens WHERE TokenKey = @TokenKey and TokenType=@TokenType

END
GO
PRINT N'Creating [Enterprise].[ProductBatch]'
GO
CREATE TABLE [Enterprise].[ProductBatch]
(
[ProductBatchId] [int] NOT NULL IDENTITY(1, 1),
[PersonPartyId] [bigint] NOT NULL,
[CreateUserPersonaId] [bigint] NOT NULL,
[AssignUserPersonaId] [bigint] NOT NULL,
[ProductId] [int] NOT NULL,
[StatusTypeId] [int] NOT NULL CONSTRAINT [DF_ProductBatch_StatusId] DEFAULT ((5)),
[RetryCount] [tinyint] NOT NULL CONSTRAINT [DF_ProductBatch_RetryCount] DEFAULT ((0)),
[InputJson] [nvarchar] (max) NOT NULL,
[LastRunDate] [smalldatetime] NULL,
[CreatedDate] [smalldatetime] NOT NULL CONSTRAINT [DF_ProductBatch_CreatedDate] DEFAULT (getutcdate()),
[ModifiedDate] [smalldatetime] NULL,
[ErrorDetails] [varchar] (max) NULL
)
GO
PRINT N'Creating primary key [PK_ProductBatch] on [Enterprise].[ProductBatch]'
GO
ALTER TABLE [Enterprise].[ProductBatch] ADD CONSTRAINT [PK_ProductBatch] PRIMARY KEY CLUSTERED  ([ProductBatchId])
GO
PRINT N'Creating [Enterprise].[ListProductBatch]'
GO
CREATE PROCEDURE [Enterprise].[ListProductBatch](
	 @IncludeErrorRecord bit = 'True'
	,@BatchSize int
	,@RetryCount tinyint = 3
	) 

AS

BEGIN

	SET NOCOUNT ON;
		DECLARE @PBFiltered TABLE(
		[ProductBatchId] [int] NOT NULL,
		[RealPageId] UNIQUEIDENTIFIER NOT NULL,
		[PersonPartyID] [bigint] NOT NULL,
		[CreateUserPersonaId] [bigint] NOT NULL,
		[AssignUserPersonaId] [bigint] NOT NULL,
		[ProductId] [int] NOT NULL,
		[StatusTypeId] [int] NOT NULL,
		[RetryCount] [tinyint] NOT NULL,
		[InputJson] [nvarchar](max) NOT NULL,
		[LastRunDate] [smalldatetime] NULL,
		[CreatedDate] [smalldatetime] NOT NULL,
		[ModifiedDate] [smalldatetime] NULL,
		[ErrorDetails] [varchar](max) NULL)
		
		BEGIN TRANSACTION-- HAve to lock the tables so that another process can't come in and scoop up our waiting processes

			INSERT INTO @PBFiltered(
				 [ProductBatchId]
				,[RealPageId]
				,[PersonPartyId]
				,[CreateUserPersonaId]
				,[AssignUserPersonaId]
				,[ProductId]
				,[StatusTypeId]
				,[RetryCount]
				,[InputJson]
				,[LastRunDate]
				,[CreatedDate]
				,[ModifiedDate]
				,[ErrorDetails]
			)
			SELECT TOP (@BatchSize)
				 [ProductBatchId]
				,[RealpageId]
				,[PersonPartyId]
				,[CreateUserPersonaId]
				,[AssignUserPersonaId]
				,[ProductId]
				,[StatusTypeId]
				,[RetryCount]
				,[InputJson]
				,[LastRunDate]
				,[CreatedDate]
				,[ModifiedDate]
				,[ErrorDetails]
			FROM [Enterprise].[ProductBatch]
			JOIN Enterprise.Party ON ProductBatch.PersonPartyID = Party.PartyID
			WHERE
				(@IncludeErrorRecord = 'True' AND ( StatusTypeId = 7 and RetryCount < @Retrycount))
				OR  
				( @IncludeErrorRecord = 'False' AND StatusTypeID = 5)


		UPDATE Enterprise.ProductBatch SET StatusTypeId = 6 --Running
		FROM Enterprise.ProductBatch
		JOIN @PBFiltered AS Filtered ON Filtered.ProductBatchId = ProductBatch.ProductBatchId

		SELECT 
			 [ProductBatchId]
			,[RealPageId]
			,[PersonPartyId]
			,[CreateUserPersonaId]
			,[AssignUserPersonaId]
			,[ProductId]
			,[StatusTypeId]
			,[RetryCount]
			,[InputJson]
			,[LastRunDate]
			,[CreatedDate]
			,[ModifiedDate]
			,[ErrorDetails]
		FROM @PBFiltered

	COMMIT TRANSACTION
END;
GO
PRINT N'Creating [Ident].[SamlUserAttribute]'
GO
CREATE TABLE [Ident].[SamlUserAttribute]
(
[SamlUserAttributeId] [int] NOT NULL IDENTITY(1, 1),
[PersonaId] [bigint] NOT NULL,
[ProductId] [int] NOT NULL,
[SamlAttributeId] [int] NOT NULL,
[Value] [nvarchar] (500) NULL,
[FromDate] [datetime] NOT NULL CONSTRAINT [DF__SamlUserA__FromD__269AB60B] DEFAULT (getutcdate()),
[ThruDate] [datetime] NULL
)
GO
PRINT N'Creating primary key [PK_SamlUserAttribute] on [Ident].[SamlUserAttribute]'
GO
ALTER TABLE [Ident].[SamlUserAttribute] ADD CONSTRAINT [PK_SamlUserAttribute] PRIMARY KEY CLUSTERED  ([SamlUserAttributeId])
GO
PRINT N'Adding constraints to [Ident].[SamlUserAttribute]'
GO
ALTER TABLE [Ident].[SamlUserAttribute] ADD CONSTRAINT [AK_SamlUserAttribute_PersonaId_ProductId_SamlAttributeId] UNIQUE NONCLUSTERED  ([PersonaId], [ProductId], [SamlAttributeId])
GO
PRINT N'Creating [dbo].[CreateSamlUserAttribute]'
GO
CREATE PROCEDURE [dbo].[CreateSamlUserAttribute]
    @PersonaId BIGINT ,
    @ProductId INT ,
    @SamlAttributeId INT ,
    @Value NVARCHAR(500)
AS
    BEGIN
        BEGIN TRY
            BEGIN TRANSACTION;

            INSERT INTO Ident.SamlUserAttribute (   PersonaId ,
                                                    ProductId ,
                                                    SamlAttributeId ,
                                                    Value
                                                )
            OUTPUT Inserted.SamlUserAttributeId AS Id ,
                   '' AS ErrorMessage
            VALUES ( @PersonaId, @ProductId, @SamlAttributeId, @Value );

            COMMIT;
        END TRY
        BEGIN CATCH
            ROLLBACK;

            DECLARE @ErrorLogID INT;
            EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

            SELECT 0 AS Id ,
                   ErrorMessage
            FROM   dbo.ErrorLog
            WHERE  ErrorLogID = @ErrorLogID;
        END CATCH;

    END;
GO
PRINT N'Creating [Auth].[Users]'
GO
CREATE TABLE [Auth].[Users]
(
[UserId] [bigint] NOT NULL IDENTITY(1, 1),
[LoginId] [nvarchar] (50) NOT NULL,
[Firstname] [nvarchar] (50) NOT NULL,
[LastName] [nvarchar] (50) NOT NULL,
[IsActive] [bit] NOT NULL CONSTRAINT [DF_Auth.Users_IsActive] DEFAULT ((1)),
[PasswordHash] [nvarchar] (1000) NULL,
[PasswordSalt] [nvarchar] (50) NULL,
[IdentityProvider] [nvarchar] (100) NOT NULL,
[Title] [nvarchar] (50) NULL,
[Email] [nvarchar] (50) NULL,
[Phone] [nvarchar] (20) NULL,
[IsLocked] [bit] NOT NULL CONSTRAINT [DF_Users_IsLocked] DEFAULT ((0)),
[IsTainted] [bit] NOT NULL CONSTRAINT [DF_Users_IsTainted] DEFAULT ((0)),
[LastPasswordModifiedDateTime] [datetime] NULL,
[AccountExpiration] [datetime] NULL
)
GO
PRINT N'Creating primary key [PK_Auth.Users] on [Auth].[Users]'
GO
ALTER TABLE [Auth].[Users] ADD CONSTRAINT [PK_Auth.Users] PRIMARY KEY CLUSTERED  ([UserId])
GO
PRINT N'Creating [Auth].[GetUserByLoginIdAndProvider]'
GO
CREATE PROCEDURE [Auth].[GetUserByLoginIdAndProvider]
	@LoginId		NVARCHAR (50),
	@IdentityProvider NVARCHAR(100) 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT * FROM Auth.Users WHERE LoginId = @LoginId AND IdentityProvider = @IdentityProvider

END
GO
PRINT N'Creating [Enterprise].[ListProductBatchByRealPageId]'
GO
CREATE PROCEDURE [Enterprise].[ListProductBatchByRealPageId](
	 @RealPageId UNIQUEIDENTIFIER
	,@AssignUserPersonaId bigint
	) 

AS

BEGIN

	SET NOCOUNT ON;
		
	SELECT 
		 [ProductBatchId]
		,[PersonPartyId]
		,[CreateUserPersonaId]
		,[AssignUserPersonaId]
		,[ProductId]
		,[StatusTypeId]
		,[RetryCount]
		,[InputJson]
		,[LastRunDate]
		,[CreatedDate]
		,[ModifiedDate]
		,[ErrorDetails]
	FROM [Enterprise].[ProductBatch]
	JOIN Enterprise.Party ON ProductBatch.PersonPartyId = Party.PartyId
	WHERE Party.RealPageId = @RealPageId
	AND AssignUserPersonaId = @AssignUserPersonaId
END;
GO
PRINT N'Creating [Enterprise].[UpdateProductSettingByPersona]'
GO
CREATE PROCEDURE [Enterprise].[UpdateProductSettingByPersona]
    (
        @PersonaId BIGINT ,
        @ProductId INT ,
        @SettingName NVARCHAR(20) ,
        @Value NVARCHAR(1000)
    )
AS
    BEGIN


        UPDATE ps
        SET    [Value] = LTRIM(RTRIM(@Value))
        FROM   Enterprise.ProductSetting ps
               JOIN Enterprise.ProductConfiguration pc ON pc.ProductSettingId = ps.ProductSettingId
               JOIN Enterprise.ProductSettingType pt ON pt.ProductSettingTypeId = ps.ProductSettingTypeId
               JOIN Enterprise.PersonaConfiguration perc ON perc.ConfigurationId = pc.ConfigurationId
        WHERE  pt.[Name] = @SettingName
               AND perc.PersonaId = @PersonaId
               AND perc.ProductId = @ProductId;

    END;
GO
PRINT N'Creating [Auth].[Clients]'
GO
CREATE TABLE [Auth].[Clients]
(
[ClientId] [int] NOT NULL IDENTITY(1, 1),
[ClientCode] [nvarchar] (200) NOT NULL,
[ClientName] [nvarchar] (200) NOT NULL,
[ClientUri] [nvarchar] (2000) NULL,
[LogoUri] [nvarchar] (max) NULL,
[Flow] [int] NOT NULL,
[LogoutUri] [nvarchar] (max) NULL,
[IdentityTokenLifetime] [int] NOT NULL,
[AccessTokenLifetime] [int] NOT NULL,
[AuthorizationCodeLifetime] [int] NOT NULL,
[AbsoluteRefreshTokenLifetime] [int] NOT NULL,
[SlidingRefreshTokenLifetime] [int] NOT NULL,
[RefreshTokenUsage] [int] NOT NULL,
[RefreshTokenExpiration] [int] NOT NULL,
[AccessTokenType] [int] NOT NULL,
[UpdateAccessTokenOnRefresh] [bit] NOT NULL,
[Enabled] [bit] NOT NULL,
[LogoutSessionRequired] [bit] NOT NULL,
[RequireSignOutPrompt] [bit] NOT NULL,
[AllowAccessToAllScopes] [bit] NOT NULL,
[AllowClientCredentialsOnly] [bit] NOT NULL,
[RequireConsent] [bit] NOT NULL,
[AllowRememberConsent] [bit] NOT NULL,
[EnableLocalLogin] [bit] NOT NULL,
[IncludeJwtId] [bit] NOT NULL,
[AlwaysSendClientClaims] [bit] NOT NULL,
[PrefixClientClaims] [bit] NOT NULL,
[AllowAccessToAllGrantTypes] [bit] NOT NULL
)
GO
PRINT N'Creating primary key [PK_dbo.Clients] on [Auth].[Clients]'
GO
ALTER TABLE [Auth].[Clients] ADD CONSTRAINT [PK_dbo.Clients] PRIMARY KEY CLUSTERED  ([ClientId])
GO
PRINT N'Creating [Auth].[GetClientByClientCode]'
GO
CREATE PROCEDURE [Auth].[GetClientByClientCode]
	@ClientCode nvarchar(200)
AS
BEGIN
	SET NOCOUNT ON;
	SELECT * FROM Auth.Clients WHERE ClientCode = @ClientCode
END
GO
PRINT N'Creating [Enterprise].[ProductType]'
GO
CREATE TABLE [Enterprise].[ProductType]
(
[ProductTypeId] [int] NOT NULL,
[ParentProductTypeId] [int] NULL,
[Name] [varchar] (50) NOT NULL,
[Description] [varchar] (1000) NULL,
[ProductTypeGUID] [uniqueidentifier] NOT NULL CONSTRAINT [DF__ProductTy__Produ__0E391C95] DEFAULT (newid())
)
GO
PRINT N'Creating primary key [PK_ProductType] on [Enterprise].[ProductType]'
GO
ALTER TABLE [Enterprise].[ProductType] ADD CONSTRAINT [PK_ProductType] PRIMARY KEY CLUSTERED  ([ProductTypeId])
GO
PRINT N'Creating [Enterprise].[Product]'
GO
CREATE TABLE [Enterprise].[Product]
(
[ProductId] [int] NOT NULL,
[ProductGUID] [uniqueidentifier] NOT NULL CONSTRAINT [DF__Product__Product__0880433F] DEFAULT (newid()),
[Name] [nvarchar] (50) NOT NULL,
[Description] [nvarchar] (100) NULL,
[ProductTypeId] [int] NULL
)
GO
PRINT N'Creating primary key [PK_Product] on [Enterprise].[Product]'
GO
ALTER TABLE [Enterprise].[Product] ADD CONSTRAINT [PK_Product] PRIMARY KEY CLUSTERED  ([ProductId])
GO
PRINT N'Creating [Enterprise].[OrganizationProduct]'
GO
CREATE TABLE [Enterprise].[OrganizationProduct]
(
[OrganizationProductId] [bigint] NOT NULL IDENTITY(1, 1),
[PartyId] [bigint] NOT NULL,
[ConfigurationId] [int] NOT NULL,
[ProductId] [int] NOT NULL,
[FromDate] [datetime] NOT NULL CONSTRAINT [DF__Organizat__FromD__0A688BB1] DEFAULT (getutcdate()),
[ThruDate] [datetime] NULL
)
GO
PRINT N'Creating primary key [PK_OrganizationProduct] on [Enterprise].[OrganizationProduct]'
GO
ALTER TABLE [Enterprise].[OrganizationProduct] ADD CONSTRAINT [PK_OrganizationProduct] PRIMARY KEY CLUSTERED  ([OrganizationProductId])
GO
PRINT N'Creating [Enterprise].[ListProductsByOrganization]'
GO
CREATE PROCEDURE [Enterprise].[ListProductsByOrganization]  
(  
 @OrganizationRealPageId UNIQUEIDENTIFIER  
)  
  
AS  
  
BEGIN  
	
	DECLARE @NOW  DATETIME = GETUTCDATE();

    SELECT pr.ProductGUID ,  
            pr.ProductId ,  
            pr.[Name] AS ProductName ,  
            pts.ProductTypeId AS SolutionId ,  
            pts.[Name] AS Solution ,  
            ptf.ProductTypeId AS FamilyId ,  
            ptf.[Name] AS Family ,  
            pr.Description AS ProductDescription  
    FROM [Enterprise].Organization o  
            JOIN [Enterprise].OrganizationProduct op ON op.PartyId = o.PartyId  
            JOIN [Enterprise].[Product] pr ON pr.ProductId = op.ProductId  
            LEFT JOIN [Enterprise].[ProductType] pts ON pts.ProductTypeId = pr.ProductTypeId  
            LEFT JOIN [Enterprise].[ProductType] ptf ON ptf.ProductTypeId = pts.ParentProductTypeId  
            JOIN Enterprise.Party par ON par.PartyId = o.PartyId  
    WHERE par.RealPageId = @OrganizationRealPageId
	AND ((@NOW BETWEEN op.FromDate AND op.ThruDate) OR (@NOW >= op.FromDate AND op.ThruDate IS NULL)) 
END;
GO
PRINT N'Creating [Ident].[SamlAttribute]'
GO
CREATE TABLE [Ident].[SamlAttribute]
(
[SamlAttributeId] [int] NOT NULL IDENTITY(1, 1),
[Name] [nvarchar] (50) NOT NULL,
[SamlAttributeTypeId] [int] NOT NULL
)
GO
PRINT N'Creating primary key [PK_SamlAttribute] on [Ident].[SamlAttribute]'
GO
ALTER TABLE [Ident].[SamlAttribute] ADD CONSTRAINT [PK_SamlAttribute] PRIMARY KEY CLUSTERED  ([SamlAttributeId])
GO
PRINT N'Creating [Ident].[CreateSamlAttribute]'
GO
CREATE PROCEDURE [Ident].[CreateSamlAttribute]
    @AttributeName NVARCHAR(100) ,
    @SamlAttributeTypeId INT
AS
    BEGIN
        BEGIN TRY
            BEGIN TRANSACTION;

            INSERT INTO Ident.SamlAttribute (   Name ,
                                                SamlAttributeTypeId
                                            )
            OUTPUT Inserted.SamlAttributeId AS Id ,
                   '' AS ErrorMessage
            VALUES ( @AttributeName, @SamlAttributeTypeId );
            COMMIT;
        END TRY
        BEGIN CATCH
            ROLLBACK;

            DECLARE @ErrorLogID INT;
            EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

            SELECT 0 AS Id ,
                   ErrorMessage
            FROM   dbo.ErrorLog
            WHERE  ErrorLogID = @ErrorLogID;
        END CATCH;
    END;

	GRANT EXECUTE ON  [Ident].[CreateSamlUserAttribute] TO [identityserver]
GO
PRINT N'Creating [Auth].[DeveloperClients]'
GO
CREATE TABLE [Auth].[DeveloperClients]
(
[DeveloperClientId] [bigint] NOT NULL IDENTITY(1, 1),
[DeveloperId] [bigint] NOT NULL,
[ClientId] [int] NOT NULL
)
GO
PRINT N'Creating primary key [PK_dbo.DeveloperClients] on [Auth].[DeveloperClients]'
GO
ALTER TABLE [Auth].[DeveloperClients] ADD CONSTRAINT [PK_dbo.DeveloperClients] PRIMARY KEY CLUSTERED  ([DeveloperClientId])
GO
PRINT N'Creating [Auth].[Developer]'
GO
CREATE TABLE [Auth].[Developer]
(
[DeveloperId] [bigint] NOT NULL IDENTITY(1, 1),
[LoginId] [nvarchar] (128) NOT NULL,
[Name] [nvarchar] (200) NOT NULL,
[LogoUri] [nvarchar] (2000) NULL,
[WebsiteUri] [nvarchar] (2000) NULL,
[Phone] [nvarchar] (30) NULL,
[ValidationToken] [nvarchar] (256) NOT NULL,
[PasswordHashed] [nvarchar] (250) NOT NULL,
[PasswordSalt] [nvarchar] (16) NULL,
[VersionId] [timestamp] NOT NULL,
[IsActive] [bit] NOT NULL CONSTRAINT [DF__Developer__IsAct__6166761E] DEFAULT ((1)),
[IsAccountValidated] [bit] NOT NULL CONSTRAINT [DF__Developer__IsAcc__625A9A57] DEFAULT ((0)),
[ValidationTokenExpiry] [datetime] NULL,
[DateCreated] [datetime] NOT NULL CONSTRAINT [DF__Developer__DateC__634EBE90] DEFAULT (getutcdate()),
[DateModified] [datetime] NULL CONSTRAINT [DF__Developer__DateM__6442E2C9] DEFAULT (getutcdate()),
[LastLoginDate] [datetime] NULL CONSTRAINT [DF__Developer__LastL__65370702] DEFAULT (getutcdate())
)
GO
PRINT N'Creating primary key [PK_dbo.Developer] on [Auth].[Developer]'
GO
ALTER TABLE [Auth].[Developer] ADD CONSTRAINT [PK_dbo.Developer] PRIMARY KEY CLUSTERED  ([DeveloperId])
GO
PRINT N'Creating [Auth].[usp_GetClientForDeveloperLogin]'
GO

CREATE PROCEDURE [Auth].[usp_GetClientForDeveloperLogin]
	@loginId NVARCHAR(128)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	SELECT C.* FROM Auth.Developer I
		INNER JOIN Auth.DeveloperClients IC ON IC.DeveloperId = I.DeveloperId
		INNER JOIN Auth.Clients C ON C.ClientId = IC.ClientId
		WHERE I.LoginId = @loginId
END
GO
PRINT N'Creating [Enterprise].[ListProductsByPersonaId]'
GO
CREATE PROCEDURE [Enterprise].[ListProductsByPersonaId] (  
    @PersonaId bigint  
)  
AS  
BEGIN  
	DECLARE @NOW DATETIME = GETUTCDATE();  
  
	SELECT	DISTINCT
			prod.ProductGUID,  
			p.ProductId,  
			prod.[Name] AS ProductName,  
			prod.ProductTypeId,  
			prod.Description AS ProductDescription,
			per.PersonaId,
			per.PersonPartyId,
			par.RealPageId,
			o.PartyId AS OrganizationPartyId,
			o.Name AS OrganizationName
	FROM	Enterprise.PersonaConfiguration p  
			JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = p.ConfigurationId  
			JOIN Enterprise.Product prod ON prod.ProductId = p.ProductId
			INNER JOIN Person.Persona per ON (p.PersonaId = per.PersonaId)
			INNER JOIN Enterprise.Party par ON (per.PersonPartyId = par.PartyId)
			INNER JOIN Enterprise.Organization o ON (per.OrganizationPartyId = o.PartyId)
	WHERE	p.PersonaId = @PersonaId  
	AND		((@NOW BETWEEN p.FromDate AND p.ThruDate) OR (@NOW >= p.FromDate AND p.ThruDate IS NULL))  
	AND		((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))  
END;
GO
PRINT N'Creating [Ident].[SamlAttributeType]'
GO
CREATE TABLE [Ident].[SamlAttributeType]
(
[SamlAttributeTypeId] [int] NOT NULL IDENTITY(1, 1),
[Name] [nvarchar] (100) NOT NULL
)
GO
PRINT N'Creating primary key [PK_SamlAttributeType] on [Ident].[SamlAttributeType]'
GO
ALTER TABLE [Ident].[SamlAttributeType] ADD CONSTRAINT [PK_SamlAttributeType] PRIMARY KEY CLUSTERED  ([SamlAttributeTypeId])
GO
PRINT N'Creating [Ident].[CreateSamlAttributeType]'
GO
CREATE PROCEDURE [Ident].[CreateSamlAttributeType]
    @AttributeTypeName NVARCHAR(100)
AS
    BEGIN
        BEGIN TRY
            BEGIN TRANSACTION;

            INSERT INTO Ident.SamlAttributeType ( [Name] )
            OUTPUT Inserted.SamlAttributeTypeId AS Id ,
                   '' AS ErrorMessage
            VALUES ( @AttributeTypeName );

            COMMIT;
        END TRY
        BEGIN CATCH
            ROLLBACK;

            DECLARE @ErrorLogID INT;
            EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

            SELECT 0 AS Id ,
                   ErrorMessage
            FROM   dbo.ErrorLog
            WHERE  ErrorLogID = @ErrorLogID;
        END CATCH;
    END;
GO
PRINT N'Creating [Auth].[ClientSecrets]'
GO
CREATE TABLE [Auth].[ClientSecrets]
(
[ClientSecretId] [int] NOT NULL IDENTITY(1, 1),
[ClientId] [int] NOT NULL,
[Value] [nvarchar] (250) NOT NULL,
[Type] [nvarchar] (250) NULL,
[Description] [nvarchar] (2000) NULL,
[Expiration] [datetimeoffset] NULL
)
GO
PRINT N'Creating primary key [PK_dbo.ClientSecrets] on [Auth].[ClientSecrets]'
GO
ALTER TABLE [Auth].[ClientSecrets] ADD CONSTRAINT [PK_dbo.ClientSecrets] PRIMARY KEY CLUSTERED  ([ClientSecretId])
GO
PRINT N'Creating [Auth].[GetClientSecretsByClientId]'
GO
CREATE PROCEDURE [Auth].[GetClientSecretsByClientId]
	@ClientId		int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT * FROM Auth.ClientSecrets WHERE ClientId = @ClientId

END
GO
PRINT N'Creating [Enterprise].[ListProductSettingsByOrganization]'
GO
CREATE PROCEDURE [Enterprise].[ListProductSettingsByOrganization] (
	@OrganizationRealPageId UNIQUEIDENTIFIER
)

AS 

BEGIN
	
	DECLARE @NOW  DATETIME = GETUTCDATE();
       
	SELECT	ps.ProductId,
			ps.ProductSettingId, 
			ps.ProductSettingTypeId, 
			pst.[Name], 
			ps.[Value], 
			pst.[Description]
	FROM	[Enterprise].[OrganizationProduct] op 
			JOIN [Enterprise].[ProductConfiguration] pc ON pc.ConfigurationId = op.ConfigurationId
			JOIN [Enterprise].[ProductSetting] ps ON ps.ProductSettingId = pc.ProductSettingId
			JOIN [Enterprise].[ProductSettingType] pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId     
			JOIN [Enterprise].[Party] par ON par.PartyId = op.PartyId
	WHERE	par.RealPageId = @OrganizationRealPageId
	AND ((@NOW BETWEEN op.FromDate AND op.ThruDate) OR (@NOW >= op.FromDate AND op.ThruDate IS NULL))
	AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))

END
GO
PRINT N'Creating [Auth].[ProviderPortfolio]'
GO
CREATE TABLE [Auth].[ProviderPortfolio]
(
[ProviderPortfolioId] [int] NOT NULL IDENTITY(1, 1),
[PortfolioIdId] [int] NOT NULL,
[ProviderName] [nvarchar] (50) NOT NULL,
[Description] [nvarchar] (100) NULL,
[AuthenticationType] [nvarchar] (20) NOT NULL,
[Caption] [nvarchar] (50) NOT NULL,
[ProviderClientId] [nvarchar] (1000) NOT NULL,
[AuthorityUri] [nvarchar] (100) NOT NULL,
[PostLogoutRedirectUri] [nvarchar] (100) NOT NULL,
[RedirectUri] [nvarchar] (100) NOT NULL,
[AuthenticationMode] [tinyint] NOT NULL CONSTRAINT [DF_ProviderProperty_AuthenticationMode] DEFAULT ((0)),
[ValidateIssuer] [bit] NOT NULL CONSTRAINT [DF_ProviderProperty_ValidateIssuer] DEFAULT ((0)),
[TokenValidationAuthenticationType] [nvarchar] (20) NOT NULL,
[Scope] [nvarchar] (500) NULL,
[OktaEntityId] [nvarchar] (100) NULL,
[OktaMetadataLocation] [nvarchar] (1000) NULL,
[ClientSecret] [nvarchar] (1000) NULL
)
GO
PRINT N'Creating primary key [PK_ProviderProperty] on [Auth].[ProviderPortfolio]'
GO
ALTER TABLE [Auth].[ProviderPortfolio] ADD CONSTRAINT [PK_ProviderProperty] PRIMARY KEY CLUSTERED  ([ProviderPortfolioId])
GO
PRINT N'Creating [Auth].[GetProviderConfigurationByProvider]'
GO
CREATE PROCEDURE [Auth].[GetProviderConfigurationByProvider]
	@providerName as nvarchar(50)
	AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT * FROM Auth.ProviderPortfolio where providername=@providerName

END
GO
PRINT N'Creating [Enterprise].[ListProductSettingsByPersona]'
GO
CREATE PROCEDURE [Enterprise].[ListProductSettingsByPersona]
(
	@PersonaId bigint   
)

AS

BEGIN

	DECLARE @NOW DATETIME = GETUTCDATE();

    SELECT	ps.ProductId,
			ps.ProductSettingId, 
			ps.ProductSettingTypeId, 
			pst.[Name], 
			ps.[Value], 
			pst.[Description],
			pc.ConfigurationId
	FROM	[Enterprise].[PersonaConfiguration] perc 
			JOIN [Enterprise].[ProductConfiguration] pc ON pc.ConfigurationId = perc.ConfigurationId
			JOIN [Enterprise].[ProductSetting] ps ON ps.ProductSettingId = pc.ProductSettingId
			JOIN [Enterprise].[ProductSettingType] pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId                  
	WHERE	perc.PersonaId = @PersonaId
			AND ((@NOW BETWEEN perc.FromDate AND perc.ThruDate) OR (@NOW >= perc.FromDate AND perc.ThruDate IS NULL))
			AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))

END;
GO
PRINT N'Creating [Auth].[ClientScopes]'
GO
CREATE TABLE [Auth].[ClientScopes]
(
[ClientScopeId] [int] NOT NULL IDENTITY(1, 1),
[ClientId] [int] NOT NULL,
[Scope] [nvarchar] (200) NOT NULL
)
GO
PRINT N'Creating primary key [PK_dbo.ClientScopes] on [Auth].[ClientScopes]'
GO
ALTER TABLE [Auth].[ClientScopes] ADD CONSTRAINT [PK_dbo.ClientScopes] PRIMARY KEY CLUSTERED  ([ClientScopeId])
GO
PRINT N'Creating [Auth].[ClientRedirectUris]'
GO
CREATE TABLE [Auth].[ClientRedirectUris]
(
[ClientRedirectUriId] [int] NOT NULL IDENTITY(1, 1),
[ClientId] [int] NOT NULL,
[Uri] [nvarchar] (2000) NOT NULL
)
GO
PRINT N'Creating primary key [PK_dbo.ClientRedirectUris] on [Auth].[ClientRedirectUris]'
GO
ALTER TABLE [Auth].[ClientRedirectUris] ADD CONSTRAINT [PK_dbo.ClientRedirectUris] PRIMARY KEY CLUSTERED  ([ClientRedirectUriId])
GO
PRINT N'Creating [Auth].[ClientPostLogoutRedirectUris]'
GO
CREATE TABLE [Auth].[ClientPostLogoutRedirectUris]
(
[ClientPostLogoutRedirectUriId] [int] NOT NULL IDENTITY(1, 1),
[ClientId] [int] NOT NULL,
[Uri] [nvarchar] (2000) NOT NULL
)
GO
PRINT N'Creating primary key [PK_dbo.ClientPostLogoutRedirectUris] on [Auth].[ClientPostLogoutRedirectUris]'
GO
ALTER TABLE [Auth].[ClientPostLogoutRedirectUris] ADD CONSTRAINT [PK_dbo.ClientPostLogoutRedirectUris] PRIMARY KEY CLUSTERED  ([ClientPostLogoutRedirectUriId])
GO
PRINT N'Creating [Auth].[GetClientDetails]'
GO
CREATE PROCEDURE [Auth].[GetClientDetails]
	@ClientId		int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT ClientRedirectUriId, ClientId, Uri FROM Auth.ClientRedirectUris WHERE ClientId = @ClientId
	SELECT ClientScopeId,ClientId,Scope FROM Auth.ClientScopes WHERE ClientId = @ClientId
	SELECT ClientSecretId,ClientId,Value,Type,Description,Expiration FROM Auth.ClientSecrets WHERE ClientId = @ClientId
	SELECT ClientPostLogoutRedirectUriId,ClientId,Uri FROM Auth.ClientPostLogoutRedirectUris WHERE ClientId = @ClientId
END
GO
PRINT N'Creating [Enterprise].[ListProductSettingsByPersonaId]'
GO
CREATE PROCEDURE [Enterprise].[ListProductSettingsByPersonaId]
	@PersonaId int
AS
    BEGIN

		DECLARE @NOW  DATETIME = GETUTCDATE();

        SELECT p.ProductId, ps.ProductSettingId, pst.Name, ps.Value
		FROM Enterprise.PersonaConfiguration p
		LEFT JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = p.ConfigurationId
		LEFT JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId
		LEFT JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
		LEFT JOIN Enterprise.Product prod ON prod.ProductId = p.ProductId
		WHERE p.PersonaId = @PersonaId
		AND ((@NOW BETWEEN p.FromDate AND p.ThruDate) OR (@NOW >= p.FromDate AND p.ThruDate IS NULL))
		AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))
    END;
GO
PRINT N'Creating [Auth].[UpdateTokenExpiry]'
GO
CREATE PROCEDURE [Auth].[UpdateTokenExpiry]
	@TokenKey		NVARCHAR (128)     ,
	@Expiry			DATETIMEOFFSET (7)
AS
BEGIN

	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	Update [Auth].[Tokens] set [Expiry] = @Expiry where [TokenKey]=@TokenKey

END
GO
PRINT N'Creating [Enterprise].[ListProductSettingType]'
GO
CREATE PROCEDURE [Enterprise].[ListProductSettingType]
AS
BEGIN  
	SELECT	ProductSettingTypeId,
			Name,
			Description
	FROM	Enterprise.ProductSettingType
END;
GO
PRINT N'Creating [Auth].[InsertToken]'
GO
CREATE PROCEDURE [Auth].[InsertToken]
		@TokenKey		NVARCHAR (128)     ,
		@TokenType		INT     ,
		@ClientCode		NVARCHAR (200) ,
		@SubjectCode	NVARCHAR (200),
		@Expiry			DATETIMEOFFSET (7),
		@JsonCode		NVARCHAR (3072),
		@AuthCodeChallenge	NVARCHAR (250)    ,
		@AuthCodeChallengeMethod	NVARCHAR (50)     , 
		@IsOpenId		BIT,
		@Nonce			NVARCHAR (200),     
		@RedirectUri	NVARCHAR (2000)   ,
		@SessionId		NVARCHAR (200) ,
		@WasConsentShown BIT 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	INSERT INTO [Auth].[Tokens]
           ([TokenKey]
           ,[TokenType]
           ,[ClientCode]
           ,[SubjectCode]
           ,[Expiry]
           ,[JsonCode]
           ,[AuthCodeChallenge]
           ,[AuthCodeChallengeMethod]
           ,[IsOpenId]
           ,[Nonce]
           ,[RedirectUri]
           ,[SessionId]
           ,[WasConsentShown])
     VALUES
           (@TokenKey
           ,@TokenType
           ,@ClientCode
           ,@SubjectCode
           ,@Expiry
           ,@JsonCode
           ,@AuthCodeChallenge
           ,@AuthCodeChallengeMethod
           ,@IsOpenId
           ,@Nonce
           ,@RedirectUri
           ,@SessionId
           ,@WasConsentShown)
END
GO
PRINT N'Creating [Enterprise].[ListProductTypes]'
GO
CREATE PROCEDURE [Enterprise].[ListProductTypes] (
    @ParentProductTypeName NVARCHAR(50) = NULL
)  
AS  
BEGIN  
	SELECT	c.ProductTypeId,
			c.ParentProductTypeId,
			c.Name,
			c.Description,
			c.ProductTypeGuid,
			p.Name AS ParentProductTypeName
	FROM	Enterprise.ProductType c  
			LEFT JOIN Enterprise.ProductType p ON c.ParentProductTypeId = p.ProductTypeId  
	WHERE	(c.Name = @ParentProductTypeName OR @ParentProductTypeName IS NULL);     
END
GO
PRINT N'Creating [Auth].[Product]'
GO
CREATE TABLE [Auth].[Product]
(
[ProductId] [int] NOT NULL IDENTITY(1, 1),
[ProductName] [nvarchar] (50) NOT NULL,
[Description] [nvarchar] (100) NULL,
[ClientID] [int] NULL,
[ClassName] [nvarchar] (50) NULL,
[SettingsUrl] [nvarchar] (500) NULL,
[ProductUrl] [nvarchar] (1000) NULL,
[SubDescription] [nvarchar] (500) NULL,
[TitleId] [nvarchar] (25) NULL,
[TitleUniqueId] [uniqueidentifier] NULL,
[IsNewTab] [bit] NULL CONSTRAINT [DF_Product_IsNewTab] DEFAULT ((0)),
[MetatagUniqueId] [nvarchar] (100) NULL
)
GO
PRINT N'Creating primary key [PK_Product] on [Auth].[Product]'
GO
ALTER TABLE [Auth].[Product] ADD CONSTRAINT [PK_Product] PRIMARY KEY CLUSTERED  ([ProductId])
GO
PRINT N'Creating [Auth].[PortfolioProductUserClaims]'
GO
CREATE TABLE [Auth].[PortfolioProductUserClaims]
(
[PortfolioProductUserClaimsId] [int] NOT NULL IDENTITY(1, 1),
[PortfolioProductUserId] [int] NOT NULL,
[Type] [nvarchar] (250) NOT NULL,
[Value] [nvarchar] (250) NOT NULL
)
GO
PRINT N'Creating primary key [PK_PortfolioProductUserClaims] on [Auth].[PortfolioProductUserClaims]'
GO
ALTER TABLE [Auth].[PortfolioProductUserClaims] ADD CONSTRAINT [PK_PortfolioProductUserClaims] PRIMARY KEY CLUSTERED  ([PortfolioProductUserClaimsId])
GO
PRINT N'Creating index [IX_PortfolioProductUserClaims_PortfolioIDProductIDUserID] on [Auth].[PortfolioProductUserClaims]'
GO
CREATE NONCLUSTERED INDEX [IX_PortfolioProductUserClaims_PortfolioIDProductIDUserID] ON [Auth].[PortfolioProductUserClaims] ([PortfolioProductUserId])
GO
PRINT N'Creating [Auth].[PortfolioProductUser]'
GO
CREATE TABLE [Auth].[PortfolioProductUser]
(
[PortfolioProductUserId] [int] NOT NULL IDENTITY(1, 1),
[PortfolioId] [int] NOT NULL,
[ProductId] [int] NOT NULL,
[UserId] [bigint] NOT NULL,
[Title] [nvarchar] (50) NULL
)
GO
PRINT N'Creating primary key [PK_PortfolioProductUser] on [Auth].[PortfolioProductUser]'
GO
ALTER TABLE [Auth].[PortfolioProductUser] ADD CONSTRAINT [PK_PortfolioProductUser] PRIMARY KEY CLUSTERED  ([PortfolioProductUserId])
GO
PRINT N'Creating index [IX_PortfolioProductUser_PortfolioIdProductIdUserId] on [Auth].[PortfolioProductUser]'
GO
CREATE NONCLUSTERED INDEX [IX_PortfolioProductUser_PortfolioIdProductIdUserId] ON [Auth].[PortfolioProductUser] ([PortfolioId], [ProductId], [UserId])
GO
PRINT N'Creating index [IX_PortfolioProductUser_PortfolioIdUserId] on [Auth].[PortfolioProductUser]'
GO
CREATE NONCLUSTERED INDEX [IX_PortfolioProductUser_PortfolioIdUserId] ON [Auth].[PortfolioProductUser] ([PortfolioId], [UserId])
GO
PRINT N'Creating [Auth].[PortfolioProduct]'
GO
CREATE TABLE [Auth].[PortfolioProduct]
(
[PortfolioProductId] [int] NOT NULL IDENTITY(1, 1),
[PortfolioId] [int] NOT NULL,
[ProductId] [int] NOT NULL
)
GO
PRINT N'Creating primary key [PK_PortfolioProduct] on [Auth].[PortfolioProduct]'
GO
ALTER TABLE [Auth].[PortfolioProduct] ADD CONSTRAINT [PK_PortfolioProduct] PRIMARY KEY CLUSTERED  ([PortfolioProductId])
GO
PRINT N'Creating index [IX_PortfolioProduct_PortfolioId_ProductId] on [Auth].[PortfolioProduct]'
GO
CREATE NONCLUSTERED INDEX [IX_PortfolioProduct_PortfolioId_ProductId] ON [Auth].[PortfolioProduct] ([PortfolioId], [ProductId])
GO
PRINT N'Creating [Auth].[Portfolio]'
GO
CREATE TABLE [Auth].[Portfolio]
(
[PortfolioId] [int] NOT NULL IDENTITY(1, 1),
[PortfolioName] [nvarchar] (100) NOT NULL
)
GO
PRINT N'Creating primary key [PK_Property] on [Auth].[Portfolio]'
GO
ALTER TABLE [Auth].[Portfolio] ADD CONSTRAINT [PK_Property] PRIMARY KEY CLUSTERED  ([PortfolioId])
GO
PRINT N'Creating [Auth].[GetAllPortfolioProductUserClaims]'
GO
CREATE PROCEDURE [Auth].[GetAllPortfolioProductUserClaims]
	@PortfolioId  int,
    @ClientCode NVARCHAR (200),
    @UserId   bigint
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT 
		PPUC.PortfolioProductUserClaimsID as Id
		,P.PortfolioId as PortfolioId
		,C.ClientCode as ClientId
		,PPU.UserID as UserId
		,PPUC.Type as [Type]
		,PPUC.Value as [Value]
	FROM 
		[Auth].[PortfolioProductUser] PPU WITH(NOLOCK)
		INNER JOIN [Auth].[PortfolioProductUserClaims] PPUC WITH(NOLOCK) ON PPU.PortfolioProductUserID = PPUC.PortfolioProductUserID
		INNER JOIN [Auth].[Portfolio] P WITH(NOLOCK) ON P.PortfolioId = PPU.PortfolioID
		INNER JOIN [Auth].[PortfolioProduct] PP WITH(NOLOCK) ON PPU.PortfolioId = PP.PortfolioID
		INNER JOIN [Auth].[Product] PR WITH(NOLOCK) ON PP.ProductId = Pr.ProductId
		INNER JOIN [Auth].[Clients] C WITH(NOLOCK) ON PR.ClientId = C.ClientId
	WHERE
		PPU.PortfolioId = @PortfolioId
		AND
		C.ClientCode = @ClientCode
		AND
		PPU.UserId = @UserId

END
GO
PRINT N'Creating [Enterprise].[MapBlueBookIdtoPartyId]'
GO
CREATE PROCEDURE [Enterprise].[MapBlueBookIdtoPartyId]
    @SourceId NVARCHAR(50) ,
    @PartyId INT
AS
    BEGIN
        INSERT INTO Enterprise.DataImportMapping (   DataImportApplicationId ,
                                                     SourceId ,
                                                     PartyId ,
                                                     DateCreated
                                                 )
		OUTPUT Inserted.DataImportMappingId AS Id, '' AS ErrorMessage
        VALUES ( 1, @SourceId, @PartyId, GETUTCDATE());
    END;
GO
PRINT N'Creating [Auth].[GetUserByLoginId]'
GO
CREATE PROCEDURE [Auth].[GetUserByLoginId] @LoginId NVARCHAR(50)
AS
    BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
        SET NOCOUNT ON;

        SELECT  UserId ,
                LoginId ,
                Firstname ,
                LastName ,
                IsActive ,
                PasswordHash ,
				PasswordSalt ,
                IdentityProvider ,
                Title ,
                Email ,
                Phone ,
                IsLocked,
				LastPasswordModifiedDateTime,
				AccountExpiration
        FROM    Auth.Users
        WHERE   LoginId = @LoginId;

    END;
GO
PRINT N'Creating [Enterprise].[UpdateProductBatch]'
GO
CREATE PROCEDURE [Enterprise].[UpdateProductBatch](
	 @ProductBatchID int
	,@StatusTypeId int
	,@InputJson nvarchar(max)
	,@ErrorDetails varchar(max) = NULL
	) 

AS

BEGIN

	SET NOCOUNT ON;
	DECLARE @NOW datetime = GETUTCDATE();
	DECLARE @RetryCount tinyint;

	IF @StatusTypeId = 7 --Error
	BEGIN
		SELECT @RetryCount = RetryCount + 1
		FROM ProductBatch 
		WHERE ProductBatchId = @ProductBatchID
	END
	ELSE
	BEGIN
		SELECT @RetryCount = RetryCount 
		FROM ProductBatch 
		WHERE ProductBatchId = @ProductBatchID
	END;

	 BEGIN TRY
            BEGIN TRAN;
				UPDATE [Enterprise].[ProductBatch] SET
					    [StatusTypeId] = @StatusTypeId
					   ,[RetryCount] = @RetryCount 
					   ,[InputJson] = COALESCE(@InputJson,[InputJson])
					   ,[LastRunDate] = CASE WHEN @StatusTypeId = 6 THEN  @NOW ELSE [LastRunDate] END--Running
					   ,[ModifiedDate] = @NOW
					   ,[ErrorDetails] = COALESCE(@ErrorDetails,[ErrorDetails])
				WHERE	
					ProductBatchId = @ProductBatchID

				SELECT @ProductBatchID as Id, '' AS ErrorMessage
			COMMIT;
        END TRY
        BEGIN CATCH
            DECLARE @ErrorLogID INT;
            EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

            SELECT 0 AS Id ,
                   ErrorMessage
            FROM   dbo.ErrorLog
            WHERE  ErrorLogID = @ErrorLogID;

            ROLLBACK;
        END CATCH;
END;
GO
PRINT N'Creating [Auth].[GetUserById]'
GO
CREATE PROCEDURE [Auth].[GetUserById]
	@UserId	bigint
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT * FROM Auth.Users WHERE UserId = @UserId

END
GO
PRINT N'Creating [Ident].[CreateSamlUserAttribute]'
GO
CREATE PROCEDURE [Ident].[CreateSamlUserAttribute] (
    @PersonaId BIGINT,
    @ProductId INT,
    @SamlAttributeId INT,
    @Value NVARCHAR(500)
)
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO Ident.SamlUserAttribute
		(
			PersonaId,
			ProductId,
			SamlAttributeId,
			Value
        )
        OUTPUT	Inserted.SamlUserAttributeId AS Id,
                '' AS ErrorMessage
        VALUES
		(
			@PersonaId,
			@ProductId,
			@SamlAttributeId,
			@Value
		);
        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT	0 AS Id ,
                ErrorMessage
        FROM	dbo.ErrorLog
        WHERE	ErrorLogID = @ErrorLogID;
    END CATCH;
END;
GO
PRINT N'Creating [Auth].[ScopeSecrets]'
GO
CREATE TABLE [Auth].[ScopeSecrets]
(
[ScopeSecretId] [int] NOT NULL IDENTITY(1, 1),
[ScopeId] [int] NOT NULL,
[Description] [nvarchar] (1000) NULL,
[Type] [nvarchar] (250) NULL,
[Value] [nvarchar] (250) NOT NULL,
[Expiration] [datetimeoffset] NULL
)
GO
PRINT N'Creating primary key [PK_dbo.ScopeSecrets] on [Auth].[ScopeSecrets]'
GO
ALTER TABLE [Auth].[ScopeSecrets] ADD CONSTRAINT [PK_dbo.ScopeSecrets] PRIMARY KEY CLUSTERED  ([ScopeSecretId])
GO
PRINT N'Creating [Auth].[GetAllScopeSecrets]'
GO
CREATE PROCEDURE [Auth].[GetAllScopeSecrets]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT * FROM Auth.ScopeSecrets

END
GO
PRINT N'Creating [Ident].[UserLogin]'
GO
CREATE TABLE [Ident].[UserLogin]
(
[UserId] [bigint] NOT NULL IDENTITY(1, 1),
[PartyId] [bigint] NOT NULL,
[LoginName] [varchar] (255) NOT NULL,
[PasswordHash] [nvarchar] (255) NULL,
[PasswordSalt] [nvarchar] (255) NULL,
[PasswordModifiedDate] [smalldatetime] NULL,
[LastLoginDate] [datetime] NULL,
[FromDate] [datetime] NOT NULL CONSTRAINT [DF__UserLogin__FromD__0F2D40CE] DEFAULT (getutcdate()),
[ThruDate] [datetime] NULL
)
GO
PRINT N'Creating primary key [PK_UserLogin] on [Ident].[UserLogin]'
GO
ALTER TABLE [Ident].[UserLogin] ADD CONSTRAINT [PK_UserLogin] PRIMARY KEY CLUSTERED  ([UserId])
GO
PRINT N'Adding constraints to [Ident].[UserLogin]'
GO
ALTER TABLE [Ident].[UserLogin] ADD CONSTRAINT [AK_UserLogin_LoginId] UNIQUE NONCLUSTERED  ([LoginName])
GO
PRINT N'Creating [Ident].[UserCurrentStatus]'
GO
CREATE TABLE [Ident].[UserCurrentStatus]
(
[UserCurrentStatusId] [bigint] NOT NULL IDENTITY(1, 1),
[UserId] [bigint] NOT NULL,
[StatusTypeId] [int] NOT NULL,
[StatusSetDate] [datetime] NOT NULL CONSTRAINT [DF_UserCurrentStatus_StatusDateTime] DEFAULT (getutcdate()),
[FromDate] [datetime] NOT NULL CONSTRAINT [DF__UserStatu__FromD__12E8C319] DEFAULT (getutcdate()),
[ThruDate] [datetime] NULL
)
GO
PRINT N'Creating primary key [PK_UserStatus] on [Ident].[UserCurrentStatus]'
GO
ALTER TABLE [Ident].[UserCurrentStatus] ADD CONSTRAINT [PK_UserStatus] PRIMARY KEY CLUSTERED  ([UserCurrentStatusId])
GO
PRINT N'Creating [Enterprise].[StatusType]'
GO
CREATE TABLE [Enterprise].[StatusType]
(
[StatusTypeId] [int] NOT NULL,
[Name] [varchar] (50) NOT NULL
)
GO
PRINT N'Creating primary key [PK_StatusType] on [Enterprise].[StatusType]'
GO
ALTER TABLE [Enterprise].[StatusType] ADD CONSTRAINT [PK_StatusType] PRIMARY KEY CLUSTERED  ([StatusTypeId])
GO
PRINT N'Creating [Ident].[GetUserCurrentStatuses]'
GO
CREATE PROCEDURE [Ident].[GetUserCurrentStatuses] (
    @RealPageId UNIQUEIDENTIFIER = NULL
)
AS
BEGIN
	SELECT ul.UserId, st.StatusTypeId, st.Name, us.StatusSetDate, us.FromDate, us.ThruDate
	FROM Ident.UserLogin AS ul INNER JOIN
	 Enterprise.Party AS p ON p.PartyId = ul.PartyId INNER JOIN
	 Ident.UserCurrentStatus AS us ON us.UserId = ul.UserId INNER JOIN
	 Enterprise.StatusType AS st ON st.StatusTypeId = us.StatusTypeId
	WHERE (p.RealPageId = @RealPageId OR @RealPageId IS NULL) and us.ThruDate is not null --and us.ThruDate >= GETUTCDATE()
END
GO
PRINT N'Creating [Auth].[ScopeClaims]'
GO
CREATE TABLE [Auth].[ScopeClaims]
(
[ScopeClaimId] [int] NOT NULL IDENTITY(1, 1),
[ScopeId] [int] NOT NULL,
[Name] [nvarchar] (200) NOT NULL,
[Description] [nvarchar] (1000) NULL,
[AlwaysIncludeInIdToken] [bit] NOT NULL
)
GO
PRINT N'Creating primary key [PK_dbo.ScopeClaims] on [Auth].[ScopeClaims]'
GO
ALTER TABLE [Auth].[ScopeClaims] ADD CONSTRAINT [PK_dbo.ScopeClaims] PRIMARY KEY CLUSTERED  ([ScopeClaimId])
GO
PRINT N'Creating [Auth].[GetAllScopeClaims]'
GO
CREATE PROCEDURE [Auth].[GetAllScopeClaims]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT * FROM Auth.ScopeClaims

END
GO
PRINT N'Creating [Ident].[GetUserStatuses]'
GO
CREATE PROCEDURE [Ident].[GetUserStatuses] (
    @RealPageId UNIQUEIDENTIFIER
)
AS
BEGIN
	SELECT st.StatusTypeId, st.Name, us.StatusSetDate, us.FromDate, us.ThruDate
	FROM Ident.UserLogin AS ul INNER JOIN
	 Enterprise.Party AS p ON p.PartyId = ul.PartyId INNER JOIN
	 Ident.UserCurrentStatus AS us ON us.UserId = ul.UserId INNER JOIN
	 Enterprise.StatusType AS st ON st.StatusTypeId = us.StatusTypeId
	WHERE (p.RealPageId = @RealPageId) and us.ThruDate IS NOT NULL
END
GO
PRINT N'Creating [Auth].[Scopes]'
GO
CREATE TABLE [Auth].[Scopes]
(
[ScopeId] [int] NOT NULL IDENTITY(1, 1),
[Name] [nvarchar] (200) NOT NULL,
[DisplayName] [nvarchar] (200) NULL,
[Description] [nvarchar] (1000) NULL,
[ClaimsRule] [nvarchar] (200) NULL,
[Enabled] [bit] NOT NULL,
[Required] [bit] NOT NULL,
[Emphasize] [bit] NOT NULL,
[Type] [int] NOT NULL,
[IncludeAllClaimsForUser] [bit] NOT NULL,
[ShowInDiscoveryDocument] [bit] NOT NULL,
[AllowUnrestrictedIntrospection] [bit] NOT NULL
)
GO
PRINT N'Creating primary key [PK_dbo.Scopes] on [Auth].[Scopes]'
GO
ALTER TABLE [Auth].[Scopes] ADD CONSTRAINT [PK_dbo.Scopes] PRIMARY KEY CLUSTERED  ([ScopeId])
GO
PRINT N'Creating [Auth].[GetAllScopes]'
GO
CREATE PROCEDURE [Auth].[GetAllScopes]
	AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT * FROM Auth.Scopes

END
GO
PRINT N'Creating [Ident].[Activity]'
GO
CREATE TABLE [Ident].[Activity]
(
[ActivityId] [int] NOT NULL,
[ActivityCode] [varchar] (50) NOT NULL,
[Description] [varchar] (100) NULL,
[MaxActivityAttemptCount] [tinyint] NOT NULL,
[ActivityTokenExpirationMinutes] [int] NOT NULL
)
GO
PRINT N'Creating primary key [PK_Activity_1] on [Ident].[Activity]'
GO
ALTER TABLE [Ident].[Activity] ADD CONSTRAINT [PK_Activity_1] PRIMARY KEY CLUSTERED  ([ActivityId])
GO
PRINT N'Creating [Ident].[ListActivity]'
GO
 
CREATE PROCEDURE [Ident].[ListActivity]
AS
BEGIN
	 
	SET NOCOUNT ON;

    SELECT   [ActivityId] ,[ActivityCode] ,[Description]
      ,[MaxActivityAttemptCount] ,[ActivityTokenExpirationMinutes]
	FROM [Ident].[Activity]
END
GO
PRINT N'Creating [Auth].[Consents]'
GO
CREATE TABLE [Auth].[Consents]
(
[SubjectCode] [nvarchar] (200) NOT NULL,
[ClientCode] [nvarchar] (200) NOT NULL,
[Scopes] [nvarchar] (2000) NOT NULL
)
GO
PRINT N'Creating primary key [PK_Consents] on [Auth].[Consents]'
GO
ALTER TABLE [Auth].[Consents] ADD CONSTRAINT [PK_Consents] PRIMARY KEY CLUSTERED  ([SubjectCode], [ClientCode])
GO
PRINT N'Creating [Auth].[UpdateConsent]'
GO
CREATE PROCEDURE [Auth].[UpdateConsent]
	@SubjectCode  NVARCHAR (200)  ,
    @ClientCode NVARCHAR (200) ,
    @Scopes   NVARCHAR (2000) 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	update [Auth].[Consents] set Scopes=@Scopes WHERE SubjectCode = @SubjectCode and  ClientCode=@ClientCode

END
GO
PRINT N'Creating [Ident].[UserSecurityAnswer]'
GO
CREATE TABLE [Ident].[UserSecurityAnswer]
(
[UserSecurityAnswerId] [int] NOT NULL IDENTITY(1, 1),
[SecurityQuestionId] [int] NOT NULL,
[UserId] [bigint] NOT NULL,
[Answer] [nvarchar] (50) NOT NULL,
[CreateDateTime] [smalldatetime] NOT NULL CONSTRAINT [DF_UserSecurityAnswer_CreateDateTime] DEFAULT (getutcdate())
)
GO
PRINT N'Creating primary key [PK_UserSecurityAnswer] on [Ident].[UserSecurityAnswer]'
GO
ALTER TABLE [Ident].[UserSecurityAnswer] ADD CONSTRAINT [PK_UserSecurityAnswer] PRIMARY KEY CLUSTERED  ([UserSecurityAnswerId])
GO
PRINT N'Creating [Ident].[SecurityQuestion]'
GO
CREATE TABLE [Ident].[SecurityQuestion]
(
[SecurityQuestionId] [int] NOT NULL,
[Question] [nvarchar] (500) NOT NULL,
[IsActive] [bit] NOT NULL CONSTRAINT [DF_SecurityQuestion_IsActive] DEFAULT ((1))
)
GO
PRINT N'Creating primary key [PK_SecurityQuestion] on [Ident].[SecurityQuestion]'
GO
ALTER TABLE [Ident].[SecurityQuestion] ADD CONSTRAINT [PK_SecurityQuestion] PRIMARY KEY CLUSTERED  ([SecurityQuestionId])
GO
PRINT N'Creating [Ident].[ListUserSecurityQuestionAnswerByRealPageId]'
GO
Create PROCEDURE [Ident].[ListUserSecurityQuestionAnswerByRealPageId]
	@realPageId uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

SELECT Ident.UserLogin.UserId, Ident.UserLogin.PartyId, Ident.UserLogin.LoginName, Ident.UserSecurityAnswer.SecurityQuestionId, Ident.UserSecurityAnswer.Answer, Ident.SecurityQuestion.Question, 
            Ident.UserSecurityAnswer.UserSecurityAnswerId 
FROM Ident.UserLogin INNER JOIN
            Ident.UserSecurityAnswer ON Ident.UserLogin.UserId = Ident.UserSecurityAnswer.UserId INNER JOIN
            Ident.SecurityQuestion ON Ident.UserSecurityAnswer.SecurityQuestionId = Ident.SecurityQuestion.SecurityQuestionId INNER JOIN
            Enterprise.Party ON Ident.UserLogin.PartyId = Enterprise.Party.PartyId
			where  Enterprise.Party.RealPageId=@realPageId

END
GO
PRINT N'Creating [Auth].[InsertConsent]'
GO
CREATE PROCEDURE [Auth].[InsertConsent]
	@SubjectCode  NVARCHAR (200)  ,
    @ClientCode NVARCHAR (200) ,
    @Scopes   NVARCHAR (2000) 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	INSERT INTO [Auth].[Consents] ([SubjectCode],[ClientCode],[Scopes]) VALUES  (@SubjectCode, @ClientCode, @Scopes)

END
GO
PRINT N'Creating [Ident].[RemoveSamlUserAttributeByProductId]'
GO
CREATE PROC [Ident].[RemoveSamlUserAttributeByProductId]
    @PersonaId INT ,
    @ProductId INT
AS
    BEGIN
        DECLARE @ThruDate DATETIME = GETUTCDATE();
        UPDATE SamlUserAttribute
        SET    ThruDate = @ThruDate
        FROM   Ident.SamlUserAttribute
        WHERE  PersonaId = @PersonaId
               AND ProductId = @ProductId
			   AND ThruDate IS NULL;
    END;
GO
PRINT N'Creating [Auth].[DeleteConsentBySubjectAndClient]'
GO
CREATE PROCEDURE [Auth].[DeleteConsentBySubjectAndClient]
	@SubjectCode		NVARCHAR (200),
	@ClientCode			NVARCHAR(200) 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	delete FROM Auth.Consents WHERE SubjectCode = @SubjectCode and ClientCode=@ClientCode

END
GO
PRINT N'Creating [Ident].[UpdateSamlUserAttribute]'
GO
CREATE PROCEDURE [Ident].[UpdateSamlUserAttribute] (
	@SamlUserAttributeId int,
	@Value nvarchar(500)
)
AS
BEGIN
	BEGIN TRY
        BEGIN TRANSACTION;
		UPDATE	Ident.SamlUserAttribute
		SET		Value = @Value
		OUTPUT	Inserted.SamlUserAttributeId AS Id,
				'' AS ErrorMessage
		WHERE	SamlUserAttributeId = @SamlUserAttributeId
        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT	0 AS Id ,
				ErrorMessage
        FROM	dbo.ErrorLog
        WHERE	ErrorLogID = @ErrorLogID;
    END CATCH;
END;
GO
PRINT N'Creating [Auth].[GetConsentsBySubject]'
GO
CREATE PROCEDURE [Auth].[GetConsentsBySubject]
	@SubjectCode		NVARCHAR (200) 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT * FROM Auth.Consents WHERE SubjectCode = @SubjectCode 

END
GO
PRINT N'Creating [Ident].[UpdateUserStatus]'
GO
CREATE PROCEDURE [Ident].[UpdateUserStatus]
    @RealPageId UNIQUEIDENTIFIER ,
    @StatusTypeId int,
	@FromDate DATETIME,
	@ThruDate DATETIME
AS
    BEGIN
	DECLARE @UserId bigint;

		SELECT @UserId=  Ident.UserLogin.UserId FROM Ident.UserLogin INNER JOIN
		Enterprise.Party ON Ident.UserLogin.PartyId = Enterprise.Party.PartyId where Enterprise.Party.RealPageId=@RealPageId
		 
		merge [Ident].[UserCurrentStatus] with(HOLDLOCK) as target
			using (values (@UserId,@StatusTypeId,GETUTCDATE(),@FromDate,@ThruDate))
			as source (UserId,StatusTypeId,StatusSetDate,FromDate,ThruDate)
			on target.UserId = @UserId and target.StatusTypeId=@StatusTypeId
		when matched then
			update set StatusSetDate =  GETUTCDATE (),	FromDate = source.FromDate, ThruDate = source.ThruDate
		when not matched by target then
			insert ( UserId,StatusTypeId,StatusSetDate, FromDate, ThruDate)
			values ( @UserId,@StatusTypeId, GETUTCDATE (),@FromDate,@ThruDate);
			 
    END;
GO
PRINT N'Creating [Auth].[GetConsentBySubjectAndClient]'
GO
CREATE PROCEDURE [Auth].[GetConsentBySubjectAndClient]
	@SubjectCode		NVARCHAR (200),
	@ClientCode			NVARCHAR(200) 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT * FROM Auth.Consents WHERE SubjectCode = @SubjectCode and ClientCode=@ClientCode

END
GO
PRINT N'Creating [Person].[PersonaType]'
GO
CREATE TABLE [Person].[PersonaType]
(
[PersonaTypeId] [int] NOT NULL IDENTITY(1, 1),
[Name] [varchar] (50) NULL
)
GO
PRINT N'Creating primary key [PK_PersonaType] on [Person].[PersonaType]'
GO
ALTER TABLE [Person].[PersonaType] ADD CONSTRAINT [PK_PersonaType] PRIMARY KEY CLUSTERED  ([PersonaTypeId])
GO
PRINT N'Creating [Person].[CreatePersonaType]'
GO
CREATE PROC [Person].[CreatePersonaType] (
    @PersonaName VARCHAR(50) ,
    @PersonaTypeId INT = NULL OUTPUT
)
AS
BEGIN
	BEGIN TRY
		BEGIN TRANSACTION;
		-- Check if it exists
		SELECT @PersonaTypeId = PersonaTypeId
		FROM   Person.PersonaType
		WHERE  Name = @PersonaName;

		IF @PersonaTypeId IS NULL
			BEGIN
				INSERT INTO Person.PersonaType ( Name )
				VALUES ( @PersonaName );

				SELECT @PersonaTypeId = SCOPE_IDENTITY();
			END;

		SELECT @PersonaTypeId AS Id, '' AS ErrorMessage;

		COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT 0 AS Id,
                ErrorMessage
        FROM   dbo.ErrorLog
        WHERE  ErrorLogID = @ErrorLogID;
    END CATCH;
END;
GO
PRINT N'Creating [Auth].[GetTokensBySubject]'
GO
CREATE PROCEDURE [Auth].[GetTokensBySubject]
	@SubjectCode		NVARCHAR (200),
	@TokenType INT 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT * FROM Auth.Tokens WHERE Subjectcode = @SubjectCode and TokenType=@TokenType

END
GO
PRINT N'Creating [Person].[PersonaEnvironmentType]'
GO
CREATE TABLE [Person].[PersonaEnvironmentType]
(
[PersonaEnvironmentTypeID] [int] NOT NULL IDENTITY(1, 1),
[Name] [nvarchar] (50) NOT NULL
)
GO
PRINT N'Creating primary key [PK_PersonaEnvironmentType] on [Person].[PersonaEnvironmentType]'
GO
ALTER TABLE [Person].[PersonaEnvironmentType] ADD CONSTRAINT [PK_PersonaEnvironmentType] PRIMARY KEY CLUSTERED  ([PersonaEnvironmentTypeID])
GO
PRINT N'Creating [Person].[ListPersonaEnvironmentType]'
GO
CREATE PROCEDURE [Person].[ListPersonaEnvironmentType](
	 @Name NVARCHAR(50) = NULL
)

AS

BEGIN
	SELECT 
		 [PersonaEnvironmentTypeID]
		,[Name]
	FROM [Person].[PersonaEnvironmentType]
	WHERE
	(@Name IS NULL OR [Name] = @NAME);
END;
GO
PRINT N'Creating [Auth].[DeleteTokenBySubjectAndClient]'
GO
CREATE PROCEDURE [Auth].[DeleteTokenBySubjectAndClient]
	@SubjectCode		NVARCHAR (200),
	@ClientCode NVARCHAR (200),
	@TokenType INT 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	delete FROM Auth.Tokens WHERE SubjectCode = @SubjectCode and ClientCode=@ClientCode and TokenType=@TokenType

END
GO
PRINT N'Creating [Person].[RemovePersona]'
GO
CREATE PROCEDURE [Person].[RemovePersona] (
    @PersonaId bigint 
)
AS
BEGIN
    BEGIN TRY
		BEGIN TRANSACTION
        UPDATE	Persona
		SET		ThruDate = GETUTCDATE()
		OUTPUT	Inserted.PersonaId AS Id,
				'' AS ErrorMessage
		WHERE	PersonaId = @PersonaId
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT	0 AS Id,
                ErrorMessage
        FROM	dbo.ErrorLog
        WHERE	ErrorLogID = @ErrorLogID;
    END CATCH;
END;
GO
PRINT N'Creating [Auth].[DeleteTokenByKey]'
GO
CREATE PROCEDURE [Auth].[DeleteTokenByKey]
	@TokenKey		NVARCHAR (128),
	@TokenType INT 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	Delete FROM Auth.Tokens WHERE TokenKey = @TokenKey and TokenType=@TokenType

END
GO
PRINT N'Creating [Enterprise].[RelationshipType]'
GO
CREATE TABLE [Enterprise].[RelationshipType]
(
[RelationshipTypeId] [int] NOT NULL IDENTITY(1, 1),
[RoleTypeIdValidFrom] [int] NOT NULL,
[RoleTypeIdValidTo] [int] NOT NULL,
[Name] [varchar] (50) NOT NULL,
[Description] [varchar] (255) NULL
)
GO
PRINT N'Creating primary key [PK_PartyRelationshipType] on [Enterprise].[RelationshipType]'
GO
ALTER TABLE [Enterprise].[RelationshipType] ADD CONSTRAINT [PK_PartyRelationshipType] PRIMARY KEY CLUSTERED  ([RelationshipTypeId])
GO
PRINT N'Creating [Enterprise].[PartyRelationship]'
GO
CREATE TABLE [Enterprise].[PartyRelationship]
(
[PartyRelationshipId] [bigint] NOT NULL IDENTITY(1, 1),
[PartyIdFrom] [bigint] NOT NULL,
[PartyIdTo] [bigint] NOT NULL,
[RoleTypeIdFrom] [int] NOT NULL,
[RoleTypeIdTo] [int] NOT NULL,
[PartyRelationshipTypeId] [int] NOT NULL,
[FromDate] [datetime] NOT NULL CONSTRAINT [DF__PartyRela__FromD__05A3D694] DEFAULT (getutcdate()),
[ThruDate] [datetime] NULL
)
GO
PRINT N'Creating primary key [PK_PartyRelationship] on [Enterprise].[PartyRelationship]'
GO
ALTER TABLE [Enterprise].[PartyRelationship] ADD CONSTRAINT [PK_PartyRelationship] PRIMARY KEY CLUSTERED  ([PartyRelationshipId])
GO
PRINT N'Creating [Person].[UnlinkPersonToOrganization]'
GO
CREATE PROCEDURE [Person].[UnlinkPersonToOrganization] (
	@PersonRealPageId UNIQUEIDENTIFIER,
	@OrganizationRealPageId UNIQUEIDENTIFIER,
	@RoleTypeIdFrom INT,
	@RoleTypeIdTo INT
)
AS
BEGIN
	BEGIN TRY
        DECLARE @PartyIdFrom BIGINT;
        DECLARE @PartyIdTo BIGINT;
		DECLARE @PartyRelationshipTypeId INT;

		SELECT  @PartyRelationshipTypeId = [RelationshipTypeId]
		FROM    Enterprise.[RelationshipType]
		WHERE   RoleTypeIdValidFrom = @RoleTypeIdFrom
		AND		RoleTypeIdValidTo = @RoleTypeIdTo;

		-- Get Party ID's
        SELECT  @PartyIdFrom = p.PartyId
        FROM    Enterprise.Party p
        WHERE   p.RealPageId = @PersonRealPageId;
		print @PartyIdFrom
        SELECT  @PartyIdTo = p.PartyId
        FROM    Enterprise.Party p
        WHERE   p.RealPageId = @OrganizationRealPageId;
		print @PartyIdto
		IF @PartyRelationshipTypeId IS NULL
		BEGIN
			RAISERROR('The Relationship is invalid between Role Type %i and Role Type %i', 16, -1, @RoleTypeIdFrom, @RoleTypeIdTo);
		END;

        BEGIN TRANSACTION; 
		
		UPDATE Enterprise.PartyRelationship
		SET ThruDate = GETUTCDATE()
		OUTPUT	Inserted.PartyRelationshipId AS Id,
					'' AS ErrorMessage
		WHERE PartyIdFrom = @PartyIdFrom AND PartyIdTo = @PartyIdTo 
			  AND RoleTypeIdFrom = @RoleTypeIdFrom
			  AND RoleTypeIdTo = @RoleTypeIdTo
			  AND ThruDate IS NULL

		COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
    END CATCH;
END;
GO
PRINT N'Creating [Person].[UpdatePersona]'
GO
CREATE PROC [Person].[UpdatePersona](
	 @PersonaId bigint
	,@PersonaTypeId INT = NULL
	,@PersonaEnvironmentTypeId INT = 1
    ,@FromDate DATETIME = NULL
	,@ThruDate DATETIME = NULL
)
AS
    BEGIN

		BEGIN TRY
			BEGIN TRANSACTION;

			UPDATE Person.Persona 
			SET PersonaTypeId = ISNULL(@PersonaTypeId, PersonaTypeId),
			    PersonaEnvironmentTypeId = ISNULL(@PersonaEnvironmentTypeId, PersonaEnvironmentTypeId),
				FromDate = ISNULL(@FromDate, FromDate),
				ThruDate = ISNULL(@ThruDate, ThruDate)
			OUTPUT Inserted.PersonaId AS Id , '' AS ErrorMessage
			WHERE PersonaID = @PersonaId

			COMMIT;
        END TRY
        BEGIN CATCH
            ROLLBACK;

            DECLARE @ErrorLogID INT;
            EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

            SELECT 0 AS Id,
                   ErrorMessage
            FROM   dbo.ErrorLog
            WHERE  ErrorLogID = @ErrorLogID;
        END CATCH;

    END;
GO
PRINT N'Creating [Auth].[SecurityQuestion]'
GO
CREATE TABLE [Auth].[SecurityQuestion]
(
[SecurityQuestionId] [int] NOT NULL,
[Question] [nvarchar] (500) NOT NULL,
[IsActive] [bit] NOT NULL CONSTRAINT [DF_SecurityQuestion_IsActive] DEFAULT ((1))
)
GO
PRINT N'Creating primary key [PK_SecurityQuestion] on [Auth].[SecurityQuestion]'
GO
ALTER TABLE [Auth].[SecurityQuestion] ADD CONSTRAINT [PK_SecurityQuestion] PRIMARY KEY CLUSTERED  ([SecurityQuestionId])
GO
PRINT N'Creating [Auth].[GetAllSecurityQuestions]'
GO
CREATE PROCEDURE [Auth].[GetAllSecurityQuestions] (
	@enterpriseUserName as nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	--TODO: Get system + user (custom) security questions
		SELECT [SecurityQuestionId]
			  ,[Question]
			  ,[IsActive]
		  FROM [Auth].[SecurityQuestion] where IsActive=1

END
GO
PRINT N'Creating [Person].[LinkPersonToOrganization]'
GO
CREATE PROCEDURE [Person].[LinkPersonToOrganization] (
	@PersonRealPageId UNIQUEIDENTIFIER,
	@OrganizationRealPageId UNIQUEIDENTIFIER,
	@RoleTypeIdFrom INT,
	@RoleTypeIdTo INT,
	@FromDate datetime = null,
	@ThruDate dateTime = null
)
AS
BEGIN
	BEGIN TRY
        DECLARE @PartyIdFrom BIGINT;
        DECLARE @PartyIdTo BIGINT;
		DECLARE @PartyRelationshipTypeId INT;

		SELECT  @PartyRelationshipTypeId = [RelationshipTypeId]
		FROM    Enterprise.[RelationshipType]
		WHERE   RoleTypeIdValidFrom = @RoleTypeIdFrom
		AND		RoleTypeIdValidTo = @RoleTypeIdTo;

		-- Get Party ID's
        SELECT  @PartyIdFrom = p.PartyId
        FROM    Enterprise.Party p
        WHERE   p.RealPageId = @PersonRealPageId;

        SELECT  @PartyIdTo = p.PartyId
        FROM    Enterprise.Party p
        WHERE   p.RealPageId = @OrganizationRealPageId;

		IF @FromDate IS NULL 
			SET @FromDate = GETUTCDATE()  

		IF @PartyRelationshipTypeId IS NULL
		BEGIN
			RAISERROR('The Relationship is invalid between Role Type %i and Role Type %i', 16, -1, @RoleTypeIdFrom, @RoleTypeIdTo);
		END;

        BEGIN TRANSACTION; 

		INSERT INTO Enterprise.PartyRelationship (
			PartyIdFrom ,
			PartyIdTo ,
			RoleTypeIdFrom ,
			RoleTypeIdTo ,
			PartyRelationshipTypeId ,
			FromDate ,
			ThruDate
		)
		OUTPUT	Inserted.PartyRelationshipId AS Id,
				'' AS ErrorMessage
		VALUES (
			@PartyIdFrom, -- PartyIdFrom - bigint
			@PartyIdTo, -- PartyIdTo - bigint
			@RoleTypeIdFrom, -- RoleTypeIdFrom - int
			@RoleTypeIdTo, -- RoleTypeIdTo - int
			@PartyRelationshipTypeId , -- PartyRelationshipTypeId - int
			@FromDate, -- FromDate - datetime
			@ThruDate  -- ThruDate - datetime
		)

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
    END CATCH;
END;
GO
PRINT N'Creating [Person].[UpdatePersonToOrganization]'
GO
CREATE PROCEDURE [Person].[UpdatePersonToOrganization] (
	@PersonRealPageId UNIQUEIDENTIFIER,
	@OrganizationRealPageId UNIQUEIDENTIFIER,
	@UnlinkRoleTypeIdFrom INT,
	@LinkRoleTypeIdFrom INT,
	@RoleTypeIdTo INT
)
AS
BEGIN
	BEGIN TRY
        DECLARE @PartyIdFrom BIGINT;
        DECLARE @PartyIdTo BIGINT;
		DECLARE @PartyRelationshipTypeId INT;

		SELECT  @PartyRelationshipTypeId = [RelationshipTypeId]
		FROM    Enterprise.[RelationshipType]
		WHERE   RoleTypeIdValidFrom = @UnlinkRoleTypeIdFrom
		AND		RoleTypeIdValidTo = @RoleTypeIdTo;

		-- Get Party ID's
        SELECT  @PartyIdFrom = p.PartyId
        FROM    Enterprise.Party p
        WHERE   p.RealPageId = @PersonRealPageId;
		
        SELECT  @PartyIdTo = p.PartyId
        FROM    Enterprise.Party p
        WHERE   p.RealPageId = @OrganizationRealPageId;
		
		IF @PartyRelationshipTypeId IS NULL
		BEGIN
			RAISERROR('The Relationship is invalid between Role Type %i and Role Type %i', 16, -1, @UnlinkRoleTypeIdFrom, @RoleTypeIdTo);
		END;

        BEGIN TRANSACTION; 
		
		EXEC Person.UnlinkPersonToOrganization @PersonRealPageId,	@OrganizationRealPageId, @UnlinkRoleTypeIdFrom, @RoleTypeIdTo 
		
		EXEC Person.LinkPersonToOrganization @PersonRealPageId,	@OrganizationRealPageId, @LinkRoleTypeIdFrom, @RoleTypeIdTo 

		COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
    END CATCH;
END;
GO
PRINT N'Creating [Auth].[UserSamlAttribute]'
GO
CREATE TABLE [Auth].[UserSamlAttribute]
(
[UserSamlAttributeId] [int] NOT NULL IDENTITY(1, 1),
[PortfolioProductUserId] [int] NOT NULL,
[SamlAttributeId] [int] NOT NULL,
[Value] [nvarchar] (500) NOT NULL
)
GO
PRINT N'Creating primary key [PK_UserSamlAttribute] on [Auth].[UserSamlAttribute]'
GO
ALTER TABLE [Auth].[UserSamlAttribute] ADD CONSTRAINT [PK_UserSamlAttribute] PRIMARY KEY CLUSTERED  ([UserSamlAttributeId])
GO
PRINT N'Creating index [IX_UserSamlAttribute_PortfolioProductUserId] on [Auth].[UserSamlAttribute]'
GO
CREATE NONCLUSTERED INDEX [IX_UserSamlAttribute_PortfolioProductUserId] ON [Auth].[UserSamlAttribute] ([PortfolioProductUserId])
GO
PRINT N'Creating [Auth].[SamlAttributeStatement]'
GO
CREATE TABLE [Auth].[SamlAttributeStatement]
(
[SamlAttributeStatementId] [int] NOT NULL IDENTITY(1, 1),
[ProductId] [int] NOT NULL,
[SamlAttributeId] [int] NOT NULL
)
GO
PRINT N'Creating primary key [PK_SamlAttributeStatement] on [Auth].[SamlAttributeStatement]'
GO
ALTER TABLE [Auth].[SamlAttributeStatement] ADD CONSTRAINT [PK_SamlAttributeStatement] PRIMARY KEY CLUSTERED  ([SamlAttributeStatementId])
GO
PRINT N'Creating index [IX_SamlAttributeStatement_ProductId] on [Auth].[SamlAttributeStatement]'
GO
CREATE NONCLUSTERED INDEX [IX_SamlAttributeStatement_ProductId] ON [Auth].[SamlAttributeStatement] ([ProductId])
GO
PRINT N'Creating [Auth].[SamlAttribute]'
GO
CREATE TABLE [Auth].[SamlAttribute]
(
[SamlAttributeId] [int] NOT NULL IDENTITY(1, 1),
[Name] [nvarchar] (50) NOT NULL,
[Type] [nvarchar] (100) NOT NULL
)
GO
PRINT N'Creating primary key [PK_SamlAttribute] on [Auth].[SamlAttribute]'
GO
ALTER TABLE [Auth].[SamlAttribute] ADD CONSTRAINT [PK_SamlAttribute] PRIMARY KEY CLUSTERED  ([SamlAttributeId])
GO
PRINT N'Creating [Auth].[GetProductSamlDetailsByUserId]'
GO
CREATE PROCEDURE [Auth].[GetProductSamlDetailsByUserId]
	@PortfolioProductUserId		int

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	SELECT        
		SAS.SamlAttributeId
		, SA.Name
		, SA.Type
		, USA.Value
	
	FROM 
		Auth.SamlAttribute SA
		INNER JOIN Auth.UserSamlAttribute USA
			ON SA.SamlAttributeId = USA.SamlAttributeId 
		INNER JOIN Auth.PortfolioProductUser PPU
			ON USA.PortfolioProductUserId = PPU.PortfolioProductUserId
		RIGHT OUTER JOIN Auth.SamlAttributeStatement SAS
			ON SA.SamlAttributeId = SAS.SamlAttributeId
		
	WHERE
		PPU.PortfolioProductUserId = @PortfolioProductUserId

END
GO
PRINT N'Creating [Auth].[UserProviderPortfolio]'
GO
CREATE TABLE [Auth].[UserProviderPortfolio]
(
[UserProviderPortfolioId] [bigint] NOT NULL,
[UserId] [bigint] NOT NULL,
[ProviderPortfolioId] [int] NOT NULL
)
GO
PRINT N'Creating primary key [PK_UserProviderProperty] on [Auth].[UserProviderPortfolio]'
GO
ALTER TABLE [Auth].[UserProviderPortfolio] ADD CONSTRAINT [PK_UserProviderProperty] PRIMARY KEY CLUSTERED  ([UserProviderPortfolioId])
GO
PRINT N'Creating [Auth].[GetUserProductDetailsByUserId]'
GO
CREATE PROCEDURE [Auth].[GetUserProductDetailsByUserId]
	@userId		int 

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	 

SELECT        Auth.Users.UserId, Auth.Users.LoginId, Auth.Users.Firstname, Auth.Users.LastName, Auth.Users.Title, Auth.Users.Email, Auth.Users.Phone, Auth.Portfolio.PortfolioName as companyname
FROM            Auth.Users INNER JOIN
                         Auth.UserProviderPortfolio ON Auth.Users.UserId = Auth.UserProviderPortfolio.UserId INNER JOIN
                         Auth.ProviderPortfolio ON Auth.UserProviderPortfolio.ProviderPortfolioId = Auth.ProviderPortfolio.ProviderPortfolioId INNER JOIN
                         Auth.Portfolio ON Auth.ProviderPortfolio.PortfolioIdId = Auth.Portfolio.PortfolioId
						 and Auth.Users.UserId = @userId 

END
GO
PRINT N'Creating [Auth].[UserSecurityAnswer]'
GO
CREATE TABLE [Auth].[UserSecurityAnswer]
(
[UserSecurityAnswerId] [int] NOT NULL IDENTITY(1, 1),
[SecurityQuestionId] [int] NOT NULL,
[UserId] [bigint] NOT NULL,
[Answer] [nvarchar] (50) NOT NULL,
[CreateDateTime] [smalldatetime] NOT NULL CONSTRAINT [DF_UserSecurityAnswer_CreateDateTime] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_UserSecurityAnswer] on [Auth].[UserSecurityAnswer]'
GO
ALTER TABLE [Auth].[UserSecurityAnswer] ADD CONSTRAINT [PK_UserSecurityAnswer] PRIMARY KEY CLUSTERED  ([UserSecurityAnswerId])
GO
PRINT N'Creating [Auth].[GetUserSecurityQuestionAnswer]'
GO
CREATE PROCEDURE [Auth].[GetUserSecurityQuestionAnswer]
	@EnterpriseUserName nvarchar(50) 
AS
BEGIN
	SET NOCOUNT ON;
	declare @UserId as bigint
	select @UserId=userid from users where LoginId=@EnterpriseUserName

SELECT        Auth.Users.UserId, Auth.Users.LoginId, Auth.UserSecurityAnswer.SecurityQuestionId AS SecurityQuestionId, Auth.UserSecurityAnswer.Answer, Auth.SecurityQuestion.Question, 
                         Auth.UserSecurityAnswer.UserSecurityAnswerId
FROM            Auth.SecurityQuestion INNER JOIN
                         Auth.UserSecurityAnswer ON Auth.SecurityQuestion.SecurityQuestionId = Auth.UserSecurityAnswer.SecurityQuestionId INNER JOIN
                         Auth.Users ON Auth.UserSecurityAnswer.UserId = Auth.Users.UserId
WHERE        (Auth.Users.userId = @UserId)

END
GO
PRINT N'Creating [Auth].[GetAllPortfolioProductUser]'
GO
CREATE PROCEDURE [Auth].[GetAllPortfolioProductUser]
	@PortfolioId  int,
    @UserId   bigint,
	@ProductId int = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	;WITH ProductLogins ( UserId, ProductId, TotalAccounts ) 
	AS ( 
			SELECT 
				UserId
				, ProductId
				, COUNT(1) 
			FROM [Auth].[PortfolioProductUser] WITH(NOLOCK)
			WHERE 
				UserId = @UserId 
			GROUP BY 
			UserId, ProductId
		)

	SELECT 
		PPU.PortfolioProductUserID as PortfolioProductUserID
		,P.PortfolioId as PortfolioId
		,P.PortfolioName as PortfolioName
		,ISNULL(C.ClientCode,'') as ClientId
		,PR.ProductName as ProductName
		,PPU.UserID as UserId
		,PL.TotalAccounts as TotalAccounts
		,PPU.Title as Title
		,PR.ClassName
		,PR.SettingsUrl 
		,PR.ProductUrl	 
		,PR.SubDescription	 
		,PR.TitleId	 
		,PR.TitleUniqueId	 
		,PR.IsNewTab	 
		,PR.MetatagUniqueId	 

	FROM
		[Auth].[PortfolioProductUser] PPU WITH(NOLOCK)
		INNER JOIN [Auth].[Portfolio] P WITH(NOLOCK) ON P.PortfolioId = PPU.PortfolioID
		INNER JOIN [Auth].[PortfolioProduct] PP WITH(NOLOCK) ON P.PortfolioId = PP.PortfolioID AND PPU.ProductId = PP.ProductId
		INNER JOIN [Auth].[Product] PR WITH(NOLOCK) ON PP.ProductId = Pr.ProductId
		LEFT OUTER JOIN [Auth].[Clients] C WITH(NOLOCK) ON PR.ClientId = C.ClientId
		INNER JOIN ProductLogins PL ON PL.ProductId = PPU.ProductId
	WHERE
		P.PortfolioId = @PortfolioId
		AND
		PPU.UserId = @UserId
		AND
		1 = CASE WHEN @ProductId != 0 THEN CASE WHEN PR.ProductId = @ProductId THEN 1 ELSE 0 END ELSE 1 END

END
GO
PRINT N'Creating [Auth].[GetProductSamlDetailsByPortfolioProductUserId]'
GO
CREATE PROCEDURE [Auth].[GetProductSamlDetailsByPortfolioProductUserId]
	@PortfolioProductUserId		int

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	SELECT        
		SAS.SamlAttributeId
		, SA.Name
		, SA.Type
		, USA.Value
	
	FROM 
		Auth.PortfolioProductUser PPU
		INNER JOIN Auth.UserSamlAttribute USA
			ON PPU.PortfolioProductUserId = USA.PortfolioProductUserId
		INNER JOIN Auth.SamlAttribute SA
				ON SA.SamlAttributeId = USA.SamlAttributeId 
		RIGHT OUTER JOIN Auth.SamlAttributeStatement SAS
			ON SA.SamlAttributeId = SAS.SamlAttributeId AND SAS.ProductId = PPU.ProductId

		
	WHERE
		PPU.PortfolioProductUserId = @PortfolioProductUserId

END
GO
PRINT N'Creating [Auth].[GetClientRedirectUrisByClientId]'
GO
CREATE PROCEDURE [Auth].[GetClientRedirectUrisByClientId]
	@ClientId		int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
		SET NOCOUNT ON;

		SELECT * FROM Auth.ClientRedirectUris WHERE ClientId = @ClientId
END
GO
PRINT N'Creating [Auth].[GetClientScopesByClientId]'
GO
CREATE PROCEDURE [Auth].[GetClientScopesByClientId]
	@ClientId		int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT * FROM Auth.ClientScopes WHERE ClientId = @ClientId

END
GO
PRINT N'Creating [Auth].[GetPasswordPolicy]'
GO
CREATE PROCEDURE [Auth].[GetPasswordPolicy] (
	@PortfolioId int = NULL
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Insert statements for procedure here
	SELECT	pp.[PasswordPolicyId],
			pp.[PortfolioId],
			p.[PortfolioName],
			pp.[MinimumLength],
			pp.[MaximumLength],
			pp.[MinimumLowercase],
			pp.[MinimumUppercase],
			pp.[MinimumNumeric],
			pp.[MinimumSpecialCharacter],
			pp.[AllowUsersToChangeOwnPassword],
			pp.[EnablePasswordExpiration],
			pp.[PasswordExpirationPeriodInDays],
			pp.[PreventPasswordReuse],
			pp.[NumberOfPasswordsToRemember],
			pp.[UserId],
			pp.[SysStartDateTime],
			pp.[SysEndDateTime]
	FROM	[Auth].[PasswordPolicy] pp WITH (NOLOCK)
			INNER JOIN [Auth].[Portfolio] p WITH (NOLOCK) ON (pp.PortfolioId = p.PortfolioId)
	WHERE	pp.[PortfolioId] = @PortfolioId
	OR		@PortfolioId IS NULL
END
GO
PRINT N'Creating [Auth].[PasswordHistory]'
GO
CREATE TABLE [Auth].[PasswordHistory]
(
[PasswordHistoryId] [int] NOT NULL IDENTITY(1, 1),
[UserId] [bigint] NOT NULL,
[ActivityId] [int] NOT NULL,
[ChangedPasswordHash] [nvarchar] (1000) NOT NULL,
[ChangedPasswordSalt] [nvarchar] (255) NULL,
[ChangedPasswordDateTime] [smalldatetime] NOT NULL CONSTRAINT [DF_PasswordHistory_ChangedPasswordDateTime] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_PasswordHistory] on [Auth].[PasswordHistory]'
GO
ALTER TABLE [Auth].[PasswordHistory] ADD CONSTRAINT [PK_PasswordHistory] PRIMARY KEY CLUSTERED  ([PasswordHistoryId])
GO
PRINT N'Creating [Auth].[VerifyPasswordHistory]'
GO
CREATE PROCEDURE [Auth].[VerifyPasswordHistory]
    @enterpriseUserName AS NVARCHAR(50) ,
    @NewPasswordHash AS NVARCHAR(1000) ,
    @MinPasswordtoRemember AS INT = 5
AS
    BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
        SET NOCOUNT ON;

        SELECT  ISNULL(MAX(CASE WHEN p.ChangedPasswordHash = @NewPasswordHash
                                THEN 1
                                ELSE 0
                           END), 0) AS PasswordExists
        FROM    Auth.Users u
                CROSS APPLY ( SELECT TOP ( @MinPasswordtoRemember )
                                        PH.UserId ,
                                        PH.ChangedPasswordHash ,
                                        PH.ChangedPasswordDateTime
                              FROM      Auth.PasswordHistory PH
                              WHERE     PH.UserId = u.UserId
                              ORDER BY  PH.ChangedPasswordDateTime DESC
                            ) AS p
        WHERE   u.LoginId = @enterpriseUserName;

    END;
GO
PRINT N'Creating [Auth].[ProductSamlSettings]'
GO
CREATE TABLE [Auth].[ProductSamlSettings]
(
[ProductSamlSettingsId] [int] NOT NULL IDENTITY(1, 1),
[ProductId] [int] NOT NULL,
[LoginUri] [nvarchar] (100) NOT NULL,
[SigningCertificateThumbprint] [nvarchar] (50) NOT NULL,
[SubjectIdSamlAttribute] [nvarchar] (20) NOT NULL
)
GO
PRINT N'Creating primary key [PK__ProductS__E0A6F172E44DA88A] on [Auth].[ProductSamlSettings]'
GO
ALTER TABLE [Auth].[ProductSamlSettings] ADD CONSTRAINT [PK__ProductS__E0A6F172E44DA88A] PRIMARY KEY CLUSTERED  ([ProductSamlSettingsId])
GO
PRINT N'Creating index [IX_ProductSamlSettings_ProductId] on [Auth].[ProductSamlSettings]'
GO
CREATE NONCLUSTERED INDEX [IX_ProductSamlSettings_ProductId] ON [Auth].[ProductSamlSettings] ([ProductId])
GO
PRINT N'Creating [Auth].[GetProductSamlSettings]'
GO
CREATE PROCEDURE [Auth].[GetProductSamlSettings]
	@ProductId		int

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	SELECT        
		ProductSamlSettingsId
		,ProductId
		,LoginUri
		,SigningCertificateThumbprint
		,SubjectIdSamlAttribute

	FROM 
		Auth.ProductSamlSettings

	WHERE
		Auth.ProductSamlSettings.ProductId  = @ProductId

END
GO
PRINT N'Creating [Auth].[ActivityAttempts]'
GO
CREATE TABLE [Auth].[ActivityAttempts]
(
[ActivityAttemptsId] [int] NOT NULL IDENTITY(1, 1),
[ActivityId] [int] NOT NULL,
[EnterpriseUserName] [nvarchar] (50) NOT NULL,
[AuthenticationServiceId] [nvarchar] (50) NULL,
[AttemptCount] [tinyint] NOT NULL CONSTRAINT [DF_ActivityAttempts_AttemptCount] DEFAULT ((0)),
[IpAddress] [nvarchar] (50) NULL,
[BrowserType] [nvarchar] (20) NULL,
[BrowserName] [nvarchar] (20) NULL,
[Version] [nvarchar] (10) NULL,
[Platform] [nvarchar] (20) NULL,
[IsMobile] [bit] NOT NULL CONSTRAINT [DF_ActivityAttempts_IsMobile] DEFAULT ((0)),
[DeviceType] [nvarchar] (20) NULL,
[LastAttemptDateTime] [smalldatetime] NOT NULL CONSTRAINT [DF_ActivityAttempts_LastAttemptDateTime] DEFAULT (getdate()),
[Timezone] [nvarchar] (100) NULL
)
GO
PRINT N'Creating primary key [PK_ActivityAttempts] on [Auth].[ActivityAttempts]'
GO
ALTER TABLE [Auth].[ActivityAttempts] ADD CONSTRAINT [PK_ActivityAttempts] PRIMARY KEY CLUSTERED  ([ActivityAttemptsId])
GO
PRINT N'Creating [Auth].[Activity]'
GO
CREATE TABLE [Auth].[Activity]
(
[ActivityId] [int] NOT NULL,
[ActivityCode] [nvarchar] (50) NOT NULL,
[Description] [nvarchar] (100) NULL,
[MaxActivityAttemptCount] [tinyint] NOT NULL CONSTRAINT [DF_Activity_MaxActivityAttemptCount] DEFAULT ((0)),
[ActivityTokenExpirationMinutes] [int] NOT NULL CONSTRAINT [DF_Activity_ActivityTokenExpirationMinutes] DEFAULT ((0))
)
GO
PRINT N'Creating primary key [PK_Activity] on [Auth].[Activity]'
GO
ALTER TABLE [Auth].[Activity] ADD CONSTRAINT [PK_Activity] PRIMARY KEY CLUSTERED  ([ActivityId])
GO
PRINT N'Creating [Auth].[GetActivityAttemptExceeds]'
GO
CREATE PROCEDURE [Auth].[GetActivityAttemptExceeds]
 		@enterpriseUserName as nvarchar(50),
		@activityId as int
AS
BEGIN

	SET NOCOUNT ON;
	 
	declare @AttemptCount as int,
	@maxActivitycount as int,
	@ActivityAttemptsId as int,
	@ActivityTokenExpirationMinutes as tinyint

	--TODO check @entepriseUserName exist

	select @maxActivitycount = MaxActivityAttemptCount, @ActivityTokenExpirationMinutes=ActivityTokenExpirationMinutes from auth.Activity where activityId=@activityId
	print @maxActivitycount
	select top 1 @AttemptCount=AttemptCount, @ActivityAttemptsId=ActivityAttemptsId from [Auth].[ActivityAttempts] 
	where [EnterpriseUserName]=@enterpriseUserName and activityId=@activityId and LastAttemptDateTime  >=   dateadd(minute, -@ActivityTokenExpirationMinutes, getdate()) order by LastAttemptDateTime desc
	print @AttemptCount

	IF @AttemptCount is not null and @AttemptCount >= @maxActivitycount	
		select 0 as IsAttemptCountSuccess
	ELSE 
		select 1 as IsAttemptCountSuccess
	 
END
GO
PRINT N'Creating [Auth].[ActivityToken]'
GO
CREATE TABLE [Auth].[ActivityToken]
(
[ActivityTokenId] [int] NOT NULL IDENTITY(1, 1),
[ActivityId] [int] NOT NULL,
[UserId] [bigint] NOT NULL,
[ActivityToken] [nvarchar] (100) NOT NULL,
[IsActive] [bit] NOT NULL CONSTRAINT [DF_ActivityToken_IsActive] DEFAULT ((0)),
[CreateDateTime] [smalldatetime] NOT NULL CONSTRAINT [DF_ActivityToken_CreateDateTime] DEFAULT (getdate()),
[ExpireDateTime] [smalldatetime] NOT NULL
)
GO
PRINT N'Creating primary key [PK_ActivityToken] on [Auth].[ActivityToken]'
GO
ALTER TABLE [Auth].[ActivityToken] ADD CONSTRAINT [PK_ActivityToken] PRIMARY KEY CLUSTERED  ([ActivityTokenId])
GO
PRINT N'Creating [Auth].[GetActivityToken]'
GO
CREATE PROCEDURE [Auth].[GetActivityToken]
	@EnterpriseUserName as nvarchar(50),
	@ActivityToken as nvarchar(50),
	@ActivityId		as int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @UserId as bigint,
	@ActivityTokenExpirationMinutes as int


	select @ActivityTokenExpirationMinutes=ActivityTokenExpirationMinutes from auth.Activity where activityId=@activityId

	select @UserId=userid from users where LoginId=@EnterpriseUserName
	--TODO check if userid exist

	select top 1  @UserId as EnterpriseUserId, [ActivityToken] as Token from [Auth].[ActivityToken]
	 where [ActivityId]= @ActivityId and IsActive =1 and UserId=@UserId and  ActivityToken=@ActivityToken and [ExpireDateTime] >  dateadd(minute,-@ActivityTokenExpirationMinutes,GetDate())


END



--select top 1  [ActivityToken] from [Auth].[ActivityToken]
--	 where  [ActivityId]=2 and IsActive =1 and UserId=17 and [ExpireDateTime] > dateadd(minute,-30,GetDate())
GO
PRINT N'Creating [Auth].[UpdateActivityAttempt]'
GO
CREATE PROCEDURE [Auth].[UpdateActivityAttempt]
 		@enterpriseUserName as nvarchar(50),
		@activityId as int,
		@browserName as nvarchar(20)='',
		@browserType as nvarchar(20)='',
		@ipAddress as nvarchar(50)='',
		@isMobile as bit=0,
		@platform as nvarchar(20)='', 
		@version as nvarchar(10)='',
		@deviceType as nvarchar(20)='',
		@timezone as nvarchar(100)='',
		@authenticationServiceId as nvarchar(50)='' 

AS
BEGIN

	SET NOCOUNT ON;
	 
	declare @AttemptCount as int,
	@maxActivitycount as int,
	@ActivityAttemptsId as int,
	@ActivityTokenExpirationMinutes as tinyint
	 
	select @maxActivitycount = MaxActivityAttemptCount, @ActivityTokenExpirationMinutes=ActivityTokenExpirationMinutes from auth.Activity where activityId=@activityId
	--print @maxActivitycount
	select top 1@AttemptCount=AttemptCount, @ActivityAttemptsId=ActivityAttemptsId from [Auth].[ActivityAttempts] where 
		[EnterpriseUserName]=@enterpriseUserName and activityId=@activityId and LastAttemptDateTime >= dateadd(minute, -@ActivityTokenExpirationMinutes, getdate())   
		order by [ActivityAttemptsId] desc
	--print @AttemptCount

	IF @activityId = 7 -- LoginSuccess activity
		BEGIN
			-- insert unique record for LoginSuccess
			if not exists (select top 1 [ActivityId] from [Auth].[ActivityAttempts] where ActivityId = @activityId and concat([EnterpriseUserName], [IpAddress], [DeviceType], [BrowserName]) 
				like concat(@enterpriseUserName, @ipAddress, @deviceType, @browserName))
				BEGIN
					INSERT INTO [Auth].[ActivityAttempts]
							   ([ActivityId] ,[EnterpriseUserName] ,[AttemptCount]  ,[IpAddress] ,[BrowserType] ,
								[BrowserName]  ,[Version] ,[Platform]  ,[IsMobile]  ,[LastAttemptDateTime], [DeviceType], [Timezone], [AuthenticationServiceId])
						 VALUES
							   (@activityId ,@enterpriseUserName,1,@ipAddress,@browserType
							   ,@browserName ,@version,@platform,@isMobile,GetDate(), @deviceType, @timezone, @authenticationServiceId)

					select * from [Auth].[ActivityAttempts] where ActivityAttemptsId = (select scope_identity())
				END
			else
				select top 1 * from [Auth].[ActivityAttempts] where ActivityId = @activityId and concat([EnterpriseUserName], [IpAddress], [DeviceType], [BrowserName]) 
				like concat(@enterpriseUserName, @ipAddress, @deviceType, @browserName)
		END
	ELSE IF @AttemptCount >= @maxActivitycount	
		select top 1 * from [Auth].[ActivityAttempts] where  activityAttemptsId=@ActivityAttemptsId -- [EntepriseUserName]=@@entepriseUserName and activityId=@activityId and LastAttemptDateTime	< DATEADD(minute,30,GETDATE())		
	ELSE if @AttemptCount is null or @AttemptCount = 0			
			BEGIN
			-- insert record
				 INSERT INTO [Auth].[ActivityAttempts]
					   ([ActivityId] ,[EnterpriseUserName] ,[AttemptCount]  ,[IpAddress] ,[BrowserType] ,
						[BrowserName]  ,[Version] ,[Platform]  ,[IsMobile]  ,[LastAttemptDateTime], [DeviceType], [Timezone], [AuthenticationServiceId])
				 VALUES
					   (@activityId ,@enterpriseUserName,1,@ipAddress,@browserType
					   ,@browserName ,@version,@platform,@isMobile,GetDate(), @deviceType, @timezone, @authenticationServiceId)
						 
				select top 1 * from [Auth].[ActivityAttempts] where [EnterpriseUserName]=@enterpriseUserName and activityId=@activityId and LastAttemptDateTime < DATEADD(minute,-@ActivityTokenExpirationMinutes,GETDATE())
				 order by [ActivityAttemptsId] desc
			END 
	ELSE
			BEGIN
				-- increment @AttemptCount
				update [Auth].[ActivityAttempts]
				set [AttemptCount]  = @AttemptCount +1 where  activityAttemptsId=@ActivityAttemptsId  --[EntepriseUserName]=@@entepriseUserName and activityId=@activityId and LastAttemptDateTime < DATEADD(minute,30,GETDATE())
    
				select top 1 * from [Auth].[ActivityAttempts] where activityAttemptsId=@ActivityAttemptsId  
				order by [ActivityAttemptsId] desc-- -- [EntepriseUserName]=@@entepriseUserName and activityId=@activityId and LastAttemptDateTime< DATEADD(minute,30,GETDATE())
			END 
	 
END
GO
PRINT N'Creating [Auth].[UpdateActivityTokenFlag]'
GO
CREATE PROCEDURE [Auth].[UpdateActivityTokenFlag]
	 @enterpriseUserName as nvarchar(50),
		@activityId as int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @UserId as bigint

	select @UserId=userid from users where LoginId=@enterpriseUserName

	update [Auth].[ActivityToken] set isActive=0 WHERE userid=@userid and activityId=@activityId

END
GO
PRINT N'Creating [Auth].[UpdateEnterpriseUserCredential]'
GO
CREATE PROCEDURE [Auth].[UpdateEnterpriseUserCredential]
	@EnterpriseUserName as nvarchar(50),
	@correctAnswerToken as nvarchar(50),
	@ActivityId		as int,
	@NewPasswordHash as  nvarchar(1000),
	@passwordSalt as  nvarchar(255)	 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @UserId as bigint,
	@oldPassword as nvarchar(1000),
	@oldPasswordSalt as nvarchar(255)
	select @UserId=userid, @oldPassword=PasswordHash,@oldPasswordSalt=PasswordSalt from auth.users where LoginId=@EnterpriseUserName
	--TODO check if userid exist

	if Exists(select top 1  [ActivityToken] from auth.ActivityToken
	 where [ActivityId]=@ActivityId and IsActive =1 and UserId=@UserId and ActivityToken=@correctAnswerToken) --and [ExpireDateTime] < dateadd(minute, -5, getdate())) -- user has to change password in 5 mins
	 Begin
		-- update password
		Update auth.Users set PasswordHash=@NewPasswordHash,PasswordSalt=@passwordSalt where UserId=@UserId 
		-- insert old pwd in history table
		INSERT INTO [Auth].[PasswordHistory]([UserId],[ActivityId],[ChangedPasswordHash],[ChangedPasswordSalt],[ChangedPasswordDateTime])
		VALUES (@UserId,@ActivityId,@oldPassword,@oldPasswordSalt,getdate())

		-- reset token flags
		update [Auth].[ActivityToken] set isActive=0 WHERE userid=@userid and activityId=2 OR activityId=6 --  activityId=2 OR activityId=6 is for ForgotPassword & CorrectAnswer
        UPDATE [Auth].[ActivityAttempts] SET  [AttemptCount] = 0 WHERE [EnterpriseUserName] = @EnterpriseUserName 
		and LastAttemptDateTime >=   dateadd(day, -3, getdate()) and ([ActivityId] = 2 or [ActivityId] = 5 or [ActivityId] = 6)
	 end
	else
		select @UserId = null --RAISERROR('Activity token expired', 16,16)

	select @UserId
END
GO
PRINT N'Creating [Auth].[CreateActivityToken]'
GO
CREATE PROCEDURE [Auth].[CreateActivityToken]
	@EnterpriseUserId bigint ,
	@activityId as int
AS
BEGIN
	SET NOCOUNT ON;
	Declare @token varchar(50),
	
	@OldActivityTokenId as int,
	@ActivityTokenExpirationMinutes as int

	select @ActivityTokenExpirationMinutes=ActivityTokenExpirationMinutes from Auth.Activity where activityId=@activityId

	select @token=NEWID ()
	
	-- Check if any active token already exists for user
	SELECT @OldActivityTokenId=[ActivityTokenId]  
	FROM [Auth].[ActivityToken]
	where [ActivityId]=@ActivityId and UserId=@EnterpriseUserId and IsActive=1-- and [ExpireDateTime] > DateAdd(minute,15,getdate())

	if  @OldActivityTokenId is not null
		Update [Auth].[ActivityToken] set IsActive=0 where [ActivityId]=@ActivityId and UserId=@EnterpriseUserId
	end
	
	INSERT INTO [Auth].[ActivityToken]
           ( ActivityId,
			[UserId]
           ,[ActivityToken]
           ,[IsActive]
           ,[CreateDateTime]
           ,[ExpireDateTime])
     VALUES
			( @ActivityId,
				@EnterpriseUserId
			   ,@token
			   ,1
			   ,GetDate()
			   ,DateAdd(minute,@ActivityTokenExpirationMinutes,GetDate())
		   )

	select @token as ActivityToken
GO
PRINT N'Creating [Auth].[UserLockAcitvity]'
GO
CREATE TABLE [Auth].[UserLockAcitvity]
(
[UserLockActivityId] [int] NOT NULL IDENTITY(1, 1),
[UserId] [bigint] NOT NULL,
[AcivityId] [int] NOT NULL,
[LockReason] [nvarchar] (100) NOT NULL,
[IsLockActive] [bit] NOT NULL CONSTRAINT [DF_UserLockAcitvity_IsLockActive] DEFAULT ((0)),
[LockDateTime] [smalldatetime] NOT NULL
)
GO
PRINT N'Creating primary key [PK_UserLockAcitvity] on [Auth].[UserLockAcitvity]'
GO
ALTER TABLE [Auth].[UserLockAcitvity] ADD CONSTRAINT [PK_UserLockAcitvity] PRIMARY KEY CLUSTERED  ([UserLockActivityId])
GO
PRINT N'Creating [Auth].[GetEnterpriseUserLockActivities]'
GO
CREATE PROCEDURE [Auth].[GetEnterpriseUserLockActivities]
	@enterpriseUserId	bigint
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

   SELECT [UserLockActivityId] ,[UserId] ,[AcivityId] ,[LockReason]
      ,[IsLockActive] ,[LockDateTime]
  FROM [Auth].[UserLockAcitvity] WHERE [UserId] = @enterpriseUserId and [IsLockActive] =1

END
GO
PRINT N'Creating [Auth].[GetEnterpriseUserStatus]'
GO
CREATE PROCEDURE [Auth].[GetEnterpriseUserStatus]
	@entepriseLoginName	nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	Declare @entepriseUserId as bigint ,
	@IsLocked as bit

	SELECT @entepriseUserId = UserId, @IsLocked=IsLocked FROM Users WHERE LoginId = @entepriseLoginName  

	IF @entepriseUserId IS NULL
       SELECT Null as EnterpriseUserId,@IsLocked as IsLocked, 'false' AS IsUserExist
	Else
		SELECT @entepriseUserId as EnterpriseUserId,@IsLocked as IsLocked,'true' AS IsUserExist
	END
GO
PRINT N'Creating [Auth].[GetAuthenticateUser]'
GO
CREATE PROCEDURE [Auth].[GetAuthenticateUser]
	@enterpriseUserName	nvarchar(50),
	@hashedPassword nvarchar(255),
	@activityId as int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
   
	if exists(SELECT [UserId] FROM [Auth].[Users] where [LoginId]=@enterpriseUserName and PasswordHash=@hashedPassword)
		BEGIN			
			SELECT [UserId] as enterpriseUserId,[LoginId] as enterpriseUserName,[Firstname],[LastName] ,[IsActive],[IdentityProvider]
			  ,[Title],[Email],[Phone],[IsLocked]  FROM [Auth].[Users] 
			  where [LoginId]=@enterpriseUserName and PasswordHash=@hashedPassword

			update [Auth].[ActivityAttempts] set [AttemptCount]  =  0 where  activityAttemptsId=@activityId and [EnterpriseUserName]=@enterpriseUserName

			print 'updated'
		END
	else
		Select null
END
GO
PRINT N'Creating [Auth].[GetPasswordHistory]'
GO
CREATE PROCEDURE [Auth].[GetPasswordHistory]
    @enterpriseUserName AS NVARCHAR(50) ,   
    @numberOfPasswordsToRemember AS INT = 5
AS
    BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
        SET NOCOUNT ON;

  --      SELECT CAST(ISNULL(MAX(CASE WHEN p.ChangedPasswordHash = @NewPasswordHash THEN 1 ELSE 0 END),0) AS BIT) AS PasswordExists
  --      FROM Auth.Users u 
		--CROSS APPLY (SELECT TOP (@numberOfPasswordsToRemember) *
		--FROM Auth.PasswordHistory PH
		--WHERE ph.UserId = u.UserId
		--ORDER BY PH.ChangedPasswordDateTime DESC) AS p
		--WHERE u.LoginId = @enterpriseUserName

		SELECT   TOP (@numberOfPasswordsToRemember) Auth.PasswordHistory.ChangedPasswordHash as PasswordHash, Auth.PasswordHistory.ChangedPasswordSalt as PasswordSalt
		FROM            Auth.Users INNER JOIN
		 Auth.PasswordHistory ON Auth.Users.UserId = Auth.PasswordHistory.UserId
		 where Auth.Users.LoginId=  @enterpriseUserName
		 order by Auth.PasswordHistory.ChangedPasswordDateTime desc

    END;
GO
PRINT N'Creating [Auth].[ResetEnterpriseUserCredential]'
GO
CREATE PROCEDURE [Auth].[ResetEnterpriseUserCredential] ( 
	@enterpriseUserName as nvarchar(50),
	@newPasswordHash as nvarchar(1000),
	@newPasswordSalt as nvarchar(255) 
)
AS
BEGIN
	BEGIN TRY
		
		declare @UserId as bigint,
		@oldPassword as nvarchar(1000),
		@oldPasswordSalt as nvarchar(255)

		select @UserId=userid, @oldPassword=PasswordHash,@oldPasswordSalt=PasswordSalt from auth.users where LoginId=@EnterpriseUserName
			 
		UPDATE	[Auth].[Users]
		SET		PasswordHash=@newPasswordHash, PasswordSalt = @newPasswordSalt WHERE	userId = @UserId

		SELECT	@@ROWCOUNT AS 'rowCount',
				@UserId AS Id, --TODO: get unique-user-id as input param instead of @enterpriseUserName
				0	AS errorNumber,
				'' AS errorMessage		 
	END TRY  
	BEGIN CATCH
		SELECT	@@ROWCOUNT AS 'rowCount',
				0 AS Id,
				ERROR_NUMBER() AS errorNumber,
				ERROR_MESSAGE() AS errorMessage
	END CATCH
END
GO
PRINT N'Creating [Auth].[CreateSecurityQuestionAnswers]'
GO
CREATE PROCEDURE [Auth].[CreateSecurityQuestionAnswers]
(
	@enterpriseUserName as nvarchar(50),
	@activityToken as nvarchar(100),
	@activityId as int,
	@securityQuestion1Id as int,
	@securityAnswer1 as nvarchar(50),
	@securityQuestion2Id as int,
	@securityAnswer2 as nvarchar(50),
	@securityQuestion3Id as int,
	@securityAnswer3 as nvarchar(50)
)
AS
BEGIN

	BEGIN TRY

	declare @UserId as int,
	@insertDateTime as smalldatetime

	select @UserId=userid from auth.users where LoginId=@EnterpriseUserName

	if Exists(select top 1  [ActivityToken] from auth.ActivityToken
	where [ActivityId]=@ActivityId and IsActive =1 and UserId=@UserId and ActivityToken=@activityToken) 
	Begin
	
		select @insertDateTime=getdate()
		
		delete from [Auth].[UserSecurityAnswer] where UserId=@UserId -- delete old questions

		INSERT INTO [Auth].[UserSecurityAnswer]
           ([UserId],[SecurityQuestionId],[Answer],[CreateDateTime])
        VALUES
           (@UserId,@securityQuestion1Id,@securityAnswer1,@insertDateTime),
		   (@UserId,@securityQuestion2Id,@securityAnswer2,@insertDateTime),
		   (@UserId,@securityQuestion3Id,@securityAnswer3,@insertDateTime)


		SELECT	@@ROWCOUNT AS 'rowCount',
				@UserId AS Id,
				0	AS errorNumber,
				'' AS errorMessage	
		end	

	END TRY  
	BEGIN CATCH
		SELECT	@@ROWCOUNT AS 'rowCount',
				0 AS Id,
				ERROR_NUMBER() AS errorNumber,
				ERROR_MESSAGE() AS errorMessage
	END CATCH
END
GO
PRINT N'Creating [Enterprise].[ContactMechanismUsageType]'
GO
CREATE TABLE [Enterprise].[ContactMechanismUsageType]
(
[ContactMechanismUsageTypeID] [int] NOT NULL,
[ParentContactMechanismUsageTypeID] [int] NULL,
[Name] [nvarchar] (50) NOT NULL
)
GO
PRINT N'Creating primary key [PK_ContactMechanismUsageType] on [Enterprise].[ContactMechanismUsageType]'
GO
ALTER TABLE [Enterprise].[ContactMechanismUsageType] ADD CONSTRAINT [PK_ContactMechanismUsageType] PRIMARY KEY CLUSTERED  ([ContactMechanismUsageTypeID])
GO
PRINT N'Adding constraints to [Enterprise].[ContactMechanismUsageType]'
GO
ALTER TABLE [Enterprise].[ContactMechanismUsageType] ADD CONSTRAINT [AK_ContactMechanismUsageType_Name] UNIQUE NONCLUSTERED  ([Name])
GO
PRINT N'Creating [Enterprise].[ListContactMechanismUsageType]'
GO
CREATE PROCEDURE [Enterprise].[ListContactMechanismUsageType] (
	@ContactMechanismUsageTypeName nvarchar(100) = NULL
)
AS
BEGIN
	SET @ContactMechanismUsageTypeName = NULLIF(@ContactMechanismUsageTypeName, '')

	SELECT	cmut.ContactMechanismUsageTypeId,
			cmut.ParentContactMechanismUsageTypeId,
			cmut.Name
	FROM	Enterprise.ContactMechanismUsageType cmut
			LEFT OUTER JOIN Enterprise.ContactMechanismUsageType pcmut ON (cmut.ParentContactMechanismUsageTypeID = pcmut.ContactMechanismUsageTypeID)
	WHERE	(@ContactMechanismUsageTypeName IS NULL OR pcmut.Name = @ContactMechanismUsageTypeName)
END
GO
PRINT N'Creating [Enterprise].[RoleType]'
GO
CREATE TABLE [Enterprise].[RoleType]
(
[PartyRoleTypeId] [int] NOT NULL,
[ParentPartyRoleTypeId] [int] NULL,
[Name] [varchar] (50) NOT NULL
)
GO
PRINT N'Creating primary key [PK_RoleType] on [Enterprise].[RoleType]'
GO
ALTER TABLE [Enterprise].[RoleType] ADD CONSTRAINT [PK_RoleType] PRIMARY KEY CLUSTERED  ([PartyRoleTypeId])
GO
PRINT N'Adding constraints to [Enterprise].[RoleType]'
GO
ALTER TABLE [Enterprise].[RoleType] ADD CONSTRAINT [AK_PartyRoleType_Name] UNIQUE NONCLUSTERED  ([Name])
GO
PRINT N'Creating [Enterprise].[GetPartyRelationshipByRealPageId]'
GO
CREATE PROCEDURE [Enterprise].[GetPartyRelationshipByRealPageId]
	@RealPageIdFrom UNIQUEIDENTIFIER,
	@RealPageIdTo UNIQUEIDENTIFIER,
	@RoleTypeName varchar(50) = NULL,
	@RelationshipTypeName varchar(50) = NULL
AS
BEGIN
DECLARE @NOW DATETIME
SELECT @NOW = GETUTCDATE()
	SELECT	pr.PartyRelationshipId,
			pr.PartyIdFrom,
			pf.RealPageId AS RealPageIdFrom,  
			pr.PartyIdTo,
			pt.RealPageId AS RealPageIdTo,  
			pr.RoleTypeIdFrom,
			pr.RoleTypeIdTo,
			pr.PartyRelationshipTypeId,
			pr.FromDate,
			pr.ThruDate
	FROM	Enterprise.PartyRelationship pr
			INNER JOIN Enterprise.Party pf ON (pr.PartyIdFrom = pf.PartyId)
			INNER JOIN Enterprise.Party pt ON (pr.PartyIdTo = pt.PartyId)
			INNER JOIN Enterprise.RoleType rtf ON (pr.RoleTypeIdFrom = rtf.PartyRoleTypeId)
			INNER JOIN Enterprise.[RelationshipType] rt ON (pr.PartyRelationshipTypeId = rt.RelationshipTypeId)			
			LEFT OUTER JOIN Enterprise.RoleType prt ON (rtf.ParentPartyRoleTypeId = prt.PartyRoleTypeId)
	WHERE	pf.RealPageId = @RealPageIdFrom
	AND		pt.RealPageId = @RealPageIdTo
	AND		(@RelationshipTypeName IS NULL OR rt.Name = @RelationshipTypeName)
	AND		(@RoleTypeName IS NULL OR prt.Name = @RoleTypeName)
	AND ((@NOW BETWEEN pr.FromDate AND pr.ThruDate) OR (@NOW >= pr.FromDate AND pr.ThruDate IS NULL));
END
GO
PRINT N'Creating [Person].[Person]'
GO
CREATE TABLE [Person].[Person]
(
[PartyId] [bigint] NOT NULL,
[Title] [nvarchar] (50) NULL,
[FirstName] [dbo].[Name] NOT NULL,
[MiddleName] [dbo].[Name] NULL,
[LastName] [dbo].[Name] NOT NULL,
[Suffix] [nvarchar] (10) NULL,
[PreferredContactMethodId] [int] NULL
)
GO
PRINT N'Creating primary key [PK_Person] on [Person].[Person]'
GO
ALTER TABLE [Person].[Person] ADD CONSTRAINT [PK_Person] PRIMARY KEY CLUSTERED  ([PartyId])
GO
PRINT N'Creating [Enterprise].[ListOrganizationByRealPageId]'
GO
CREATE PROCEDURE [Enterprise].[ListOrganizationByRealPageId] (
	@RealPageId UNIQUEIDENTIFIER,
	@RelationshipTypeName NVARCHAR(50) = NULL
)
AS  
BEGIN  
	SELECT	DISTINCT
			o.PartyId ,
			o.Name ,
			po.RealPageId ,
			po.CreateDate ,
			rtf.Name RoleNameFrom,
			rtt.Name RoleNameTo,
			rt.Name AS RelationshipType
	FROM	Enterprise.PartyRelationship pr
			INNER JOIN Enterprise.Party pp ON (pr.PartyIdFrom = pp.PartyId)
			INNER JOIN Person.Person p ON (pp.PartyId = p.PartyId)
			INNER JOIN Enterprise.Organization o ON (pr.PartyIdTo = o.PartyId)
			INNER JOIN Enterprise.Party po ON (o.PartyId = po.PartyId)
			INNER JOIN Enterprise.[RelationshipType] rt ON (pr.PartyRelationshipTypeId = rt.RelationshipTypeId)
			INNER JOIN Enterprise.RoleType rtf ON (pr.RoleTypeIdFrom = rtf.PartyRoleTypeId)
			INNER JOIN Enterprise.RoleType rtt ON (pr.RoleTypeIdTo = rtt.PartyRoleTypeId)
	WHERE	pp.RealPageId = @RealPageId
			AND (rt.Name = @RelationshipTypeName OR @RelationshipTypeName IS NULL)   
END;
GO
PRINT N'Creating [Enterprise].[ListRoleType]'
GO
CREATE PROCEDURE [Enterprise].[ListRoleType]
	@RoleTypeName varchar(50) = NULL
AS
BEGIN
	SELECT	rt.PartyRoleTypeId,
			rt.ParentPartyRoleTypeId,
			rt.Name
	FROM	Enterprise.RoleType rt
			LEFT OUTER JOIN Enterprise.RoleType prt ON (rt.ParentPartyRoleTypeId = prt.PartyRoleTypeId)
	WHERE	(@RoleTypeName IS NULL OR prt.Name = @RoleTypeName)
END
GO
PRINT N'Creating [Enterprise].[ListRelationshipType]'
GO
CREATE PROCEDURE [Enterprise].[ListRelationshipType]
	@RelationshipTypeName varchar(50) = NULL
AS
BEGIN
	SELECT	RelationshipTypeId,
			RoleTypeIdValidFrom,
			RoleTypeIdValidTo,
			Name,
			Description
	FROM	Enterprise.[RelationshipType]
	WHERE	(@RelationshipTypeName IS NULL OR Name = @RelationshipTypeName)
END
GO
PRINT N'Creating [Enterprise].[ListProductsByRealPageId]'
GO
CREATE PROCEDURE [Enterprise].[ListProductsByRealPageId]
	@RealPageId UNIQUEIDENTIFIER
AS

SET NOCOUNT ON;

DECLARE @products TABLE
(
    ProductGUID             UNIQUEIDENTIFIER,
	ProductId			    int,
	[Name]                   nvarchar(100), 
	SolutionId              int, 
	Solution                nvarchar(100), 
	FamilyId                int,  
	Family                  nvarchar(100), 
	[Description] 			nvarchar(100),	                    
	ProductSettingId		int,
	ProductSettingTypeId	int,
	SettingName				nvarchar(50),
	SettingValue			nvarchar(1000),
	SettingDescription		nvarchar(100),
    HasAccess				bit
)	

INSERT INTO @products
	SELECT  ep.ProductGUID,
		ep.ProductId, 
		ep.Name, 
		eptSln.ProductTypeId as SolutionId, 
		eptSln.Name as Solution, 
		eptFam.ProductTypeId AS FamilyId,  
		eptFam.Name as Family,
		ep.Description, 		                    
		eps.ProductSettingId,
		epst.ProductSettingTypeId,
		epst.Name AS SettingName,
		eps.Value AS SettingValue,
		epst.Description AS SettingDescription,
		1 AS HasAcccess	 
		FROM Enterprise.Product ep 
		JOIN [Enterprise].[ProductSetting] eps ON eps.ProductId = ep.ProductId
		JOIN [Enterprise].[ProductSettingType] epst ON epst.ProductSettingTypeId = eps.ProductSettingTypeId 
		JOIN (
			SELECT DISTINCT(isua.ProductId) FROM [Ident].[SamlUserAttribute] isua
				JOIN [Enterprise].[Party] epar ON epar.PartyId = isua.[PersonaId]
				JOIN  Enterprise.Product ep ON isua.ProductId = ep.ProductId AND epar.RealPageId = @RealPageId
		) saml ON saml.ProductId = ep.ProductId		                                        
		LEFT JOIN [Enterprise].[ProductType] eptSln ON eptSln.ProductTypeId = ep.ProductTypeId	                    
		JOIN [Enterprise].[ProductType] eptFam ON eptFam.ProductTypeId = eptSln.ParentProductTypeId                            
	WHERE   ep.Name NOT IN ('Landing', 'ClientPortal', 'Product Learning Portal') ;

--Return the products
SELECT	DISTINCT(ProductId), 
        ProductGUID,		                     
		[Name],                             
		[Description]
FROM	@products;

--Return the product settings/solution/family for each product
SELECT	ProductId, 	
		ProductSettingId,
		ProductSettingTypeId,
		SettingName AS Name,
		SettingValue AS Value,
		SettingDescription AS Description,
        Solution,
        Family
FROM	@products;
GO
PRINT N'Creating [Enterprise].[GetPartyRoleByRealPageId]'
GO
CREATE PROCEDURE [Enterprise].[GetPartyRoleByRealPageId] (
	@RealPageId UNIQUEIDENTIFIER
)
AS
BEGIN
	SELECT	pr.PartyRoleId,
			pr.PartyId,
			pr.RoleTypeId,
			ert.Name
	FROM	Enterprise.Party pa
			INNER JOIN Person.Person p ON (pa.PartyId = p.PartyId)
			INNER JOIN Enterprise.PartyRole pr ON (p.PartyId = pr.PartyId)
			JOIN Enterprise.RoleType ert on ert.PartyRoleTypeId = pr.RoleTypeId
	WHERE	pa.RealPageId = @RealPageId
END
GO
PRINT N'Creating [Enterprise].[ListResourcesByRealPageId]'
GO
CREATE PROCEDURE [Enterprise].[ListResourcesByRealPageId]
	@RealPageId UNIQUEIDENTIFIER

AS

	SET NOCOUNT ON;
    DECLARE @products TABLE
    (
        ProductGUID             UNIQUEIDENTIFIER,
	    ProductId   			int,
	    Name                    nvarchar(100), 
		HasAccess               bit,
	    Description 			nvarchar(100),	                   
	    ProductSettingId		int,
	    ProductSettingTypeId	int,
	    SettingName				nvarchar(50),
	    SettingValue			nvarchar(1000),
	    SettingDescription		nvarchar(100)
    )	

    INSERT INTO @products
		SELECT	ep.ProductGUID,
				ep.ProductId, 
				ep.Name, 							
				CASE WHEN epar.RealPageId = @RealPageId THEN 1 ELSE 0 END HasAccess,
				ep.Description, 		                    
				eps.ProductSettingId,
				epst.ProductSettingTypeId,
				epst.Name AS SettingName,
				eps.Value AS SettingValue,
				epst.Description AS SettingDescription	 
		FROM Enterprise.Product ep							
				LEFT JOIN [Enterprise].[ProductSetting] eps ON eps.ProductId = ep.ProductId
				LEFT JOIN [Enterprise].[ProductSettingType] epst ON epst.ProductSettingTypeId = eps.ProductSettingTypeId 
				LEFT JOIN [Ident].[SamlUserAttribute] isua ON isua.ProductId = eps.ProductId
				LEFT JOIN [Enterprise].[Party] epar ON epar.PartyId = isua.[PersonaId] 
				JOIN (
					SELECT ProductId FROM [Enterprise].[ProductSetting] eps2 
						JOIN [Enterprise].[ProductSettingType] epst2 ON epst2.ProductSettingTypeId = eps2.ProductSettingTypeId 
					WHERE eps2.Value = '1' AND epst2.Name='IsResource') epsst ON epsst.ProductId = eps.ProductId;

    SELECT	DISTINCT(ProductId), 
            ProductGUID,		                     
		    Name,                             
		    Description		                    
    FROM	@products;

    SELECT	ProductId, 	
		    ProductSettingId,
		    ProductSettingTypeId,
		    SettingName AS Name,
		    SettingValue AS Value,
		    SettingDescription AS Description,
            HasAccess
    FROM	@products;
GO
PRINT N'Creating [Enterprise].[ListProductSolutions]'
GO
CREATE PROCEDURE [Enterprise].[ListProductSolutions]
	@ParentProductTypeId int
AS
	SET NOCOUNT ON;

	IF (@ParentProductTypeId IS NOT NULL)
    BEGIN
        SELECT  ept.ProductTypeId,
                ept.ParentProductTypeId,
	            ept.Name, 
	            ept.Description 	                        								
	    FROM    [Enterprise].[ProductType] ept
		        WHERE ept.ParentProductTypeId = @ParentProductTypeId
    END
    ELSE
    BEGIN
        SELECT  ept.ProductTypeId,  
                ept.ParentProductTypeId,
	            ept.Name, 
	            ept.Description 	                        								
	    FROM    [Enterprise].[ProductType] ept
		        WHERE ept.ParentProductTypeId IS NOT NULL
    END
GO
PRINT N'Creating [Enterprise].[ListProductFamilies]'
GO
CREATE PROCEDURE [Enterprise].[ListProductFamilies]
	
AS
	SET NOCOUNT ON;

	SELECT  ept.ProductTypeId,  
	        ept.Name, 
	        ept.Description 	                        								
	FROM    [Enterprise].[ProductType] ept
		    WHERE ept.ParentProductTypeId IS NULL
	;
GO
PRINT N'Creating [Ident].[GetUserLogin]'
GO
CREATE PROCEDURE [Ident].[GetUserLogin] (
    @RealPageId UNIQUEIDENTIFIER
)
AS
BEGIN
	SELECT	ul.UserId,
			ul.PartyId,
			ul.[LoginName],
			p.RealPageId,
			ul.PasswordModifiedDate,
			ul.PasswordHash,
			ul.PasswordSalt,
			ul.FromDate,
			ul.ThruDate
	FROM	Ident.UserLogin ul
			JOIN Enterprise.Party p ON p.PartyId = ul.PartyId
	WHERE	p.RealPageId = @RealPageId
END
GO
PRINT N'Creating [Ident].[PasswordHistory]'
GO
CREATE TABLE [Ident].[PasswordHistory]
(
[PasswordHistoryId] [int] NOT NULL IDENTITY(1, 1),
[UserId] [bigint] NOT NULL,
[ActivityId] [int] NOT NULL,
[ChangedPasswordHash] [nvarchar] (1000) NOT NULL,
[ChangedPasswordSalt] [nvarchar] (255) NULL,
[ChangedPasswordDateTime] [smalldatetime] NOT NULL CONSTRAINT [DF_PasswordHistory_ChangedPasswordDateTime] DEFAULT (getutcdate())
)
GO
PRINT N'Creating primary key [PK_PasswordHistory] on [Ident].[PasswordHistory]'
GO
ALTER TABLE [Ident].[PasswordHistory] ADD CONSTRAINT [PK_PasswordHistory] PRIMARY KEY CLUSTERED  ([PasswordHistoryId])
GO
PRINT N'Creating [Ident].[GetPasswordHistory]'
GO
CREATE PROCEDURE [Ident].[GetPasswordHistory]
    @enterpriseUserName AS NVARCHAR(255) ,   
    @numberOfPasswordsToRemember AS INT = 5
AS
    BEGIN
	 
        SET NOCOUNT ON;

  --      SELECT CAST(ISNULL(MAX(CASE WHEN p.ChangedPasswordHash = @NewPasswordHash THEN 1 ELSE 0 END),0) AS BIT) AS PasswordExists
  --      FROM [Ident].Users u 
		--CROSS APPLY (SELECT TOP (@numberOfPasswordsToRemember) *
		--FROM [Ident].PasswordHistory PH
		--WHERE ph.UserId = u.UserId
		--ORDER BY PH.ChangedPasswordDateTime DESC) AS p
		--WHERE u.LoginId = @enterpriseUserName

		SELECT   TOP (@numberOfPasswordsToRemember) [Ident].PasswordHistory.ChangedPasswordHash as PasswordHash, [Ident].PasswordHistory.ChangedPasswordSalt as PasswordSalt
		FROM            [Ident].UserLogin INNER JOIN
		 [Ident].PasswordHistory ON [Ident].UserLogin.UserId = [Ident].PasswordHistory.UserId
		 where [Ident].UserLogin.LoginName =  @enterpriseUserName
		 order by [Ident].PasswordHistory.PasswordHistoryId desc

    END;
GO
PRINT N'Creating [Ident].[GetPasswordPolicy]'
GO
CREATE PROCEDURE [Ident].[GetPasswordPolicy] (
	@PartyId bigint = NULL
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Insert statements for procedure here
	SELECT	pp.[PasswordPolicyId],
			pp.[PartyId],
			p.[Name],
			pp.[MinimumLength],
			pp.[MaximumLength],
			pp.[MinimumLowercase],
			pp.[MinimumUppercase],
			pp.[MinimumNumeric],
			pp.[MinimumSpecialCharacter],
			pp.[AllowUsersToChangeOwnPassword],
			pp.[EnablePasswordExpiration],
			pp.[PasswordExpirationPeriodInDays],
			pp.[PreventPasswordReuse],
			pp.[NumberOfPasswordsToRemember],
			pp.[UserId],
			pp.[SysStartDateTime],
			pp.[SysEndDateTime]
	FROM	[Ident].[PasswordPolicy] pp WITH (NOLOCK)
			INNER JOIN Enterprise.Organization p WITH (NOLOCK) ON (pp.PartyId = p.PartyId)
	WHERE	pp.[PartyId] = @PartyId OR @PartyId IS NULL
END
GO
PRINT N'Creating [Ident].[GetUserSecurityQuestionAnswer]'
GO
Create PROCEDURE [Ident].[GetUserSecurityQuestionAnswer]
	@EnterpriseUserName nvarchar(255) 
AS
BEGIN
	SET NOCOUNT ON;

	SELECT Ident.UserLogin.UserId, Ident.UserLogin.PartyId, Ident.UserLogin.LoginName, Ident.UserSecurityAnswer.SecurityQuestionId, Ident.UserSecurityAnswer.Answer, 
	Ident.SecurityQuestion.Question, Ident.UserSecurityAnswer.UserSecurityAnswerId FROM Ident.UserLogin 
	INNER JOIN Ident.UserSecurityAnswer ON Ident.UserLogin.UserId = Ident.UserSecurityAnswer.UserId 
	INNER JOIN Ident.SecurityQuestion ON Ident.UserSecurityAnswer.SecurityQuestionId = Ident.SecurityQuestion.SecurityQuestionId
	WHERE ([Ident].UserLogin.LoginName = @EnterpriseUserName)

END
GO
PRINT N'Creating [Ident].[IdentityProviderType]'
GO
CREATE TABLE [Ident].[IdentityProviderType]
(
[IdentityProviderTypeId] [int] NOT NULL IDENTITY(1, 1),
[Name] [nvarchar] (50) NOT NULL,
[Description] [nvarchar] (50) NULL
)
GO
PRINT N'Creating primary key [PK_IdentityProvider] on [Ident].[IdentityProviderType]'
GO
ALTER TABLE [Ident].[IdentityProviderType] ADD CONSTRAINT [PK_IdentityProvider] PRIMARY KEY CLUSTERED  ([IdentityProviderTypeId])
GO
PRINT N'Creating [Ident].[IdentityProviderSettingType]'
GO
CREATE TABLE [Ident].[IdentityProviderSettingType]
(
[IdentityProviderSettingTypeId] [int] NOT NULL IDENTITY(1, 1),
[IdentityProviderTypeId] [int] NOT NULL,
[Name] [nvarchar] (50) NOT NULL
)
GO
PRINT N'Creating primary key [PK_IdentityProviderSettingType] on [Ident].[IdentityProviderSettingType]'
GO
ALTER TABLE [Ident].[IdentityProviderSettingType] ADD CONSTRAINT [PK_IdentityProviderSettingType] PRIMARY KEY CLUSTERED  ([IdentityProviderSettingTypeId])
GO
PRINT N'Creating [Ident].[IdentityProviderSetting]'
GO
CREATE TABLE [Ident].[IdentityProviderSetting]
(
[IdentityProviderSettingId] [int] NOT NULL IDENTITY(1, 1),
[IdentityProviderSettingTypeId] [int] NOT NULL,
[Value] [nvarchar] (255) NOT NULL
)
GO
PRINT N'Creating primary key [PK_IdentityProviderSetting] on [Ident].[IdentityProviderSetting]'
GO
ALTER TABLE [Ident].[IdentityProviderSetting] ADD CONSTRAINT [PK_IdentityProviderSetting] PRIMARY KEY CLUSTERED  ([IdentityProviderSettingId])
GO
PRINT N'Creating [Ident].[ContactMechanismIdentity]'
GO
CREATE TABLE [Ident].[ContactMechanismIdentity]
(
[ContactMechanismIdentityId] [int] NOT NULL IDENTITY(1, 1),
[ContactMechanismId] [int] NOT NULL,
[IdentityProviderSettingId] [int] NOT NULL,
[FromDate] [datetime] NOT NULL CONSTRAINT [DF__ContactMe__FromD__11158940] DEFAULT (getutcdate()),
[ThruDate] [datetime] NULL
)
GO
PRINT N'Creating primary key [PK_ContactMechanismIdentity] on [Ident].[ContactMechanismIdentity]'
GO
ALTER TABLE [Ident].[ContactMechanismIdentity] ADD CONSTRAINT [PK_ContactMechanismIdentity] PRIMARY KEY CLUSTERED  ([ContactMechanismIdentityId])
GO
PRINT N'Creating [Ident].[ProviderConfigurationByName]'
GO
CREATE PROCEDURE [Ident].[ProviderConfigurationByName] (
	@providerName nvarchar(50)
)
AS
BEGIN
	SELECT	pvt.ContactMechanismId AS ProviderPortfolioId,
			pvt.PartyId AS PortfolioIdId,
			pvt.AuthenticationMode,
			CONVERT(bit, pvt.ValidateIssuer) AS ValidateIssuer,
			pvt.IdentityTypeName AS ProviderName,
			pvt.Description,
			pvt.AuthenticationType,
			pvt.Caption,
			pvt.ProviderClientId,
			pvt.AuthorityUri,
			pvt.PostLogoutRedirectUri,
			pvt.RedirectUri,
			pvt.TokenValidationAuthenticationType,
			pvt.Scope,
			pvt.OktaEntityId,
			pvt.OktaMetadataLocation,
			pvt.ClientSecret
	FROM	(
			SELECT	p.PartyId,
					p.RealPageId,
					cmi.ContactMechanismId,
					ipt.IdentityProviderTypeId,
					ipt.Name AS IdentityTypeName,
					ipt.Description,
					ipst.Name AS SettingTypeName,
					ips.Value
			FROM	Enterprise.Party p
					INNER JOIN Enterprise.Organization o ON (p.PartyId = o.PartyId)
					INNER JOIN Enterprise.PartyContactMechanism pcm ON (p.PartyId = pcm.PartyId)
					INNER JOIN Ident.ContactMechanismIdentity cmi ON (pcm.ContactMechanismId = cmi.ContactMechanismId)
					INNER JOIN [Ident].[IdentityProviderSetting] ips ON (cmi.IdentityProviderSettingId = ips.IdentityProviderSettingId)
					INNER JOIN [Ident].[IdentityProviderSettingType] ipst ON (ips.IdentityProviderSettingTypeId = ipst.IdentityProviderSettingTypeId)
					INNER JOIN [Ident].[IdentityProviderType] ipt ON ipst.IdentityProviderTypeId = ipt.IdentityProviderTypeId
			WHERE	ipt.Name = @providerName
			) p
	PIVOT	(
			MAX(Value) FOR SettingTypeName IN (
				[AuthenticationType],
				[Caption],
				[ProviderClientId],
				[AuthorityUri],
				[PostLogoutRedirectUri],
				[RedirectUri],
				[AuthenticationMode],
				[ValidateIssuer],
				[TokenValidationAuthenticationType],
				[Scope],
				[OktaEntityId],
				[OktaMetadataLocation],
				[ClientSecret]
				)
			) AS pvt;
END
GO
PRINT N'Creating [Ident].[ResetEnterpriseUserCredential]'
GO
CREATE PROCEDURE [Ident].[ResetEnterpriseUserCredential] ( 
	@realPageId as uniqueidentifier,
	@newPasswordHash as nvarchar(255),
	@newPasswordSalt as nvarchar(255) 
)
AS
BEGIN
	BEGIN TRY
		
		declare @UserId as bigint,
		@oldPassword as nvarchar(255),
		@oldPasswordSalt as nvarchar(255),
		@currentUtcDate datetime

		select @currentUtcDate = GETUTCDATE()
			
		SELECT @UserId=Ident.UserLogin.UserId, @oldPassword=Ident.UserLogin.PasswordHash, @oldPasswordSalt=Ident.UserLogin.PasswordSalt  FROM Enterprise.Party 
		INNER JOIN Ident.UserLogin ON Enterprise.Party.PartyId = Ident.UserLogin.PartyId
		where Enterprise.Party.RealPageId =@realPageId
					 				 
		UPDATE	[ident].[UserLogin]
		SET		PasswordHash=@newPasswordHash, PasswordSalt = @newPasswordSalt, PasswordModifiedDate=@currentUtcDate WHERE userId = @UserId

		-- insert old password in history table
		if(@oldPassword is not null)
			INSERT INTO [Ident].[PasswordHistory] ([UserId],[ActivityId],[ChangedPasswordHash],[ChangedPasswordSalt],[ChangedPasswordDateTime])
			VALUES(@UserId,2,@oldPassword,@oldPasswordSalt,@currentUtcDate)
 

		SELECT	@@ROWCOUNT AS 'rowCount',
				@UserId AS Id, 
				0	AS errorNumber,
				'' AS errorMessage		 
	END TRY  
	BEGIN CATCH
		SELECT	@@ROWCOUNT AS 'rowCount',
				0 AS Id,
				ERROR_NUMBER() AS errorNumber,
				ERROR_MESSAGE() AS errorMessage
	END CATCH
END
GO
PRINT N'Creating [Ident].[ActivityAttempts]'
GO
CREATE TABLE [Ident].[ActivityAttempts]
(
[ActivityAttemptsId] [int] NOT NULL IDENTITY(1, 1),
[ActivityId] [int] NOT NULL,
[EnterpriseUserName] [nvarchar] (50) NOT NULL,
[AuthenticationServiceId] [nvarchar] (50) NULL,
[AttemptCount] [tinyint] NOT NULL CONSTRAINT [DF_ActivityAttempts_AttemptCount] DEFAULT ((0)),
[IpAddress] [nvarchar] (50) NULL,
[BrowserType] [nvarchar] (20) NULL,
[BrowserName] [nvarchar] (20) NULL,
[Version] [nvarchar] (10) NULL,
[Platform] [nvarchar] (20) NULL,
[IsMobile] [bit] NOT NULL CONSTRAINT [DF_ActivityAttempts_IsMobile] DEFAULT ((0)),
[DeviceType] [nvarchar] (20) NULL,
[LastAttemptDateTime] [smalldatetime] NOT NULL CONSTRAINT [DF_ActivityAttempts_LastAttemptDateTime] DEFAULT (getutcdate()),
[Timezone] [nvarchar] (100) NULL
)
GO
PRINT N'Creating primary key [PK_ActivityAttempts] on [Ident].[ActivityAttempts]'
GO
ALTER TABLE [Ident].[ActivityAttempts] ADD CONSTRAINT [PK_ActivityAttempts] PRIMARY KEY CLUSTERED  ([ActivityAttemptsId])
GO
PRINT N'Creating [Ident].[UpdateActivityAttempt]'
GO
CREATE PROCEDURE [Ident].[UpdateActivityAttempt]
 		@enterpriseUserName as nvarchar(255),
		@activityId as int,
		@browserName as nvarchar(20)='',
		@browserType as nvarchar(20)='',
		@ipAddress as nvarchar(50)='',
		@isMobile as bit=0,
		@platform as nvarchar(20)='', 
		@version as nvarchar(10)='',
		@deviceType as nvarchar(20)='',
		@timezone as nvarchar(100)='',
		@authenticationServiceId as nvarchar(50)='' 

AS
BEGIN

	SET NOCOUNT ON;
	 
	declare @AttemptCount as int,
	@maxActivitycount as int,
	@ActivityAttemptsId as int,
	@ActivityTokenExpirationMinutes as tinyint,
	@currentUtcDate datetime

	select @currentUtcDate=GETUTCDATE()
	 
	select @maxActivitycount = MaxActivityAttemptCount, @ActivityTokenExpirationMinutes=ActivityTokenExpirationMinutes from Ident.Activity where activityId=@activityId
 
	select top 1 @AttemptCount=AttemptCount, @ActivityAttemptsId=ActivityAttemptsId from Ident.[ActivityAttempts] where 
		[EnterpriseUserName]=@enterpriseUserName and activityId=@activityId and LastAttemptDateTime >= dateadd(minute, -@ActivityTokenExpirationMinutes, @currentUtcDate)   
		order by [ActivityAttemptsId] desc
	 

	IF @activityId = 10 -- unlock user - login / forgot password
		BEGIN				
			update Ident.[ActivityAttempts]	set [AttemptCount] = 0 
			where (activityid= 1 or activityid= 2 or activityid=5) and EnterpriseUserName=@enterpriseUserName and [LastAttemptDateTime] >DATEADD(minute,-60, @currentUtcDate)				
		END
	ELSE IF @activityId = 7 -- LoginSuccess activity
		BEGIN
			-- insert unique record for LoginSuccess			
			INSERT INTO Ident.[ActivityAttempts]
						([ActivityId] ,[EnterpriseUserName] ,[AttemptCount]  ,[IpAddress] ,[BrowserType] ,
						[BrowserName]  ,[Version] ,[Platform]  ,[IsMobile]  ,[LastAttemptDateTime], [DeviceType], [Timezone], [AuthenticationServiceId])
					VALUES
						(@activityId ,@enterpriseUserName,1,@ipAddress,@browserType
						,@browserName ,@version,@platform,@isMobile,@currentUtcDate, @deviceType, @timezone, @authenticationServiceId)

			select * from Ident.[ActivityAttempts] where ActivityAttemptsId = (select scope_identity())

			-- after successful login reset falied login activity count for last 1 hr
			update Ident.[ActivityAttempts]	set [AttemptCount] = 0 
			where activityid= 1 and EnterpriseUserName=@enterpriseUserName and [LastAttemptDateTime] >DATEADD(minute,-60,@currentUtcDate)				

			update Ident.[UserLogin] set [LastLoginDate] = GETUTCDATE() WHERE LoginName = @enterpriseUserName

		END
	ELSE IF @AttemptCount >= @maxActivitycount	
		BEGIN
			select top 1 * from Ident.[ActivityAttempts] where  activityAttemptsId=@ActivityAttemptsId 
			-- increment @AttemptCount
			update Ident.[ActivityAttempts]
			set [AttemptCount]  = @AttemptCount +1 where  activityAttemptsId=@ActivityAttemptsId  
		END		
	ELSE if @AttemptCount is null or @AttemptCount = 0			
			BEGIN
			-- insert record
				 INSERT INTO Ident.[ActivityAttempts]
					   ([ActivityId] ,[EnterpriseUserName] ,[AttemptCount]  ,[IpAddress] ,[BrowserType] ,
						[BrowserName]  ,[Version] ,[Platform]  ,[IsMobile]  ,[LastAttemptDateTime], [DeviceType], [Timezone], [AuthenticationServiceId])
				 VALUES
					   (@activityId ,@enterpriseUserName,1,@ipAddress,@browserType
					   ,@browserName ,@version,@platform,@isMobile,@currentUtcDate, @deviceType, @timezone, @authenticationServiceId)
						 
				select top 1 * from Ident.[ActivityAttempts] where [EnterpriseUserName]=@enterpriseUserName 
				and activityId=@activityId and LastAttemptDateTime < DATEADD(minute,-@ActivityTokenExpirationMinutes,@currentUtcDate)
				order by [ActivityAttemptsId] desc
			END 
	ELSE
			BEGIN
				-- increment @AttemptCount
				update Ident.[ActivityAttempts]
				set [AttemptCount]  = @AttemptCount +1 where  activityAttemptsId=@ActivityAttemptsId   
    
				select top 1 * from Ident.[ActivityAttempts] where activityAttemptsId=@ActivityAttemptsId  
				order by [ActivityAttemptsId] desc
			END 
	 
END
GO
PRINT N'Creating [Ident].[ActivityToken]'
GO
CREATE TABLE [Ident].[ActivityToken]
(
[ActivityTokenId] [int] NOT NULL IDENTITY(1, 1),
[ActivityId] [int] NOT NULL,
[RealPageId] [uniqueidentifier] NOT NULL,
[ActivityToken] [nvarchar] (100) NOT NULL,
[IsActive] [bit] NOT NULL CONSTRAINT [DF_ActivityToken_IsActive] DEFAULT ((0)),
[CreateDateTime] [smalldatetime] NOT NULL CONSTRAINT [DF_ActivityToken_CreateDateTime] DEFAULT (getutcdate()),
[ExpireDateTime] [smalldatetime] NOT NULL
)
GO
PRINT N'Creating primary key [PK_ActivityToken] on [Ident].[ActivityToken]'
GO
ALTER TABLE [Ident].[ActivityToken] ADD CONSTRAINT [PK_ActivityToken] PRIMARY KEY CLUSTERED  ([ActivityTokenId])
GO
PRINT N'Creating [Ident].[UpdateEnterpriseUserCredential]'
GO
CREATE PROCEDURE [Ident].[UpdateEnterpriseUserCredential]
	@EnterpriseUserName as nvarchar(255),
	@correctAnswerToken as nvarchar(50),
	@ActivityId		as int,
	@NewPasswordHash as  nvarchar(255),
	@passwordSalt as  nvarchar(255)	 
AS
BEGIN
	
	SET NOCOUNT ON;

	declare @UserId as bigint,
	@realPageId as uniqueidentifier,
	@oldPassword as nvarchar(255),
	@oldPasswordSalt as nvarchar(255),
	@currentUtcDate as datetime

	select @currentUtcDate = getutcdate()

	SELECT @UserId=ul.UserId, @oldPassword=ul.PasswordHash, @oldPasswordSalt=ul.PasswordSalt, @realPageId=p.RealPageId
	FROM Ident.UserLogin ul 
	INNER JOIN Enterprise.Party p ON ul.PartyId = p.PartyId where ul.LoginName=@EnterpriseUserName
		 

	if Exists(select top 1  [ActivityToken] from Ident.ActivityToken
	 where [ActivityId]=@ActivityId and IsActive =1 and realPageId=@realPageId and ActivityToken=@correctAnswerToken)  
	 Begin
		-- update password
		Update Ident.UserLogin set PasswordHash=@NewPasswordHash,PasswordSalt=@passwordSalt,PasswordModifiedDate=@currentUtcDate where UserId=@UserId 

		-- insert old pwd in history table
		IF(@oldPassword IS NOT NULL)
			INSERT INTO Ident.[PasswordHistory]([UserId],[ActivityId],[ChangedPasswordHash],[ChangedPasswordSalt],[ChangedPasswordDateTime])
			VALUES (@UserId,@ActivityId,@oldPassword,@oldPasswordSalt,@currentUtcDate)
		
		-- reset token flags
		UPDATE Ident.[ActivityToken] set isActive=0 WHERE realPageId=@realPageId and activityId=2 OR activityId=6  
        UPDATE Ident.[ActivityAttempts] SET  [AttemptCount] = 0 WHERE [EnterpriseUserName] = @EnterpriseUserName 
		and LastAttemptDateTime >=   dateadd(day, -3, @currentUtcDate) and ([ActivityId] = 2 or [ActivityId] = 5 or [ActivityId] = 6)
	 end
	else
		select @UserId = null --RAISERROR('Activity token expired', 16,16)

	select @UserId
END
GO
PRINT N'Creating [Ident].[VerifyPasswordHistory]'
GO
CREATE PROCEDURE [Ident].[VerifyPasswordHistory]
    @enterpriseUserName AS NVARCHAR(255) ,
    @NewPasswordHash AS NVARCHAR(255) ,
    @MinPasswordtoRemember AS INT = 5
AS
    BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
        SET NOCOUNT ON;

        SELECT  ISNULL(MAX(CASE WHEN p.ChangedPasswordHash = @NewPasswordHash
                                THEN 1
                                ELSE 0
                           END), 0) AS PasswordExists
        FROM    [ident].UserLogin u
                CROSS APPLY ( SELECT TOP ( @MinPasswordtoRemember )
                                        PH.UserId ,
                                        PH.ChangedPasswordHash ,
                                        PH.ChangedPasswordDateTime
                              FROM      [ident].PasswordHistory PH
                              WHERE     PH.UserId = u.UserId
                              ORDER BY  PH.ChangedPasswordDateTime DESC
                            ) AS p
        WHERE   u.LoginName = @enterpriseUserName;

    END;
GO
PRINT N'Creating [Ident].[ListIdentityProviderByIdentityProviderTypeId]'
GO
CREATE PROCEDURE [Ident].[ListIdentityProviderByIdentityProviderTypeId] (
	@IdentityProviderTypeId int
)
AS
BEGIN
	SELECT	pvt.ContactMechanismId AS ProviderPortfolioId,
			pvt.PartyId AS PortfolioIdId,
			pvt.AuthenticationMode,
			CONVERT(bit, pvt.ValidateIssuer) AS ValidateIssuer,
			pvt.IdentityTypeName AS ProviderName,
			pvt.Description,
			pvt.AuthenticationType,
			pvt.Caption,
			pvt.ProviderClientId,
			pvt.AuthorityUri,
			pvt.PostLogoutRedirectUri,
			pvt.RedirectUri,
			pvt.TokenValidationAuthenticationType,
			pvt.Scope,
			pvt.OktaEntityId,
			pvt.OktaMetadataLocation,
			pvt.ClientSecret
	FROM	(
			SELECT	p.PartyId,
					p.RealPageId,
					cmi.ContactMechanismId,
					ipt.IdentityProviderTypeId,
					ipt.Name AS IdentityTypeName,
					ipt.Description,
					ipst.Name AS SettingTypeName,
					ips.Value
			FROM	Enterprise.Party p
					INNER JOIN Enterprise.Organization o ON (p.PartyId = o.PartyId)
					INNER JOIN Enterprise.PartyContactMechanism pcm ON (p.PartyId = pcm.PartyId)
					INNER JOIN Ident.ContactMechanismIdentity cmi ON (pcm.ContactMechanismId = cmi.ContactMechanismId)
					INNER JOIN [Ident].[IdentityProviderSetting] ips ON (cmi.IdentityProviderSettingId = ips.IdentityProviderSettingId)
					INNER JOIN [Ident].[IdentityProviderSettingType] ipst ON (ips.IdentityProviderSettingTypeId = ipst.IdentityProviderSettingTypeId)
					INNER JOIN [Ident].[IdentityProviderType] ipt ON ipst.IdentityProviderTypeId = ipt.IdentityProviderTypeId
			WHERE	ipt.IdentityProviderTypeId = @IdentityProviderTypeId
			) p
	PIVOT	(
			MAX(Value) FOR SettingTypeName IN (
				[AuthenticationType],
				[Caption],
				[ProviderClientId],
				[AuthorityUri],
				[PostLogoutRedirectUri],
				[RedirectUri],
				[AuthenticationMode],
				[ValidateIssuer],
				[TokenValidationAuthenticationType],
				[Scope],
				[OktaEntityId],
				[OktaMetadataLocation],
				[ClientSecret]
				)
			) AS pvt;
END
GO
PRINT N'Creating [Ident].[ListIdentityProviderByIdentityProviderTypeName]'
GO
CREATE PROCEDURE [Ident].[ListIdentityProviderByIdentityProviderTypeName] (
	@IdentityProviderTypeName VARCHAR(50)
)
AS
BEGIN
	SELECT	pvt.AuthenticationMode,
			CONVERT(bit, pvt.ValidateIssuer) AS ValidateIssuer,
			pvt.IdentityTypeName AS ProviderName,
			pvt.Description,
			pvt.AuthenticationType,
			pvt.Caption,
			pvt.ProviderClientId,
			pvt.AuthorityUri,
			pvt.PostLogoutRedirectUri,
			pvt.RedirectUri,
			pvt.TokenValidationAuthenticationType,
			pvt.Scope,
			pvt.OktaEntityId,
			pvt.OktaMetadataLocation,
			pvt.ClientSecret
	FROM	(
			SELECT	p.PartyId,
					p.RealPageId,
					cmi.ContactMechanismId,
					ipt.IdentityProviderTypeId,
					ipt.Name AS IdentityTypeName,
					ipt.Description,
					ipst.Name AS SettingTypeName,
					ips.Value
			FROM	Enterprise.Party p
					INNER JOIN Enterprise.Organization o ON (p.PartyId = o.PartyId)
					INNER JOIN Enterprise.PartyContactMechanism pcm ON (p.PartyId = pcm.PartyId)
					INNER JOIN Ident.ContactMechanismIdentity cmi ON (pcm.ContactMechanismId = cmi.ContactMechanismId)
					INNER JOIN [Ident].[IdentityProviderSetting] ips ON (cmi.IdentityProviderSettingId = ips.IdentityProviderSettingId)
					INNER JOIN [Ident].[IdentityProviderSettingType] ipst ON (ips.IdentityProviderSettingTypeId = ipst.IdentityProviderSettingTypeId)
					INNER JOIN [Ident].[IdentityProviderType] ipt ON ipst.IdentityProviderTypeId = ipt.IdentityProviderTypeId
			WHERE	ipt.Name = @IdentityProviderTypeName
			) p
	PIVOT	(
			MAX(Value) FOR SettingTypeName IN (
				[AuthenticationType],
				[Caption],
				[ProviderClientId],
				[AuthorityUri],
				[PostLogoutRedirectUri],
				[RedirectUri],
				[AuthenticationMode],
				[ValidateIssuer],
				[TokenValidationAuthenticationType],
				[Scope],
				[OktaEntityId],
				[OktaMetadataLocation],
				[ClientSecret]
				)
			) AS pvt;
END
GO
PRINT N'Creating [Ident].[GetUserLoginByName]'
GO
CREATE PROCEDURE [Ident].[GetUserLoginByName] (
    @EnterpriseUserName  varchar(255)
)
AS
BEGIN
	SELECT	ul.UserId,
			ul.PartyId,
			ul.[LoginName],
			ul.PasswordModifiedDate,
			p.RealPageId,
			ul.PasswordHash,
			ul.PasswordSalt,
			ul.FromDate,
			ul.ThruDate
	FROM	Ident.UserLogin ul
			JOIN Enterprise.Party p ON p.PartyId = ul.PartyId
	WHERE	ul.[LoginName] = @EnterpriseUserName
END
GO
PRINT N'Creating [Ident].[CreateActivityToken]'
GO
CREATE PROCEDURE [Ident].[CreateActivityToken]
	@realPageId uniqueidentifier ,
	@activityId as int
AS
BEGIN
	SET NOCOUNT ON;
	Declare @token varchar(50), 
	@currentUtcDate  datetime,	
	@OldActivityTokenId as int,
	@ActivityTokenExpirationMinutes as int

	-- Get expiration time for inputted activity
	select @ActivityTokenExpirationMinutes=ActivityTokenExpirationMinutes from [Ident].Activity where activityId=@activityId

	select @token=NEWID ()
	select @currentUtcDate = GetUtcDate()

	-- Check if any active token already exists for user
	SELECT @OldActivityTokenId=[ActivityTokenId]  
	FROM [Ident].[ActivityToken]
	where [ActivityId]=@ActivityId and RealPageId=@realPageId and IsActive=1 

	-- if exist then de-activate it
	if  @OldActivityTokenId is not null
		Update [Ident].[ActivityToken] set IsActive=0 where [ActivityId]=@ActivityId and RealPageId=@realPageId
	end
	
	-- create new token with expiration time
	INSERT INTO [Ident].[ActivityToken]
			([ActivityId], [RealPageId],[ActivityToken],[IsActive],[CreateDateTime],[ExpireDateTime])
	VALUES
			(@ActivityId,@realPageId,@token,1,@currentUtcDate,DateAdd(minute,@ActivityTokenExpirationMinutes,@currentUtcDate))

	-- return new token
	select @token as ActivityToken
GO
PRINT N'Creating [Ident].[CreateSecurityQuestionAnswers]'
GO
CREATE PROCEDURE [Ident].[CreateSecurityQuestionAnswers]
(
	@enterpriseUserName as nvarchar(255),
	@activityToken as nvarchar(50),
	@activityId as int,
	@securityQuestion1Id as int,
	@securityAnswer1 as nvarchar(50),
	@securityQuestion2Id as int,
	@securityAnswer2 as nvarchar(50),
	@securityQuestion3Id as int,
	@securityAnswer3 as nvarchar(50)
)
AS
BEGIN

	BEGIN TRY

	declare @realPageId as uniqueidentifier,
	@userid as bigint,
	@insertDateTime as smalldatetime

	SELECT   @realPageId = p.RealPageId, @userid=u.userid  FROM Ident.UserLogin u
	INNER JOIN Enterprise.Party p ON u.PartyId = p.PartyId
	where u.LoginName=@EnterpriseUserName

	if Exists(select top 1  [ActivityToken] from [Ident].ActivityToken
	where [ActivityId]=@ActivityId and IsActive =1 and  realPageId=@realPageId and ActivityToken=@activityToken) 
	Begin
	
		select @insertDateTime=getutcdate()
		
		delete from [Ident].[UserSecurityAnswer] where userid=@userid -- delete old questions-answers

		INSERT INTO [Ident].[UserSecurityAnswer]
           ([UserId],[SecurityQuestionId],[Answer],[CreateDateTime])
        VALUES
           (@UserId,@securityQuestion1Id,@securityAnswer1,@insertDateTime),
		   (@UserId,@securityQuestion2Id,@securityAnswer2,@insertDateTime),
		   (@UserId,@securityQuestion3Id,@securityAnswer3,@insertDateTime)


		SELECT	@@ROWCOUNT AS 'rowCount',
				@UserId AS Id,
				0	AS errorNumber,
				'' AS errorMessage	
		end	

	END TRY  
	BEGIN CATCH
		SELECT	@@ROWCOUNT AS 'rowCount',
				0 AS Id,
				ERROR_NUMBER() AS errorNumber,
				ERROR_MESSAGE() AS errorMessage
	END CATCH
END
GO
PRINT N'Creating [Ident].[GetActivityAttemptExceeds]'
GO
CREATE PROCEDURE [Ident].[GetActivityAttemptExceeds]
 		@enterpriseUserName as nvarchar(255),
		@activityId as int 
AS
BEGIN

	SET NOCOUNT ON;
	 
	declare @AttemptCount as int,
	@maxActivitycount as int,
	@ActivityTokenExpirationMinutes as int
	 
	select @maxActivitycount = MaxActivityAttemptCount, @ActivityTokenExpirationMinutes=ActivityTokenExpirationMinutes from [Ident].Activity where activityId=@activityId
	 
	select top 1 @AttemptCount=AttemptCount from [Ident].[ActivityAttempts] 
	where [EnterpriseUserName]=@enterpriseUserName and activityId=@activityId and LastAttemptDateTime  >=   dateadd(minute, -@ActivityTokenExpirationMinutes, getutcdate()) order by LastAttemptDateTime desc
 
	select @AttemptCount as  AttemptCount,   @maxActivitycount as  maxActivitycount, @ActivityTokenExpirationMinutes as  ActivityTokenExpirationMinutes
END
GO
PRINT N'Creating [Ident].[GetActivityToken]'
GO
CREATE PROCEDURE [Ident].[GetActivityToken]
	@EnterpriseUserName as nvarchar(255),
	@ActivityToken as nvarchar(50),
	@ActivityId		as int
AS
BEGIN
	
	SET NOCOUNT ON;

	declare @UserId as bigint,
	@realPageId as uniqueidentifier,
	@ActivityTokenExpirationMinutes as int
	
	select @ActivityTokenExpirationMinutes=ActivityTokenExpirationMinutes from [Ident].Activity where activityId=@activityId
		 
	SELECT @UserId=u.UserId, @realPageId=p.RealPageId	FROM Ident.UserLogin u
	INNER JOIN Enterprise.Party p ON u.PartyId = p.PartyId
	where u.LoginName=@EnterpriseUserName
	 
	select top 1  @UserId as EnterpriseUserId, [ActivityToken] as Token, @realPageId as realPageId from [Ident].[ActivityToken]
	where [ActivityId]= @ActivityId and IsActive =1 and  realPageId=@realPageId and  ActivityToken=@ActivityToken and [ExpireDateTime] >  dateadd(minute,-@ActivityTokenExpirationMinutes,GetUtcDate())
	
END
GO
PRINT N'Creating [Ident].[GetAllSecurityQuestions]'
GO
CREATE PROCEDURE [Ident].[GetAllSecurityQuestions] (
	@enterpriseUserName as nvarchar(255) -- added to get user (custom) security questions in the future
)
AS
BEGIN
	 
	SET NOCOUNT ON;
	  
	SELECT [SecurityQuestionId],[Question],[IsActive]
	FROM [Ident].[SecurityQuestion] where IsActive=1

END
GO
PRINT N'Creating [Ident].[GetUserSelectedSecurityQuestions]'
GO
CREATE PROCEDURE [Ident].[GetUserSelectedSecurityQuestions]
	@realPageId uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

		SELECT Ident.UserSecurityAnswer.SecurityQuestionId, Ident.SecurityQuestion.Question
		FROM Ident.UserLogin INNER JOIN Ident.UserSecurityAnswer ON Ident.UserLogin.UserId = Ident.UserSecurityAnswer.UserId
		INNER JOIN Ident.SecurityQuestion ON Ident.UserSecurityAnswer.SecurityQuestionId = Ident.SecurityQuestion.SecurityQuestionId
		INNER JOIN Enterprise.Party ON Ident.UserLogin.PartyId = Enterprise.Party.PartyId
		where Enterprise.Party.RealPageId = @realPageId

END
GO
PRINT N'Creating [Ident].[GetIdentityProviderTypeByLoginName]'
GO
CREATE PROCEDURE [Ident].[GetIdentityProviderTypeByLoginName] (
	@LoginName varchar(255),
	@SettingTypeName NVARCHAR(50) = 'AuthenticationType'
)
AS
BEGIN
		
		DECLARE @NOW  DATETIME = GETUTCDATE();

        SELECT DISTINCT 
               ips.Value
        FROM   Ident.UserLogin ul
               JOIN Enterprise.PartyRelationship pr ON ul.PartyId = pr.PartyIdFrom
               JOIN Enterprise.RelationshipType rt ON rt.RelationshipTypeId = pr.PartyRelationshipTypeId
               JOIN Enterprise.PartyContactMechanism pcm ON pr.PartyIdTo = pcm.PartyId
               JOIN Ident.ContactMechanismIdentity cmi ON cmi.ContactMechanismId = pcm.ContactMechanismId
               JOIN Ident.IdentityProviderSetting ips ON ips.IdentityProviderSettingId = cmi.IdentityProviderSettingId
               JOIN Ident.IdentityProviderSettingType ipst ON ipst.IdentityProviderSettingTypeId = ips.IdentityProviderSettingTypeId
               JOIN Ident.IdentityProviderType ipt ON ipt.IdentityProviderTypeId = ipst.IdentityProviderTypeId
        WHERE  rt.Name = 'Employment'
               AND cmi.ContactMechanismIdentityId IS NOT NULL
			   AND ipst.Name = @SettingTypeName
               AND ul.LoginName = @LoginName
			   AND ((@NOW BETWEEN pcm.FromDate AND pcm.ThruDate) OR (@NOW >= pcm.FromDate AND pcm.ThruDate IS NULL)) 
			   AND ((@NOW BETWEEN pcm.FromDate AND pcm.ThruDate) OR (@NOW >= pcm.FromDate AND pcm.ThruDate IS NULL)) 
			   AND ((@NOW BETWEEN ul.FromDate AND ul.ThruDate) OR (@NOW >= ul.FromDate AND ul.ThruDate IS NULL));
END
GO
PRINT N'Creating [Ident].[GetProductSamlDetails]'
GO
CREATE PROCEDURE [Ident].[GetProductSamlDetails] (
	@PersonaId bigint,
	@ProductId int
)
AS
BEGIN

		DECLARE @NOW  DATETIME = GETUTCDATE();

	SELECT	sa.SamlAttributeId,
			sa.Name,
			sat.Name AS [Type],
			sua.SamlUserAttributeId,
			sua.Value
	FROM	Person.Persona p
			INNER JOIN Ident.SamlUserAttribute sua ON (p.PersonaId = sua.PersonaId)
			INNER JOIN Ident.SamlAttribute sa ON (sua.SamlAttributeId = sa.SamlAttributeId)
			INNER JOIN Ident.SamlAttributeType sat ON (sa.SamlAttributeTypeId = sat.SamlAttributeTypeId)
	WHERE	p.PersonaId = @PersonaId
	AND		sua.ProductId = @ProductId
	AND ((@NOW BETWEEN p.FromDate AND p.ThruDate) OR (@NOW >= p.FromDate AND p.ThruDate IS NULL))
END
GO
PRINT N'Creating [Ident].[SamlProductSettings]'
GO
CREATE TABLE [Ident].[SamlProductSettings]
(
[SamlProductSettingsId] [int] NOT NULL IDENTITY(1, 1),
[ProductId] [int] NOT NULL,
[LoginUri] [nvarchar] (100) NOT NULL,
[SigningCertificateThumbprint] [nvarchar] (50) NOT NULL,
[SubjectIdSamlAttribute] [nvarchar] (20) NOT NULL
)
GO
PRINT N'Creating primary key [PK_SamlProductSettings] on [Ident].[SamlProductSettings]'
GO
ALTER TABLE [Ident].[SamlProductSettings] ADD CONSTRAINT [PK_SamlProductSettings] PRIMARY KEY CLUSTERED  ([SamlProductSettingsId])
GO
PRINT N'Creating [Ident].[GetProductSamlSettings]'
GO
CREATE PROCEDURE [Ident].[GetProductSamlSettings] (
	@ProductId int
)
AS
BEGIN
	SELECT	SamlProductSettingsId,
					ProductId,
					LoginUri,
					SigningCertificateThumbprint,
					SubjectIdSamlAttribute
	FROM		Ident.SamlProductSettings
	WHERE	ProductId = @ProductId
END
GO
PRINT N'Creating [Ident].[GetProductsByPersonaId]'
GO
CREATE PROCEDURE [Ident].[GetProductsByPersonaId] (
	@personaId bigint,
	@productId int = 0
	, @productSelectType nvarchar(10) = null -- Added by Gilbert so that product can be filtered by "type" (IsFavorite, IsResource)
)
AS
BEGIN

	DECLARE @NOW DATETIME = GETUTCDATE();

	SELECT	pvt.PersonaId,
			pvt.PersonPartyId,
			pvt.RealPageId,
			pvt.OrganizationPartyId,
			pvt.OrganizationName,
			pvt.ProductId,
			pvt.ProductName,
			pvt.ProductDescription,
			pvt.PersonPartyId,
			pvt.TotalAccounts,
			pvt.ClientId,
			pvt.ClassName,
			pvt.SettingsUrl,
			pvt.ProductUrl,
			pvt.TitleId,
			CONVERT(uniqueidentifier, CASE WHEN LEN(LTRIM(RTRIM(pvt.TitleUniqueId))) = 0 THEN NULL ELSE pvt.TitleUniqueId END ) AS TitleUniqueId,
			CONVERT(tinyint, pvt.IsNewTab) AS IsNewTab,
			pvt.MetatagUniqueId,
			CONVERT(tinyint, pvt.IsResource) AS IsResource,
			CONVERT(tinyint, pvt.IsFavorite) AS IsFavorite,
			pvt.Subsolution,
			pvt.Family,
			pvt.FamilyId,
			pvt.SolutionId,
			pvt.Solution,
			pvt.LearnMore
	FROM	(
		SELECT	p.PersonaId,
				pa.RealPageId,
				o.PartyId AS OrganizationPartyId,
				o.Name AS OrganizationName,
				pr.ProductId,
				pr.Name AS ProductName,
				pr.Description AS ProductDescription,
				p.PersonPartyId AS PersonPartyId,
				pst.Name AS ProductSettingTypeName,
				1 AS TotalAccounts,
			
	ps.Value,
				pts.ProductTypeId as SolutionId, 
				pts.Name as Solution, 
				ptf.ProductTypeId AS FamilyId,  
				ptf.Name as Family
		FROM	Person.Persona p
				INNER JOIN Enterprise.Party pa ON (p.PersonPartyId = pa.PartyId)
				INNER JOIN Enterprise.Organization o ON (p.OrganizationPartyId = o.PartyId)
				INNER JOIN Ident.SamlUserAttribute sua ON (p.PersonaId = sua.PersonaId)
				INNER JOIN Enterprise.Product pr ON (sua.ProductId = pr.ProductId)
				INNER JOIN Enterprise.ProductSetting ps ON (pr.ProductId = ps.ProductId)
				INNER JOIN Enterprise.ProductSettingType pst ON (ps.ProductSettingTypeId = pst.ProductSettingTypeId)
				-- Changed to left join so that products with no parents (IsResource type are included in result set)

				LEFT JOIN Enterprise.ProductType pts ON pts.ProductTypeId = pr.ProductTypeId                    
				LEFT JOIN Enterprise.ProductType ptf ON ptf.ProductTypeId = pts.ParentProductTypeId
				-- GILBERT: PLEASE ADD MISSING JOINS TO PERSONA PRODUCT CONFIGURATIONS TO GET PERSONA PRODUCT FAVORITES
		WHERE	p.PersonaId = @personaId
		AND		(@productId = 0 OR pr.ProductId = @productId)
		AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))
	) p
	PIVOT	(
		MAX(Value) FOR ProductSettingTypeName IN (
			[ClientId],
			[ClassName],
			[SettingsUrl],
			[ProductUrl],
		
	[TitleId],
			[TitleUniqueId],
			[IsNewTab],
			[MetatagUniqueId],
			[IsResource],
			[IsFavorite],
			[Subsolution],
			[LearnMore]
		)
	) AS pvt

	-- Added by Gilbert so that product can be filtered by "type" (IsFavorite, IsResource)
	WHERE	((@productSelectType IS NULL AND pvt.IsResource IS NULL) OR pvt.IsResource = 0)
			OR
			(@productSelectType IS NOT NULL AND @productSelectType = 'ProductWithFavorites' AND pvt.IsResource IS NULL OR pvt.IsResource = 0)
			OR 
			(@productSelectType IS NOT NULL AND @productSelectType = 'IsResource' AND pvt.IsResource = 1)
			OR
			(@productSelectType IS NOT NULL AND @productSelectType = 'IsFavorite' AND pvt.IsFavorite = 1)
	
	;
END
GO
PRINT N'Creating [Enterprise].[ContactMechanismBoundary]'
GO
CREATE TABLE [Enterprise].[ContactMechanismBoundary]
(
[ContactMechanismBoundaryId] [int] NOT NULL IDENTITY(1, 1),
[ContactMechanismId] [int] NOT NULL,
[GeographicBoundaryId] [int] NOT NULL,
[FromDate] [datetime] NOT NULL,
[ThruDate] [datetime] NULL
)
GO
PRINT N'Creating primary key [PK_PostalAddressBoundary] on [Enterprise].[ContactMechanismBoundary]'
GO
ALTER TABLE [Enterprise].[ContactMechanismBoundary] ADD CONSTRAINT [PK_PostalAddressBoundary] PRIMARY KEY CLUSTERED  ([ContactMechanismBoundaryId])
GO
PRINT N'Adding constraints to [Enterprise].[ContactMechanismBoundary]'
GO
ALTER TABLE [Enterprise].[ContactMechanismBoundary] ADD CONSTRAINT [AK_ContactMechanismBoundary_ContactMechanismId_GeographicBoundaryId] UNIQUE NONCLUSTERED  ([ContactMechanismId], [GeographicBoundaryId], [ThruDate])
GO
PRINT N'Creating [Person].[ListPostalAddressesForPerson]'
GO
CREATE PROCEDURE [Person].[ListPostalAddressesForPerson] (
	@RealPageId UNIQUEIDENTIFIER
)
AS
BEGIN
	SELECT	unpvt.PartyContactMechanismId,
			unpvt.ContactMechanismID,
			unpvt.AddressString,
			unpvt.AddressType,
			unpvt.ContactMechanismUsageTypeID
	FROM    (
				SELECT	pcm.PartyContactMechanismId,
						cm.ContactMechanismID,
						pa.StreetAddress1,
						pa.StreetAddress2,
						pa.StreetAddress3,
						cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeID
				FROM	Enterprise.Party p
						JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyId = p.PartyId AND p.RealPageId = @RealPageId
						JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
						JOIN Enterprise.[StreetAddress] pa ON pa.ContactMechanismID = cm.ContactMechanismID
						JOIN Enterprise.ContactMechanismUsage cmu ON cmu.PartyContactMechanismID = pcm.PartyContactMechanismId
				WHERE	(pcm.ThruDate IS NULL OR pcm.ThruDate >= GETUTCDATE())
			) AS pvt UNPIVOT
			(AddressString FOR AddressType IN (StreetAddress1, StreetAddress2, StreetAddress3)) AS unpvt
	UNION ALL
	SELECT	pcm.PartyContactMechanismId,
			cm.ContactMechanismID ,
			gb.Name AS AddressString ,
			gbt.Name AS AddressType ,
			cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeID
	FROM	Enterprise.ContactMechanismUsage cmu
			JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
			JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
			JOIN Enterprise.ContactMechanismBoundary cmb ON cmb.ContactMechanismId = cm.ContactMechanismID
			JOIN Enterprise.GeographicBoundary gb ON gb.GeographicBoundaryId = cmb.GeographicBoundaryId
			JOIN Enterprise.GeographicBoundaryType gbt ON gbt.GeographicBoundaryTypeId = gb.GeographicBoundaryTypeId
			JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
	WHERE	p.RealPageId = @RealPageId
	AND		(pcm.ThruDate IS NULL OR pcm.ThruDate >= GETUTCDATE());
END;
GO
PRINT N'Creating [Enterprise].[TelecommunicationsNumber]'
GO
CREATE TABLE [Enterprise].[TelecommunicationsNumber]
(
[ContactMechanismID] [int] NOT NULL,
[CountryCode] [varchar] (10) NOT NULL,
[AreaCode] [varchar] (10) NOT NULL,
[PhoneNumber] [varchar] (10) NOT NULL
)
GO
PRINT N'Creating primary key [PK_TelecommunicationsNumber] on [Enterprise].[TelecommunicationsNumber]'
GO
ALTER TABLE [Enterprise].[TelecommunicationsNumber] ADD CONSTRAINT [PK_TelecommunicationsNumber] PRIMARY KEY CLUSTERED  ([ContactMechanismID])
GO
PRINT N'Creating [Person].[ListTelecommunicationNumbersForPerson]'
GO
CREATE PROCEDURE [Person].[ListTelecommunicationNumbersForPerson] (
	@RealPageId UNIQUEIDENTIFIER
)
AS
BEGIN
	SELECT  pcm.PartyContactMechanismId,
			cm.ContactMechanismID,
			tm.CountryCode,
			tm.AreaCode,
			tm.PhoneNumber,
			'Telecommunications Number' AS AddressType,
			cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeID
	FROM    Enterprise.ContactMechanismUsage cmu
			JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
			JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
			JOIN Enterprise.TelecommunicationsNumber tm ON tm.ContactMechanismID = cm.ContactMechanismID
			JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
	WHERE   p.RealPageId = @RealPageId
	AND		(pcm.ThruDate IS NULL OR pcm.ThruDate > GETUTCDATE());
END
GO
PRINT N'Creating [Enterprise].[ElectronicAddress]'
GO
CREATE TABLE [Enterprise].[ElectronicAddress]
(
[ContactMechanismID] [int] NOT NULL,
[ElectronicAddressString] [varchar] (255) NOT NULL,
[ElectronicAddressType] [varchar] (20) NOT NULL CONSTRAINT [DF__Electroni__Elect__04AFB25B] DEFAULT ('E-Mail')
)
GO
PRINT N'Creating primary key [PK_ElectronicAddress] on [Enterprise].[ElectronicAddress]'
GO
ALTER TABLE [Enterprise].[ElectronicAddress] ADD CONSTRAINT [PK_ElectronicAddress] PRIMARY KEY CLUSTERED  ([ContactMechanismID])
GO
PRINT N'Creating [Person].[ListEmailsForPerson]'
GO
CREATE PROCEDURE [Person].[ListEmailsForPerson] (
	@RealPageId UNIQUEIDENTIFIER
)
AS
BEGIN
	SELECT  pcm.PartyContactMechanismId,
			cm.ContactMechanismID,
			ea.ElectronicAddressString AS AddressString,
			ea.ElectronicAddressType AS AddressType,
			cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeID
	FROM    Enterprise.ContactMechanismUsage cmu
			JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
			JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
			JOIN Enterprise.ElectronicAddress ea ON ea.ContactMechanismID = cm.ContactMechanismID
			JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
	WHERE	p.RealPageId = @RealPageId
	AND		(pcm.ThruDate IS NULL OR pcm.ThruDate > GETUTCDATE());
END
GO
PRINT N'Creating [Person].[GetPerson]'
GO
CREATE PROCEDURE [Person].[GetPerson] (
    @RealPageId UNIQUEIDENTIFIER
)
AS
BEGIN
    SELECT  Party.RealPageId,
			Person.PartyId,
            Person.Title,
            Person.FirstName,
            Person.MiddleName,
            Person.LastName,
            Person.Suffix,
			Person.PreferredContactMethodId
    FROM    Person.Person
			JOIN Enterprise.Party ON Party.PartyId = Person.PartyId
    WHERE   Party.RealPageId = @RealPageId;
END
GO
PRINT N'Creating [Person].[ListContactMechanismsForPerson]'
GO
CREATE PROCEDURE [Person].[ListContactMechanismsForPerson] (
    @RealPageId UNIQUEIDENTIFIER
)
AS
BEGIN
	DECLARE @NOW  DATETIME = GETUTCDATE();

    SELECT  pcm.PartyContactMechanismId,
			cm.ContactMechanismID,
            ea.ElectronicAddressString AS AddressString,
            'Email' AS AddressType,
			cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeId
    FROM    Enterprise.ContactMechanismUsage cmu
            JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
            JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
            JOIN Enterprise.ElectronicAddress ea ON ea.ContactMechanismID = cm.ContactMechanismID
            JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
    WHERE   p.RealPageId = @RealPageId
	AND ((@NOW BETWEEN pcm.FromDate AND pcm.ThruDate) OR (@NOW >= pcm.FromDate AND pcm.ThruDate IS NULL))
    UNION ALL
    SELECT  pcm.PartyContactMechanismId,
			cm.ContactMechanismID,
            pa.StreetAddress1 AS AddressString,
            'Street Address' AS AddressType,
			cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeId
    FROM    Enterprise.ContactMechanismUsage cmu
            JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
            JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
            JOIN Enterprise.[StreetAddress] pa ON pa.ContactMechanismID = cm.ContactMechanismID
            JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
    WHERE   p.RealPageId = @RealPageId
	AND ((@NOW BETWEEN pcm.FromDate AND pcm.ThruDate) OR (@NOW >= pcm.FromDate AND pcm.ThruDate IS NULL))
    UNION ALL
    SELECT  pcm.PartyContactMechanismId,
			cm.ContactMechanismID,
            gb.Name AS AddressString,
            gbt.Name AS AddressType,
			cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeId
    FROM    Enterprise.ContactMechanismUsage cmu
            JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
            JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
            JOIN Enterprise.ContactMechanismBoundary cmb ON cmb.ContactMechanismId = cm.ContactMechanismID
            JOIN Enterprise.GeographicBoundary gb ON gb.GeographicBoundaryId = cmb.GeographicBoundaryId
            JOIN Enterprise.GeographicBoundaryType gbt ON gbt.GeographicBoundaryTypeId = gb.GeographicBoundaryTypeId
            JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
    WHERE   p.RealPageId = @RealPageId
	AND ((@NOW BETWEEN pcm.FromDate AND pcm.ThruDate) OR (@NOW >= pcm.FromDate AND pcm.ThruDate IS NULL))
	AND ((@NOW BETWEEN cmb.FromDate AND cmb.ThruDate) OR (@NOW >= cmb.FromDate AND cmb.ThruDate IS NULL))
    UNION ALL
    SELECT  pcm.PartyContactMechanismId,
			cm.ContactMechanismID,
            CONCAT(tm.CountryCode, tm.AreaCode, tm.PhoneNumber) AS AddressString,
            'Telecommunications Number' AS AddressType,
			cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeId
    FROM    Enterprise.ContactMechanismUsage cmu
            JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
            JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
            JOIN Enterprise.TelecommunicationsNumber tm ON tm.ContactMechanismID = cm.ContactMechanismID
            JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
    WHERE   p.RealPageId = @RealPageId
	AND ((@NOW BETWEEN pcm.FromDate AND pcm.ThruDate) OR (@NOW >= pcm.FromDate AND pcm.ThruDate IS NULL));
END;
GO
PRINT N'Creating [Person].[ActivePersona]'
GO
CREATE TABLE [Person].[ActivePersona]
(
[PartyId] [bigint] NOT NULL,
[PersonaId] [bigint] NOT NULL
)
GO
PRINT N'Creating primary key [PK_ActivePersona] on [Person].[ActivePersona]'
GO
ALTER TABLE [Person].[ActivePersona] ADD CONSTRAINT [PK_ActivePersona] PRIMARY KEY CLUSTERED  ([PartyId], [PersonaId])
GO
PRINT N'Creating [Person].[ListPersons]'
GO
CREATE PROCEDURE  [Person].[ListPersons]
(
	@RealPageId UNIQUEIDENTIFIER = NULL,
	@Name NVARCHAR(100) = NULL,
	@ProductId INT = NULL
)

AS

BEGIN

	DECLARE @NOW  DATETIME = GETUTCDATE();

	SELECT	DISTINCT 
			personParty.RealPageId,
			Person.PartyId,
			Person.FirstName ,
			Person.MiddleName ,
			Person.LastName ,
			Person.Title ,
			Person.Suffix ,
			UserLogin.UserId ,
			UserLogin.LoginName ,
			UserLogin.LastLoginDate AS LastLogin,
			UserLogin.FromDate,
			UserLogin.ThruDate,			
			ISNULL(Products.ProductCount,0) AS [Products],
			0 AS [Properties],
			ISNULL(rtf.Name, '') AS [UserType]
	FROM	Enterprise.PartyRelationship
			JOIN Person.Person ON PartyRelationship.PartyIdFrom = Person.PartyId
			JOIN Enterprise.Party personParty ON personParty.PartyId = Person.PartyId
			JOIN Ident.UserLogin ON UserLogin.PartyId = Person.PartyId			
			JOIN Enterprise.Organization ON PartyRelationship.PartyIdTo = Organization.PartyId
			JOIN Enterprise.Party OrgParty ON Organization.PartyId = OrgParty.PartyId
			LEFT JOIN (
				SELECT	COUNT(DISTINCT ProductId) AS ProductCount, PartyId
				FROM	Ident.SamlUserAttribute
						JOIN Person.ActivePersona ON ActivePersona.PersonaId = SamlUserAttribute.PersonaId
						WHERE ProductId = @ProductId OR @ProductId IS NULL
						GROUP BY PartyId ) 
				AS Products ON Products.PartyId = Person.PartyId

			JOIN Enterprise.RoleType rtf ON (PartyRelationship.RoleTypeIdFrom = rtf.PartyRoleTypeId)
			JOIN Enterprise.[RelationshipType] rt ON (PartyRelationship.PartyRelationshipTypeId = rt.RelationshipTypeId)			
			LEFT OUTER JOIN Enterprise.RoleType prt ON (rtf.ParentPartyRoleTypeId = prt.PartyRoleTypeId)

	WHERE  (OrgParty.RealPageId = @RealPageId OR @RealPageId IS NULL)
		   AND 
		   (
				(FirstName LIKE @Name +'%' OR @Name IS NULL)
				OR 
				(LastName LIKE @Name + '%' OR @Name IS NULL)    
				OR 
				(LoginName LIKE @Name + '%' OR @Name IS NULL)    
		   )

	AND ((@NOW BETWEEN PartyRelationship.FromDate AND PartyRelationship.ThruDate) OR (@NOW >= PartyRelationship.FromDate AND PartyRelationship.ThruDate IS NULL))
	
END;
GO
PRINT N'Creating [Person].[GetDefaultPersona]'
GO
CREATE PROCEDURE [Person].[GetDefaultPersona]
		@RealPageId UNIQUEIDENTIFIER
AS
BEGIN

	DECLARE @NOW  DATETIME = GETUTCDATE();

	SELECT	per.PersonaId, 
			per.PersonaTypeId as PersonaType, 
			ppt.Name 
	FROM	Person.Persona per
			JOIN Enterprise.Party epar ON epar.PartyId = per.PersonPartyId
			JOIN Person.PersonaType ppt ON ppt.PersonaTypeId = per.PersonaTypeId	
	WHERE	epar.RealPageId  = @RealPageId and per.IsDefault = 1
	AND ((@NOW BETWEEN per.FromDate AND per.ThruDate) OR (@NOW >= per.FromDate AND per.ThruDate IS NULL))
END
GO
PRINT N'Creating [Person].[GetPersona]'
GO
CREATE PROCEDURE [Person].[GetPersona] (
	@PersonaId bigint
)
AS
BEGIN

	DECLARE @NOW  DATETIME = GETUTCDATE();

	SELECT	pe.PersonaId,
			pe.PersonPartyId,
			p.RealPageId,
			pe.OrganizationPartyId,
			pe.PersonaTypeId,
			pe.PersonaEnvironmentTypeId,
			pt.Name,
			pe.FromDate,
			pe.ThruDate,
			pe.IsDefault
	FROM	Person.Persona pe
			INNER JOIN Enterprise.Party p ON (pe.PersonPartyId = p.PartyId)
			INNER JOIN Person.PersonaType pt ON (pe.PersonaTypeId = pt.PersonaTypeId)
	WHERE	pe.PersonaId = @PersonaId
	AND ((@NOW BETWEEN pe.FromDate AND pe.ThruDate) OR (@NOW >= pe.FromDate AND pe.ThruDate IS NULL))
END
GO
PRINT N'Creating [Person].[ListPersona]'
GO
CREATE PROCEDURE [Person].[ListPersona] (
	@RealPageId UNIQUEIDENTIFIER,
	@IsDefault bit = NULL
)
AS
BEGIN
	DECLARE @NOW DATETIME = GETUTCDATE()

	SELECT	pe.PersonaId,
			pe.PersonPartyId,
			p.RealPageId,
			pe.OrganizationPartyId,
			pe.PersonaTypeId,
			pe.PersonaEnvironmentTypeId,
			pt.Name,
			pe.FromDate,
			pe.ThruDate,
			pe.IsDefault
	FROM	Person.Persona pe
			INNER JOIN Enterprise.Party p ON (pe.PersonPartyId = p.PartyId)
			INNER JOIN Person.PersonaType pt ON (pe.PersonaTypeId = pt.PersonaTypeId)
	WHERE	p.RealPageId = @RealPageId
	AND		(@IsDefault IS NULL OR pe.IsDefault = @IsDefault)
	AND ((@NOW BETWEEN pe.FromDate AND pe.ThruDate) OR (@NOW >= pe.FromDate AND pe.ThruDate IS NULL))

END
GO
PRINT N'Creating [Person].[UpdateActivePersona]'
GO
CREATE PROCEDURE [Person].[UpdateActivePersona]
    @RealPageID UNIQUEIDENTIFIER ,
    @PersonaId BIGINT
AS
    BEGIN

        UPDATE ap
        SET    PersonaId = @PersonaId
        FROM   Person.ActivePersona ap
               JOIN Person.Person per ON per.PartyId = ap.PartyId
               JOIN Enterprise.Party p ON p.PartyId = per.PartyId
        WHERE  p.RealPageId = @RealPageID;


    END;
GO
PRINT N'Creating [Person].[GetActivePersona]'
GO
CREATE PROCEDURE [Person].[GetActivePersona]
    @RealPageId UNIQUEIDENTIFIER
AS
    BEGIN

        SELECT ap.PersonaId
        FROM   Person.ActivePersona ap
               JOIN Person.Person per ON per.PartyId = ap.PartyId
               JOIN Enterprise.Party p ON p.PartyId = per.PartyId
        WHERE  p.RealPageId = @RealPageId;

    END;
GO
PRINT N'Creating [dbo].[ListPersonContactMethods]'
GO
CREATE PROC [dbo].[ListPersonContactMethods]
@PersonId UNIQUEIDENTIFIER 
AS

SELECT  cm.ContactMechanismID ,
        ea.ElectronicAddressString AS AddressString ,
        'Email' AS AddressType ,
        cmt.Name AS UsageType
FROM    Enterprise.ContactMechanismUsage cmu
        JOIN Enterprise.ContactMechanismUsageType cmt ON cmt.ContactMechanismUsageTypeID = cmu.ContactMechanismUsageTypeID
        JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
        JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
        JOIN Enterprise.ElectronicAddress ea ON ea.ContactMechanismID = cm.ContactMechanismID
		JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
WHERE   p.RealPageId = @PersonId
UNION ALL
SELECT  cm.ContactMechanismID ,
        pa.StreetAddress1 AS AddressString ,
        'Street Address' AS AddressType ,
        cmt.Name AS UsageType
FROM    Enterprise.ContactMechanismUsage cmu
        JOIN Enterprise.ContactMechanismUsageType cmt ON cmt.ContactMechanismUsageTypeID = cmu.ContactMechanismUsageTypeID
        JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
        JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
        JOIN Enterprise.[StreetAddress] pa ON pa.ContactMechanismID = cm.ContactMechanismID
		JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
WHERE   p.RealPageId = @PersonId
UNION ALL
SELECT  cm.ContactMechanismID ,
        gb.Name AS AddressString ,
        gbt.Name AS AddressType ,
        cmt.Name AS UsageType
FROM    Enterprise.ContactMechanismUsage cmu
        JOIN Enterprise.ContactMechanismUsageType cmt ON cmt.ContactMechanismUsageTypeID = cmu.ContactMechanismUsageTypeID
        JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
        JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
        JOIN Enterprise.ContactMechanismBoundary cmb ON cmb.ContactMechanismId = cm.ContactMechanismID
        JOIN Enterprise.GeographicBoundary gb ON gb.GeographicBoundaryId = cmb.GeographicBoundaryId
        JOIN Enterprise.GeographicBoundaryType gbt ON gbt.GeographicBoundaryTypeId = gb.GeographicBoundaryTypeId
		JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
WHERE   p.RealPageId = @PersonId
UNION ALL
SELECT  cm.ContactMechanismID ,
        CONCAT(tm.CountryCode, tm.AreaCode, tm.PhoneNumber) AS AddressString ,
        'Telecommunications Number' AS AddressType ,
        cmt.Name AS UsageType
FROM    Enterprise.ContactMechanismUsage cmu
        JOIN Enterprise.ContactMechanismUsageType cmt ON cmt.ContactMechanismUsageTypeID = cmu.ContactMechanismUsageTypeID
        JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
        JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
        JOIN Enterprise.TelecommunicationsNumber tm ON tm.ContactMechanismID = cm.ContactMechanismID
		JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
WHERE   p.RealPageId = @PersonId;
GO
PRINT N'Creating [Auth].[UpdatePasswordPolicy]'
GO
CREATE PROCEDURE [Auth].[UpdatePasswordPolicy] (
	@PasswordPolicyId [int],
	@PortfolioId [int],
	@MinimumLength [tinyint],
	@MaximumLength [tinyint],
	@MinimumLowercase [tinyint],
	@MinimumUppercase [tinyint],
	@MinimumNumeric [tinyint],
	@MinimumSpecialCharacter [tinyint],
	@AllowUsersToChangeOwnPassword [bit],
	@EnablePasswordExpiration [bit],
	@PasswordExpirationPeriodInDays [smallint],
	@PreventPasswordReuse [bit],
	@NumberOfPasswordsToRemember [tinyint],
	@UserId bigint
)
AS
BEGIN
	BEGIN TRY
        BEGIN TRANSACTION;
		-- Insert statements for procedure here
		UPDATE	[Auth].[PasswordPolicy]
		SET		[MinimumLength] = @MinimumLength,
				[MaximumLength] = @MaximumLength,
				[MinimumLowercase] = @MinimumLowercase,
				[MinimumUppercase] = @MinimumUppercase,
				[MinimumNumeric] = @MinimumNumeric,
				[MinimumSpecialCharacter] = @MinimumSpecialCharacter,
				[AllowUsersToChangeOwnPassword] = @AllowUsersToChangeOwnPassword,
				[EnablePasswordExpiration] = @EnablePasswordExpiration,
				[PasswordExpirationPeriodInDays] = @PasswordExpirationPeriodInDays,
				[PreventPasswordReuse] = @PreventPasswordReuse,
				[NumberOfPasswordsToRemember] = @NumberOfPasswordsToRemember,
				[UserId] = @UserId
		OUTPUT	inserted.PasswordPolicyId AS Id,
				'' AS ErrorMessage
		WHERE	[PasswordPolicyId] = @PasswordPolicyId

		COMMIT;
	END TRY  
	BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
	END CATCH
END
GO
PRINT N'Creating [Auth].[CreatePasswordPolicy]'
GO
CREATE PROCEDURE [Auth].[CreatePasswordPolicy] (
	@PortfolioId [int],
	@MinimumLength [tinyint] = 8,
	@MaximumLength [tinyint] = 128,
	@MinimumLowercase [tinyint] = 0,
	@MinimumUppercase [tinyint] = 0,
	@MinimumNumeric [tinyint] = 0,
	@MinimumSpecialCharacter [tinyint] = 0,
	@AllowUsersToChangeOwnPassword [bit] = 1,
	@EnablePasswordExpiration [bit] = 0,
	@PasswordExpirationPeriodInDays [smallint] = NULL,
	@PreventPasswordReuse [bit] = 0,
	@NumberOfPasswordsToRemember [tinyint] = NULL,
	@UserId [bigint]
)
AS
BEGIN
	BEGIN TRY
        BEGIN TRANSACTION; 
		-- Insert statements for procedure here
		INSERT INTO [Auth].[PasswordPolicy] (
			[PortfolioId],
			[MinimumLength],
			[MaximumLength],
			[MinimumLowercase],
			[MinimumUppercase],
			[MinimumNumeric],
			[MinimumSpecialCharacter],
			[AllowUsersToChangeOwnPassword],
			[EnablePasswordExpiration],
			[PasswordExpirationPeriodInDays],
			[PreventPasswordReuse],
			[NumberOfPasswordsToRemember],
			[UserId]
		)
		VALUES (
			@PortfolioId,
			@MinimumLength,
			@MaximumLength,
			@MinimumLowercase,
			@MinimumUppercase,
			@MinimumNumeric,
			@MinimumSpecialCharacter,
			@AllowUsersToChangeOwnPassword,
			@EnablePasswordExpiration,
			@PasswordExpirationPeriodInDays,
			@PreventPasswordReuse,
			@NumberOfPasswordsToRemember,
			@UserId
		)

		SELECT	SCOPE_IDENTITY() AS Id,
				'' AS ErrorMessage
		COMMIT;
	END TRY  
	BEGIN CATCH
        ROLLBACK;
		
        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
	END CATCH
END
GO
PRINT N'Creating [Enterprise].[UpdateOrganization]'
GO
CREATE PROCEDURE [Enterprise].[UpdateOrganization]
    @OrganizationId UNIQUEIDENTIFIER,
	@OrganizationName NVARCHAR(50)
AS
    BEGIN
		BEGIN TRY
			SET NOCOUNT ON;
			BEGIN TRANSACTION;
			UPDATE o
			SET Name = @OrganizationName
			FROM [Enterprise].Organization o
			JOIN [Enterprise].Party p ON p.PartyId = o.PartyId
			WHERE p.RealPageId = @OrganizationId
			SET NOCOUNT OFF;

			COMMIT;
			SELECT @OrganizationId AS RealPageId
		END TRY
		BEGIN CATCH
			ROLLBACK;

			DECLARE @ErrorLogID INT;
			EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

			SELECT  0 AS Id,
					ErrorMessage
			FROM    [dbo].ErrorLog
			WHERE   ErrorLogID = @ErrorLogID;
		END CATCH
    END;
GO
PRINT N'Creating [Enterprise].[LinkOrganizationToOrganization]'
GO
CREATE PROCEDURE [Enterprise].[LinkOrganizationToOrganization]
    @OrganizationRealPageIdFrom UNIQUEIDENTIFIER ,
    @OrganizationRealPageIdTo UNIQUEIDENTIFIER ,
    @RoleTypeIdFrom INT ,
    @RoleTypeIdTo INT
AS
BEGIN
	BEGIN TRY

		DECLARE @PartyIdFrom BIGINT;
		DECLARE @PartyIdTo BIGINT;

		DECLARE @PartyRelationshipTypeId INT;

		SELECT	@PartyRelationshipTypeId = [RelationshipTypeId]
		FROM	Enterprise.[RelationshipType]
		WHERE	RoleTypeIdValidFrom = @RoleTypeIdFrom
		AND		RoleTypeIdValidTo = @RoleTypeIdTo;

		-- Get Party ID's
		SELECT	@PartyIdFrom = p.PartyId
		FROM	Enterprise.Party p
		WHERE	p.RealPageId = @OrganizationRealPageIdFrom

		SELECT	@PartyIdTo = p.PartyId
		FROM	Enterprise.Party p
		WHERE	p.RealPageId = @OrganizationRealPageIdTo

		IF @PartyRelationshipTypeId IS NULL
		BEGIN
			RAISERROR('The Relationship is invalid between Role Type %i and Role Type %i', 16, -1, @RoleTypeIdFrom, @RoleTypeIdTo);
		END;

        BEGIN TRANSACTION; 

		INSERT INTO Enterprise.PartyRelationship (
			PartyIdFrom,
			PartyIdTo,
			RoleTypeIdFrom,
			RoleTypeIdTo,
			PartyRelationshipTypeId,
			FromDate,
			ThruDate
		)
		OUTPUT	Inserted.PartyRelationshipId AS Id,
				'' AS ErrorMessage
		VALUES (
			@PartyIdFrom , -- PartyIdFrom - bigint
			@PartyIdTo , -- PartyIdTo - bigint
			@RoleTypeIdFrom , -- RoleTypeIdFrom - int
			@RoleTypeIdTo , -- RoleTypeIdTo - int
			@PartyRelationshipTypeId , -- PartyRelationshipTypeId - int
			GETUTCDATE() , -- FromDate - datetime
			NULL  -- ThruDate - datetime
		)

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
    END CATCH;
END;
GO
PRINT N'Creating [Enterprise].[CreateOrganization]'
GO
CREATE PROCEDURE [Enterprise].[CreateOrganization]
    @OrganizationId UNIQUEIDENTIFIER = NULL,
	@OrganizationName NVARCHAR(50)
AS
BEGIN
	BEGIN TRY
		IF @OrganizationId IS NULL
		BEGIN
		    SET @OrganizationId = NEWID();
		END

		BEGIN TRANSACTION
		SET NOCOUNT ON
		DECLARE @PartyId BIGINT;

		INSERT  INTO [Enterprise].Party
		(
			RealPageId,
			CreateDate
		)
		VALUES
		(
			@OrganizationId,
			GETUTCDATE()
		);

		SET @PartyId = SCOPE_IDENTITY();

		INSERT INTO [Enterprise].Organization
		(
			PartyId,
			Name
		)
		VALUES
		(
			@PartyId,
			@OrganizationName
		)
		SET NOCOUNT OFF
		COMMIT;
		SELECT	@PartyId AS Id,
				@OrganizationId AS RealPageId,
				'' AS ErrorMessage
	END TRY
	BEGIN CATCH
		ROLLBACK;

		DECLARE @ErrorLogID INT;
		EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

		SELECT  0 AS Id,
				ErrorMessage
		FROM    [dbo].ErrorLog
		WHERE   ErrorLogID = @ErrorLogID;
	END CATCH
END
GO
PRINT N'Creating [Enterprise].[CreatePartyRoleByRealPageId]'
GO
CREATE PROCEDURE [Enterprise].[CreatePartyRoleByRealPageId] (
	@RealPageId UNIQUEIDENTIFIER,
	@RoleTypeID int
)
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION; 
		UPDATE pr
		   SET RoleTypeId = @RoleTypeId
		   FROM Enterprise.Party pa 
		   INNER JOIN Person.Person p ON (pa.PartyId = p.PartyId)
		   INNER JOIN Enterprise.PartyRole pr ON (p.PartyId = pr.PartyId)
		   WHERE	pa.RealPageId = @RealPageId

		IF @@ROWCOUNT = 0
		BEGIN
		
			INSERT  INTO Enterprise.PartyRole (
				PartyId,
				RoleTypeId
			)
			OUTPUT	Inserted.PartyRoleId AS Id,
					@RealPageId AS RealPageId,
					'' AS ErrorMessage
			SELECT	p.PartyId,
					@RoleTypeID
			FROM	Enterprise.Party pa
					INNER JOIN Person.Person p ON (pa.PartyId = p.PartyId)
			WHERE	pa.RealPageId = @RealPageId

			
		END
        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				'' AS RealPageId,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
    END CATCH;
END;
GO
PRINT N'Creating [Enterprise].[UpdatePartyRoleByRealPageId]'
GO
CREATE PROCEDURE [Enterprise].[UpdatePartyRoleByRealPageId] (
	@PartyRoleId int,
	@RoleTypeID int
)
AS
BEGIN
	BEGIN TRY
        BEGIN TRANSACTION;
		UPDATE	Enterprise.PartyRole
		SET		RoleTypeId = @RoleTypeID
		OUTPUT	inserted.PartyRoleId AS Id,
				'' AS ErrorMessage
		WHERE	PartyRoleId = @PartyRoleId
		COMMIT;
	END TRY  
	BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
	END CATCH
END
GO
PRINT N'Creating [Ident].[UpdateUserLogin]'
GO
CREATE PROCEDURE [Ident].[UpdateUserLogin] (
    @RealPageId UNIQUEIDENTIFIER ,
    @LoginName VARCHAR(255) = NULL ,
    @PasswordHash NVARCHAR(255) = NULL ,
    @PasswordSalt NVARCHAR(255) = NULL ,
    @FromDate DATETIME = NULL ,
    @ThruDate DATETIME = NULL
)
AS
BEGIN
	BEGIN TRY
		
		-- BEGIN USER UPDATE --
        BEGIN TRANSACTION;
		UPDATE  UserLogin
		SET     [LoginName] = ISNULL(@LoginName, [LoginName]) ,
				[PasswordHash] = ISNULL(@PasswordHash, PasswordHash) ,
				[PasswordSalt] = ISNULL(@PasswordSalt, PasswordSalt) ,
				[FromDate] = ISNULL(@FromDate, FromDate) ,
				[ThruDate] = CASE WHEN @ThruDate = '12/31/9999' THEN NULL ELSE ISNULL(@ThruDate, ThruDate) END
		OUTPUT	inserted.UserId AS Id,
				'' AS ErrorMessage
		FROM    Ident.UserLogin
				JOIN Enterprise.Party ON Party.PartyId = UserLogin.PartyId
		WHERE   RealPageId = @RealPageId;

		-- END USER UPDATE --

		COMMIT;
	END TRY  
	BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
	END CATCH
END;
GO
PRINT N'Creating [Ident].[CreateUserLogin]'
GO
CREATE PROCEDURE [Ident].[CreateUserLogin]
    (
      @RealPageId UNIQUEIDENTIFIER ,
      @LoginName NVARCHAR(255) ,
      @FromDate DATETIME = NULL ,
      @ThruDate DATETIME = NULL
    )
AS
    BEGIN
        BEGIN TRY
            DECLARE @PartyId BIGINT;
            DECLARE @UserId BIGINT;

            SELECT  @PartyId = PartyId
            FROM    Enterprise.Party
            WHERE   RealPageId = @RealPageId;

			IF @FromDate IS NULL
				SELECT  @FromDate = GETUTCDATE();

			BEGIN TRAN
            IF ( SELECT 1
                 FROM   Ident.UserLogin
                 WHERE  [LoginName] = @LoginName
               ) IS NOT NULL
                BEGIN
                    RAISERROR('The User Login already exists', 10, 1);
                END;
            ELSE
                BEGIN
                    IF @PartyId IS NOT NULL
                        BEGIN
                            INSERT  INTO Ident.UserLogin
                                    ( PartyId ,
                                      [LoginName] ,
                                      FromDate ,
                                      ThruDate
						            )
                            VALUES  ( @PartyId , -- PartyId - bigint
                                      @LoginName ,
                                      @FromDate,
                                      @ThruDate
						            );

                            SET @UserId = SCOPE_IDENTITY();							

                            INSERT  INTO Ident.[UserCurrentStatus]
                                    ( UserId ,
                                      StatusTypeId ,
                                      StatusSetDate ,
                                      FromDate 
				                    )
                            VALUES  ( @UserId ,
                                      1 ,
                                      GETUTCDATE() ,
                                      @FromDate
                                    );
                        END;
                END;

            SELECT  @UserId AS Id ,
                    '' AS ErrorMessage;
            COMMIT;
        END TRY
        BEGIN CATCH
            DECLARE @ErrorLogID INT;
            EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

            SELECT  0 AS Id ,
                    ErrorMessage
            FROM    dbo.ErrorLog
            WHERE   ErrorLogID = @ErrorLogID;

            ROLLBACK;
        END CATCH;
    END;
GO
PRINT N'Creating [Ident].[LinkIdentityProviderSettingToContactMechanism]'
GO
CREATE PROCEDURE [Ident].[LinkIdentityProviderSettingToContactMechanism] (
	@ContactMechanismId int,
	@IdentityProviderSettingId int,
	@FromDate DateTime,
	@ThruDate DateTime = NULL
)
AS
BEGIN
	--This is the stored procedure that takes a runtime value for an identity provider
	--setting for a given identity provider type and links it to a ContactMechanismId
	BEGIN TRY
	    BEGIN TRANSACTION;

		INSERT INTO [Ident].[ContactMechanismIdentity] (
			[ContactMechanismId],
			[IdentityProviderSettingId],
			[FromDate],
			[ThruDate]
		)
		OUTPUT	Inserted.ContactMechanismIdentityId AS Id,
				'' AS ErrorMessage
		VALUES (
			@ContactMechanismId, -- ContactMechanismId - int
			@IdentityProviderSettingId, -- IdentityProviderSettingId - int
			@FromDate, -- FromDate - datetime
			@ThruDate  -- ThruDate - datetime
		)

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
    END CATCH;
END
GO
PRINT N'Creating [Ident].[CreateIdentityProviderSetting]'
GO
CREATE PROCEDURE [Ident].[CreateIdentityProviderSetting] (
	@IdentityProviderSettingTypeId int,
	@Value nvarchar(255)
)
AS
BEGIN
	--This will assign a runtime value to a setting type for a specific identity provider type.
	BEGIN TRY
	    BEGIN TRANSACTION;

		--DECLARE @IdentityProviderTypeId INT,
		--	@IdentityProviderSettingTypeId INT;
	
		--SELECT	@IdentityProviderTypeId = IdentityProviderTypeId
		--FROM	[Ident].IdentityProviderType
		--WHERE	[Name] = @IdentityproviderTypeName;

		--SELECT	@IdentityProviderSettingTypeId = IdentityProviderSettingTypeId
		--FROM	[Ident].IdentityProviderSettingType
		--WHERE	IdentityProviderTypeId = @IdentityProviderTypeId
		--AND		Name = @IdentityProviderSettingTypeName

		INSERT INTO [Ident].[IdentityProviderSetting]
		(
			[IdentityProviderSettingTypeId],
			[Value]
		)
		OUTPUT	Inserted.IdentityProviderSettingId AS Id,
				'' AS ErrorMessage
		VALUES
		(
			@IdentityProviderSettingTypeId,
			@Value
		)

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
    END CATCH;
END
GO
PRINT N'Creating [Ident].[CreateIdentityProviderSettingType]'
GO
CREATE PROCEDURE [Ident].[CreateIdentityProviderSettingType] (
	@IdentityProviderTypeId int,
	@Name nvarchar(50)
)
AS
BEGIN
	--This is to create a new attribute to use when setting up a new Identity Provider.
	BEGIN TRY
	    BEGIN TRANSACTION; 

		INSERT INTO [Ident].[IdentityProviderSettingType]
		(
			[IdentityProviderTypeId],
			[Name]
		)
		OUTPUT	Inserted.IdentityProviderSettingTypeId AS Id,
				'' AS ErrorMessage
		VALUES
		(
			@IdentityProviderTypeId,
			@Name
		)

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
    END CATCH;
END
GO
PRINT N'Creating [Ident].[CreateIdentityProviderType]'
GO
CREATE PROCEDURE [Ident].[CreateIdentityProviderType] (
	@Name nvarchar(50),
	@Description nvarchar(50)
)
AS
BEGIN
	--This is to create a new global type. Currently, we have Azure AD, Okta, and Identity Server
	BEGIN TRY
	    BEGIN TRANSACTION; 

		INSERT INTO [Ident].[IdentityProviderType]
		(
			[Name],
			[Description]
		)
		OUTPUT	Inserted.IdentityProviderTypeId AS Id,
				'' AS ErrorMessage
		VALUES
		(
			@Name,
			@Description
		)

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
    END CATCH;
END
GO
PRINT N'Creating [Ident].[UpdatePasswordPolicy]'
GO
CREATE PROCEDURE [Ident].[UpdatePasswordPolicy] (
	@PasswordPolicyId [int],
	@MinimumLength [tinyint],
	@MaximumLength [tinyint],
	@MinimumLowercase [tinyint],
	@MinimumUppercase [tinyint],
	@MinimumNumeric [tinyint],
	@MinimumSpecialCharacter [tinyint],
	@AllowUsersToChangeOwnPassword [bit],
	@EnablePasswordExpiration [bit],
	@PasswordExpirationPeriodInDays [smallint],
	@PreventPasswordReuse [bit],
	@NumberOfPasswordsToRemember [tinyint],
	@UserId bigint
)
AS
BEGIN
	BEGIN TRY
        BEGIN TRANSACTION;
		-- Insert statements for procedure here
		UPDATE	Ident.[PasswordPolicy]
		SET		[MinimumLength] = @MinimumLength,
				[MaximumLength] = @MaximumLength,
				[MinimumLowercase] = @MinimumLowercase,
				[MinimumUppercase] = @MinimumUppercase,
				[MinimumNumeric] = @MinimumNumeric,
				[MinimumSpecialCharacter] = @MinimumSpecialCharacter,
				[AllowUsersToChangeOwnPassword] = @AllowUsersToChangeOwnPassword,
				[EnablePasswordExpiration] = @EnablePasswordExpiration,
				[PasswordExpirationPeriodInDays] = @PasswordExpirationPeriodInDays,
				[PreventPasswordReuse] = @PreventPasswordReuse,
				[NumberOfPasswordsToRemember] = @NumberOfPasswordsToRemember,
				[UserId] = @UserId
		OUTPUT	inserted.PasswordPolicyId AS Id,
				'' AS ErrorMessage
		WHERE	[PasswordPolicyId] = @PasswordPolicyId

		COMMIT;
	END TRY  
	BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
	END CATCH
END
GO
PRINT N'Creating [Ident].[CreatePasswordPolicy]'
GO
CREATE PROCEDURE [Ident].[CreatePasswordPolicy] (
	@PartyId [bigint], -- for organization
	@MinimumLength [tinyint] = 8,
	@MaximumLength [tinyint] = 128,
	@MinimumLowercase [tinyint] = 0,
	@MinimumUppercase [tinyint] = 0,
	@MinimumNumeric [tinyint] = 0,
	@MinimumSpecialCharacter [tinyint] = 0,
	@AllowUsersToChangeOwnPassword [bit] = 1,
	@EnablePasswordExpiration [bit] = 0,
	@PasswordExpirationPeriodInDays [smallint] = NULL,
	@PreventPasswordReuse [bit] = 0,
	@NumberOfPasswordsToRemember [tinyint] = NULL,
	@UserId [bigint]
)
AS
BEGIN
	BEGIN TRY
        BEGIN TRANSACTION; 
		-- Insert statements for procedure here
		INSERT INTO [Ident].[PasswordPolicy] (
			[PartyId],
			[MinimumLength],
			[MaximumLength],
			[MinimumLowercase],
			[MinimumUppercase],
			[MinimumNumeric],
			[MinimumSpecialCharacter],
			[AllowUsersToChangeOwnPassword],
			[EnablePasswordExpiration],
			[PasswordExpirationPeriodInDays],
			[PreventPasswordReuse],
			[NumberOfPasswordsToRemember],
			[UserId]
		)
		VALUES (
			@PartyId,
			@MinimumLength,
			@MaximumLength,
			@MinimumLowercase,
			@MinimumUppercase,
			@MinimumNumeric,
			@MinimumSpecialCharacter,
			@AllowUsersToChangeOwnPassword,
			@EnablePasswordExpiration,
			@PasswordExpirationPeriodInDays,
			@PreventPasswordReuse,
			@NumberOfPasswordsToRemember,
			@UserId
		)

		SELECT	SCOPE_IDENTITY() AS Id,
				'' AS ErrorMessage
		COMMIT;
	END TRY  
	BEGIN CATCH
        ROLLBACK;
		
        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
	END CATCH
END
GO
PRINT N'Creating trigger [Person].[insertPersonaTrigger] on [Person].[Persona]'
GO
CREATE TRIGGER [Person].[insertPersonaTrigger]
ON [Person].[Persona]
AFTER INSERT
AS
    BEGIN
        SET NOCOUNT ON;

        INSERT INTO Person.ActivePersona (   PartyId ,
                                             PersonaId
                                         )
                    SELECT Inserted.PersonPartyId ,
                           Inserted.PersonaId
                    FROM   Inserted;

    END;
GO
PRINT N'Creating [Ident].[CreateUserSelectedSecurityQuestions]'
GO
CREATE PROCEDURE [Ident].[CreateUserSelectedSecurityQuestions]
	@realPageId as uniqueidentifier,
    @questionId1 as int,
	@questionId2 as int,
	@questionId3 as int,
	@answer1 NVARCHAR(50),
	@answer2 NVARCHAR(50),
	@answer3 NVARCHAR(50)
AS
    BEGIN
		BEGIN TRY
			SET NOCOUNT ON;
			BEGIN TRANSACTION;
				declare @UserId as bigint, @insertDateTime as smalldatetime

				select @insertDateTime=getutcdate()

				-- Get UserId
				SELECT @UserId=Ident.UserLogin.UserId FROM Enterprise.Party 
				INNER JOIN Ident.UserLogin ON Enterprise.Party.PartyId = Ident.UserLogin.PartyId
				where Enterprise.Party.RealPageId=@realPageId

				-- delete old questions
				delete from [Ident].[UserSecurityAnswer] where UserId=@UserId -- delete old questions
				
				INSERT INTO [Ident].[UserSecurityAnswer]
					([UserId],[SecurityQuestionId],[Answer],[CreateDateTime])
				VALUES
					(@UserId,@questionId1,@answer1,@insertDateTime),
					(@UserId,@questionId2,@answer2,@insertDateTime),
					(@UserId,@questionId3,@answer3,@insertDateTime)

			SET NOCOUNT OFF;

			COMMIT;
			SELECT @UserId AS UserId
		END TRY
		BEGIN CATCH
			ROLLBACK;

			DECLARE @ErrorLogID INT;
			EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

			SELECT  0 AS Id,
					ErrorMessage
			FROM    [dbo].ErrorLog
			WHERE   ErrorLogID = @ErrorLogID;
		END CATCH
    END;
GO
PRINT N'Creating [Enterprise].[CreateParty]'
GO
CREATE PROCEDURE [Enterprise].[CreateParty]
	@PartyID BIGINT OUTPUT,
	@RealPageId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
	BEGIN TRY
		DECLARE @PartyResult AS TABLE
            (
                PartyId INT ,
                RealPageId UNIQUEIDENTIFIER
            );
		
        INSERT  INTO Enterprise.Party
        OUTPUT  Inserted.PartyId ,
                Inserted.RealPageId
                INTO @PartyResult
                DEFAULT VALUES;

        SELECT  @PartyId = PartyId,
				@RealPageId = RealPageId
        FROM    @PartyResult;
	END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				'' AS RealPageId,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
    END CATCH;
END;
GO
PRINT N'Creating [Person].[CreatePerson]'
GO
CREATE PROCEDURE [Person].[CreatePerson]
    @Title NVARCHAR(50) = NULL ,
    @FirstName Name ,
    @MiddleName Name = NULL ,
    @LastName Name ,
    @Suffix NVARCHAR(10) = NULL,
	@PreferredContactMethodId int = NULL,
	@RealPageId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    BEGIN TRY
        DECLARE @PartyResult AS TABLE
            (
                PartyId INT ,
                RealPageId UNIQUEIDENTIFIER
            );
		
        DECLARE @PartyId bigint;

        BEGIN TRANSACTION; 
        INSERT  INTO Enterprise.Party
        OUTPUT  Inserted.PartyId ,
                Inserted.RealPageId
                INTO @PartyResult
                DEFAULT VALUES;

        SELECT  @PartyId = PartyId
        FROM    @PartyResult;

        INSERT  INTO Person.Person
                ( PartyId ,
                    Title ,
                    FirstName ,
                    MiddleName ,
                    LastName ,
                    Suffix ,
					PreferredContactMethodId
				)
        VALUES  ( @PartyId ,
                    @Title ,
                    @FirstName ,
                    @MiddleName ,
                    @LastName ,
                    @Suffix  ,
					@PreferredContactMethodId
				);

		SELECT	PartyId AS Id,
				RealPageId,
				'' AS ErrorMessage
        FROM    @PartyResult;

		SELECT @RealPageId = RealPageId
		FROM @PartyResult

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				'' AS RealPageId,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
    END CATCH;
END;
GO
PRINT N'Creating [Enterprise].[Configuration]'
GO
CREATE TABLE [Enterprise].[Configuration]
(
[ConfigurationId] [int] NOT NULL IDENTITY(1, 1),
[CreateDate] [datetime] NOT NULL CONSTRAINT [DF__Configura__Creat__0D44F85C] DEFAULT (getutcdate())
)
GO
PRINT N'Creating primary key [PK_Configuration] on [Enterprise].[Configuration]'
GO
ALTER TABLE [Enterprise].[Configuration] ADD CONSTRAINT [PK_Configuration] PRIMARY KEY CLUSTERED  ([ConfigurationId])
GO
PRINT N'Creating [Enterprise].[CreatePersonaConfiguration]'
GO
CREATE PROCEDURE [Enterprise].[CreatePersonaConfiguration](
	 @PersonaId int
	,@ProductId int
	,@FromDate datetime = NULL
	,@ThruDate datetime = NULL
)

AS

BEGIN

	SET NOCOUNT ON;
	DECLARE @NOW datetime = GETUTCDATE();
	DECLARE @ProductSettingId int = NULL
	DECLARE @ConfigurationId int = NULL

	IF @Fromdate IS NULL
		SET @FromDate = @NOW;

	--check the exoistence of a PersonaConfiguration for persona and product
		--inser or update (by exipration
		--get the configurationID
	SELECT @ConfigurationId = ConfigurationId 
	FROM PersonaConfiguration 
	WHERE PersonaId = @PersonaId AND ProductId = @ProductId
	AND ((@NOW BETWEEN FromDate AND ThruDate) OR (@NOW >= FromDate AND ThruDate IS NULL))

	 BEGIN TRY
				IF @ConfigurationId IS NULL
				BEGIN
					INSERT INTO Configuration (CreateDate) 
					VALUES (@NOW);

					SELECT @ConfigurationId = SCOPE_IDENTITY();

					INSERT INTO PersonaConfiguration (PersonaId,ConfigurationId, ProductId, FromDate, ThruDate)
					OUTPUT Inserted.PersonaConfigurationId AS Id, '' AS ErrorMessage
					VALUES(@PersonaId,@ConfigurationId,@ProductId, @FromDate,@ThruDate);
				END
				ELSE
				BEGIN
					SELECT @ConfigurationId as Id, '' AS ErrorMessage
				END
        END TRY
        BEGIN CATCH
            DECLARE @ErrorLogID INT;
            EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

            SELECT 0 AS Id ,
                   ErrorMessage
            FROM   dbo.ErrorLog
            WHERE  ErrorLogID = @ErrorLogID;

        END CATCH;
END;
GO
PRINT N'Creating [Person].[UpdatePerson]'
GO
CREATE PROCEDURE [Person].[UpdatePerson] (
    @RealPageId UNIQUEIDENTIFIER ,
    @Title NVARCHAR(50) = NULL ,
    @FirstName Name = NULL ,
    @MiddleName Name = NULL ,
    @LastName Name = NULL ,
    @Suffix NVARCHAR(10) = NULL,
	@PreferredContactMethodId int = NULL
)
AS
BEGIN
	BEGIN TRY
        BEGIN TRANSACTION;
		UPDATE  [Person]
		SET     [Title] = ISNULL(@Title, Title) ,
				[FirstName] = ISNULL(@FirstName, FirstName) ,
				[MiddleName] = ISNULL(@MiddleName, MiddleName) ,
				[LastName] = ISNULL(@LastName, LastName) ,
				[Suffix] = ISNULL(@Suffix, Suffix),
				[PreferredContactMethodId] = ISNULL(@PreferredContactMethodId, PreferredContactMethodId)			
		OUTPUT	inserted.PartyId AS Id,
				'' AS ErrorMessage
		FROM    [Person].[Person]
				JOIN [Enterprise].[Party] ON [Party].[PartyId] = [Person].[PartyId]
		WHERE   [Party].[RealPageId] = @RealPageId;
		COMMIT;
	END TRY  
	BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
	END CATCH
END;
GO
PRINT N'Creating [Enterprise].[CreateProductBatch]'
GO
CREATE PROCEDURE [Enterprise].[CreateProductBatch](  
	@PersonRealPageId UNIQUEIDENTIFIER   
	,@CreateUserPersonaId bigint   
	,@AssignUserPersonaId bigint  
	,@ProductId int  
	,@StatusTypeId int = 5 --waiting  
	,@RetryCount tinyint = 0 -- no retries on a new  
	,@InputJson nvarchar(max)  
	,@LastRunDate smalldatetime = NULL  
	,@CreatedDate smalldatetime = NULL
	,@ModifiedDate smalldatetime = NULL  
	,@ErrorDetails varchar(max) = NULL  
 )   
AS  
BEGIN  
	SET NOCOUNT ON;  

	DECLARE @NOW smalldatetime = GETUTCDATE(),
		@PersonPartyId bigint;  
   
	IF @CreatedDate IS NULL
	BEGIN
		SET @CreatedDate = @NOW;
	END

	IF @ModifiedDate IS NULL
	BEGIN
		SET @ModifiedDate = @NOW;
	END
  
	SELECT	@PersonPartyId = PartyId   
	FROM	Enterprise.Party  
	WHERE	RealPageId = @PersonRealPageId;  
	   
	BEGIN TRY  
		BEGIN TRAN;  
		INSERT INTO [Enterprise].[ProductBatch]  
		(
			[PersonPartyId]  
			,[CreateUserPersonaId]  
			,[AssignUserPersonaId]  
			,[ProductId]  
			,[StatusTypeId]  
			,[RetryCount]  
			,[InputJson]  
			,[LastRunDate]  
			,[CreatedDate]  
			,[ModifiedDate]  
			,[ErrorDetails]
		)
		OUTPUT	Inserted.ProductBatchId AS Id,
				'' AS ErrorMessage  
		VALUES
		(
			@PersonPartyId  
			,@CreateUserPersonaId  
			,@AssignUserPersonaId  
			,@ProductId  
			,@StatusTypeId  
			,@RetryCount  
			,@InputJson  
			,@LastRunDate  
			,@CreatedDate  
			,@ModifiedDate  
			,@ErrorDetails
		)
		COMMIT;  
	END TRY  
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT	0 AS Id,
				ErrorMessage
        FROM	dbo.ErrorLog
        WHERE	ErrorLogID = @ErrorLogID;
    END CATCH; 
END;
GO
PRINT N'Creating [Enterprise].[CreateProductConfiguration]'
GO
CREATE PROCEDURE [Enterprise].[CreateProductConfiguration] (
    @ConfigurationId INT OUTPUT
)
AS
BEGIN
	BEGIN TRY
        BEGIN TRANSACTION;
		INSERT INTO Enterprise.Configuration (
			CreateDate
		)
		OUTPUT	Inserted.ConfigurationId AS Id,
				'' AS ErrorMessage
		VALUES (
			GETUTCDATE()
		);

		SET @ConfigurationId = SCOPE_IDENTITY();
		COMMIT;
	END TRY  
	BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
	END CATCH
END;
GO
PRINT N'Creating [Person].[CreateTelecommunicationsNumber]'
GO
CREATE PROCEDURE [Person].[CreateTelecommunicationsNumber] (
	@ContactMechanismId INT,
	@CountryCode VARCHAR(10) = 1,
	@AreaCode VARCHAR(10),
	@Phonenumber VARCHAR(10)
)
AS
BEGIN
    BEGIN TRY
		-- Check if the ContactMechanism Exists, If it does, then update it.
        BEGIN TRANSACTION; 

        UPDATE  t
        SET     t.CountryCode = @CountryCode,
                t.AreaCode = @AreaCode,
                t.PhoneNumber = @Phonenumber
        FROM    Enterprise.TelecommunicationsNumber t
        WHERE   ContactMechanismID = @ContactMechanismId;

        IF @@ROWCOUNT = 0
        BEGIN
			INSERT  INTO Enterprise.TelecommunicationsNumber (
				ContactMechanismID ,
				CountryCode ,
				AreaCode ,
				PhoneNumber
			)
			VALUES (
				@ContactMechanismId , -- ContactMechanismID - int
				@CountryCode , -- CountryCode - varchar(10)
				@AreaCode , -- AreaCode - varchar(10)
				@Phonenumber  -- PhoneNumber - varchar(10)
			);		    
        END;

		SELECT	@ContactMechanismId AS Id,
                '' AS ErrorMessage
        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id ,
                ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
    END CATCH;
END;
GO
PRINT N'Creating [Enterprise].[CreateProductConfigurationbyPersonaId]'
GO
CREATE PROCEDURE [Enterprise].[CreateProductConfigurationbyPersonaId](    
  @PersonaId int    
 ,@ProductId int    
 ,@ProductSettingID int    
 ,@FromDate datetime = NULL    
 ,@ThruDate datetime = NULL    
)    
    
AS    
BEGIN    
    
  SET NOCOUNT ON;    
  DECLARE @NOW datetime = GETUTCDATE();    
  DECLARE @ConfigurationId int = NULL    
  DECLARE @ProductConfigurationId int = null    
  DECLARE @ProductSettingTypeId int = null    
     DECLARE @ProductConfigurationIds TABLE ( ProductConfigurationId INT )  
  
  IF @Fromdate IS NULL    
  SET @FromDate = @NOW;    
    
  --the persona configuration MUST be set up before this procedure is called    
  SELECT @ConfigurationId = ConfigurationId     
  FROM Enterprise.PersonaConfiguration     
  WHERE PersonaId = @PersonaId AND ProductId = @ProductId    
  AND ((@NOW BETWEEN FromDate AND ThruDate) OR (@NOW >= dateadd(mi, -1, FromDate) AND ThruDate IS NULL))    
    
  IF @ConfigurationId IS NULL    
  BEGIN    
   SELECT 0 AS Id, 'A valid record in Persona configuration with this PersonaId and ProductId must exist to extract the ConfigurationId before this procedure is called.' AS ErrorMessage    
  END    
   ELSE
       BEGIN 
         SELECT @ProductSettingTypeId = ProductSettingTypeId  
         FROM Enterprise.ProductSetting  
         WHERE ProductSettingId = @ProductSettingID  
  
         -- check for the existing product setting  
         INSERT INTO @ProductConfigurationIds ( ProductConfigurationId )  
         SELECT ProductConfigurationId     
         FROM Enterprise.ProductConfiguration    
         WHERE   
         ProductSettingId = @ProductSettingID    
         AND ConfigurationId = @ConfigurationId    
  
         -- now check for any other settings under the same configuration that have the same setting type and end them  
         INSERT INTO @ProductConfigurationIds ( ProductConfigurationId )  
         SELECT ProductConfigurationId  
         FROM Enterprise.ProductConfiguration PC  
         INNER JOIN Enterprise.ProductSetting PS  
          ON PC.ProductSettingId = PS.ProductSettingId  
         WHERE   
         PS.ProductSettingTypeId = @ProductSettingTypeId  
         AND PC.ConfigurationId = @ConfigurationId     

          BEGIN TRY    
          IF EXISTS ( SELECT TOP 1 'X' FROM @ProductConfigurationIds )  
          BEGIN    
           UPDATE PC   
           SET ThruDate = @NOW   
           FROM  
           Enterprise.ProductConfiguration PC   
           INNER JOIN @ProductConfigurationIds PCID  
            ON PC.ProductConfigurationId = PCID.ProductConfigurationId  
          END    
    
          INSERT INTO Enterprise.ProductConfiguration (ConfigurationId,ProductSettingId,FromDate,ThruDate)     
          OUTPUT Inserted.ProductConfigurationId AS Id, '' AS ErrorMessage    
          VALUES (@ConfigurationId, @ProductSettingID,@FromDate,@ThruDate);    
   
        END TRY 
         BEGIN CATCH    
  DECLARE @ErrorLogID INT;    
  EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;    
    
  SELECT 0 AS Id ,    
    ErrorMessage    
  FROM   dbo.ErrorLog    
  WHERE  ErrorLogID = @ErrorLogID;    
    
 END CATCH;      
END


END;
GO
PRINT N'Creating [Person].[CreateElectronicAddress]'
GO
CREATE PROCEDURE [Person].[CreateElectronicAddress] (
	@ContactMechanismId INT,
	@ElectronicAddressString VARCHAR(255),
	@ElectronicAddressType VARCHAR(20) = 'E-Mail'
)
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;

		UPDATE	Enterprise.ElectronicAddress
		SET		ElectronicAddressString = @ElectronicAddressString,
				ElectronicAddressType = @ElectronicAddressType
		WHERE	ContactMechanismID = @ContactMechanismId

        IF @@ROWCOUNT = 0
        BEGIN
			INSERT  INTO Enterprise.ElectronicAddress (
				ContactMechanismID,
				ElectronicAddressString,
				ElectronicAddressType
			)
			VALUES (
				@ContactMechanismId,
				@ElectronicAddressString,
				@ElectronicAddressType
			);
		END

		SELECT	@ContactMechanismId AS Id,
                '' AS ErrorMessage

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
    END CATCH;
END
GO
PRINT N'Creating [Enterprise].[CreateProductSetting]'
GO
CREATE PROCEDURE [Enterprise].[CreateProductSetting] (
	@ProductId INT,
	@ProductSettingTypeId INT,
	@Value NVARCHAR(1000),
	@FromDate DATETIME,
	@ThruDate DATETIME = NULL,
	@ProductSettingId INT OUTPUT
)
AS
BEGIN
	SET @ProductSettingId = NULL
	BEGIN TRY
		SELECT	@ProductSettingId = ps.ProductSettingId
		FROM	Enterprise.ProductSetting ps
				INNER JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
		WHERE	ps.ProductId = @ProductId
		AND		ps.Value = @Value
		AND		ps.ProductSettingTypeId = @ProductSettingTypeId
		AND		(ps.ThruDate >= @ThruDate OR ThruDate IS NULL);

		IF @ProductSettingId IS NULL
		BEGIN
			INSERT INTO Enterprise.ProductSetting (
				ProductId,
				ProductSettingTypeId,
				Value,
				FromDate,
				ThruDate
			)
			OUTPUT	Inserted.ProductSettingId AS Id,
					'' AS ErrorMessage
			VALUES (
				@ProductId,
				@ProductSettingTypeId,
				@Value,
				ISNULL(@FromDate, GETUTCDATE()),
				@ThruDate
			)

			SET @ProductSettingId = SCOPE_IDENTITY();
		END
		ELSE
		BEGIN
			SELECT @ProductSettingId as Id, '' AS ErrorMessage
		END
	END TRY  
	BEGIN CATCH
        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
	END CATCH
END;
GO
PRINT N'Creating [Person].[LinkGeographicBoundaryToContactMechanism]'
GO
CREATE PROCEDURE [Person].[LinkGeographicBoundaryToContactMechanism] (
	@ContactMechanismId INT,
	@GeographicBoundaryId INT,
	@FromDate DATETIME,
	@ThruDate DATETIME = NULL
)
AS
BEGIN
	BEGIN TRY
	    BEGIN TRANSACTION;

		INSERT INTO Enterprise.ContactMechanismBoundary (
			ContactMechanismId,
			GeographicBoundaryId,
			FromDate,
			ThruDate
		)
		OUTPUT	Inserted.ContactMechanismBoundaryId AS Id,
				'' AS ErrorMessage
		VALUES (
			@ContactMechanismId, -- ContactMechanismId - int
			@GeographicBoundaryId, -- GeographicBoundaryId - int
			@FromDate, -- FromDate - datetime
			@ThruDate  -- ThruDate - datetime
		)

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
    END CATCH;
END
GO
PRINT N'Creating [Enterprise].[ProductRelationship]'
GO
CREATE TABLE [Enterprise].[ProductRelationship]
(
[ProductRelationshipId] [int] NOT NULL IDENTITY(1, 1),
[PartyIdFrom] [bigint] NOT NULL,
[ProductIdTo] [int] NOT NULL,
[RoleTypeIdFrom] [int] NOT NULL,
[RoleTypeIdTo] [int] NOT NULL,
[FromDate] [datetime] NOT NULL CONSTRAINT [DF__ProductRe__FromD__0697FACD] DEFAULT (getutcdate()),
[ThruDate] [datetime] NULL
)
GO
PRINT N'Creating primary key [PK_ProductRelationship] on [Enterprise].[ProductRelationship]'
GO
ALTER TABLE [Enterprise].[ProductRelationship] ADD CONSTRAINT [PK_ProductRelationship] PRIMARY KEY CLUSTERED  ([ProductRelationshipId])
GO
PRINT N'Creating [Enterprise].[PersonaOrganization]'
GO
CREATE TABLE [Enterprise].[PersonaOrganization]
(
[PersonaOrganizationId] [bigint] NOT NULL IDENTITY(1, 1),
[PersonaConfigurationId] [bigint] NOT NULL,
[OrganizationId] [bigint] NOT NULL,
[FromDate] [datetime] NOT NULL CONSTRAINT [DF__PersonaOr__FromD__09746778] DEFAULT (getutcdate()),
[ThruDate] [datetime] NULL
)
GO
PRINT N'Creating primary key [PK_PersonaOrganization] on [Enterprise].[PersonaOrganization]'
GO
ALTER TABLE [Enterprise].[PersonaOrganization] ADD CONSTRAINT [PK_PersonaOrganization] PRIMARY KEY CLUSTERED  ([PersonaOrganizationId])
GO
PRINT N'Creating [Auth].[Certificates]'
GO
CREATE TABLE [Auth].[Certificates]
(
[CertificatesId] [int] NOT NULL IDENTITY(1, 1),
[CertificatesTypeId] [int] NOT NULL,
[CertificatesLocationTypeID] [int] NOT NULL,
[Name] [nvarchar] (100) NOT NULL,
[Description] [nvarchar] (4000) NULL,
[SubjectName] [nvarchar] (1024) NULL,
[CreatedDate] [datetime] NOT NULL CONSTRAINT [DF_Certificates_CreatedDate] DEFAULT (getutcdate()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_Certificates_ModifiedDate] DEFAULT (getutcdate()),
[Thumbprint] [nvarchar] (100) NULL,
[Issuer] [nvarchar] (100) NULL,
[RawData] [varbinary] (max) NULL,
[HasPassword] [bit] NOT NULL CONSTRAINT [DF_Certificates_HasPassword] DEFAULT ((0)),
[Password] [varbinary] (max) NULL,
[CertificateExpirationDate] [datetime] NULL,
[CertificateStartDate] [datetime] NULL,
[HasPrivateKey] [bit] NOT NULL CONSTRAINT [DF_Certificates_HasPrivateKey] DEFAULT ((0))
)
GO
PRINT N'Creating primary key [PK_Certificates] on [Auth].[Certificates]'
GO
ALTER TABLE [Auth].[Certificates] ADD CONSTRAINT [PK_Certificates] PRIMARY KEY CLUSTERED  ([CertificatesId])
GO
PRINT N'Creating [dbo].[BuildVersion]'
GO
CREATE TABLE [dbo].[BuildVersion]
(
[SystemInformationID] [tinyint] NOT NULL IDENTITY(1, 1),
[Database Version] [nvarchar] (25) NOT NULL,
[VersionDate] [datetime] NOT NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_BuildVersion_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_BuildVersion_SystemInformationID] on [dbo].[BuildVersion]'
GO
ALTER TABLE [dbo].[BuildVersion] ADD CONSTRAINT [PK_BuildVersion_SystemInformationID] PRIMARY KEY CLUSTERED  ([SystemInformationID])
GO
PRINT N'Creating [Enterprise].[StatusTypeCategoryClassification]'
GO
CREATE TABLE [Enterprise].[StatusTypeCategoryClassification]
(
[StatusTypeCategoryClassificationId] [int] NOT NULL IDENTITY(1, 1),
[StatusTypeId] [int] NOT NULL,
[StatusTypeCategoryId] [int] NOT NULL,
[FromDate] [datetime] NOT NULL CONSTRAINT [DF__StatusTyp__FromD__03BB8E22] DEFAULT (getutcdate()),
[ThruDate] [datetime] NULL
)
GO
PRINT N'Creating primary key [PK_StatusTypeCategoryClassification] on [Enterprise].[StatusTypeCategoryClassification]'
GO
ALTER TABLE [Enterprise].[StatusTypeCategoryClassification] ADD CONSTRAINT [PK_StatusTypeCategoryClassification] PRIMARY KEY CLUSTERED  ([StatusTypeCategoryClassificationId])
GO
PRINT N'Creating [Auth].[CertificatesLocationType]'
GO
CREATE TABLE [Auth].[CertificatesLocationType]
(
[CertificatesLocationTypeId] [int] NOT NULL IDENTITY(1, 1),
[Name] [nvarchar] (100) NOT NULL
)
GO
PRINT N'Creating primary key [PK_CertificatesLocationType] on [Auth].[CertificatesLocationType]'
GO
ALTER TABLE [Auth].[CertificatesLocationType] ADD CONSTRAINT [PK_CertificatesLocationType] PRIMARY KEY CLUSTERED  ([CertificatesLocationTypeId])
GO
PRINT N'Creating [Auth].[CertificatesType]'
GO
CREATE TABLE [Auth].[CertificatesType]
(
[CertificatesTypeId] [int] NOT NULL IDENTITY(1, 1),
[Name] [nvarchar] (100) NOT NULL
)
GO
PRINT N'Creating primary key [PK_CertificatesType] on [Auth].[CertificatesType]'
GO
ALTER TABLE [Auth].[CertificatesType] ADD CONSTRAINT [PK_CertificatesType] PRIMARY KEY CLUSTERED  ([CertificatesTypeId])
GO
PRINT N'Creating [Enterprise].[CommunicationEvent]'
GO
CREATE TABLE [Enterprise].[CommunicationEvent]
(
[CommunicationEventID] [bigint] NOT NULL IDENTITY(1, 1),
[StatusTypeID] [int] NOT NULL,
[ContactMechanismTypeID] [int] NOT NULL,
[CommunicationEventPurposeTypeID] [int] NOT NULL,
[FromPartyId] [bigint] NOT NULL,
[ToPartyId] [bigint] NOT NULL,
[Started] [datetime] NOT NULL,
[Ended] [datetime] NULL,
[Note] [nvarchar] (1000) NULL
)
GO
PRINT N'Creating primary key [PK_CommunicationEvent] on [Enterprise].[CommunicationEvent]'
GO
ALTER TABLE [Enterprise].[CommunicationEvent] ADD CONSTRAINT [PK_CommunicationEvent] PRIMARY KEY CLUSTERED  ([CommunicationEventID])
GO
PRINT N'Creating [Enterprise].[CESCommunicationEvent]'
GO
CREATE TABLE [Enterprise].[CESCommunicationEvent]
(
[CESCommunicationEventId] [bigint] NOT NULL,
[CESId] [nvarchar] (255) NOT NULL,
[CommunicationEventId] [bigint] NOT NULL
)
GO
PRINT N'Creating primary key [PK_CESCommunicationEvent] on [Enterprise].[CESCommunicationEvent]'
GO
ALTER TABLE [Enterprise].[CESCommunicationEvent] ADD CONSTRAINT [PK_CESCommunicationEvent] PRIMARY KEY CLUSTERED  ([CESCommunicationEventId])
GO
PRINT N'Creating [Enterprise].[ContactMechanismValidRole]'
GO
CREATE TABLE [Enterprise].[ContactMechanismValidRole]
(
[ContactMechanismValidRoleID] [int] NOT NULL IDENTITY(1, 1),
[ContactMechanismTypeID] [int] NOT NULL,
[CommunicationEventRoleTypeID] [int] NOT NULL
)
GO
PRINT N'Creating primary key [PK_ContactMechanismValidRole] on [Enterprise].[ContactMechanismValidRole]'
GO
ALTER TABLE [Enterprise].[ContactMechanismValidRole] ADD CONSTRAINT [PK_ContactMechanismValidRole] PRIMARY KEY CLUSTERED  ([ContactMechanismValidRoleID])
GO
PRINT N'Creating [Enterprise].[DataImportApplication]'
GO
CREATE TABLE [Enterprise].[DataImportApplication]
(
[DataImportApplicationId] [int] NOT NULL IDENTITY(1, 1),
[Name] [nvarchar] (50) NOT NULL,
[Description] [nvarchar] (100) NULL
)
GO
PRINT N'Creating primary key [PK_DataImportApplication] on [Enterprise].[DataImportApplication]'
GO
ALTER TABLE [Enterprise].[DataImportApplication] ADD CONSTRAINT [PK_DataImportApplication] PRIMARY KEY CLUSTERED  ([DataImportApplicationId])
GO
PRINT N'Creating [Auth].[ClientClaims]'
GO
CREATE TABLE [Auth].[ClientClaims]
(
[ClientClaimsId] [int] NOT NULL IDENTITY(1, 1),
[ClientId] [int] NOT NULL,
[Type] [nvarchar] (100) NOT NULL,
[Value] [nvarchar] (100) NOT NULL
)
GO
PRINT N'Creating primary key [PK_dbo.ClientClaims] on [Auth].[ClientClaims]'
GO
ALTER TABLE [Auth].[ClientClaims] ADD CONSTRAINT [PK_dbo.ClientClaims] PRIMARY KEY CLUSTERED  ([ClientClaimsId])
GO
PRINT N'Creating [Auth].[ClientCorsOrigins]'
GO
CREATE TABLE [Auth].[ClientCorsOrigins]
(
[ClientCorsOriginId] [int] NOT NULL IDENTITY(1, 1),
[ClientId] [int] NOT NULL,
[Origin] [nvarchar] (150) NOT NULL
)
GO
PRINT N'Creating primary key [PK_dbo.ClientCorsOrigins] on [Auth].[ClientCorsOrigins]'
GO
ALTER TABLE [Auth].[ClientCorsOrigins] ADD CONSTRAINT [PK_dbo.ClientCorsOrigins] PRIMARY KEY CLUSTERED  ([ClientCorsOriginId])
GO
PRINT N'Creating [Auth].[ClientCustomGrantTypes]'
GO
CREATE TABLE [Auth].[ClientCustomGrantTypes]
(
[ClientCustomGrantTypeId] [int] NOT NULL IDENTITY(1, 1),
[ClientId] [int] NOT NULL,
[GrantType] [nvarchar] (250) NOT NULL
)
GO
PRINT N'Creating primary key [PK_dbo.ClientCustomGrantTypes] on [Auth].[ClientCustomGrantTypes]'
GO
ALTER TABLE [Auth].[ClientCustomGrantTypes] ADD CONSTRAINT [PK_dbo.ClientCustomGrantTypes] PRIMARY KEY CLUSTERED  ([ClientCustomGrantTypeId])
GO
PRINT N'Creating [Auth].[ClientIdentityProviderRestrictions]'
GO
CREATE TABLE [Auth].[ClientIdentityProviderRestrictions]
(
[ClientIdentityProviderRestrictionId] [int] NOT NULL IDENTITY(1, 1),
[ClientId] [int] NOT NULL,
[Provider] [nvarchar] (200) NOT NULL
)
GO
PRINT N'Creating primary key [PK_dbo.ClientIdPRestrictions] on [Auth].[ClientIdentityProviderRestrictions]'
GO
ALTER TABLE [Auth].[ClientIdentityProviderRestrictions] ADD CONSTRAINT [PK_dbo.ClientIdPRestrictions] PRIMARY KEY CLUSTERED  ([ClientIdentityProviderRestrictionId])
GO
PRINT N'Creating [Enterprise].[CommunicationEventRole]'
GO
CREATE TABLE [Enterprise].[CommunicationEventRole]
(
[CommunicationEventRoleID] [int] NOT NULL IDENTITY(1, 1),
[CommunicationEventID] [bigint] NOT NULL,
[PartyID] [bigint] NOT NULL,
[CommunicationEventRoleTypeID] [int] NOT NULL
)
GO
PRINT N'Creating primary key [PK_Enterprise.CommunicationEventRole] on [Enterprise].[CommunicationEventRole]'
GO
ALTER TABLE [Enterprise].[CommunicationEventRole] ADD CONSTRAINT [PK_Enterprise.CommunicationEventRole] PRIMARY KEY CLUSTERED  ([CommunicationEventRoleID])
GO
PRINT N'Creating [Ident].[SamlAttributeStatement]'
GO
CREATE TABLE [Ident].[SamlAttributeStatement]
(
[SamlAttributeStatementId] [int] NOT NULL IDENTITY(1, 1),
[ProductId] [int] NOT NULL,
[SamlAttributeId] [int] NOT NULL
)
GO
PRINT N'Creating primary key [PK_SamlAttributeStatement] on [Ident].[SamlAttributeStatement]'
GO
ALTER TABLE [Ident].[SamlAttributeStatement] ADD CONSTRAINT [PK_SamlAttributeStatement] PRIMARY KEY CLUSTERED  ([SamlAttributeStatementId])
GO
PRINT N'Creating [Enterprise].[StatusTypeCategoryType]'
GO
CREATE TABLE [Enterprise].[StatusTypeCategoryType]
(
[StatusTypeCategoryTypeId] [int] NOT NULL IDENTITY(1, 1),
[ParentStatusTypeCategoryTypeId] [int] NULL,
[Name] [varchar] (50) NOT NULL
)
GO
PRINT N'Creating primary key [PK_StatusTypeCategoryType] on [Enterprise].[StatusTypeCategoryType]'
GO
ALTER TABLE [Enterprise].[StatusTypeCategoryType] ADD CONSTRAINT [PK_StatusTypeCategoryType] PRIMARY KEY CLUSTERED  ([StatusTypeCategoryTypeId])
GO
PRINT N'Creating [Enterprise].[StatusTypeCategory]'
GO
CREATE TABLE [Enterprise].[StatusTypeCategory]
(
[StatusTypeCategoryId] [int] NOT NULL IDENTITY(1, 1),
[ParentStatusTypeCategoryId] [int] NULL,
[StatusTypeCategoryTypeId] [int] NOT NULL,
[Name] [varchar] (50) NOT NULL
)
GO
PRINT N'Creating primary key [PK_StatusTypeCategory] on [Enterprise].[StatusTypeCategory]'
GO
ALTER TABLE [Enterprise].[StatusTypeCategory] ADD CONSTRAINT [PK_StatusTypeCategory] PRIMARY KEY CLUSTERED  ([StatusTypeCategoryId])
GO
PRINT N'Creating [dbo].[DatabaseLog]'
GO
CREATE TABLE [dbo].[DatabaseLog]
(
[DatabaseLogID] [int] NOT NULL IDENTITY(1, 1),
[PostTime] [datetime] NOT NULL,
[DatabaseUser] [sys].[sysname] NOT NULL,
[Event] [sys].[sysname] NOT NULL,
[Schema] [sys].[sysname] NULL,
[Object] [sys].[sysname] NULL,
[TSQL] [nvarchar] (max) NOT NULL,
[XmlEvent] [xml] NOT NULL
)
GO
PRINT N'Creating primary key [PK_DatabaseLog_DatabaseLogID] on [dbo].[DatabaseLog]'
GO
ALTER TABLE [dbo].[DatabaseLog] ADD CONSTRAINT [PK_DatabaseLog_DatabaseLogID] PRIMARY KEY NONCLUSTERED  ([DatabaseLogID])
GO
PRINT N'Creating [Auth].[GetAllOrganizationClientUserClaims]'
GO
CREATE PROCEDURE [Auth].[GetAllOrganizationClientUserClaims]
	@OrganizationId  int ,
    @ClientCode NVARCHAR (200) ,
    @UserID   bigint 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--SELECT OCC.ID,OCC.OrganizationID,C.ClientID,OCC.UserID,OCC.Type,OCC.Value
	--FROM Auth.OrganizationClientUserClaims OCC INNER JOIN Auth.Clients C ON C.ClientId = OCC.ClientId
	--WHERE OCC.OrganizationID = @OrganizationID AND C.ClientCode = @ClientCode AND OCC.UserID = @UserID

END
GO
PRINT N'Creating [Auth].[GetProducts]'
GO
CREATE PROCEDURE [Auth].[GetProducts]
(
    @skipRows int = 0,
	@takeRows int = 0,
    @whereCondition nvarchar(max) = null,	   
    @sortColumnList nvarchar(max) = null	   
)
AS
BEGIN
       
	SELECT 'No. This is bad.' AS Information

--declare @sql nvarchar(max) = ''
--declare @condition nvarchar(max) = ' WHERE TitleId IS NOT NULL '


--SET NOCOUNT ON;

--if (@whereCondition is not null and @whereCondition <> '')
--begin
--	set @condition += ' AND '
--end

--if (@whereCondition is not null and @whereCondition <> '')
--begin
--	set @condition += ' (' + cast(@whereCondition as nvarchar(max)) + ') '
--end

--set @sql += '
--	SELECT COUNT(1) as [COUNT] from [Auth].[Product] WITH(NOLOCK) '
--set @sql += @condition 

--set @sql += '
--	SELECT ProductId, ProductName, Description, ClientId, ClassName, SettingsUrl, ProductUrl, SubDescription, TitleId, 
--		IsNewTab FROM [Auth].[Product] WITH(NOLOCK) '
--set @sql += @condition 

--if (@sortColumnList is not null and @sortColumnList <> '')
--begin
--	set @sql += ' ORDER BY ' + cast(@sortColumnList as nvarchar(max))
--end
--else
--begin
--    set @sql += ' ORDER BY TitleId ASC '
--end

--set @sql += ' OFFSET @_skipRows ROWS '

--if (@takeRows > 0)
--begin
--	set @sql += ' FETCH NEXT @_takeRows ROWS ONLY '
--end

--print @sql

--exec sp_executesql @sql, N'@_skipRows int, @_takeRows int', @skipRows, @takeRows

END
GO
PRINT N'Creating [Auth].[GetUsers]'
GO
CREATE PROCEDURE [Auth].[GetUsers]
       @skipRows int = 0,
       @takeRows int = 0,
       @whereCondition nvarchar(max) = null,
	   @whereProductSubCondition nvarchar(max) = null,
       @sortColumnList nvarchar(max) = null	   
AS
BEGIN

SELECT 'No. Do not do this.' AS Information

--declare @sql nvarchar(max) = ''
--declare @joinProduct nvarchar(max) = ''
--declare @condition nvarchar(max) = ''


--SET NOCOUNT ON;

--if (@whereProductSubCondition is not null and @whereProductSubCondition <> '')
--begin
--	set @joinProduct += '		
--		INNER JOIN [Auth].[PortfolioProductUser] WITH(NOLOCK) PPU ON PPU.UserId = U.UserId 
--		LEFT JOIN Product WITH(NOLOCK) ON PPU.ProductId = Product.ProductId '
--end

--if (@whereCondition is not null and @whereCondition <> '')
--begin
--	set @condition += ' WHERE '
--end

--if (@whereCondition is not null and @whereCondition <> '')
--begin
--	set @condition += ' (' + cast(@whereCondition as nvarchar(max)) + ') '
--end

--if (@whereProductSubCondition is not null and @whereProductSubCondition <> '')
--begin
--	if (@whereCondition is not null and @whereCondition <> '')
--		set @condition += ' AND '
--	set @condition += ' ' + cast(@whereProductSubCondition as nvarchar(max))	
--end

--set @sql += 'SELECT COUNT(DISTINCT U.UserId) as [COUNT] from [Auth].[Users] U WITH(NOLOCK) '
--set @sql += @joinProduct 
--set @sql += @condition 

--set @sql += '         
--	SELECT * INTO #tempUsers
--		FROM (SELECT DISTINCT U.* FROM [Auth].[Users] U WITH(NOLOCK) '

--set @sql += @joinProduct 
--set @sql += @condition 

--if (@sortColumnList is not null and @sortColumnList <> '')
--begin
--	set @sql += ' ORDER BY ' + cast(@sortColumnList as nvarchar(max))
--end
--else
--begin
--    set @sql += ' ORDER BY IsActive ASC, FirstName ASC '
--end

--set @sql += ' OFFSET @_skipRows ROWS '

--if (@takeRows > 0)
--begin
--	set @sql += ' FETCH NEXT @_takeRows ROWS ONLY '
--end

--set @sql += ') AS U '
	
--Set @sql += '

--	SELECT * FROM #tempUsers WITH(NOLOCK) 
	
--	SELECT PPU.*, Product.* 
--		FROM #tempUsers U WITH(NOLOCK)
--		INNER JOIN
--		(
--			SELECT ProductId, UserId 
--			FROM [Auth].[PortfolioProductUser] WITH(NOLOCK)
--			GROUP BY ProductId, UserId 
--		) PPU 
--		ON PPU.UserId = U.UserId 
--		INNER JOIN Product WITH(NOLOCK) ON PPU.ProductId = Product.ProductId AND Product.ProductName NOT IN (''landing'', ''clientportal'') '
		
--set @sql += @condition 

--Set @sql += ' 	
--	DROP TABLE IF EXISTS #tempUsers '

----print @sql

--exec sp_executesql @sql, N'@_skipRows int, @_takeRows int', @skipRows, @takeRows

END
GO
PRINT N'Creating [Auth].[UnlockUsers]'
GO
CREATE PROCEDURE [Auth].[UnlockUsers] 
(
	@enterpriseUserIds NVARCHAR(MAX)
)
AS
BEGIN
	SELECT 'Deprecated: Moved to ident schema.' AS Information

	--DECLARE @forgotPassword INT = 2
	--DECLARE @questionAttempts INT = 5
	--DECLARE @verifyAnswers INT = 6

	--BEGIN TRY
 --       BEGIN TRANSACTION; 			
	--		UPDATE	[Auth].[Users] 
	--		SET		IsLocked = 0 
	--		WHERE	UserId IN (SELECT * FROM STRING_SPLIT (@enterpriseUserIds, ','))
	       
	--		-- Reset user activity attempts for ForgotPassword, QuestionAttempts and VerifyAnswers
	--		UPDATE	[Auth].[ActivityAttempts] 
	--		SET		AttemptCount = 0 
	--		WHERE	EnterpriseUserName IN (
	--					SELECT	LoginId 
	--						FROM (
	--							SELECT	value, 
	--									U.LoginId 
	--							FROM	STRING_SPLIT (@enterpriseUserIds, ',') SS 
	--						INNER JOIN [Auth].[Users] U WITH(NOLOCK) ON U.UserId = SS.value) SSU
	--				)
	--				AND ActivityId IN (@forgotPassword, @questionAttempts, @verifyAnswers)
              
	--		SELECT  UserId,
	--				LoginId,
	--				Firstname,
	--				LastName,
	--				IsActive,
	--				PasswordHash,
	--				PasswordSalt,
	--				IdentityProvider,
	--				Title,
	--				Email,
	--				Phone,
	--				IsLocked,
	--				LastPasswordModifiedDateTime,
	--				AccountExpiration
	--		FROM	[Auth].[Users] WITH(NOLOCK) 
	--		WHERE	UserId IN (SELECT * FROM STRING_SPLIT (@enterpriseUserIds, ',')) AND IsLocked = 0			

	--	COMMIT;
	--END TRY  
	--BEGIN CATCH
 --       ROLLBACK;
		
 --       DECLARE @ErrorLogID INT;
 --       EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

 --       SELECT  0 AS Id,
	--			ErrorMessage
 --       FROM    dbo.ErrorLog WITH(NOLOCK)
 --       WHERE   ErrorLogID = @ErrorLogID;
	--END CATCH
END;
GO
PRINT N'Creating [Person].[ListPreferredContactMethods]'
GO
CREATE PROCEDURE [Person].[ListPreferredContactMethods]
AS
BEGIN
	SELECT	1 AS PreferredContactMethodId,
			'Phone' AS Name
	UNION ALL
	SELECT	2 AS PreferredContactMethodId,
			'Email' AS Name
END
GO
PRINT N'Adding constraints to [Auth].[PasswordPolicy]'
GO
ALTER TABLE [Auth].[PasswordPolicy] ADD CONSTRAINT [CHK_PasswordPolicy_PasswordExpirationPeriodInDays] CHECK (([EnablePasswordExpiration]=(0) AND isnull([PasswordExpirationPeriodInDays],(0))=(0) OR [EnablePasswordExpiration]=(1) AND [PasswordExpirationPeriodInDays]>(0)))
GO
ALTER TABLE [Auth].[PasswordPolicy] ADD CONSTRAINT [CHK_PasswordPolicy_NumberOfPasswordsToRemember] CHECK (([PreventPasswordReuse]=(0) AND isnull([NumberOfPasswordsToRemember],(0))=(0) OR [PreventPasswordReuse]=(1) AND [NumberOfPasswordsToRemember]>(0)))
GO
PRINT N'Adding constraints to [Ident].[PasswordPolicy]'
GO
ALTER TABLE [Ident].[PasswordPolicy] ADD CONSTRAINT [CHK_PasswordPolicy_PasswordExpirationPeriodInDays] CHECK (([EnablePasswordExpiration]=(0) AND isnull([PasswordExpirationPeriodInDays],(0))=(0) OR [EnablePasswordExpiration]=(1) AND [PasswordExpirationPeriodInDays]>(0)))
GO
ALTER TABLE [Ident].[PasswordPolicy] ADD CONSTRAINT [CHK_PasswordPolicy_NumberOfPasswordsToRemember] CHECK (([PreventPasswordReuse]=(0) AND isnull([NumberOfPasswordsToRemember],(0))=(0) OR [PreventPasswordReuse]=(1) AND [NumberOfPasswordsToRemember]>(0)))
GO
PRINT N'Adding foreign keys to [Auth].[ActivityAttempts]'
GO
ALTER TABLE [Auth].[ActivityAttempts] ADD CONSTRAINT [FK_ActivityAttempts_Activity] FOREIGN KEY ([ActivityId]) REFERENCES [Auth].[Activity] ([ActivityId])
GO
PRINT N'Adding foreign keys to [Auth].[ActivityToken]'
GO
ALTER TABLE [Auth].[ActivityToken] ADD CONSTRAINT [FK_ActivityToken_Activity] FOREIGN KEY ([ActivityId]) REFERENCES [Auth].[Activity] ([ActivityId])
GO
ALTER TABLE [Auth].[ActivityToken] ADD CONSTRAINT [FK_ActivityToken_Users] FOREIGN KEY ([UserId]) REFERENCES [Auth].[Users] ([UserId])
GO
PRINT N'Adding foreign keys to [Auth].[PasswordHistory]'
GO
ALTER TABLE [Auth].[PasswordHistory] ADD CONSTRAINT [FK_PasswordHistory_Activity] FOREIGN KEY ([ActivityId]) REFERENCES [Auth].[Activity] ([ActivityId])
GO
ALTER TABLE [Auth].[PasswordHistory] ADD CONSTRAINT [FK_PasswordHistory_Users] FOREIGN KEY ([UserId]) REFERENCES [Auth].[Users] ([UserId])
GO
PRINT N'Adding foreign keys to [Auth].[UserLockAcitvity]'
GO
ALTER TABLE [Auth].[UserLockAcitvity] ADD CONSTRAINT [FK_UserLockAcitvity_Activity] FOREIGN KEY ([AcivityId]) REFERENCES [Auth].[Activity] ([ActivityId])
GO
ALTER TABLE [Auth].[UserLockAcitvity] ADD CONSTRAINT [FK_UserLockAcitvity_Users] FOREIGN KEY ([UserId]) REFERENCES [Auth].[Users] ([UserId])
GO
PRINT N'Adding foreign keys to [Auth].[Certificates]'
GO
ALTER TABLE [Auth].[Certificates] ADD CONSTRAINT [FK_Certificates_CertificatesLocationType] FOREIGN KEY ([CertificatesLocationTypeID]) REFERENCES [Auth].[CertificatesLocationType] ([CertificatesLocationTypeId])
GO
ALTER TABLE [Auth].[Certificates] ADD CONSTRAINT [FK_Certificates_CertificatesType] FOREIGN KEY ([CertificatesTypeId]) REFERENCES [Auth].[CertificatesType] ([CertificatesTypeId])
GO
PRINT N'Adding foreign keys to [Auth].[ClientClaims]'
GO
ALTER TABLE [Auth].[ClientClaims] ADD CONSTRAINT [FK_dbo.ClientClaims_dbo.Clients_Client_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([ClientId]) ON DELETE CASCADE
GO
PRINT N'Adding foreign keys to [Auth].[ClientCorsOrigins]'
GO
ALTER TABLE [Auth].[ClientCorsOrigins] ADD CONSTRAINT [FK_dbo.ClientCorsOrigins_dbo.Clients_Client_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([ClientId]) ON DELETE CASCADE
GO
PRINT N'Adding foreign keys to [Auth].[ClientCustomGrantTypes]'
GO
ALTER TABLE [Auth].[ClientCustomGrantTypes] ADD CONSTRAINT [FK_dbo.ClientCustomGrantTypes_dbo.Clients_Client_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([ClientId]) ON DELETE CASCADE
GO
PRINT N'Adding foreign keys to [Auth].[ClientIdentityProviderRestrictions]'
GO
ALTER TABLE [Auth].[ClientIdentityProviderRestrictions] ADD CONSTRAINT [FK_dbo.ClientIdPRestrictions_dbo.Clients_Client_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([ClientId]) ON DELETE CASCADE
GO
PRINT N'Adding foreign keys to [Auth].[ClientPostLogoutRedirectUris]'
GO
ALTER TABLE [Auth].[ClientPostLogoutRedirectUris] ADD CONSTRAINT [FK_dbo.ClientPostLogoutRedirectUris_dbo.Clients_Client_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([ClientId]) ON DELETE CASCADE
GO
PRINT N'Adding foreign keys to [Auth].[ClientRedirectUris]'
GO
ALTER TABLE [Auth].[ClientRedirectUris] ADD CONSTRAINT [FK_dbo.ClientRedirectUris_dbo.Clients_Client_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([ClientId]) ON DELETE CASCADE
GO
PRINT N'Adding foreign keys to [Auth].[ClientScopes]'
GO
ALTER TABLE [Auth].[ClientScopes] ADD CONSTRAINT [FK_dbo.ClientScopes_dbo.Clients_Client_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([ClientId]) ON DELETE CASCADE
GO
PRINT N'Adding foreign keys to [Auth].[ClientSecrets]'
GO
ALTER TABLE [Auth].[ClientSecrets] ADD CONSTRAINT [FK_dbo.ClientSecrets_dbo.Clients_Client_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([ClientId]) ON DELETE CASCADE
GO
PRINT N'Adding foreign keys to [Auth].[Product]'
GO
ALTER TABLE [Auth].[Product] ADD CONSTRAINT [FK_Clients_ClientID] FOREIGN KEY ([ClientID]) REFERENCES [Auth].[Clients] ([ClientId])
GO
PRINT N'Adding foreign keys to [Auth].[DeveloperClients]'
GO
ALTER TABLE [Auth].[DeveloperClients] ADD CONSTRAINT [FK_DeveloperClients_Clients] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([ClientId])
GO
ALTER TABLE [Auth].[DeveloperClients] ADD CONSTRAINT [FK_DeveloperClients_Developer] FOREIGN KEY ([DeveloperId]) REFERENCES [Auth].[Developer] ([DeveloperId])
GO
PRINT N'Adding foreign keys to [Auth].[PasswordPolicy]'
GO
ALTER TABLE [Auth].[PasswordPolicy] ADD CONSTRAINT [FK_PasswordPolicy_PortfolioId] FOREIGN KEY ([PortfolioId]) REFERENCES [Auth].[Portfolio] ([PortfolioId])
GO
PRINT N'Adding foreign keys to [Auth].[PortfolioProductUserClaims]'
GO
ALTER TABLE [Auth].[PortfolioProductUserClaims] ADD CONSTRAINT [FK_PortfolioProductUserClaims_PortfolioProductUserId] FOREIGN KEY ([PortfolioProductUserId]) REFERENCES [Auth].[PortfolioProductUser] ([PortfolioProductUserId])
GO
PRINT N'Adding foreign keys to [Auth].[UserSamlAttribute]'
GO
ALTER TABLE [Auth].[UserSamlAttribute] ADD CONSTRAINT [FK_UserSamlAttribute_PortfolioProductUser] FOREIGN KEY ([PortfolioProductUserId]) REFERENCES [Auth].[PortfolioProductUser] ([PortfolioProductUserId])
GO
ALTER TABLE [Auth].[UserSamlAttribute] ADD CONSTRAINT [FK_UserSamlAttribute_SamlAttribute] FOREIGN KEY ([SamlAttributeId]) REFERENCES [Auth].[SamlAttribute] ([SamlAttributeId])
GO
PRINT N'Adding foreign keys to [Auth].[PortfolioProductUser]'
GO
ALTER TABLE [Auth].[PortfolioProductUser] ADD CONSTRAINT [FK_PortfolioProductUser_PortfolioID] FOREIGN KEY ([PortfolioId]) REFERENCES [Auth].[Portfolio] ([PortfolioId])
GO
ALTER TABLE [Auth].[PortfolioProductUser] ADD CONSTRAINT [FK_PortfolioProductUser_ProductID] FOREIGN KEY ([ProductId]) REFERENCES [Auth].[Product] ([ProductId])
GO
ALTER TABLE [Auth].[PortfolioProductUser] ADD CONSTRAINT [FK_PortfolioProductUser_UserID] FOREIGN KEY ([UserId]) REFERENCES [Auth].[Users] ([UserId])
GO
PRINT N'Adding foreign keys to [Auth].[PortfolioProduct]'
GO
ALTER TABLE [Auth].[PortfolioProduct] ADD CONSTRAINT [FK_PortfolioProduct_PortfolioId] FOREIGN KEY ([PortfolioId]) REFERENCES [Auth].[Portfolio] ([PortfolioId])
GO
ALTER TABLE [Auth].[PortfolioProduct] ADD CONSTRAINT [FK_PortfolioProduct_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Auth].[Product] ([ProductId])
GO
PRINT N'Adding foreign keys to [Auth].[ProviderPortfolio]'
GO
ALTER TABLE [Auth].[ProviderPortfolio] ADD CONSTRAINT [FK_ProviderPortfolio_Portfolio] FOREIGN KEY ([PortfolioIdId]) REFERENCES [Auth].[Portfolio] ([PortfolioId])
GO
PRINT N'Adding foreign keys to [Auth].[ProductSamlSettings]'
GO
ALTER TABLE [Auth].[ProductSamlSettings] ADD CONSTRAINT [FK_Product_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Auth].[Product] ([ProductId])
GO
PRINT N'Adding foreign keys to [Auth].[SamlAttributeStatement]'
GO
ALTER TABLE [Auth].[SamlAttributeStatement] ADD CONSTRAINT [FK_SamlAttributeStatement_Product] FOREIGN KEY ([ProductId]) REFERENCES [Auth].[Product] ([ProductId])
GO
ALTER TABLE [Auth].[SamlAttributeStatement] ADD CONSTRAINT [FK_SamlAttributeStatement_SamlAttribute] FOREIGN KEY ([SamlAttributeId]) REFERENCES [Auth].[SamlAttribute] ([SamlAttributeId])
GO
PRINT N'Adding foreign keys to [Auth].[UserProviderPortfolio]'
GO
ALTER TABLE [Auth].[UserProviderPortfolio] ADD CONSTRAINT [FK_UserProviderPortfolio_ProviderPortfolio] FOREIGN KEY ([ProviderPortfolioId]) REFERENCES [Auth].[ProviderPortfolio] ([ProviderPortfolioId])
GO
ALTER TABLE [Auth].[UserProviderPortfolio] ADD CONSTRAINT [FK_UserProviderPortfolio_Users] FOREIGN KEY ([UserId]) REFERENCES [Auth].[Users] ([UserId])
GO
PRINT N'Adding foreign keys to [Auth].[ScopeClaims]'
GO
ALTER TABLE [Auth].[ScopeClaims] ADD CONSTRAINT [FK_dbo.ScopeClaims_dbo.Scopes_Scope_Id] FOREIGN KEY ([ScopeId]) REFERENCES [Auth].[Scopes] ([ScopeId]) ON DELETE CASCADE
GO
PRINT N'Adding foreign keys to [Auth].[ScopeSecrets]'
GO
ALTER TABLE [Auth].[ScopeSecrets] ADD CONSTRAINT [FK_dbo.ScopeSecrets_dbo.Scopes_Scope_Id] FOREIGN KEY ([ScopeId]) REFERENCES [Auth].[Scopes] ([ScopeId]) ON DELETE CASCADE
GO
PRINT N'Adding foreign keys to [Auth].[UserSecurityAnswer]'
GO
ALTER TABLE [Auth].[UserSecurityAnswer] ADD CONSTRAINT [FK_UserSecurityAnswer_SecurityQuestion] FOREIGN KEY ([SecurityQuestionId]) REFERENCES [Auth].[SecurityQuestion] ([SecurityQuestionId])
GO
ALTER TABLE [Auth].[UserSecurityAnswer] ADD CONSTRAINT [FK_UserSecurityAnswer_Users] FOREIGN KEY ([UserId]) REFERENCES [Auth].[Users] ([UserId])
GO
PRINT N'Adding foreign keys to [Enterprise].[CESCommunicationEvent]'
GO
ALTER TABLE [Enterprise].[CESCommunicationEvent] ADD CONSTRAINT [FK_CESCommunicationEvent_CommunicationEvent] FOREIGN KEY ([CommunicationEventId]) REFERENCES [Enterprise].[CommunicationEvent] ([CommunicationEventID])
GO
PRINT N'Adding foreign keys to [Enterprise].[CommunicationEventEmail]'
GO
ALTER TABLE [Enterprise].[CommunicationEventEmail] ADD CONSTRAINT [FK_CommunicationEventEmail_CommunicationEmailTemplate] FOREIGN KEY ([CommunicationEmailTemplateID]) REFERENCES [Enterprise].[CommunicationEmailTemplate] ([CommunicationEmailTemplateID])
GO
ALTER TABLE [Enterprise].[CommunicationEventEmail] ADD CONSTRAINT [FK_CommunicationEventEmail_ContactMechanismValidEmail] FOREIGN KEY ([ContactMechanismValidEmailID]) REFERENCES [Enterprise].[ContactMechanismValidEmail] ([ContactMechanismValidEmailID])
GO
PRINT N'Adding foreign keys to [Enterprise].[CommunicationEventPurposeUsage]'
GO
ALTER TABLE [Enterprise].[CommunicationEventPurposeUsage] ADD CONSTRAINT [FK_CommunicationEventPurposeUsage_CommunicationEventPurposeType] FOREIGN KEY ([CommunicationEventPurposeTypeID]) REFERENCES [Enterprise].[CommunicationEventPurposeType] ([CommunicationEventPurposeTypeID])
GO
ALTER TABLE [Enterprise].[CommunicationEventPurposeUsage] ADD CONSTRAINT [FK_CommunicationEventPurposeUsage_CommunicationEvent] FOREIGN KEY ([CommunicationEventID]) REFERENCES [Enterprise].[CommunicationEvent] ([CommunicationEventID])
GO
PRINT N'Adding foreign keys to [Enterprise].[CommunicationEventRole]'
GO
ALTER TABLE [Enterprise].[CommunicationEventRole] ADD CONSTRAINT [FK_Enterprise.CommunicationEventRole_Enterprise.CommunicationEventRoleType] FOREIGN KEY ([CommunicationEventRoleTypeID]) REFERENCES [Enterprise].[CommunicationEventRoleType] ([CommunicationEventRoleTypeID])
GO
ALTER TABLE [Enterprise].[CommunicationEventRole] ADD CONSTRAINT [FK_Enterprise.CommunicationEventRole_CommunicationEvent] FOREIGN KEY ([CommunicationEventID]) REFERENCES [Enterprise].[CommunicationEvent] ([CommunicationEventID])
GO
ALTER TABLE [Enterprise].[CommunicationEventRole] ADD CONSTRAINT [FK_Enterprise.CommunicationEventRole_Party] FOREIGN KEY ([PartyID]) REFERENCES [Enterprise].[Party] ([PartyId])
GO
PRINT N'Adding foreign keys to [Enterprise].[ContactMechanismValidRole]'
GO
ALTER TABLE [Enterprise].[ContactMechanismValidRole] ADD CONSTRAINT [FK_Enterprise.ContactMechanismValidRole_Enterprise.CommunicationEventRoleType] FOREIGN KEY ([CommunicationEventRoleTypeID]) REFERENCES [Enterprise].[CommunicationEventRoleType] ([CommunicationEventRoleTypeID])
GO
ALTER TABLE [Enterprise].[ContactMechanismValidRole] ADD CONSTRAINT [FK_ContactMechanismValidRole_ContactMechanismType] FOREIGN KEY ([ContactMechanismTypeID]) REFERENCES [Enterprise].[ContactMechanismType] ([ContactMechanismTypeID])
GO
PRINT N'Adding foreign keys to [Enterprise].[CommunicationEvent]'
GO
ALTER TABLE [Enterprise].[CommunicationEvent] ADD CONSTRAINT [FK_CommunicationEvent_StatusType] FOREIGN KEY ([StatusTypeID]) REFERENCES [Enterprise].[StatusType] ([StatusTypeId])
GO
ALTER TABLE [Enterprise].[CommunicationEvent] ADD CONSTRAINT [FK_CommunicationEvent_ContactMechanismType] FOREIGN KEY ([ContactMechanismTypeID]) REFERENCES [Enterprise].[ContactMechanismType] ([ContactMechanismTypeID])
GO
ALTER TABLE [Enterprise].[CommunicationEvent] ADD CONSTRAINT [FK_CommunicationEvent_Party] FOREIGN KEY ([FromPartyId]) REFERENCES [Enterprise].[Party] ([PartyId])
GO
ALTER TABLE [Enterprise].[CommunicationEvent] ADD CONSTRAINT [FK_CommunicationEvent_Party1] FOREIGN KEY ([ToPartyId]) REFERENCES [Enterprise].[Party] ([PartyId])
GO
PRINT N'Adding foreign keys to [Enterprise].[GlobalProductConfiguration]'
GO
ALTER TABLE [Enterprise].[GlobalProductConfiguration] ADD CONSTRAINT [FK_GlobalProductConfiguration_Configuration] FOREIGN KEY ([ConfigurationId]) REFERENCES [Enterprise].[Configuration] ([ConfigurationId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Enterprise].[GlobalProductConfiguration] ADD CONSTRAINT [FK_GlobalProductConfiguration_Product] FOREIGN KEY ([ProductId]) REFERENCES [Enterprise].[Product] ([ProductId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT N'Adding foreign keys to [Enterprise].[OrganizationProduct]'
GO
ALTER TABLE [Enterprise].[OrganizationProduct] ADD CONSTRAINT [FK_OrganizationProduct_Configuration] FOREIGN KEY ([ConfigurationId]) REFERENCES [Enterprise].[Configuration] ([ConfigurationId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Enterprise].[OrganizationProduct] ADD CONSTRAINT [FK_OrganizationProduct_Product] FOREIGN KEY ([ProductId]) REFERENCES [Enterprise].[Product] ([ProductId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT N'Adding foreign keys to [Enterprise].[PersonaConfiguration]'
GO
ALTER TABLE [Enterprise].[PersonaConfiguration] ADD CONSTRAINT [FK_PersonaConfiguration_Configuration] FOREIGN KEY ([ConfigurationId]) REFERENCES [Enterprise].[Configuration] ([ConfigurationId]) ON DELETE CASCADE
GO
ALTER TABLE [Enterprise].[PersonaConfiguration] ADD CONSTRAINT [FK_PersonaConfiguration_Persona] FOREIGN KEY ([PersonaId]) REFERENCES [Person].[Persona] ([PersonaId]) ON DELETE CASCADE
GO
PRINT N'Adding foreign keys to [Enterprise].[ProductConfiguration]'
GO
ALTER TABLE [Enterprise].[ProductConfiguration] ADD CONSTRAINT [FK_ProductConfiguration_Configuration] FOREIGN KEY ([ConfigurationId]) REFERENCES [Enterprise].[Configuration] ([ConfigurationId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Enterprise].[ProductConfiguration] ADD CONSTRAINT [FK_ProductConfiguration_ProductSetting] FOREIGN KEY ([ProductSettingId]) REFERENCES [Enterprise].[ProductSetting] ([ProductSettingId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT N'Adding foreign keys to [Enterprise].[ContactMechanismBoundary]'
GO
ALTER TABLE [Enterprise].[ContactMechanismBoundary] ADD CONSTRAINT [FK_ContactMechanismBoundary_ContactMechanism] FOREIGN KEY ([ContactMechanismId]) REFERENCES [Enterprise].[ContactMechanism] ([ContactMechanismID]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Enterprise].[ContactMechanismBoundary] ADD CONSTRAINT [FK_ContactMechanismBoundary_GeographicBoundary] FOREIGN KEY ([GeographicBoundaryId]) REFERENCES [Enterprise].[GeographicBoundary] ([GeographicBoundaryId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT N'Adding foreign keys to [Enterprise].[ContactMechanismValidEmail]'
GO
ALTER TABLE [Enterprise].[ContactMechanismValidEmail] ADD CONSTRAINT [FK_ContactMechanismValidEmail_ContactMechanismType] FOREIGN KEY ([ContactMechanismTypeID]) REFERENCES [Enterprise].[ContactMechanismType] ([ContactMechanismTypeID])
GO
PRINT N'Adding foreign keys to [Enterprise].[ContactMechanismUsage]'
GO
ALTER TABLE [Enterprise].[ContactMechanismUsage] ADD CONSTRAINT [FK_ContactMechanismUsage_ContactMechanismUsageType] FOREIGN KEY ([ContactMechanismUsageTypeID]) REFERENCES [Enterprise].[ContactMechanismUsageType] ([ContactMechanismUsageTypeID]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Enterprise].[ContactMechanismUsage] ADD CONSTRAINT [FK_ContactMechanismUsage_PartyContactMechanism] FOREIGN KEY ([PartyContactMechanismID]) REFERENCES [Enterprise].[PartyContactMechanism] ([PartyContactMechanismId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT N'Adding foreign keys to [Enterprise].[ContactMechanismUsageType]'
GO
ALTER TABLE [Enterprise].[ContactMechanismUsageType] ADD CONSTRAINT [FK_ContactMechanismUsageType_ParentContactMechanism] FOREIGN KEY ([ParentContactMechanismUsageTypeID]) REFERENCES [Enterprise].[ContactMechanismUsageType] ([ContactMechanismUsageTypeID])
GO
PRINT N'Adding foreign keys to [Ident].[ContactMechanismIdentity]'
GO
ALTER TABLE [Ident].[ContactMechanismIdentity] ADD CONSTRAINT [FK_ContactMechanismIdentity_ContactMechanism] FOREIGN KEY ([ContactMechanismId]) REFERENCES [Enterprise].[ContactMechanism] ([ContactMechanismID]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT N'Adding foreign keys to [Enterprise].[ElectronicAddress]'
GO
ALTER TABLE [Enterprise].[ElectronicAddress] ADD CONSTRAINT [FK_ElectronicAddress_ContactMechanism] FOREIGN KEY ([ContactMechanismID]) REFERENCES [Enterprise].[ContactMechanism] ([ContactMechanismID]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT N'Adding foreign keys to [Enterprise].[PartyContactMechanism]'
GO
ALTER TABLE [Enterprise].[PartyContactMechanism] ADD CONSTRAINT [FK_PartyContactMechanism_ContactMechanism] FOREIGN KEY ([ContactMechanismId]) REFERENCES [Enterprise].[ContactMechanism] ([ContactMechanismID]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Enterprise].[PartyContactMechanism] ADD CONSTRAINT [FK_PartyContactMechanism_Party] FOREIGN KEY ([PartyId]) REFERENCES [Enterprise].[Party] ([PartyId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT N'Adding foreign keys to [Enterprise].[StreetAddress]'
GO
ALTER TABLE [Enterprise].[StreetAddress] ADD CONSTRAINT [FK_StreetAddress_ContactMechanism] FOREIGN KEY ([ContactMechanismID]) REFERENCES [Enterprise].[ContactMechanism] ([ContactMechanismID]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT N'Adding foreign keys to [Enterprise].[TelecommunicationsNumber]'
GO
ALTER TABLE [Enterprise].[TelecommunicationsNumber] ADD CONSTRAINT [FK_TelecommunicationsNumber_ContactMechanism] FOREIGN KEY ([ContactMechanismID]) REFERENCES [Enterprise].[ContactMechanism] ([ContactMechanismID]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT N'Adding foreign keys to [Enterprise].[DataImportMapping]'
GO
ALTER TABLE [Enterprise].[DataImportMapping] ADD CONSTRAINT [FK_DataImportMapping_DataImportApplication] FOREIGN KEY ([DataImportApplicationId]) REFERENCES [Enterprise].[DataImportApplication] ([DataImportApplicationId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Enterprise].[DataImportMapping] ADD CONSTRAINT [FK_DataImportMapping_Party] FOREIGN KEY ([PartyId]) REFERENCES [Enterprise].[Party] ([PartyId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT N'Adding foreign keys to [Enterprise].[GeographicBoundary]'
GO
ALTER TABLE [Enterprise].[GeographicBoundary] ADD CONSTRAINT [FK_GeographicBoundary_GeographicBoundaryType] FOREIGN KEY ([GeographicBoundaryTypeId]) REFERENCES [Enterprise].[GeographicBoundaryType] ([GeographicBoundaryTypeId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT N'Adding foreign keys to [Enterprise].[Organization]'
GO
ALTER TABLE [Enterprise].[Organization] ADD CONSTRAINT [FK_Organization_Party] FOREIGN KEY ([PartyId]) REFERENCES [Enterprise].[Party] ([PartyId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT N'Adding foreign keys to [Enterprise].[PersonaOrganization]'
GO
ALTER TABLE [Enterprise].[PersonaOrganization] ADD CONSTRAINT [FK_PersonaOrganization_Organization] FOREIGN KEY ([OrganizationId]) REFERENCES [Enterprise].[Organization] ([PartyId])
GO
ALTER TABLE [Enterprise].[PersonaOrganization] ADD CONSTRAINT [FK_PersonaOrganization_PersonaConfiguration] FOREIGN KEY ([PersonaConfigurationId]) REFERENCES [Enterprise].[PersonaConfiguration] ([PersonaConfigurationId]) ON DELETE CASCADE
GO
PRINT N'Adding foreign keys to [Enterprise].[PartyRelationship]'
GO
ALTER TABLE [Enterprise].[PartyRelationship] ADD CONSTRAINT [FK_PartyRelationship_PartyFrom] FOREIGN KEY ([PartyIdFrom]) REFERENCES [Enterprise].[Party] ([PartyId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Enterprise].[PartyRelationship] ADD CONSTRAINT [FK_PartyRelationship_PartyTo] FOREIGN KEY ([PartyIdTo]) REFERENCES [Enterprise].[Party] ([PartyId])
GO
ALTER TABLE [Enterprise].[PartyRelationship] ADD CONSTRAINT [FK_PartyRelationship_PartyRelationshipType] FOREIGN KEY ([PartyRelationshipTypeId]) REFERENCES [Enterprise].[RelationshipType] ([RelationshipTypeId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT N'Adding foreign keys to [Enterprise].[PartyRole]'
GO
ALTER TABLE [Enterprise].[PartyRole] ADD CONSTRAINT [FK_PartyRole_Party] FOREIGN KEY ([PartyId]) REFERENCES [Enterprise].[Party] ([PartyId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Enterprise].[PartyRole] ADD CONSTRAINT [FK_PartyRole_RoleType] FOREIGN KEY ([RoleTypeId]) REFERENCES [Enterprise].[RoleType] ([PartyRoleTypeId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT N'Adding foreign keys to [Ident].[PasswordPolicy]'
GO
ALTER TABLE [Ident].[PasswordPolicy] ADD CONSTRAINT [FK_PasswordPolicy_PartyId] FOREIGN KEY ([PartyId]) REFERENCES [Enterprise].[Party] ([PartyId])
GO
PRINT N'Adding foreign keys to [Person].[Person]'
GO
ALTER TABLE [Person].[Person] ADD CONSTRAINT [FK_Person_Party] FOREIGN KEY ([PartyId]) REFERENCES [Enterprise].[Party] ([PartyId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT N'Adding foreign keys to [Enterprise].[ProductRelationship]'
GO
ALTER TABLE [Enterprise].[ProductRelationship] ADD CONSTRAINT [FK_ProductRelationship_Party] FOREIGN KEY ([PartyIdFrom]) REFERENCES [Enterprise].[Party] ([PartyId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Enterprise].[ProductRelationship] ADD CONSTRAINT [FK_ProductRelationship_ProductType] FOREIGN KEY ([ProductIdTo]) REFERENCES [Enterprise].[Product] ([ProductId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT N'Adding foreign keys to [Ident].[UserLogin]'
GO
ALTER TABLE [Ident].[UserLogin] ADD CONSTRAINT [FK_UserLogin_Party] FOREIGN KEY ([PartyId]) REFERENCES [Enterprise].[Party] ([PartyId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT N'Adding foreign keys to [Ident].[ActivityToken]'
GO
ALTER TABLE [Ident].[ActivityToken] ADD CONSTRAINT [FK_ActivityToken_Party] FOREIGN KEY ([RealPageId]) REFERENCES [Enterprise].[Party] ([RealPageId])
GO
ALTER TABLE [Ident].[ActivityToken] ADD CONSTRAINT [FK_ActivityToken_Activity] FOREIGN KEY ([ActivityId]) REFERENCES [Ident].[Activity] ([ActivityId])
GO
PRINT N'Adding foreign keys to [Enterprise].[ProductBatch]'
GO
ALTER TABLE [Enterprise].[ProductBatch] ADD CONSTRAINT [FK_ProductBatch_Person] FOREIGN KEY ([PersonPartyId]) REFERENCES [Person].[Person] ([PartyId])
GO
ALTER TABLE [Enterprise].[ProductBatch] ADD CONSTRAINT [FK_ProductBatch_StatusType] FOREIGN KEY ([StatusTypeId]) REFERENCES [Enterprise].[StatusType] ([StatusTypeId])
GO
PRINT N'Adding foreign keys to [Enterprise].[ProductSetting]'
GO
ALTER TABLE [Enterprise].[ProductSetting] ADD CONSTRAINT [FK_ProductSetting_ProductSettingType] FOREIGN KEY ([ProductSettingTypeId]) REFERENCES [Enterprise].[ProductSettingType] ([ProductSettingTypeId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Enterprise].[ProductSetting] ADD CONSTRAINT [FK_ProductSetting_ProductType] FOREIGN KEY ([ProductId]) REFERENCES [Enterprise].[Product] ([ProductId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT N'Adding foreign keys to [Enterprise].[Product]'
GO
ALTER TABLE [Enterprise].[Product] ADD CONSTRAINT [FK_Product_ProductType] FOREIGN KEY ([ProductTypeId]) REFERENCES [Enterprise].[ProductType] ([ProductTypeId])
GO
PRINT N'Adding foreign keys to [Ident].[SamlAttributeStatement]'
GO
ALTER TABLE [Ident].[SamlAttributeStatement] ADD CONSTRAINT [FK_SamlAttributeStatement_Product] FOREIGN KEY ([ProductId]) REFERENCES [Enterprise].[Product] ([ProductId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Ident].[SamlAttributeStatement] ADD CONSTRAINT [FK_SamlAttributeStatement_SamlAttribute] FOREIGN KEY ([SamlAttributeId]) REFERENCES [Ident].[SamlAttribute] ([SamlAttributeId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT N'Adding foreign keys to [Ident].[SamlProductSettings]'
GO
ALTER TABLE [Ident].[SamlProductSettings] ADD CONSTRAINT [FK_SamlProductSettings_Product] FOREIGN KEY ([ProductId]) REFERENCES [Enterprise].[Product] ([ProductId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT N'Adding foreign keys to [Ident].[SamlUserAttribute]'
GO
ALTER TABLE [Ident].[SamlUserAttribute] ADD CONSTRAINT [FK_SamlUserAttribute_Product] FOREIGN KEY ([ProductId]) REFERENCES [Enterprise].[Product] ([ProductId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Ident].[SamlUserAttribute] ADD CONSTRAINT [FK_SamlUserAttribute_SamlAttribute] FOREIGN KEY ([SamlAttributeId]) REFERENCES [Ident].[SamlAttribute] ([SamlAttributeId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Ident].[SamlUserAttribute] ADD CONSTRAINT [FK_SamlUserAttribute_Persona] FOREIGN KEY ([PersonaId]) REFERENCES [Person].[Persona] ([PersonaId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT N'Adding foreign keys to [Enterprise].[RelationshipType]'
GO
ALTER TABLE [Enterprise].[RelationshipType] ADD CONSTRAINT [FK_PartyRelationshipType_PartyRoleTypeFrom] FOREIGN KEY ([RoleTypeIdValidFrom]) REFERENCES [Enterprise].[RoleType] ([PartyRoleTypeId])
GO
ALTER TABLE [Enterprise].[RelationshipType] ADD CONSTRAINT [FK_PartyRelationshipType_PartyRoleTypeTo] FOREIGN KEY ([RoleTypeIdValidTo]) REFERENCES [Enterprise].[RoleType] ([PartyRoleTypeId])
GO
PRINT N'Adding foreign keys to [Enterprise].[RoleType]'
GO
ALTER TABLE [Enterprise].[RoleType] ADD CONSTRAINT [FK_RoleType_ParentRoleType] FOREIGN KEY ([ParentPartyRoleTypeId]) REFERENCES [Enterprise].[RoleType] ([PartyRoleTypeId])
GO
PRINT N'Adding foreign keys to [Enterprise].[StatusTypeCategoryClassification]'
GO
ALTER TABLE [Enterprise].[StatusTypeCategoryClassification] ADD CONSTRAINT [FK_StatusTypeCategoryClassification_StatusType] FOREIGN KEY ([StatusTypeId]) REFERENCES [Enterprise].[StatusType] ([StatusTypeId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Enterprise].[StatusTypeCategoryClassification] ADD CONSTRAINT [FK_StatusTypeCategoryClassification_StatusTypeCategory] FOREIGN KEY ([StatusTypeCategoryId]) REFERENCES [Enterprise].[StatusTypeCategory] ([StatusTypeCategoryId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT N'Adding foreign keys to [Enterprise].[StatusTypeCategoryType]'
GO
ALTER TABLE [Enterprise].[StatusTypeCategoryType] ADD CONSTRAINT [FK_ParentStatusTypeCategoryType_ChildStatusTypeCategoryType] FOREIGN KEY ([ParentStatusTypeCategoryTypeId]) REFERENCES [Enterprise].[StatusTypeCategoryType] ([StatusTypeCategoryTypeId])
GO
PRINT N'Adding foreign keys to [Enterprise].[StatusTypeCategory]'
GO
ALTER TABLE [Enterprise].[StatusTypeCategory] ADD CONSTRAINT [FK_StatusTypeCategory_StatusTypeCategoryType] FOREIGN KEY ([StatusTypeCategoryTypeId]) REFERENCES [Enterprise].[StatusTypeCategoryType] ([StatusTypeCategoryTypeId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Enterprise].[StatusTypeCategory] ADD CONSTRAINT [FK_ParentStatusTypeCategory_ChildStatusTypeCategory] FOREIGN KEY ([ParentStatusTypeCategoryId]) REFERENCES [Enterprise].[StatusTypeCategory] ([StatusTypeCategoryId])
GO
PRINT N'Adding foreign keys to [Ident].[UserCurrentStatus]'
GO
ALTER TABLE [Ident].[UserCurrentStatus] ADD CONSTRAINT [FK_UserStatus_StatusType] FOREIGN KEY ([StatusTypeId]) REFERENCES [Enterprise].[StatusType] ([StatusTypeId])
GO
ALTER TABLE [Ident].[UserCurrentStatus] ADD CONSTRAINT [FK_UserStatus_UserLogin] FOREIGN KEY ([UserId]) REFERENCES [Ident].[UserLogin] ([UserId]) ON DELETE CASCADE
GO
PRINT N'Adding foreign keys to [Ident].[ActivityAttempts]'
GO
ALTER TABLE [Ident].[ActivityAttempts] ADD CONSTRAINT [FK_ActivityAttempts_Activity] FOREIGN KEY ([ActivityId]) REFERENCES [Ident].[Activity] ([ActivityId])
GO
PRINT N'Adding foreign keys to [Ident].[PasswordHistory]'
GO
ALTER TABLE [Ident].[PasswordHistory] ADD CONSTRAINT [FK_PasswordHistory_Activity] FOREIGN KEY ([ActivityId]) REFERENCES [Ident].[Activity] ([ActivityId])
GO
ALTER TABLE [Ident].[PasswordHistory] ADD CONSTRAINT [FK_PasswordHistory_UserLogin] FOREIGN KEY ([UserId]) REFERENCES [Ident].[UserLogin] ([UserId])
GO
PRINT N'Adding foreign keys to [Ident].[IdentityProviderSetting]'
GO
ALTER TABLE [Ident].[IdentityProviderSetting] ADD CONSTRAINT [FK_IdentityProviderSetting_IdentityProviderSettingType] FOREIGN KEY ([IdentityProviderSettingTypeId]) REFERENCES [Ident].[IdentityProviderSettingType] ([IdentityProviderSettingTypeId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT N'Adding foreign keys to [Ident].[IdentityProviderSettingType]'
GO
ALTER TABLE [Ident].[IdentityProviderSettingType] ADD CONSTRAINT [FK_IdentityProviderSettingType_IdentityProviderTypeId] FOREIGN KEY ([IdentityProviderTypeId]) REFERENCES [Ident].[IdentityProviderType] ([IdentityProviderTypeId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT N'Adding foreign keys to [Ident].[SamlAttribute]'
GO
ALTER TABLE [Ident].[SamlAttribute] ADD CONSTRAINT [FK_SamlAttribute_SamlAttributeType] FOREIGN KEY ([SamlAttributeTypeId]) REFERENCES [Ident].[SamlAttributeType] ([SamlAttributeTypeId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT N'Adding foreign keys to [Ident].[UserSecurityAnswer]'
GO
ALTER TABLE [Ident].[UserSecurityAnswer] ADD CONSTRAINT [FK_UserSecurityAnswer_SecurityQuestion] FOREIGN KEY ([SecurityQuestionId]) REFERENCES [Ident].[SecurityQuestion] ([SecurityQuestionId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Ident].[UserSecurityAnswer] ADD CONSTRAINT [FK_UserSecurityAnswer_UserLogin] FOREIGN KEY ([UserId]) REFERENCES [Ident].[UserLogin] ([UserId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT N'Adding foreign keys to [Person].[ActivePersona]'
GO
ALTER TABLE [Person].[ActivePersona] ADD CONSTRAINT [FK_ActivePersona_Person] FOREIGN KEY ([PartyId]) REFERENCES [Person].[Person] ([PartyId])
GO
ALTER TABLE [Person].[ActivePersona] ADD CONSTRAINT [FK_ActivePersona_Persona] FOREIGN KEY ([PersonaId]) REFERENCES [Person].[Persona] ([PersonaId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT N'Adding foreign keys to [Person].[Persona]'
GO
ALTER TABLE [Person].[Persona] ADD CONSTRAINT [FK_Persona_Party] FOREIGN KEY ([PersonPartyId]) REFERENCES [Person].[Person] ([PartyId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Person].[Persona] ADD CONSTRAINT [FK_Persona_PersonaEnvironmentType] FOREIGN KEY ([PersonaEnvironmentTypeId]) REFERENCES [Person].[PersonaEnvironmentType] ([PersonaEnvironmentTypeID]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Person].[Persona] ADD CONSTRAINT [FK_Persona_PersonaType] FOREIGN KEY ([PersonaTypeId]) REFERENCES [Person].[PersonaType] ([PersonaTypeId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
PRINT N'Creating DDL triggers'
GO

CREATE TRIGGER [ddlDatabaseTriggerLog] ON DATABASE 
FOR DDL_DATABASE_LEVEL_EVENTS AS 
BEGIN
    SET NOCOUNT ON;

    DECLARE @data XML;
    DECLARE @schema sysname;
    DECLARE @object sysname;
    DECLARE @eventType sysname;

    SET @data = EVENTDATA();
    SET @eventType = @data.value('(/EVENT_INSTANCE/EventType)[1]', 'sysname');
    SET @schema = @data.value('(/EVENT_INSTANCE/SchemaName)[1]', 'sysname');
    SET @object = @data.value('(/EVENT_INSTANCE/ObjectName)[1]', 'sysname') 

    IF @object IS NOT NULL
        PRINT '  ' + @eventType + ' - ' + @schema + '.' + @object;
    ELSE
        PRINT '  ' + @eventType + ' - ' + @schema;

    IF @eventType IS NULL
        PRINT CONVERT(nvarchar(max), @data);

    INSERT [dbo].[DatabaseLog] 
        (
        [PostTime], 
        [DatabaseUser], 
        [Event], 
        [Schema], 
        [Object], 
        [TSQL], 
        [XmlEvent]
        ) 
    VALUES 
        (
        GETDATE(), 
        CONVERT(sysname, CURRENT_USER), 
        @eventType, 
        CONVERT(sysname, @schema), 
        CONVERT(sysname, @object), 
        @data.value('(/EVENT_INSTANCE/TSQLCommand)[1]', 'nvarchar(max)'), 
        @data
        );
END;
GO
DISABLE TRIGGER ddlDatabaseTriggerLog ON DATABASE
GO
PRINT N'Creating extended properties'
GO
EXEC sp_addextendedproperty N'MS_Description', N'This table holds the RealPage Password Policy for each Portofio.', 'SCHEMA', N'Auth', 'TABLE', N'PasswordPolicy', NULL, NULL
GO
EXEC sp_addextendedproperty N'MS_Description', N'Permit Users to Change Their Own Password', 'SCHEMA', N'Auth', 'TABLE', N'PasswordPolicy', 'COLUMN', N'AllowUsersToChangeOwnPassword'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Enable user passwords to be valid for only the specified number of days', 'SCHEMA', N'Auth', 'TABLE', N'PasswordPolicy', 'COLUMN', N'EnablePasswordExpiration'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Specify the maximum number of characters allowed in a user password', 'SCHEMA', N'Auth', 'TABLE', N'PasswordPolicy', 'COLUMN', N'MaximumLength'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Specify the minimum number of characters allowed in a user password', 'SCHEMA', N'Auth', 'TABLE', N'PasswordPolicy', 'COLUMN', N'MinimumLength'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Minimum lowercase characters required in a user password', 'SCHEMA', N'Auth', 'TABLE', N'PasswordPolicy', 'COLUMN', N'MinimumLowercase'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Minimum numbers required in a user password', 'SCHEMA', N'Auth', 'TABLE', N'PasswordPolicy', 'COLUMN', N'MinimumNumeric'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Minimum special characters required in a user password', 'SCHEMA', N'Auth', 'TABLE', N'PasswordPolicy', 'COLUMN', N'MinimumSpecialCharacter'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Minimum uppercase characters required in a user password', 'SCHEMA', N'Auth', 'TABLE', N'PasswordPolicy', 'COLUMN', N'MinimumUppercase'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Number of previous passwords a user is not allowed to reuse', 'SCHEMA', N'Auth', 'TABLE', N'PasswordPolicy', 'COLUMN', N'NumberOfPasswordsToRemember'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Number of days a password is valid for', 'SCHEMA', N'Auth', 'TABLE', N'PasswordPolicy', 'COLUMN', N'PasswordExpirationPeriodInDays'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Unique Password Policy ID', 'SCHEMA', N'Auth', 'TABLE', N'PasswordPolicy', 'COLUMN', N'PasswordPolicyId'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Prevent users from reusing a specified number of previous passwords', 'SCHEMA', N'Auth', 'TABLE', N'PasswordPolicy', 'COLUMN', N'PreventPasswordReuse'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Created by User ID', 'SCHEMA', N'Auth', 'TABLE', N'PasswordPolicy', 'COLUMN', N'UserId'
GO
EXEC sp_addextendedproperty N'MS_Description', N'This table joins geographic boundaries to a contact mechanism.', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismBoundary', NULL, NULL
GO
EXEC sp_addextendedproperty N'MS_Description', N'Identity backing field', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismBoundary', 'COLUMN', N'ContactMechanismBoundaryId'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The contact mechanism id that this boundary definition applies to. There may be multiple rows for the same contact mechanism, such as a city,state, zip combination.', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismBoundary', 'COLUMN', N'ContactMechanismId'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The date this boundary became active.', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismBoundary', 'COLUMN', N'FromDate'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The geographic boundary that applies to this boundary. ', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismBoundary', 'COLUMN', N'GeographicBoundaryId'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The date this boundary was deactivated.', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismBoundary', 'COLUMN', N'ThruDate'
GO
EXEC sp_addextendedproperty N'MS_Description', N'A contact mechanism is an agency or means by which two or more persons, groups (parties), or other item (facility) are placed in communication with each other.', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismType', NULL, NULL
GO
EXEC sp_addextendedproperty N'MS_Description', N'Identity Column for Contact Mechanisms', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismType', 'COLUMN', N'ContactMechanismTypeID'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Defines the seed data for contact usage. Such as Personal, Work, or Account Recovery.', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismUsageType', NULL, NULL
GO
EXEC sp_addextendedproperty N'MS_Description', N'Identity Backing Field', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismUsageType', 'COLUMN', N'ContactMechanismUsageTypeID'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The usage type name', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismUsageType', 'COLUMN', N'Name'
GO
EXEC sp_addextendedproperty N'MS_Description', N'A contact mechanism usage.', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismUsage', NULL, NULL
GO
EXEC sp_addextendedproperty N'MS_Description', N'Identity Backing Field', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismUsage', 'COLUMN', N'ContactMechanismUsageID'
GO
EXEC sp_addextendedproperty N'MS_Description', N'References ContactMechanismUsageType.ContactMechanismUsageTypeID', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismUsage', 'COLUMN', N'ContactMechanismUsageTypeID'
GO
EXEC sp_addextendedproperty N'MS_Description', N'References PartyContactMechanism.PartyContactMechanismID', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismUsage', 'COLUMN', N'PartyContactMechanismID'
GO
EXEC sp_addextendedproperty N'MS_Description', N'A contact mechanism is an agency or means by which two or more persons, groups (parties), or other item (facility) are placed in communication with each other.', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanism', NULL, NULL
GO
EXEC sp_addextendedproperty N'MS_Description', N'Identity Column for Contact Mechanisms', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanism', 'COLUMN', N'ContactMechanismID'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Contact Methods can be electronic addresses such as email or websites. This table is used for that.', 'SCHEMA', N'Enterprise', 'TABLE', N'ElectronicAddress', NULL, NULL
GO
EXEC sp_addextendedproperty N'MS_Description', N'Identity backing field', 'SCHEMA', N'Enterprise', 'TABLE', N'ElectronicAddress', 'COLUMN', N'ContactMechanismID'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The string of the electronic address.', 'SCHEMA', N'Enterprise', 'TABLE', N'ElectronicAddress', 'COLUMN', N'ElectronicAddressString'
GO
EXEC sp_addextendedproperty N'MS_Description', N'This table defines the different kinds of Geographic Boundaries. Please note: Geographic "locations" are NOT the same as boundaries. Do not store Geographic "location" types here.', 'SCHEMA', N'Enterprise', 'TABLE', N'GeographicBoundaryType', NULL, NULL
GO
EXEC sp_addextendedproperty N'MS_Description', N'Identity backing field', 'SCHEMA', N'Enterprise', 'TABLE', N'GeographicBoundaryType', 'COLUMN', N'GeographicBoundaryTypeId'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The name of the type that will be used to group things under. For example, State, Region, Country as it relates to a geographic boundary.', 'SCHEMA', N'Enterprise', 'TABLE', N'GeographicBoundaryType', 'COLUMN', N'Name'
GO
EXEC sp_addextendedproperty N'MS_Description', N'If the geographic boundary has a known standard abbreviation, then it would go here, such as TX for Texas.', 'SCHEMA', N'Enterprise', 'TABLE', N'GeographicBoundary', 'COLUMN', N'Abbreviation'
GO
EXEC sp_addextendedproperty N'MS_Description', N'If the geographic boundary has a specific code, this is where it goes.', 'SCHEMA', N'Enterprise', 'TABLE', N'GeographicBoundary', 'COLUMN', N'GeographicBoundaryCode'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Identity Backing Field', 'SCHEMA', N'Enterprise', 'TABLE', N'GeographicBoundary', 'COLUMN', N'GeographicBoundaryId'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The Geographic Boundary Type, which can be any type defined by the GeographicBoundaryType table.', 'SCHEMA', N'Enterprise', 'TABLE', N'GeographicBoundary', 'COLUMN', N'GeographicBoundaryTypeId'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The named value of this Geographic Boundary, such as 75078 in the case of a ZipCode, or Texas if it''s a state.', 'SCHEMA', N'Enterprise', 'TABLE', N'GeographicBoundary', 'COLUMN', N'Name'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Street Address is part of a contact mechanism based on geographic location. Therefore it has the street address fields, and is related to the contact mechanism geographic boundary to retrieve the other fields related to the location.', 'SCHEMA', N'Enterprise', 'TABLE', N'StreetAddress', NULL, NULL
GO
EXEC sp_addextendedproperty N'MS_Description', N'The Contact Mechanism that has a postal address.', 'SCHEMA', N'Enterprise', 'TABLE', N'StreetAddress', 'COLUMN', N'ContactMechanismID'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Text that goes in the first Address line', 'SCHEMA', N'Enterprise', 'TABLE', N'StreetAddress', 'COLUMN', N'StreetAddress1'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Text that goes in the second Address line', 'SCHEMA', N'Enterprise', 'TABLE', N'StreetAddress', 'COLUMN', N'StreetAddress2'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Text that goes in the third Address line', 'SCHEMA', N'Enterprise', 'TABLE', N'StreetAddress', 'COLUMN', N'StreetAddress3'
GO
EXEC sp_addextendedproperty N'MS_Description', N'This table contains telephone numbers.', 'SCHEMA', N'Enterprise', 'TABLE', N'TelecommunicationsNumber', NULL, NULL
GO
EXEC sp_addextendedproperty N'MS_Description', N'Area code of the Telecommunications number', 'SCHEMA', N'Enterprise', 'TABLE', N'TelecommunicationsNumber', 'COLUMN', N'AreaCode'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Contact Mechanism that has a Telecommunications number tied to it.', 'SCHEMA', N'Enterprise', 'TABLE', N'TelecommunicationsNumber', 'COLUMN', N'ContactMechanismID'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Country Code of the Telecommunications number', 'SCHEMA', N'Enterprise', 'TABLE', N'TelecommunicationsNumber', 'COLUMN', N'CountryCode'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Phone number of the Telecommunicatoins number.', 'SCHEMA', N'Enterprise', 'TABLE', N'TelecommunicationsNumber', 'COLUMN', N'PhoneNumber'
GO
EXEC sp_addextendedproperty N'MS_Description', N'This table holds the RealPage Password Policy for each Portofio.', 'SCHEMA', N'Ident', 'TABLE', N'PasswordPolicy', NULL, NULL
GO
EXEC sp_addextendedproperty N'MS_Description', N'Permit Users to Change Their Own Password', 'SCHEMA', N'Ident', 'TABLE', N'PasswordPolicy', 'COLUMN', N'AllowUsersToChangeOwnPassword'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Enable user passwords to be valid for only the specified number of days', 'SCHEMA', N'Ident', 'TABLE', N'PasswordPolicy', 'COLUMN', N'EnablePasswordExpiration'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Specify the maximum number of characters allowed in a user password', 'SCHEMA', N'Ident', 'TABLE', N'PasswordPolicy', 'COLUMN', N'MaximumLength'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Specify the minimum number of characters allowed in a user password', 'SCHEMA', N'Ident', 'TABLE', N'PasswordPolicy', 'COLUMN', N'MinimumLength'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Minimum lowercase characters required in a user password', 'SCHEMA', N'Ident', 'TABLE', N'PasswordPolicy', 'COLUMN', N'MinimumLowercase'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Minimum numbers required in a user password', 'SCHEMA', N'Ident', 'TABLE', N'PasswordPolicy', 'COLUMN', N'MinimumNumeric'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Minimum special characters required in a user password', 'SCHEMA', N'Ident', 'TABLE', N'PasswordPolicy', 'COLUMN', N'MinimumSpecialCharacter'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Minimum uppercase characters required in a user password', 'SCHEMA', N'Ident', 'TABLE', N'PasswordPolicy', 'COLUMN', N'MinimumUppercase'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Number of previous passwords a user is not allowed to reuse', 'SCHEMA', N'Ident', 'TABLE', N'PasswordPolicy', 'COLUMN', N'NumberOfPasswordsToRemember'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Number of days a password is valid for', 'SCHEMA', N'Ident', 'TABLE', N'PasswordPolicy', 'COLUMN', N'PasswordExpirationPeriodInDays'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Unique Password Policy ID', 'SCHEMA', N'Ident', 'TABLE', N'PasswordPolicy', 'COLUMN', N'PasswordPolicyId'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Prevent users from reusing a specified number of previous passwords', 'SCHEMA', N'Ident', 'TABLE', N'PasswordPolicy', 'COLUMN', N'PreventPasswordReuse'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Created by User ID', 'SCHEMA', N'Ident', 'TABLE', N'PasswordPolicy', 'COLUMN', N'UserId'
GO
EXEC sp_addextendedproperty N'MS_Description', N'This table holds the RealPage user names and passwords of the users.', 'SCHEMA', N'Ident', 'TABLE', N'UserLogin', NULL, NULL
GO
EXEC sp_addextendedproperty N'MS_Description', N'Identity backing field', 'SCHEMA', N'Ident', 'TABLE', N'UserLogin', 'COLUMN', N'LoginName'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The party this user login belongs to.', 'SCHEMA', N'Ident', 'TABLE', N'UserLogin', 'COLUMN', N'PartyId'
GO
EXEC sp_addextendedproperty N'MS_Description', N'PBKDF2 hashed password', 'SCHEMA', N'Ident', 'TABLE', N'UserLogin', 'COLUMN', N'PasswordHash'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The salt used to hash the password.', 'SCHEMA', N'Ident', 'TABLE', N'UserLogin', 'COLUMN', N'PasswordSalt'
GO
EXEC sp_addextendedproperty N'MS_Description', N'User Name for the Party', 'SCHEMA', N'Ident', 'TABLE', N'UserLogin', 'COLUMN', N'UserId'
GO
EXEC sp_addextendedproperty N'MS_Description', N'This table contains information on People.', 'SCHEMA', N'Person', 'TABLE', N'Person', NULL, NULL
GO
EXEC sp_addextendedproperty N'MS_Description', N'The First Name of the Person', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'FirstName'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The Last Name of the Person', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'LastName'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The Middle Name of the Person', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'MiddleName'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The Party Id of the person.', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'PartyId'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Preferred Contact Method for a person', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'PreferredContactMethodId'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The Suffix of the person, such as MD', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'Suffix'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The title of the person, such as Mr., Mrs., Ms.', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'Title'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Log to show schema changes that occur against the database and who performed them.', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', NULL, NULL
GO
EXEC sp_addextendedproperty N'MS_Description', N'Identity Column for the table', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'COLUMN', N'DatabaseLogID'
GO
EXEC sp_addextendedproperty N'MS_Description', N'User that performed the action', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'COLUMN', N'DatabaseUser'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The type of event.', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'COLUMN', N'Event'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Date the event occurred.', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'COLUMN', N'PostTime'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The T-SQL that was run.', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'COLUMN', N'TSQL'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The XML details of the event.', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'COLUMN', N'XmlEvent'
GO
EXEC sp_addextendedproperty N'Build', '0', NULL, NULL, NULL, NULL, NULL, NULL
GO
EXEC sp_addextendedproperty N'Major Version', '0', NULL, NULL, NULL, NULL, NULL, NULL
GO
EXEC sp_addextendedproperty N'Minor Version', '0', NULL, NULL, NULL, NULL, NULL, NULL
GO
EXEC sp_addextendedproperty N'Revision', '0', NULL, NULL, NULL, NULL, NULL, NULL
GO
PRINT N'Altering permissions on  [Ident].[GetUserCurrentStatuses]'
GO
GRANT EXECUTE ON  [Ident].[GetUserCurrentStatuses] TO [IdentityServer]
GO
PRINT N'Altering permissions on  [Ident].[ListIdentityProviderByIdentityProviderTypeName]'
GO
GRANT EXECUTE ON  [Ident].[ListIdentityProviderByIdentityProviderTypeName] TO [IdentityServer]
GO
REVOKE CONNECT TO [IdentityServer]
PRINT N'Re-enabling DDL triggers'
GO
ENABLE TRIGGER ddlDatabaseTriggerLog ON DATABASE
GO
