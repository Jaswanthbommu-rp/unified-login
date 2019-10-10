CREATE PROCEDURE [Enterprise].[SetupOrganization]
( 
 @OrganizationName nvarchar(150), @BlueBookId int, @ThirdPartyIDP int = NULL
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
    1. Update the IDP setting in oranization in case is thirdparty
    2. Changed Output list

1/30/2018
    1. Addded support to create user types for the organization.
*/
    SET NOCOUNT ON
	--Create Organization
	DECLARE @OrganizationID int;
	DECLARE @ContactMechanismId INT
	DECLARE @Now DATETIME
	CREATE TABLE #Org  (PartyId BIGINT, RealpageID UNIQUEIDENTIFIER, ErrorMessage NVARCHAR(200))
	CREATE TABLE #BlueBookMap  (DataImportId INT, ErrorMessage Nvarchar(100))
	SELECT @Now = GETUTCDATE()
	
	SELECT @OrganizationId = PartyID
	FROM Enterprise.Organization
	WHERE [Name] = @OrganizationName;
	
	IF @OrganizationId IS NULL
	BEGIN 
		INSERT INTO #Org
		EXEC Enterprise.CreateOrganization NULL, @OrganizationName;
	END 
	ELSE
	BEGIN
		INSERT INTO #Org (PartyId, RealPageId)
		SELECT O.PartyId as OrganizationId, P.RealPageId as RealPageId
			FROM Enterprise.Organization O
				INNER JOIN Enterprise.Party P
					On P.PartyId = O.PartyId WHERE O.Name = @OrganizationName
	END;
	SELECT @OrganizationId = PartyID
	FROM Enterprise.Organization
	WHERE [Name] = @OrganizationName;
	
	--Map BlueBook --> bluebookid to party
	IF
	(
		SELECT 1
		FROM Enterprise.DataImportMapping
		WHERE PartyId = @OrganizationId
	) IS NULL
		BEGIN
			INSERT INTO #BlueBookMap
			EXEC Enterprise.MapBlueBookIdtoPartyId @BlueBookId, @OrganizationId;
	END;

	--Setup password policy
	IF NOT EXISTS
	(
		SELECT 1
		FROM Ident.PasswordPolicy
		WHERE PartyId = @OrganizationId
	)
		BEGIN
			INSERT INTO [Ident].[PasswordPolicy]( [PartyId], [MinimumLength], [MaximumLength], [MinimumLowercase], [MinimumUppercase], [MinimumNumeric], [MinimumSpecialCharacter], [AllowUsersToChangeOwnPassword], [EnablePasswordExpiration], [PasswordExpirationPeriodInDays], [PreventPasswordReuse], [NumberOfPasswordsToRemember], [UserId] )
				   SELECT @OrganizationId, [MinimumLength], [MaximumLength], [MinimumLowercase], [MinimumUppercase], [MinimumNumeric], [MinimumSpecialCharacter], [AllowUsersToChangeOwnPassword], [EnablePasswordExpiration], [PasswordExpirationPeriodInDays], [PreventPasswordReuse], [NumberOfPasswordsToRemember], 3
				   FROM [Ident].[PasswordPolicy]
				   WHERE PartyId = 3;
	END;

	--Setup thirdparty IDP
	IF @ThirdpartyIDP is NOT NULL or @ThirdpartyIDP <> ''
	BEGIN
		SELECT @ContactMechanismId = ContactMechanismId FROM Ident.IdentityProviderType
		WHERE IdentityProviderTypeId = @ThirdPartyIDP
		INSERT INTO [Enterprise].[PartyContactMechanism]
			   ([PartyId]
			   ,[ContactMechanismId]
			   ,[FromDate])
		 VALUES
			   (@OrganizationId, @ContactMechanismId, @Now) 
	      UPDATE Enterprise.Organization 
			 SET IdentityProviderTypeId = @ThirdPartyIDP
		  WHERE PartyId = @OrganizationId
	END

	--Setup User Types for the organization
    INSERT INTO Enterprise.PartyRole (Partyid, RoleTypeId)
    VALUES (@OrganizationId, 401), (@OrganizationId, 402), (@OrganizationId, 404), (@OrganizationId, 405)

	--Setup activity configuration
	INSERT INTO Ident.ActivityConfiguration( PartyId, ActivityTypeId, MaxActivityAttemptCount, ActivityTokenExpirationMinutes )
		VALUES( @OrganizationId, 1, 4, 30 ), ( @OrganizationId, 2, 3, 30 ), ( @OrganizationId, 3, 3, 30 ), ( @OrganizationId, 4, 3, 30 ), ( @OrganizationId, 5, 3, 30 ), ( @OrganizationId, 6, 2, 5 ), ( @OrganizationId, 7, 0, 30 ), ( @OrganizationId, 8, 5, 10080 ), ( @OrganizationId, 9, 5, 10080 ), ( @OrganizationId, 10, 0, 0 );
	
	SELECT PartyId as ID, RealPageId FROM #Org

END;
