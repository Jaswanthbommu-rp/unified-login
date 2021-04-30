CREATE PROCEDURE [Enterprise].[SetupOrganization_Ver01]
( 
	@OrganizationName nvarchar(150),
	@BlueBookId int= 0,
	@BlackBookId int= 0,
	@ThirdPartyIDP int= NULL,
	@OrganizationTypeId INT,
	@OrganizationDomainId INT = 1,
	@OrganizationStatus TinyInt = 1
)
AS
BEGIN
	/*
	1/23/2018 - Created wrapper stored procedure to support
		1. Create an organization
		2. Setup password policy
		3. Assign Thirdparty IDP in case there is any
		4. Map bluebook id with Gb

	Revision History:
	1/25/2018: 
		1. Update the IDP setting in organization in case is thirdparty
		2. Changed Output list

	1/30/2018
		1. Addded support to create user types for the organization.
	*/

	SET NOCOUNT ON;
	--Create Organization
	DECLARE @OrganizationID int;
	DECLARE @RealPageId uniqueidentifier;
	DECLARE @ContactMechanismId int;
	DECLARE @BlueBookAppId int;
	DECLARE @BlackBookAppId int;
	DECLARE @ContactMechanismUsageTypeID int;
	DECLARE @PartyContactMechanismId int;
	DECLARE @Now datetime = GETUTCDATE();

	CREATE TABLE #Result
	( 
		Id bigint,
		RealpageID uniqueidentifier,
		ErrorMessage nvarchar(4000)
	);

	CREATE TABLE #BlueBookMap
	( 
			DataImportId int,
			ErrorMessage nvarchar(100)
	);

	SELECT	@BlackBookAppId = DataImportApplicationId
	FROM		Enterprise.DataImportApplication
	WHERE	Name = 'BlackBook';

	SELECT	@BlueBookAppId = DataImportApplicationId
	FROM		Enterprise.DataImportApplication
	WHERE	Name = 'BlueBook';

	INSERT INTO #Result (
		Id,
		RealPageID,
		ErrorMessage
	)
	EXEC Enterprise.CreateOrganization
		@OrganizationId = NULL,
		@OrganizationName = @OrganizationName,
		@OrganizationTypeId = @OrganizationTypeId,
		@OrganizationDomainId = @OrganizationDomainId,
		@OrganizationStatus = @OrganizationStatus;

	SELECT	@OrganizationId = Id,
					@RealPageId = RealPageId
	FROM		#Result
	
	--Map BlackBook --> BlackBookId to party
	IF
		(
			SELECT	1
			FROM		Enterprise.DataImportMapping AS D
							INNER JOIN Enterprise.DataImportApplication AS A ON A.DataImportApplicationId = D.DataImportApplicationId
			WHERE	PartyId = @OrganizationId
			AND			A.Name = 'BlackBook'
		) IS NULL
	BEGIN
		INSERT INTO #BlueBookMap (
			DataImportId,
			ErrorMessage
		)
		EXEC Enterprise.[MapBooksIdtoPartyId_Ver01]
			@ApplicationId = @BlackBookAppId,
			@SourceId = @BlackBookId,
			@PartyId = @OrganizationId;
	END;

	--Map BlueBook --> bluebookid to party
	IF
	(
		SELECT	1
		FROM		Enterprise.DataImportMapping AS D
						INNER JOIN Enterprise.DataImportApplication AS A ON A.DataImportApplicationId = D.DataImportApplicationId
		WHERE	PartyId = @OrganizationId
		AND			A.Name = 'BlueBook'
	) IS NULL
	BEGIN
		INSERT INTO #BlueBookMap (
			DataImportId,
			ErrorMessage
		)
		EXEC Enterprise.[MapBooksIdtoPartyId_Ver01]
			@ApplicationId = @BlueBookAppId,
			@SourceId = @BlueBookId,
			@PartyId = @OrganizationId;
	END;

	--Setup password policy
	IF NOT EXISTS
	(
		SELECT	1
		FROM	Settings.OrganizationSettings
		WHERE	PartyId = @OrganizationId
	)
	BEGIN
		DECLARE @UserId bigint

		SELECT	@UserId = UserId
		FROM	Ident.UserLogin
		WHERE	LoginName LIKE 'realpagead@%'
		
		INSERT INTO Settings.OrganizationSettings (PartyId,SettingCategoryTypeId,MappingName,MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
		SELECT @OrganizationId,SettingCategoryTypeId,MappingName,MappingValue,Editable,[Hidden],@UserId,GETDATE()
		FROM [Settings].[OrganizationSettings]
		WHERE	PartyId = 3				
	END;

	--Setup thirdparty IDP
	IF @ThirdpartyIDP IS NOT NULL OR @ThirdpartyIDP <> ''
	BEGIN
		SELECT	@ContactMechanismId = ContactMechanismId
		FROM		Ident.IdentityProviderType
		WHERE	IdentityProviderTypeId = @ThirdPartyIDP;

		INSERT INTO [Enterprise].[PartyContactMechanism] (
			[PartyId],
			[ContactMechanismId],
			[FromDate]
		)
		VALUES (
			@OrganizationId,
			@ContactMechanismId,
			@Now
		);

		UPDATE	Enterprise.Organization
		SET			IdentityProviderTypeId = @ThirdPartyIDP
		WHERE	PartyId = @OrganizationId;
	END;

	--Setup User Types for the organization
	INSERT INTO Enterprise.PartyRole (
		Partyid,
		RoleTypeId
	)
	VALUES
	(
		@OrganizationId,
		401
	),
	(
		@OrganizationId,
		402 
	), 
	(
		@OrganizationId,
		404
	), 
	(
		@OrganizationId,
		405
	);

	--Setup activity configuration
	INSERT INTO Ident.ActivityConfiguration (
		PartyId,
		ActivityTypeId,
		MaxActivityAttemptCount,
		ActivityTokenExpirationMinutes
	)
	VALUES
	(
		@OrganizationId,
		1,
		4,
		30 
	),
	(
		@OrganizationId,
		2,
		3,
		30 
	),
	(
		@OrganizationId,
		3,
		3,
		30 
	),
	(
		@OrganizationId,
		4,
		3,
		30 
	),
	(
		@OrganizationId,
		5,
		3,
		30 
	),
	(
		@OrganizationId,
		6,
		2,
		5 )
	,
	(
		@OrganizationId,
		7,
		0,
		30 
	),
	(
		@OrganizationId,
		8,
		5,
		10080
	),
	(
		@OrganizationId,
		9,
		5,
		10080
	),
	(
		@OrganizationId,
		10,
		0,
		0
	);

	--Start: Insert of no-reply email for new company
	SELECT	@ContactMechanismUsageTypeID = cecmut.ContactMechanismUsageTypeID
	FROM		Enterprise.ContactMechanismUsageType pecmut
					INNER JOIN Enterprise.ContactMechanismUsageType cecmut ON (pecmut.ContactMechanismUsageTypeID = cecmut.ParentContactMechanismUsageTypeID)
	WHERE	pecmut.Name = 'Email Notification'
	AND			cecmut.Name = 'Email'

	TRUNCATE TABLE #Result

	INSERT INTO #Result (
		Id,
		ErrorMessage
	)
	EXEC Person.CreateContactMechanism
		@ContactMechanismId = @ContactMechanismId OUT

	TRUNCATE TABLE #Result

	INSERT INTO #Result (
		Id,
		RealPageId,
		ErrorMessage
	)
	EXEC Person.LinkContactMechanismToParty
		@RealPageId = @RealPageId,
		@ContactMechanismId = @ContactMechanismId,
		@FromDate = @Now,
		@ThruDate = '9999-12-31 23:59:59.997'

	SELECT	@PartyContactMechanismId = Id
	FROM		#Result

	TRUNCATE TABLE #Result

	INSERT INTO #Result (
		Id,
		ErrorMessage
	)
	EXEC Person.LinkUsageTypeToPartyContactMechanism
		@PartyContactMechanismId = @PartyContactMechanismId,
		@ContactMechanismUsageTypeId = @ContactMechanismUsageTypeID

	TRUNCATE TABLE #Result

	INSERT INTO #Result (
		Id,
		ErrorMessage
	)
	EXEC Person.CreateElectronicAddress
		@ContactMechanismId = @ContactMechanismId,
		@ElectronicAddressString = 'no-reply@realpage.com',
		@ElectronicAddressType = 'Email'  
	--End: Insert of no-reply email for new company

	SELECT	@OrganizationId AS Id,
					@RealPageId AS RealPageId
END;

