GO
IF NOT EXISTS ( SELECT TOP 1 1 FROM Ident.IdentityProviderType where NAME = 'SAMLPingTest' )
BEGIN
	DECLARE @identityProviderTypeId INT
			,@ContactMechanismId INT
			,@IdentityProviderSettingTypeID INT
			,@CurrentId INT
			,@CurrentSettingName NVARCHAR(100)
			,@CurrentSettingValue NVARCHAR(500)

	DECLARE @IdentityProviderSetting TABLE ( SEQ INT IDENTITY(1,1), SettingName NVARCHAR(100), SettingValue NVARCHAR(510) )

	EXEC Person.CreateContactMechanism
		 @ContactMechanismId = @ContactMechanismId OUTPUT;
	EXECUTE [Ident].[CreateIdentityProviderType]
			@Name = 'SAMLPingTest',
			@Description = 'SAML Provider For Test',
			@ContactMechanismId = @ContactMechanismId;
	SET @ContactMechanismId = NULL;
	EXEC Person.CreateContactMechanism
		 @ContactMechanismId = @ContactMechanismId OUTPUT;

	SELECT @IdentityProviderTypeId = IdentityProviderTypeId
	FROM Ident.IdentityProviderType
	WHERE Name = 'SAMLPingTest';


	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'AuthenticationMode', '0' )
	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'AuthenticationType', 'samlpingtest' )
	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'AuthorityUri', 'https://myllocal.corp.realpage.com/identity/ping' )
	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'Caption', 'Sign in with SAML' )
	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'EntityId', 'https://pingone.com/idp/realpagedev' )
	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'MetadataLocation', 'd:\iis\idpmetadata\saml2-metadata-idp.xml' )
	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'PostLogoutRedirectUri', '' )
	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'RedirectUri', 'https://mylocal.corp.realpage.com/auth.aspx?idp=samlpingtest' )

	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'ValidateIssuer', '1' )

	--INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'ClientSecret', '' )
	--INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'TokenValidationAuthenticationType', '' )
	--INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'Scope', '' )
	--INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'ProviderClientId', '' ) 

	DECLARE ListSetting CURSOR
	FOR SELECT SEQ 
		FROM @IdentityProviderSetting

	OPEN ListSetting;
	FETCH ListSetting INTO @CurrentId;
	WHILE @@FETCH_STATUS = 0
		BEGIN
			SELECT @CurrentSettingName = SettingName
					,@CurrentSettingValue = SettingValue
			FROM @IdentityProviderSetting WHERE SEQ = @CurrentId

			EXECUTE [Ident].[CreateIdentityProviderSettingType]
				@IdentityProviderTypeId = @IdentityProviderTypeId,
				@Name = @CurrentSettingName;

			INSERT INTO Ident.IdentityProviderSetting ( IdentityProviderSettingTypeID, Value )
			SELECT 
				IPST.IdentityProviderSettingTypeID,
				@CurrentSettingValue
			FROM Ident.IdentityProviderType IPT
					INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
						AND IPT.IdentityProviderTypeId = @IdentityProviderTypeId
						AND IPST.Name = @CurrentSettingName;

			FETCH ListSetting INTO @CurrentId;
		END;
	CLOSE ListSetting;
	DEALLOCATE ListSetting;
END
GO


IF NOT EXISTS ( SELECT TOP 1 1 FROM Ident.IdentityProviderType where NAME = 'OIDCOktaTest' )
BEGIN
	DECLARE @identityProviderTypeId INT
			,@ContactMechanismId INT
			,@IdentityProviderSettingTypeID INT
			,@CurrentId INT
			,@CurrentSettingName NVARCHAR(100)
			,@CurrentSettingValue NVARCHAR(500)

	DECLARE @IdentityProviderSetting TABLE ( SEQ INT IDENTITY(1,1), SettingName NVARCHAR(100), SettingValue NVARCHAR(510) )

	EXEC Person.CreateContactMechanism
		 @ContactMechanismId = @ContactMechanismId OUTPUT;
	EXECUTE [Ident].[CreateIdentityProviderType]
			@Name = 'OIDCOktaTest',
			@Description = 'OIDC Provider For Okta',
			@ContactMechanismId = @ContactMechanismId;
	SET @ContactMechanismId = NULL;
	EXEC Person.CreateContactMechanism
		 @ContactMechanismId = @ContactMechanismId OUTPUT;

	SELECT @IdentityProviderTypeId = IdentityProviderTypeId
	FROM Ident.IdentityProviderType
	WHERE Name = 'OIDCOktaTest';


	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'AuthenticationMode', '0' )
	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'AuthenticationType', 'oidcokta' )
	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'AuthorityUri', 'https://dev-489082.oktapreview.com' )
	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'Caption', 'Sign in with OIDC Okta' )
	--INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'EntityId', 'https://pingone.com/idp/realpagedev' )
	--INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'MetadataLocation', 'd:\iis\idpmetadata\saml2-metadata-idp.xml' )
	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'PostLogoutRedirectUri', 'https://mylocal.corp.realpage.com' )
	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'RedirectUri', 'https://myllocal.corp.realpage.com/identity/connect/authorize' )

	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'ValidateIssuer', '1' )
	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'ProviderClientId', '0oaeous2qaIMbwSGa0h7' ) 
	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'TokenValidationAuthenticationType', 'idsrv.external' )
	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'Scope', 'openid email profile' )
	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'ValidAudience', '0oaeous2qaIMbwSGa0h7' )
	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'UserLoginClaim', 'email' )

	DECLARE ListSetting CURSOR
	FOR SELECT SEQ 
		FROM @IdentityProviderSetting

	OPEN ListSetting;
	FETCH ListSetting INTO @CurrentId;
	WHILE @@FETCH_STATUS = 0
		BEGIN
			SELECT @CurrentSettingName = SettingName
					,@CurrentSettingValue = SettingValue
			FROM @IdentityProviderSetting WHERE SEQ = @CurrentId

			EXECUTE [Ident].[CreateIdentityProviderSettingType]
				@IdentityProviderTypeId = @IdentityProviderTypeId,
				@Name = @CurrentSettingName;

			INSERT INTO Ident.IdentityProviderSetting ( IdentityProviderSettingTypeID, Value )
			SELECT 
				IPST.IdentityProviderSettingTypeID,
				@CurrentSettingValue
			FROM Ident.IdentityProviderType IPT
					INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
						AND IPT.IdentityProviderTypeId = @IdentityProviderTypeId
						AND IPST.Name = @CurrentSettingName;

			FETCH ListSetting INTO @CurrentId;
		END;
	CLOSE ListSetting;
	DEALLOCATE ListSetting;
END
GO

IF NOT EXISTS ( SELECT TOP 1 1 FROM Ident.IdentityProviderType where NAME = 'OIDCPingTest' )
BEGIN
	DECLARE @identityProviderTypeId INT
			,@ContactMechanismId INT
			,@IdentityProviderSettingTypeID INT
			,@CurrentId INT
			,@CurrentSettingName NVARCHAR(100)
			,@CurrentSettingValue NVARCHAR(500)

	DECLARE @IdentityProviderSetting TABLE ( SEQ INT IDENTITY(1,1), SettingName NVARCHAR(100), SettingValue NVARCHAR(510) )

	EXEC Person.CreateContactMechanism
		 @ContactMechanismId = @ContactMechanismId OUTPUT;
	EXECUTE [Ident].[CreateIdentityProviderType]
			@Name = 'OIDCPingTest',
			@Description = 'OIDC Provider For Ping',
			@ContactMechanismId = @ContactMechanismId;
	SET @ContactMechanismId = NULL;
	EXEC Person.CreateContactMechanism
		 @ContactMechanismId = @ContactMechanismId OUTPUT;

	SELECT @IdentityProviderTypeId = IdentityProviderTypeId
	FROM Ident.IdentityProviderType
	WHERE Name = 'OIDCPingTest';


	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'AuthenticationMode', '0' )
	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'AuthenticationType', 'oidcping' )
	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'AuthorityUri', 'https://dev-489082.oktapreview.com' )
	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'Caption', 'Sign in with OIDC Ping' )
	--INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'EntityId', 'https://pingone.com/idp/realpagedev' )
	--INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'MetadataLocation', 'd:\iis\idpmetadata\saml2-metadata-idp.xml' )
	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'PostLogoutRedirectUri', 'https://mylocal.corp.realpage.com' )
	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'RedirectUri', 'https://myllocal.corp.realpage.com/identity/connect/authorize' )

	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'ValidateIssuer', '1' )
	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'ProviderClientId', '18cd6ae0-d944-4026-a81a-ce1a141d5785' ) 
	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'TokenValidationAuthenticationType', 'idsrv.external' )
	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'Scope', 'openid email profile' )
	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'ValidAudience', '18cd6ae0-d944-4026-a81a-ce1a141d5785' )
	INSERT INTO @IdentityProviderSetting ( SettingName, SettingValue ) values ( 'UserLoginClaim', 'email')

	DECLARE ListSetting CURSOR
	FOR SELECT SEQ 
		FROM @IdentityProviderSetting

	OPEN ListSetting;
	FETCH ListSetting INTO @CurrentId;
	WHILE @@FETCH_STATUS = 0
		BEGIN
			SELECT @CurrentSettingName = SettingName
					,@CurrentSettingValue = SettingValue
			FROM @IdentityProviderSetting WHERE SEQ = @CurrentId

			EXECUTE [Ident].[CreateIdentityProviderSettingType]
				@IdentityProviderTypeId = @IdentityProviderTypeId,
				@Name = @CurrentSettingName;

			INSERT INTO Ident.IdentityProviderSetting ( IdentityProviderSettingTypeID, Value )
			SELECT 
				IPST.IdentityProviderSettingTypeID,
				@CurrentSettingValue
			FROM Ident.IdentityProviderType IPT
					INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
						AND IPT.IdentityProviderTypeId = @IdentityProviderTypeId
						AND IPST.Name = @CurrentSettingName;

			FETCH ListSetting INTO @CurrentId;
		END;
	CLOSE ListSetting;
	DEALLOCATE ListSetting;
END
GO


UPDATE IPS
SET VALUE = 'http://www.okta.com/'+value 
FROM Ident.IdentityProviderSetting IPS 
	WHERE IdentityProviderSettingTypeId in ( select identityprovidersettingtypeid from ident.IdentityProviderSettingType where name in ('OktaEntityId'))
	AND CHARINDEX('HTTP', value) = 0

-- update existing OKTA names to saml
UPDATE Ident.IdentityProviderType set Name = replace(Name, 'OKTA', 'SAML') where Name LIKE 'OKTA%' 
UPDATE Ident.IdentityProviderType set Description = replace(Description, 'OKTA', 'SAML') where Description LIKE 'OKTA%' 
UPDATE Ident.IdentityProviderSetting set value = replace(value, 'OKTA', 'saml') where value LIKE 'OKTA%' AND IdentityProviderSettingTypeId in ( select IdentityProviderSettingTypeId from Ident.IdentityProviderSettingType where name = 'AuthenticationType' )
UPDATE Ident.IdentityProviderSetting set value = replace(value, 'OKTA', 'saml') where value LIKE '%OKTA%' AND IdentityProviderSettingTypeId in ( select IdentityProviderSettingTypeId from Ident.IdentityProviderSettingType where name = 'RedirectUri' )

-- replace attribute names to remove okta
UPDATE Ident.IdentityProviderSettingType set name = 'EntityId' where name = 'OktaEntityId'
UPDATE Ident.IdentityProviderSettingType set name = 'MetadataLocation' where name = 'OktaMetadataLocation'

IF NOT EXISTS (Select top 1 1 from Ident.IdentityProviderSettingType IPST INNER JOIN Ident.IdentityProviderType IPT on IPST.IdentityProviderTypeId = IPT.IdentityProviderTypeId
					WHERE IPT.Name = 'Google' AND IPST.Name = 'UserLoginClaim' )
BEGIN
	INSERT INTO Ident.IdentityProviderSettingType ( IdentityProviderTypeId, Name ) 
		SELECT IdentityProviderTypeId, 'UserLoginClaim' FROM Ident.IdentityProviderType WHERE Name = 'Google'

	INSERT INTO Ident.IdentityProviderSetting ( IdentityProviderSettingTypeId, Value )
		SELECT IdentityProviderSettingTypeId, 'email' from Ident.IdentityProviderSettingType IPST INNER JOIN Ident.IdentityProviderType IPT on IPST.IdentityProviderTypeId = IPT.IdentityProviderTypeId
					WHERE IPT.Name = 'Google' AND IPST.Name = 'UserLoginClaim'
END
GO

IF NOT EXISTS (Select top 1 1 from Ident.IdentityProviderSettingType IPST INNER JOIN Ident.IdentityProviderType IPT on IPST.IdentityProviderTypeId = IPT.IdentityProviderTypeId
					WHERE IPT.Name = 'AzureActiveDirectory' AND IPST.Name = 'UserLoginClaim' )
BEGIN
	INSERT INTO Ident.IdentityProviderSettingType ( IdentityProviderTypeId, Name ) 
		SELECT IdentityProviderTypeId, 'UserLoginClaim' FROM Ident.IdentityProviderType WHERE Name = 'AzureActiveDirectory'

	INSERT INTO Ident.IdentityProviderSetting ( IdentityProviderSettingTypeId, Value )
		SELECT IdentityProviderSettingTypeId, 'upn' from Ident.IdentityProviderSettingType IPST INNER JOIN Ident.IdentityProviderType IPT on IPST.IdentityProviderTypeId = IPT.IdentityProviderTypeId
					WHERE IPT.Name = 'AzureActiveDirectory' AND IPST.Name = 'UserLoginClaim'
END
GO

--http://jira.realpage.com/browse/GB-1991

DECLARE @OrgRowNum INT;
DECLARE @ActionID INT;
DECLARE @RightID INT;
DECLARE @RoleID INT;
DECLARE @Status INT;
DECLARE @ActionValueID INT;
DECLARE @OrgID INT;
DECLARE @ProductID INT;
DECLARE @ParentActionId INT;
DECLARE @UserActionId INT;
DECLARE @RightCategoryId INT;
DECLARE @PartyId INT;
DECLARE @RightName VARCHAR(100);
DECLARE @RVT INT;
DECLARE @DefaultRoute NVARCHAR(200);
DECLARE @RightValueTypeId INT;
DECLARE @StatusId INT;
DECLARE @PersonaId INT;
DECLARE @FromDate DATETIME;
DECLARE @TRoleId INT;
DECLARE @TRoleName NVARCHAR(500);
DECLARE @TRoleDesc NVARCHAR(500);
DECLARE @TRightId INT;
DECLARE @TRightName NVARCHAR(500);
DECLARE @TRightDesc NVARCHAR(500);
DECLARE @RightCategory INT;
DECLARE @RoleCategory INT;
DECLARE @RoleName NVARCHAR(500);
DECLARE @RoleTypeID INT;
DECLARE @PerosonaP INT;
DECLARE @PartyRowNum INT;
DECLARE @TRightShortName NVARCHAR(100)
IF OBJECT_ID('tempdb..#RightsUnifiedSettings') IS NOT NULL
BEGIN
	DROP TABLE #RightsUnifiedSettings;
END;

IF OBJECT_ID('tempdb..#HoldPartyForUnifiedSettings') IS NOT NULL
BEGIN
	DROP TABLE #HoldPartyForUnifiedSettings;
END;

CREATE TABLE #RightsUnifiedSettings
( 
			 RightId int, Name nvarchar(500), description nvarchar(500), shortname varchar(100)
);

INSERT INTO #RightsUnifiedSettings( rightid, name, description, shortname )
VALUES
( 1, 'View only access to Unified Platform from Support Tool', 'It has non-functional product tiles and only has Read-Only access to Unified Platform functionality (using new System Role which was already been created in all companies).', 'ViewOnlySupportToolAccess' )

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

SELECT @ActionValueID = [ActionValueTypeID]
FROM Enterprise.ActionValueType
WHERE Value = 'Right';

SELECT @RoleCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Role Type' AND 
	  ST.Name = 'System';

SELECT @RightCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Right Type' AND 
	  ST.Name = 'System';

SELECT @Status = StatusType.StatusTypeID
FROM Enterprise.StatusTypeCategoryType
     JOIN Enterprise.StatusTypeCategory ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
     JOIN Enterprise.StatusTypeCategoryClassification ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
     JOIN Enterprise.StatusType ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
WHERE StatusType.name = 'Internal Only'
      AND StatusTypeCategoryType.Name = 'Security';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'View Only Support Tool Access' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'View Only Support Tool Access', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = 'Ability to access / view Support Tool Console', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = 'SupportTool' AND 
	  ObjectType = 'Route' AND 
	  Description = 'SuperUser';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'View Only Support Tool Access' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'View Only Support Tool Access', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;



SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings
FROM Person.Persona AS P
	 INNER JOIN
	 Enterprise.Organization AS O
	 ON P.OrganizationPartyId = O.PartyId
WHERE O.Name = 'RealPage Employee'

WHILE EXISTS
(
	SELECT 1
	FROM #HoldPartyForUnifiedSettings
	WHERE PStatus = 0
)
BEGIN
	SELECT TOP 1 @PartyRowNum = Rownumber, @PartyId = OrganizationPartyID
	FROM #HoldPartyForUnifiedSettings
	WHERE PStatus = 0;
	SELECT @RoleId = RoleId
	FROM Enterprise.Role AS R
		 INNER JOIN
		 Enterprise.RoleValueType AS RR
		 ON RR.RoleValueTypeId = R.RoleValueTypeId
	WHERE RR.Value = 'User Administrator' AND 
		  R.PartyId = @PartyId;
	
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ViewOnlySupportToolAccess', @ShortName = 'ViewOnlySupportToolAccess', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = '', @RightId = @RightId OUTPUT;
	SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'View Only Support Tool Access' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
		

	

	DECLARE Rights CURSOR
	FOR SELECT RightId, Name, Description, ShortName
		FROM #RightsUnifiedSettings;
	OPEN Rights;
	FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @RightId = @RightId OUTPUT;
		
		FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	END;
	CLOSE Rights;
	DEALLOCATE Rights;
	

	SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'View only access to Unified Platform from Support Tool'
		AND R.PartyId = @PartyId

		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'View Only Support Tool Access' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
	

	
	UPDATE #HoldPartyForUnifiedSettings
	  SET PStatus = 1
	WHERE RowNumber = @PartyRowNum;
END;

DECLARE @Access INT

SELECT  @Access = RightValueTypeId
FROM Enterprise.RightValueType 
WHERE value = 'Default_ViewOnlySupportToolAccess';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'View only access to Unified Platform from Support Tool');

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @Access
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @Access );
END;

GO
-- New RP System ROle

DECLARE @OrganizationId INT;
DECLARE @RoleId INT;
DECLARE @OrgRowNum INT;
DECLARE @PerRowNum INT;
DECLARE @PerPriv INT;
DECLARE @RoleName VARCHAR(200);
DECLARE @RightID INT;
DECLARE @ActionID INT;
DECLARE @Status INT;
DECLARE @UserActionID INT;
DECLARE @PersonRoleID INT;
DECLARE @Status_Role INT;
DECLARE @Status_Right INT;
DECLARE @HoldUserId INT;
IF OBJECT_ID('tempdb..#HoldOrgs') IS NULL
    BEGIN
        CREATE TABLE #HoldOrgs
(RowNumber           INT IDENTITY(1, 1),
 OrganizationPartyID INT,
 PStatus             BIT DEFAULT 0
);
    END;
BEGIN
    SELECT @Status_Right = ST.StatusTypeId
    FROM Enterprise.StatusTypeCategoryType AS STCT
         JOIN Enterprise.StatusTypeCategory AS STC ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
         JOIN Enterprise.StatusTypeCategoryClassification AS STCC ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
         JOIN Enterprise.StatusType AS ST ON ST.StatusTypeId = STCC.StatusTypeId
    WHERE STC.Name = 'Right Type'
          AND ST.Name = 'System';
    SELECT @Status_Role = ST.StatusTypeId
    FROM Enterprise.StatusTypeCategoryType AS STCT
         JOIN Enterprise.StatusTypeCategory AS STC ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
         JOIN Enterprise.StatusTypeCategoryClassification AS STCC ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
         JOIN Enterprise.StatusType AS ST ON ST.StatusTypeId = STCC.StatusTypeId
    WHERE STC.Name = 'Role Type'
          AND ST.Name = 'System';
    INSERT INTO #HoldOrgs(OrganizationPartyID)
           SELECT DISTINCT
                  OrganizationPartyID
           FROM Person.Persona AS P
                INNER JOIN Enterprise.Organization AS O ON P.OrganizationPartyId = O.PartyId;
    WHILE EXISTS
	(
		SELECT 1
		FROM #HoldOrgs
		WHERE PStatus = 0
	)
        BEGIN
            SELECT TOP 1 @OrganizationId = OrganizationPartyID,
                         @OrgRowNum = RowNumber
            FROM #HoldOrgs
            WHERE PStatus = 0;
            IF NOT EXISTS
			(
				SELECT 1
				FROM Enterprise.Role AS R
					 INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
				WHERE value = 'Read only for Unified Platform'
					  AND PartyID = @OrganizationId
			)
                BEGIN
                    EXEC Enterprise.CreateRole
                         @RoleName = N'Read only for Unified Platform',
                         @ShortName = 'ROForUnifiedPlatform',
                         @Description = N'Read only for Unified Platform',
                         @RoleTypeID = 402,
                         @RoleCategoryId = @Status_Role,
                         @PartyID = @OrganizationId,
                         @RoleID = @RoleID OUTPUT;
                    SET @RoleName = 'Read only for Unified Platform';
                    
					SELECT @RoleID = RoleId
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = @RoleName
                          AND PartyId = @OrganizationId;
                    EXECUTE Enterprise.CreateRight
                            @RoleId = @RoleId,
                            @PartyId = @OrganizationId,
                            @ProductId = 3,
                            @RightName = 'Ability to view users',
                            @RightCategoryId = @Status_Right,
                            @RightID = @RightID OUTPUT,
                            @Description = '';
                    SELECT @RightId;
                    

                    
					EXECUTE Enterprise.CreateRight
                            @RoleId = @RoleId,
                            @PartyId = @OrganizationId,
                            @ProductId = 3,
                            @RightName = 'Ability to edit my own profile',
                            @RightCategoryId = @Status_Right,
                            @RightID = @RightID OUTPUT,
                            @Description = '';
                    SELECT @RightId;
                    EXECUTE Enterprise.CreateRight
                            @RoleId = @RoleId,
                            @PartyId = @OrganizationId,
                            @ProductId = 3,
                            @RightName = 'Ability to view roles and rights',
                            @RightCategoryId = @Status_Right,
                            @RightID = @RightID OUTPUT,
                            @Description = '';
                    SELECT @RightId;
					

                    EXECUTE Enterprise.CreateRight
                            @RoleId = @RoleId,
                            @PartyId = @OrganizationId,
                            @ProductId = 3,
                            @RightName = 'Access to Product Learning Portal',
                            @RightCategoryId = @Status_Right,
                            @RightID = @RightID OUTPUT,
                            @Description = '';
                    SELECT @RightId;
                    EXECUTE Enterprise.CreateRight
                            @RoleId = @RoleId,
                            @PartyId = @OrganizationId,
                            @ProductId = 3,
                            @RightName = 'Ability to view audit trail on user data',
                            @RightCategoryId = @Status_Right,
                            @RightID = @RightID OUTPUT,
                            @Description = '';
                    SELECT @RightId;
                    EXECUTE Enterprise.CreateRight
                            @RoleId = @RoleId,
                            @PartyId = @OrganizationId,
                            @ProductId = 3,
                            @RightName = 'Default_SideMenu_Users',
                            @RightCategoryId = @Status_Right,
                            @RightID = @RightID OUTPUT,
                            @Description = '';
                    SELECT @RightId;
                    EXECUTE Enterprise.CreateRight
                            @RoleId = @RoleId,
                            @PartyId = @OrganizationId,
                            @ProductId = 3,
                            @RightName = 'Default_Dashboard_Users',
                            @RightCategoryId = @Status_Right,
                            @RightID = @RightID OUTPUT,
                            @Description = '';
                    SELECT @RightId;
                    
					SELECT @ActionID = ActionID
                    FROM Enterprise.ACTION
                    WHERE ObjectValue = 'SideMenu'
                          AND ObjectType = 'ROUTE'
                          AND Description = 'User'
                    SELECT @RoleID = RoleID
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = 'Read only for Unified Platform'
                          AND partyid = @OrganizationId;
                    SELECT @RightID = RightId
                    FROM Enterprise.[Right] AS R
                         INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                    WHERE Value = 'Default_SideMenu_Users'
                          AND RoleId = @RoleID;
                    EXEC Enterprise.LinkActionToRights
                         @ActionID = @ActionID,
                         @RightId = @RightId,
                         @StatusId = @Status_Right,
                         @UserActionId = @UserActionId OUTPUT;

                    SELECT @ActionID = ActionID
                    FROM Enterprise.ACTION
                    WHERE ObjectValue = 'Dashboard'
                          AND ObjectType = 'ROUTE'
                          AND Description = 'User';
                    SELECT @RoleID = RoleID
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = 'Read only for Unified Platform'
                          AND partyid = @OrganizationId;
                    SELECT @RightID = RightId
                    FROM Enterprise.[Right] AS R
                         INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                    WHERE Value = 'Default_dashboard_Users'
                          AND RoleId = @RoleID;
                    EXEC Enterprise.LinkActionToRights
                         @ActionID = @ActionID,
                         @RightId = @RightId,
                         @StatusId = @Status_Right,
                         @UserActionId = @UserActionId OUTPUT;
						 
					SELECT @ActionID = ActionID
                    FROM Enterprise.ACTION
                    WHERE ObjectValue = 'EditUser'
                          AND ObjectType = 'ROUTE'
                          AND Description = 'Edit User Route'
						  AND ParentActionId IS NULL
 					SELECT @RoleID = RoleID
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = 'Read only for Unified Platform'
                          AND partyid = @OrganizationId;
                    SELECT @RightID = RightId
                    FROM Enterprise.[Right] AS R
                         INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                    WHERE Value = 'Ability to edit my own profile'
                          AND RoleId = @RoleID;
					EXEC Enterprise.LinkActionToRights
                         @ActionID = @ActionID,
                         @RightId = @RightId,
                         @StatusId = @Status_Right,
                         @UserActionId = @UserActionId OUTPUT;

					SELECT @ActionID = ActionID
                    FROM Enterprise.ACTION
                    WHERE ObjectValue = 'UsersList'
                          AND ObjectType = 'ROUTE'
                          AND Description = 'User';
                    SELECT @RoleID = RoleID
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = 'Read only for Unified Platform'
                          AND partyid = @OrganizationId;
                    SELECT @RightID = RightId
                    FROM Enterprise.[Right] AS R
                         INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                    WHERE Value = 'Ability to view users'
                          AND RoleId = @RoleID;
					EXEC Enterprise.LinkActionToRights
                         @ActionID = @ActionID,
                         @RightId = @RightId,
                         @StatusId = @Status_Right,
                         @UserActionId = @UserActionId OUTPUT;

					SELECT @ActionId = ActionID
                    FROM Enterprise.ACTION
                    WHERE ObjectValue = 'EditUser'
                          AND ObjectType = 'ROUTE'
                          AND Description = 'Edit User Route';
                    SELECT @RoleID = RoleID
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = 'Read only for Unified Platform'
                          AND partyid = @OrganizationId;
                    SELECT @RightID = RightId
                    FROM Enterprise.[Right] AS R
                         INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                    WHERE Value = 'Ability to view audit trail on user data'
                          AND RoleId = @RoleID;
					EXEC Enterprise.LinkActionToRights
                         @ActionID = @ActionID,
                         @RightId = @RightId,
                         @StatusId = @Status_Right,
                         @UserActionId = @UserActionId OUTPUT;

					SELECT @ActionId = ActionID
                    FROM Enterprise.ACTION
                    WHERE ObjectValue = 'RolesAndRights'
                          AND ObjectType = 'ROUTE'
                          AND Description = 'User';
                    SELECT @RoleID = RoleID
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = 'Read only for Unified Platform'
                          AND partyid = @OrganizationId;
                    SELECT @RightID = RightId
                    FROM Enterprise.[Right] AS R
                         INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                    WHERE Value = 'Ability to view roles and rights'
                          AND RoleId = @RoleID;
					EXEC Enterprise.LinkActionToRights
                         @ActionID = @ActionID,
                         @RightId = @RightId,
                         @StatusId = @Status_Right,
                         @UserActionId = @UserActionId OUTPUT;
						
					SELECT @ActionId = ActionID
                    FROM Enterprise.ACTION
                    WHERE ObjectValue = 'Dashboard'
                          AND ObjectType = 'ROUTE'
                          AND Description = 'User';
                    SELECT @RoleID = RoleID
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = 'Read only for Unified Platform'
                          AND partyid = @OrganizationId;
                    SELECT @RightID = RightId
                    FROM Enterprise.[Right] AS R
                         INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                    WHERE Value = 'Access to Product Learning Portal'
                          AND RoleId = @RoleID;
					EXEC Enterprise.LinkActionToRights
                         @ActionID = @ActionID,
                         @RightId = @RightId,
                         @StatusId = @Status_Right,
                         @UserActionId = @UserActionId OUTPUT;

                END;
            UPDATE #HoldOrgs
              SET
                  PStatus = 1
            WHERE RowNumber = @OrgRowNum;
        END;
END;

GO
--http://jira.realpage.com/browse/GB-2639

IF EXISTS (SELECT 1 FROM Enterprise.Product  WHERE Name = 'Revenue Management')
BEGIN
	UPDATE Enterprise.Product 
		SET Name = 'YieldStar'
	WHERE Name = 'Revenue Management'
END

GO
IF EXISTS (SELECT 1 FROM Enterprise.ProductType WHERE Name = 'Revenue Management')
BEGIN
       UPDATE Enterprise.ProductType
             SET Name = 'YieldStar',
             description = 'YieldStar'     
       WHERE Name = 'Revenue Management'
       
END
UPDATE PS
SET PS.Value = 'YieldStar'
FROM [Enterprise].[ProductSetting] PS
	INNER JOIN Enterprise.ProductSettingType PST
		ON PST.ProductSettingTYpeId = PS.ProductSettingTypeId
		WHERE PS.ProductId = 32 AND PST.Name = 'TitleId'

GO
--

--Blackbook
IF EXISTS (SELECT * FROM Enterprise.Product  WHERE Name = 'Research Application')
BEGIN
	UPDATE Enterprise.Product 
		SET Name = 'Black Book'
	WHERE Name = 'Research Application'
END

GO
IF EXISTS (SELECT * FROM Enterprise.ProductType WHERE Name = 'Research Tool')
BEGIN
       UPDATE Enterprise.ProductType
             SET Name = 'Black Book',
             description = 'Black Book'     
       WHERE Name = 'Research Tool'
       
END
UPDATE PS
SET PS.Value = 'BlackBook'
FROM [Enterprise].[ProductSetting] PS
	INNER JOIN Enterprise.ProductSettingType PST
		ON PST.ProductSettingTYpeId = PS.ProductSettingTypeId
		WHERE PS.ProductId = 24 AND PST.Name = 'TitleId'

GO

--Reseach Tools

DECLARE @TRoleId INT;
DECLARE @TRoleName NVARCHAR(200);
DECLARE @TRoleDesc NVARCHAR(200);
DECLARE @TRightId INT;
DECLARE @TRightName NVARCHAR(200);
DECLARE @TRightDesc NVARCHAR(200);
DECLARE @RoleId INT;
DECLARE @RightId INT;
DECLARE @RightCategory INT;
DECLARE @RoleCategory INT;
DECLARE @PartyId INT;
DECLARE @ProductId INT;
DECLARE @RoleName NVARCHAR(200);
DECLARE @RightName NVARCHAR(200);
DECLARE @RoleTypeID INT;
DECLARE @PerosonaP INT

IF OBJECT_ID('tempdb..#Role') IS NOT NULL
    DROP TABLE #Role

IF OBJECT_ID('tempdb..#Right') IS NOT NULL
    DROP TABLE #Right

IF OBJECT_ID('tempdb..#Mapping') IS NOT NULL
    DROP TABLE #Mapping

SELECT @RoleTypeId = PartyROleTypeId
FROM enterprise.roletype
WHERE Name = 'Product Role';

SELECT @PartyId = PartyId
FROM enterprise.Organization
WHERE Name = 'RealPage Employee';

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Research Application';

SELECT @RoleCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Role Type' AND 
	  ST.Name = 'System';

SELECT @RightCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Right Type' AND 
	  ST.Name = 'System';

CREATE TABLE #Role
( 
			 RoleID int, Name nvarchar(100), shortname nvarchar(100)
);

INSERT INTO #Role( RoleId, Name, shortname )
VALUES
(1,'Research Analyst','analyst')
,(2,'Research QA','QA')
,(3,'Research Manager','manager')
,(4,'Black-Book Director','director')
,(5,'Executive','executive')
,(6,'External Analyst','external')
,(7,'Migration Analyst','migration')
,(8,'Implementation Analyst','implementation')
, (9, 'White Space', 'whitespace')


CREATE TABLE #Right
( 
			 RightId int, Name nvarchar(100), shortname nvarchar(100)
);

INSERT INTO #Right( rightid, name, shortname )
VALUES
(1,'ViewOnly','view.only')

DELETE R  FROM Enterprise.[Right] R
	INNER JOIN Enterprise.RightValueTYpe RVT
		ON RVT.RightValueTYpeId = R.RightValueTYpeId
	WHERE ProductId = @ProductId 

DELETE RVT  FROM Enterprise.[Right] R
	INNER JOIN Enterprise.RightValueTYpe RVT
		ON RVT.RightValueTYpeId = R.RightValueTYpeId
	WHERE RVT.Value IN (SELECT Name FROM #Right)
	AND 
	ProductId = @ProductId 



CREATE TABLE #Mapping
( 
			 RoleId int, RightId int
);

INSERT INTO #Mapping( rightid, roleid )
VALUES 
(1,1),
(1,2),
(1,3),
(1,4),
(1,5),
(1,6),
(1,7),
(1,8),
(1,9)



DECLARE Roles CURSOR
FOR SELECT RoleId, Name, SHortNAme
	FROM #Role;

OPEN Roles;

FETCH Roles INTO @TRoleId, @TRoleName, @TRoleDesc;

WHILE @@FETCH_STATUS = 0
BEGIN
	EXECUTE Enterprise.CreateRole @RoleName = @TRoleName, @Shortname = @TRoleDesc, @Description= @TROleName, @RoleTypeId = @RoleTypeId, @RoleCategoryId = @RoleCategory, @PartyId = @PartyId, @RoleId = @RoleId OUTPUT;
	DECLARE RightsL CURSOR
	FOR SELECT Name, shortname, b.RightId
		FROM #Right AS a
			 INNER JOIN
			 #Mapping AS b
			 ON a.RightId = b.RightId AND 
				b.RoleID = @TRoleId;
	OPEN RightsL;
	FETCH RightsL INTO @TRightName, @TRightDesc, @TRightId;
	WHILE @@FETCH_STATUS = 0
	BEGIN
		Print @TRightName
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightDesc,  @Description = @TRightName, @RightId = @RightId OUTPUT;
		FETCH RightsL INTO @TRightName, @TRightDesc, @TRightId;
	END;
	CLOSE RightsL;
	DEALLOCATE RightsL;

	--EXECUTE [Enterprise].[LinkPersonaToRole] @PersonaId = 33, @RoleId = @RoleId, @PersonaPrivilgeID = @PerosonaP OUTPUT
	FETCH Roles INTO @TRoleId, @TRoleName, @TRoleDesc;
END;

CLOSE Roles;

DEALLOCATE Roles;


DELETE RVT  FROM Enterprise.[Right] R
	INNER JOIN Enterprise.RightValueTYpe RVT
		ON RVT.RightValueTYpeId = R.RightValueTYpeId
	WHERE RVT.Value NOT IN (SELECT name FROM #Right)
	AND ProductId = @ProductId 
--Link to Persona 33

--Execute Enterprise.ListRightsAssociatedWithRoles 10639, 24

DECLARE @VisibilityStatus INT  
DECLARE @RightTypeId INT         

			
SET @VisibilityStatus = NULL
SELECT @VisibilityStatus = StatusType.StatusTypeID
         FROM Enterprise.StatusTypeCategoryType
              JOIN Enterprise.StatusTypeCategory ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
              JOIN Enterprise.StatusTypeCategoryClassification ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
              JOIN Enterprise.StatusType ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
         WHERE StatusType.name = 'ALL'
               AND StatusTypeCategoryType.Name = 'Security';

UPDATE Enterprise.RightValueType
SET ProductId = @ProductId,
	VisibilityStatusId = @VisibilityStatus
WHERE  Value IN 
	(
	SELECT Name FROM #Right
	)

--Link to Persona 33

--Execute Enterprise.ListRightsAssociatedWithRoles 10639, 24

GO
	--Prodcut Settings
	

SET NOCOUNT ON;


DECLARE @ProductId INT , 
		 @CurrentProductConfigurationID INT , 
		 @ProductSettingTypeId INT , 
		 @ProductSettingId INT , 
		 @NotificationEmailRequiredForUserWithNoEmail INT , 
		 @LockOnProductAccess INT;

IF
(
	SELECT 1
	FROM Enterprise.ProductSettingType
	WHERE Name = 'NotificationEmailRequiredForUserWithNoEmail'
) IS NULL
BEGIN
	EXEC Enterprise.CreateProductSettingType 'NotificationEmailRequiredForUserWithNoEmail', 'Does Notification Email is required for Regular User (No Email) by product.', @ProductSettingTypeId OUTPUT;
END;

IF
(
	SELECT 1
	FROM Enterprise.ProductSettingType
	WHERE Name = 'LockOnProductAccess'
) IS NULL
BEGIN
	EXEC Enterprise.CreateProductSettingType 'LockOnProductAccess', 'Lock selection on product access.', @ProductSettingTypeId OUTPUT;
END;

DECLARE @ProductSetting TABLE
( 
 ProductId int, NotificationEmailRequiredForUserWithNoEmail int, LockOnProductAccess int
);

INSERT INTO @ProductSetting( ProductId, NotificationEmailRequiredForUserWithNoEmail, LockOnProductAccess )
VALUES	( 1, 0, 0 ), -- OneSite
		( 3, 0, 1 ), -- Unified Platform
		( 4, 0, 0 ), -- Asset Optimization
		( 6, 0, 0 ), -- Lead2Lease
		( 7, 0, 0 ), -- NOT USED
		( 8, 0, 0 ), -- RealPage Accounting
		( 9, 0, 0 ), -- Marketing Center
		( 10, 0, 0 ), -- Prospect Contact Center
		( 13, 0, 0 ), -- Spend Management
		( 14, 0, 0 ), -- Client Portal
		( 15, 1, 0 ), -- Renters Insurance
		( 16, 0, 0 ), -- Vendor Services
		( 17, 1, 0 ), -- Resident Portals
		( 18, 0, 0 ), -- Utility Management
		( 19, 0, 0 ), -- Product Learning Portal
		( 20, 0, 0 ), -- RealPage Document Management
		( 21, 0, 0 ), -- OneSite Conversions
		( 23, 0, 0 ), -- On-Site
		( 24, 0, 0 ), -- Research Application
		( 25, 0, 0 ), -- Self-provisioning portal
		( 26, 0, 0 ), -- Unified Amenities
		( 27, 0, 0 ), -- Migration Tool Application
		( 28, 0, 0 ), -- Product Updates
		( 29, 0, 0 ), -- Business Intelligence
		( 30, 0, 0 ), -- Performance Analytics
		( 31, 0, 0 ), -- Investment Analytics
		( 32, 0, 0 ), -- Revenue Management
		( 33, 0, 0 ), -- Axiometrics
		( 34, 0, 0 ), -- Benchmarking
		( 35, 0, 0 ), -- Support Tool
		( 36, 0, 1 ), -- EasyLMS
		( 37, 0, 0 ), -- PropertyPhotos
		( 38, 0, 0 ); -- VendorMarketplace


DECLARE @NOW datetime= GETUTCDATE();

DECLARE Products CURSOR
FOR SELECT ProductId, NotificationEmailRequiredForUserWithNoEmail, LockOnProductAccess
	FROM @ProductSetting;

OPEN Products;

FETCH Products INTO @ProductId, @NotificationEmailRequiredForUserWithNoEmail, @LockOnProductAccess;

WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT TOP 1 @CurrentProductConfigurationID = ConfigurationId
	FROM Enterprise.GlobalProductConfiguration AS gpc
	WHERE gpc.ProductId = @ProductId AND 
		  ( ( @NOW BETWEEN gpc.FromDate AND gpc.ThruDate
			) OR 
			( @NOW >= gpc.FromDate AND 
			  gpc.ThruDate IS NULL
			)
		  )
	ORDER BY GlobalProductConfigurationId DESC;

	
	--- NotificationEmailRequiredForUserWithNoEmail 
	SET @ProductSettingTypeId = NULL;
	EXEC Enterprise.GetProductSettingType @Name = 'NotificationEmailRequiredForUserWithNoEmail', @ProductSettingTypeId = @ProductSettingTypeId OUTPUT;

	IF @ProductSettingTypeId IS NOT NULL AND 
	   NOT EXISTS
	(
		SELECT TOP 1 1
		FROM Enterprise.ProductSetting
		WHERE ProductID = @productId AND 
			  ProductSettingTypeId = @ProductSettingTypeId AND 
			  ThruDate IS NULL
	)
	BEGIN
	
		-- Create the Value and assign it to the Product and ProductSettingType
		EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
		@ProductSettingTypeId = @ProductSettingTypeId, -- int
		@Value = @NotificationEmailRequiredForUserWithNoEmail, 
		@FromDate = @NOW, -- datetime
		@ThruDate = NULL, -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT; -- int

		-- Link the Product Setting to an actual configuration
		EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @CurrentProductConfigurationID, -- int
		@ProductSettingId = @ProductSettingId, -- int
		@FromDate = @NOW, -- datetime
		@ThruDate = NULL;   -- datetime
	END;
	--- NotificationEmailRequiredForUserWithNoEmail 

	--- LockOnProductAccess 
	SET @ProductSettingTypeId = NULL;
	EXEC Enterprise.GetProductSettingType @Name = 'LockOnProductAccess', @ProductSettingTypeId = @ProductSettingTypeId OUTPUT;

	IF @ProductSettingTypeId IS NOT NULL AND 
	  
	   NOT EXISTS
	(
		SELECT TOP 1 1
		FROM Enterprise.ProductSetting
		WHERE ProductID = @productId AND 
			  ProductSettingTypeId = @ProductSettingTypeId AND 
			  ThruDate IS NULL
	)
	BEGIN
		
		-- Create the Value and assign it to the Product and ProductSettingType
		EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
		@ProductSettingTypeId = @ProductSettingTypeId, -- int
		@Value = @LockOnProductAccess, -- nvarchar(1000)
		@FromDate = @NOW, -- datetime
		@ThruDate = NULL, -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT; -- int

		-- Link the Product Setting to an actual configuration
		EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @CurrentProductConfigurationID, -- int
		@ProductSettingId = @ProductSettingId, -- int
		@FromDate = @NOW, -- datetime
		@ThruDate = NULL;   -- datetime
	END;
	--- LockOnProductAccess




	FETCH Products INTO @ProductId, @NotificationEmailRequiredForUserWithNoEmail, @LockOnProductAccess;
END;
GO

--Settings and Support Tools
DECLARE @ResendInvite int;
DECLARE @Lock INT
DECLARE @OrgRowNum INT;
DECLARE @ActionID INT;
DECLARE @RightID INT;
DECLARE @RoleID INT;
DECLARE @Status INT;
DECLARE @ActionValueID INT;
DECLARE @OrgID INT;
DECLARE @ProductID INT;
DECLARE @ParentActionId INT;
DECLARE @UserActionId INT;
DECLARE @RightCategoryId INT;
DECLARE @PartyId INT;
DECLARE @RightName VARCHAR(100);
DECLARE @RVT INT;
DECLARE @DefaultRoute NVARCHAR(200);
DECLARE @RightValueTypeId INT;
DECLARE @StatusId INT;
DECLARE @PersonaId INT;
DECLARE @FromDate DATETIME;
DECLARE @TRoleId INT;
DECLARE @TRoleName NVARCHAR(500);
DECLARE @TRoleDesc NVARCHAR(500);
DECLARE @TRightId INT;
DECLARE @TRightName NVARCHAR(500);
DECLARE @TRightDesc NVARCHAR(500);
DECLARE @RightCategory INT;
DECLARE @RoleCategory INT;
DECLARE @RoleName NVARCHAR(500);
DECLARE @RoleTypeID INT;
DECLARE @PerosonaP INT;
DECLARE @PartyRowNum INT;


SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Unified Platform';

SELECT @ActionValueID = [ActionValueTypeID]
FROM Enterprise.ActionValueType
WHERE Value = 'Right';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Settings' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Settings', @ActionTarget = N'Route', @ActionbValueTypeId = @ActionValueID, @Description = 'SupportRoute Route.', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;



SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = 'SideMenu' AND 
	  ObjectType = 'Route' AND 
	  Description = 'SuperUser';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Settings' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Settings', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Settings' AND Description = 'SuperUser' AND OBjectType = 'Route'
		 AND ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Settings', @ActionTarget = N'Route', @ActionbValueTypeId = @ActionValueID, @Description = 'Superuser', @ActionID = @ActionID OUTPUT;
	SELECT @ParentActionId = @ActionID;
END;

GO
-- New RP System ROle

DECLARE @OrganizationId INT;
DECLARE @RoleId INT;
DECLARE @OrgRowNum INT;
DECLARE @PerRowNum INT;
DECLARE @PerPriv INT;
DECLARE @RoleName VARCHAR(200);
DECLARE @RightID INT;
DECLARE @ActionID INT;
DECLARE @Status INT;
DECLARE @UserActionID INT;
DECLARE @PersonRoleID INT;
DECLARE @Status_Role INT;
DECLARE @Status_Right INT;
DECLARE @HoldUserId INT;
IF OBJECT_ID('tempdb..#HoldOrgs') IS NULL
    BEGIN
        CREATE TABLE #HoldOrgs
(RowNumber           INT IDENTITY(1, 1),
 OrganizationPartyID INT,
 PStatus             BIT DEFAULT 0
);
    END;
BEGIN
    SELECT @Status_Right = ST.StatusTypeId
    FROM Enterprise.StatusTypeCategoryType AS STCT
         JOIN Enterprise.StatusTypeCategory AS STC ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
         JOIN Enterprise.StatusTypeCategoryClassification AS STCC ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
         JOIN Enterprise.StatusType AS ST ON ST.StatusTypeId = STCC.StatusTypeId
    WHERE STC.Name = 'Right Type'
          AND ST.Name = 'System';
    SELECT @Status_Role = ST.StatusTypeId
    FROM Enterprise.StatusTypeCategoryType AS STCT
         JOIN Enterprise.StatusTypeCategory AS STC ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
         JOIN Enterprise.StatusTypeCategoryClassification AS STCC ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
         JOIN Enterprise.StatusType AS ST ON ST.StatusTypeId = STCC.StatusTypeId
    WHERE STC.Name = 'Role Type'
          AND ST.Name = 'System';
    INSERT INTO #HoldOrgs(OrganizationPartyID)
           SELECT DISTINCT
                  OrganizationPartyID
           FROM Person.Persona AS P
                INNER JOIN Enterprise.Organization AS O ON P.OrganizationPartyId = O.PartyId;
    WHILE EXISTS
	(
		SELECT 1
		FROM #HoldOrgs
		WHERE PStatus = 0
	)
        BEGIN
            SELECT TOP 1 @OrganizationId = OrganizationPartyID,
                         @OrgRowNum = RowNumber
            FROM #HoldOrgs
            WHERE PStatus = 0;
            IF NOT EXISTS
			(
				SELECT 1
				FROM Enterprise.Role AS R
					 INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
				WHERE value = 'Read only for Unified Platform'
					  AND PartyID = @OrganizationId
			)
                BEGIN
                    EXEC Enterprise.CreateRole
                         @RoleName = N'Read only for Unified Platform',
                         @ShortName = 'ROForUnifiedPlatform',
                         @Description = N'Read only for Unified Platform',
                         @RoleTypeID = 402,
                         @RoleCategoryId = @Status_Role,
                         @PartyID = @OrganizationId,
                         @RoleID = @RoleID OUTPUT;
                    SET @RoleName = 'Read only for Unified Platform';
                    
					SELECT @RoleID = RoleId
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = @RoleName
                          AND PartyId = @OrganizationId;
                    EXECUTE Enterprise.CreateRight
                            @RoleId = @RoleId,
                            @PartyId = @OrganizationId,
                            @ProductId = 3,
                            @RightName = 'Ability to view users',
                            @RightCategoryId = @Status_Right,
                            @RightID = @RightID OUTPUT,
                            @Description = '';
                    SELECT @RightId;
                    

                    
					EXECUTE Enterprise.CreateRight
                            @RoleId = @RoleId,
                            @PartyId = @OrganizationId,
                            @ProductId = 3,
                            @RightName = 'Ability to edit my own profile',
                            @RightCategoryId = @Status_Right,
                            @RightID = @RightID OUTPUT,
                            @Description = '';
                    SELECT @RightId;
                    EXECUTE Enterprise.CreateRight
                            @RoleId = @RoleId,
                            @PartyId = @OrganizationId,
                            @ProductId = 3,
                            @RightName = 'Ability to view roles and rights',
                            @RightCategoryId = @Status_Right,
                            @RightID = @RightID OUTPUT,
                            @Description = '';
                    SELECT @RightId;
					

                    EXECUTE Enterprise.CreateRight
                            @RoleId = @RoleId,
                            @PartyId = @OrganizationId,
                            @ProductId = 3,
                            @RightName = 'Access to Product Learning Portal',
                            @RightCategoryId = @Status_Right,
                            @RightID = @RightID OUTPUT,
                            @Description = '';
                    SELECT @RightId;
                    EXECUTE Enterprise.CreateRight
                            @RoleId = @RoleId,
                            @PartyId = @OrganizationId,
                            @ProductId = 3,
                            @RightName = 'Ability to view audit trail on user data',
                            @RightCategoryId = @Status_Right,
                            @RightID = @RightID OUTPUT,
                            @Description = '';
                    SELECT @RightId;
                    EXECUTE Enterprise.CreateRight
                            @RoleId = @RoleId,
                            @PartyId = @OrganizationId,
                            @ProductId = 3,
                            @RightName = 'Default_SideMenu_Users',
                            @RightCategoryId = @Status_Right,
                            @RightID = @RightID OUTPUT,
                            @Description = '';
                    SELECT @RightId;
                    EXECUTE Enterprise.CreateRight
                            @RoleId = @RoleId,
                            @PartyId = @OrganizationId,
                            @ProductId = 3,
                            @RightName = 'Default_Dashboard_Users',
                            @RightCategoryId = @Status_Right,
                            @RightID = @RightID OUTPUT,
                            @Description = '';
                    SELECT @RightId;
                    
					SELECT @ActionID = ActionID
                    FROM Enterprise.ACTION
                    WHERE ObjectValue = 'SideMenu'
                          AND ObjectType = 'ROUTE'
                          AND Description = 'User'
                    SELECT @RoleID = RoleID
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = 'Read only for Unified Platform'
                          AND partyid = @OrganizationId;
                    SELECT @RightID = RightId
                    FROM Enterprise.[Right] AS R
                         INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                    WHERE Value = 'Default_SideMenu_Users'
                          AND RoleId = @RoleID;
                    EXEC Enterprise.LinkActionToRights
                         @ActionID = @ActionID,
                         @RightId = @RightId,
                         @StatusId = @Status_Right,
                         @UserActionId = @UserActionId OUTPUT;

                    SELECT @ActionID = ActionID
                    FROM Enterprise.ACTION
                    WHERE ObjectValue = 'Dashboard'
                          AND ObjectType = 'ROUTE'
                          AND Description = 'User';
                    SELECT @RoleID = RoleID
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = 'Read only for Unified Platform'
                          AND partyid = @OrganizationId;
                    SELECT @RightID = RightId
                    FROM Enterprise.[Right] AS R
                         INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                    WHERE Value = 'Default_dashboard_Users'
                          AND RoleId = @RoleID;
                    EXEC Enterprise.LinkActionToRights
                         @ActionID = @ActionID,
                         @RightId = @RightId,
                         @StatusId = @Status_Right,
                         @UserActionId = @UserActionId OUTPUT;
						 
					SELECT @ActionID = ActionID
                    FROM Enterprise.ACTION
                    WHERE ObjectValue = 'EditUser'
                          AND ObjectType = 'ROUTE'
                          AND Description = 'Edit User Route'
						  AND ParentActionId IS NULL
 					SELECT @RoleID = RoleID
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = 'Read only for Unified Platform'
                          AND partyid = @OrganizationId;
                    SELECT @RightID = RightId
                    FROM Enterprise.[Right] AS R
                         INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                    WHERE Value = 'Ability to edit my own profile'
                          AND RoleId = @RoleID;
					EXEC Enterprise.LinkActionToRights
                         @ActionID = @ActionID,
                         @RightId = @RightId,
                         @StatusId = @Status_Right,
                         @UserActionId = @UserActionId OUTPUT;

					SELECT @ActionID = ActionID
                    FROM Enterprise.ACTION
                    WHERE ObjectValue = 'UsersList'
                          AND ObjectType = 'ROUTE'
                          AND Description = 'User';
                    SELECT @RoleID = RoleID
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = 'Read only for Unified Platform'
                          AND partyid = @OrganizationId;
                    SELECT @RightID = RightId
                    FROM Enterprise.[Right] AS R
                         INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                    WHERE Value = 'Ability to view users'
                          AND RoleId = @RoleID;
					EXEC Enterprise.LinkActionToRights
                         @ActionID = @ActionID,
                         @RightId = @RightId,
                         @StatusId = @Status_Right,
                         @UserActionId = @UserActionId OUTPUT;

					SELECT @ActionId = ActionID
                    FROM Enterprise.ACTION
                    WHERE ObjectValue = 'EditUser'
                          AND ObjectType = 'ROUTE'
                          AND Description = 'Edit User Route';
                    SELECT @RoleID = RoleID
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = 'Read only for Unified Platform'
                          AND partyid = @OrganizationId;
                    SELECT @RightID = RightId
                    FROM Enterprise.[Right] AS R
                         INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                    WHERE Value = 'Ability to view audit trail on user data'
                          AND RoleId = @RoleID;
					EXEC Enterprise.LinkActionToRights
                         @ActionID = @ActionID,
                         @RightId = @RightId,
                         @StatusId = @Status_Right,
                         @UserActionId = @UserActionId OUTPUT;

					SELECT @ActionId = ActionID
                    FROM Enterprise.ACTION
                    WHERE ObjectValue = 'RolesAndRights'
                          AND ObjectType = 'ROUTE'
                          AND Description = 'User';
                    SELECT @RoleID = RoleID
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = 'Read only for Unified Platform'
                          AND partyid = @OrganizationId;
                    SELECT @RightID = RightId
                    FROM Enterprise.[Right] AS R
                         INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                    WHERE Value = 'Ability to view roles and rights'
                          AND RoleId = @RoleID;
					EXEC Enterprise.LinkActionToRights
                         @ActionID = @ActionID,
                         @RightId = @RightId,
                         @StatusId = @Status_Right,
                         @UserActionId = @UserActionId OUTPUT;
						
					SELECT @ActionId = ActionID
                    FROM Enterprise.ACTION
                    WHERE ObjectValue = 'Dashboard'
                          AND ObjectType = 'ROUTE'
                          AND Description = 'User';
                    SELECT @RoleID = RoleID
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = 'Read only for Unified Platform'
                          AND partyid = @OrganizationId;
                    SELECT @RightID = RightId
                    FROM Enterprise.[Right] AS R
                         INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                    WHERE Value = 'Access to Product Learning Portal'
                          AND RoleId = @RoleID;
					EXEC Enterprise.LinkActionToRights
                         @ActionID = @ActionID,
                         @RightId = @RightId,
                         @StatusId = @Status_Right,
                         @UserActionId = @UserActionId OUTPUT;

                END;
            UPDATE #HoldOrgs
              SET
                  PStatus = 1
            WHERE RowNumber = @OrgRowNum;
        END;
END;

GO
--http://jira.realpage.com/browse/GB-2639

IF EXISTS (SELECT 1 FROM Enterprise.Product  WHERE Name = 'Revenue Management')
BEGIN
	UPDATE Enterprise.Product 
		SET Name = 'YieldStar'
	WHERE Name = 'Revenue Management'
END

GO

--Reseach Tools

DECLARE @TRoleId INT;
DECLARE @TRoleName NVARCHAR(200);
DECLARE @TRoleDesc NVARCHAR(200);
DECLARE @TRightId INT;
DECLARE @TRightName NVARCHAR(200);
DECLARE @TRightDesc NVARCHAR(200);
DECLARE @RoleId INT;
DECLARE @RightId INT;
DECLARE @RightCategory INT;
DECLARE @RoleCategory INT;
DECLARE @PartyId INT;
DECLARE @ProductId INT;
DECLARE @RoleName NVARCHAR(200);
DECLARE @RightName NVARCHAR(200);
DECLARE @RoleTypeID INT;
DECLARE @PerosonaP INT

IF OBJECT_ID('tempdb..#Role') IS NOT NULL
    DROP TABLE #Role

IF OBJECT_ID('tempdb..#Right') IS NOT NULL
    DROP TABLE #Right

IF OBJECT_ID('tempdb..#Mapping') IS NOT NULL
    DROP TABLE #Mapping

SELECT @RoleTypeId = PartyROleTypeId
FROM enterprise.roletype
WHERE Name = 'Product Role';

SELECT @PartyId = PartyId
FROM enterprise.Organization
WHERE Name = 'RealPage Employee';

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Research Application';

SELECT @RoleCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Role Type' AND 
	  ST.Name = 'System';

SELECT @RightCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Right Type' AND 
	  ST.Name = 'System';

CREATE TABLE #Role
( 
			 RoleID int, Name nvarchar(100), shortname nvarchar(100)
);

INSERT INTO #Role( RoleId, Name, shortname )
VALUES
(1,'Research Analyst','analyst')
,(2,'Research QA','QA')
,(3,'Research Manager','manager')
,(4,'Black-Book Director','director')
,(5,'Executive','executive')
,(6,'External Analyst','external')
,(7,'Migration Analyst','migration')
,(8,'Implementation Analyst','implementation')


CREATE TABLE #Right
( 
			 RightId int, Name nvarchar(100), shortname nvarchar(100)
);

INSERT INTO #Right( rightid, name, shortname )
VALUES
(1,'ViewOnly','view.only')

DELETE R  FROM Enterprise.[Right] R
	INNER JOIN Enterprise.RightValueTYpe RVT
		ON RVT.RightValueTYpeId = R.RightValueTYpeId
	WHERE ProductId = @ProductId 

DELETE RVT  FROM Enterprise.[Right] R
	INNER JOIN Enterprise.RightValueTYpe RVT
		ON RVT.RightValueTYpeId = R.RightValueTYpeId
	WHERE RVT.Value IN (SELECT Name FROM #Right)
	AND 
	ProductId = @ProductId 



CREATE TABLE #Mapping
( 
			 RoleId int, RightId int
);

INSERT INTO #Mapping( rightid, roleid )
VALUES 
(1,1),
(1,2),
(1,3),
(1,4),
(1,5),
(1,6),
(1,7),
(1,8)



DECLARE Roles CURSOR
FOR SELECT RoleId, Name, SHortNAme
	FROM #Role;

OPEN Roles;

FETCH Roles INTO @TRoleId, @TRoleName, @TRoleDesc;

WHILE @@FETCH_STATUS = 0
BEGIN
	EXECUTE Enterprise.CreateRole @RoleName = @TRoleName, @Shortname = @TRoleDesc, @Description= @TROleName, @RoleTypeId = @RoleTypeId, @RoleCategoryId = @RoleCategory, @PartyId = @PartyId, @RoleId = @RoleId OUTPUT;
	DECLARE RightsL CURSOR
	FOR SELECT Name, shortname, b.RightId
		FROM #Right AS a
			 INNER JOIN
			 #Mapping AS b
			 ON a.RightId = b.RightId AND 
				b.RoleID = @TRoleId;
	OPEN RightsL;
	FETCH RightsL INTO @TRightName, @TRightDesc, @TRightId;
	WHILE @@FETCH_STATUS = 0
	BEGIN
		Print @TRightName
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightDesc,  @Description = @TRightName, @RightId = @RightId OUTPUT;
		FETCH RightsL INTO @TRightName, @TRightDesc, @TRightId;
	END;
	CLOSE RightsL;
	DEALLOCATE RightsL;

	--EXECUTE [Enterprise].[LinkPersonaToRole] @PersonaId = 33, @RoleId = @RoleId, @PersonaPrivilgeID = @PerosonaP OUTPUT
	FETCH Roles INTO @TRoleId, @TRoleName, @TRoleDesc;
END;

CLOSE Roles;

DEALLOCATE Roles;


DELETE RVT  FROM Enterprise.[Right] R
	INNER JOIN Enterprise.RightValueTYpe RVT
		ON RVT.RightValueTYpeId = R.RightValueTYpeId
	WHERE RVT.Value NOT IN (SELECT name FROM #Right)
	AND ProductId = @ProductId 
--Link to Persona 33

--Execute Enterprise.ListRightsAssociatedWithRoles 10639, 24

DECLARE @VisibilityStatus INT  
DECLARE @RightTypeId INT         

			
SET @VisibilityStatus = NULL
SELECT @VisibilityStatus = StatusType.StatusTypeID
         FROM Enterprise.StatusTypeCategoryType
              JOIN Enterprise.StatusTypeCategory ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
              JOIN Enterprise.StatusTypeCategoryClassification ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
              JOIN Enterprise.StatusType ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
         WHERE StatusType.name = 'ALL'
               AND StatusTypeCategoryType.Name = 'Security';

UPDATE Enterprise.RightValueType
SET ProductId = @ProductId,
	VisibilityStatusId = @VisibilityStatus
WHERE  Value IN 
	(
	SELECT Name FROM #Right
	)
	GO

	--Prodcut Settings
	

SET NOCOUNT ON;


DECLARE @ProductId INT , 
		 @CurrentProductConfigurationID INT , 
		 @ProductSettingTypeId INT , 
		 @ProductSettingId INT , 
		 @NotificationEmailRequiredForUserWithNoEmail INT , 
		 @LockOnProductAccess INT;

IF
(
	SELECT 1
	FROM Enterprise.ProductSettingType
	WHERE Name = 'NotificationEmailRequiredForUserWithNoEmail'
) IS NULL
BEGIN
	EXEC Enterprise.CreateProductSettingType 'NotificationEmailRequiredForUserWithNoEmail', 'Does Notification Email is required for Regular User (No Email) by product.', @ProductSettingTypeId OUTPUT;
END;

IF
(
	SELECT 1
	FROM Enterprise.ProductSettingType
	WHERE Name = 'LockOnProductAccess'
) IS NULL
BEGIN
	EXEC Enterprise.CreateProductSettingType 'LockOnProductAccess', 'Lock selection on product access.', @ProductSettingTypeId OUTPUT;
END;

DECLARE @ProductSetting TABLE
( 
 ProductId int, NotificationEmailRequiredForUserWithNoEmail int, LockOnProductAccess int
);

INSERT INTO @ProductSetting( ProductId, NotificationEmailRequiredForUserWithNoEmail, LockOnProductAccess )
VALUES	( 1, 0, 0 ), -- OneSite
		( 3, 0, 1 ), -- Unified Platform
		( 4, 0, 0 ), -- Asset Optimization
		( 6, 0, 0 ), -- Lead2Lease
		( 7, 0, 0 ), -- NOT USED
		( 8, 0, 0 ), -- RealPage Accounting
		( 9, 0, 0 ), -- Marketing Center
		( 10, 0, 0 ), -- Prospect Contact Center
		( 13, 0, 0 ), -- Spend Management
		( 14, 0, 0 ), -- Client Portal
		( 15, 1, 0 ), -- Renters Insurance
		( 16, 0, 0 ), -- Vendor Services
		( 17, 1, 0 ), -- Resident Portals
		( 18, 0, 0 ), -- Utility Management
		( 19, 0, 0 ), -- Product Learning Portal
		( 20, 0, 0 ), -- RealPage Document Management
		( 21, 0, 0 ), -- OneSite Conversions
		( 23, 0, 0 ), -- On-Site
		( 24, 0, 0 ), -- Research Application
		( 25, 0, 0 ), -- Self-provisioning portal
		( 26, 0, 0 ), -- Unified Amenities
		( 27, 0, 0 ), -- Migration Tool Application
		( 28, 0, 0 ), -- Product Updates
		( 29, 0, 0 ), -- Business Intelligence
		( 30, 0, 0 ), -- Performance Analytics
		( 31, 0, 0 ), -- Investment Analytics
		( 32, 0, 0 ), -- Revenue Management
		( 33, 0, 0 ), -- Axiometrics
		( 34, 0, 0 ), -- Benchmarking
		( 35, 0, 0 ), -- Support Tool
		( 36, 0, 1 ), -- EasyLMS
		( 37, 0, 0 ), -- PropertyPhotos
		( 38, 0, 0 ); -- VendorMarketplace


DECLARE @NOW datetime= GETUTCDATE();

DECLARE Products CURSOR
FOR SELECT ProductId, NotificationEmailRequiredForUserWithNoEmail, LockOnProductAccess
	FROM @ProductSetting;

OPEN Products;

FETCH Products INTO @ProductId, @NotificationEmailRequiredForUserWithNoEmail, @LockOnProductAccess;

WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT TOP 1 @CurrentProductConfigurationID = ConfigurationId
	FROM Enterprise.GlobalProductConfiguration AS gpc
	WHERE gpc.ProductId = @ProductId AND 
		  ( ( @NOW BETWEEN gpc.FromDate AND gpc.ThruDate
			) OR 
			( @NOW >= gpc.FromDate AND 
			  gpc.ThruDate IS NULL
			)
		  )
	ORDER BY GlobalProductConfigurationId DESC;

	
	--- NotificationEmailRequiredForUserWithNoEmail 
	SET @ProductSettingTypeId = NULL;
	EXEC Enterprise.GetProductSettingType @Name = 'NotificationEmailRequiredForUserWithNoEmail', @ProductSettingTypeId = @ProductSettingTypeId OUTPUT;

	IF @ProductSettingTypeId IS NOT NULL AND 
	   NOT EXISTS
	(
		SELECT TOP 1 1
		FROM Enterprise.ProductSetting
		WHERE ProductID = @productId AND 
			  ProductSettingTypeId = @ProductSettingTypeId AND 
			  ThruDate IS NULL
	)
	BEGIN
	
		-- Create the Value and assign it to the Product and ProductSettingType
		EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
		@ProductSettingTypeId = @ProductSettingTypeId, -- int
		@Value = @NotificationEmailRequiredForUserWithNoEmail, 
		@FromDate = @NOW, -- datetime
		@ThruDate = NULL, -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT; -- int

		-- Link the Product Setting to an actual configuration
		EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @CurrentProductConfigurationID, -- int
		@ProductSettingId = @ProductSettingId, -- int
		@FromDate = @NOW, -- datetime
		@ThruDate = NULL;   -- datetime
	END;
	--- NotificationEmailRequiredForUserWithNoEmail 

	--- LockOnProductAccess 
	SET @ProductSettingTypeId = NULL;
	EXEC Enterprise.GetProductSettingType @Name = 'LockOnProductAccess', @ProductSettingTypeId = @ProductSettingTypeId OUTPUT;

	IF @ProductSettingTypeId IS NOT NULL AND 
	  
	   NOT EXISTS
	(
		SELECT TOP 1 1
		FROM Enterprise.ProductSetting
		WHERE ProductID = @productId AND 
			  ProductSettingTypeId = @ProductSettingTypeId AND 
			  ThruDate IS NULL
	)
	BEGIN
		
		-- Create the Value and assign it to the Product and ProductSettingType
		EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
		@ProductSettingTypeId = @ProductSettingTypeId, -- int
		@Value = @LockOnProductAccess, -- nvarchar(1000)
		@FromDate = @NOW, -- datetime
		@ThruDate = NULL, -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT; -- int

		-- Link the Product Setting to an actual configuration
		EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @CurrentProductConfigurationID, -- int
		@ProductSettingId = @ProductSettingId, -- int
		@FromDate = @NOW, -- datetime
		@ThruDate = NULL;   -- datetime
	END;
	--- LockOnProductAccess




	FETCH Products INTO @ProductId, @NotificationEmailRequiredForUserWithNoEmail, @LockOnProductAccess;
END;
GO

--Settings and Support Tools
DECLARE @ResendInvite int;
DECLARE @Lock INT
DECLARE @OrgRowNum INT;
DECLARE @ActionID INT;
DECLARE @RightID INT;
DECLARE @RoleID INT;
DECLARE @Status INT;
DECLARE @ActionValueID INT;
DECLARE @OrgID INT;
DECLARE @ProductID INT;
DECLARE @ParentActionId INT;
DECLARE @UserActionId INT;
DECLARE @RightCategoryId INT;
DECLARE @PartyId INT;
DECLARE @RightName VARCHAR(100);
DECLARE @RVT INT;
DECLARE @DefaultRoute NVARCHAR(200);
DECLARE @RightValueTypeId INT;
DECLARE @StatusId INT;
DECLARE @PersonaId INT;
DECLARE @FromDate DATETIME;
DECLARE @TRoleId INT;
DECLARE @TRoleName NVARCHAR(500);
DECLARE @TRoleDesc NVARCHAR(500);
DECLARE @TRightId INT;
DECLARE @TRightName NVARCHAR(500);
DECLARE @TRightDesc NVARCHAR(500);
DECLARE @RightCategory INT;
DECLARE @RoleCategory INT;
DECLARE @RoleName NVARCHAR(500);
DECLARE @RoleTypeID INT;
DECLARE @PerosonaP INT;
DECLARE @PartyRowNum INT;


SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Unified Platform';

SELECT @ActionValueID = [ActionValueTypeID]
FROM Enterprise.ActionValueType
WHERE Value = 'Right';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Settings' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Settings', @ActionTarget = N'Route', @ActionbValueTypeId = @ActionValueID, @Description = 'SupportRoute Route.', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;



SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = 'SideMenu' AND 
	  ObjectType = 'Route' AND 
	  Description = 'SuperUser';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Settings' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Settings', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Settings' AND Description = 'SuperUser' AND OBjectType = 'Route'
		 AND ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Settings', @ActionTarget = N'Route', @ActionbValueTypeId = @ActionValueID, @Description = 'Superuser', @ActionID = @ActionID OUTPUT;
	SELECT @ParentActionId = @ActionID;
END;


IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Settings' AND ObjectType = 'Right' AND
		  ParentActionID IS NULL
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Settings', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;


IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Manage Unified Settings' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Manage Unified Settings', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'View Unified Settings' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'View Unified Settings', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;


IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Manage Unified Settings' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Manage Unified Settings', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = 'Edit Other User Profile', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'View Unified Settings' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'View Unified Settings', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = 'Edit Other User Profile', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;





IF OBJECT_ID('tempdb..#HoldOrgsEditOthersProfile') IS NOT NULL
BEGIN
	DROP TABLE #HoldOrgsEditOthersProfile;
END;

SELECT 
	DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldOrgsEditOthersProfile
FROM Person.Persona PE
INNER JOIN Enterprise.Organization O
ON PE.OrganizationPartyId = O.Partyid
WHERE O.Name = 'RealPage Employee';-- WHERE Person.Persona.OrganizationPartyId = 353

SELECT @Status = StatusType.StatusTypeID
FROM Enterprise.StatusTypeCategoryType
	 JOIN
	 Enterprise.StatusTypeCategory
	 ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification
	 ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType
	 ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
WHERE StatusType.name = 'Internal Only' AND 
	  StatusTypeCategoryType.Name = 'Security';

SELECT TOP 1 @RightCategoryId = StatusType.StatusTypeID
FROM Enterprise.StatusTypeCategoryType
	 JOIN
	 Enterprise.StatusTypeCategory
	 ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification
	 ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType
	 ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
WHERE StatusType.name = 'System' AND 
	  StatusTypeCategory.Name = 'Right Type' AND 
	  StatusTypeCategoryType.Name = 'Security';


WHILE EXISTS
(
	SELECT 1
	FROM #HoldOrgsEditOthersProfile
	WHERE PStatus = 0
)
BEGIN
	SELECT TOP 1 @OrgRowNum = Rownumber, @OrgID = OrganizationPartyID
	FROM #HoldOrgsEditOthersProfile
	WHERE PStatus = 0;

	SELECT @RoleId = RoleId
		FROM Enterprise.Role AS R
			 INNER JOIN
			 Enterprise.RoleValueType AS RR
			 ON RR.RoleValueTypeId = R.RoleValueTypeId
		WHERE RR.Value = 'User Administrator' AND 
			  R.PartyId = @OrgId;

		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = 'View Unified Settings', @ShortName = 'ViewUnifiedSettings', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'Access to Unified Platform Login via Support Tool', @RightId = @RightId OUTPUT;
		SELECT @ActionID = ActionID FROM Enterprise.Action WHERE ObjectValue = 'Settings' and ObjectType = 'ROUTE' and Description = 'SuperUser'
		SELECT @RightID = RightId FROM Enterprise.[Right] R
		INNER JOIN Enterprise.RightValueType RR on RR.RightValueTypeId = R.RightValueTypeId
			 WHERE Value = 'View Unified Settings' and RoleId = @RoleID
		EXEC	[Enterprise].[LinkActionToRights]  @ActionID = @ActionID, @RightId = @RightId,	@StatusId = @Status,	@UserActionId = @UserActionId OUTPUT

		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = 'Ability to manage Unified Settings', @ShortName = 'ManageUnifiedSettings', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'Ability to manage settings for Unified Platform', @RightId = @RightId OUTPUT;
		SELECT @ActionID = ActionID FROM Enterprise.Action WHERE ObjectValue = 'Settings' and ObjectType = 'ROUTE' and Description = 'SuperUser'
		SELECT @RightID = RightId FROM Enterprise.[Right] R
		INNER JOIN Enterprise.RightValueType RR on RR.RightValueTypeId = R.RightValueTypeId
			 WHERE Value = 'Access to Unified Settings via Support Tool' and RoleId = @RoleID
		EXEC	[Enterprise].[LinkActionToRights]  @ActionID = @ActionID, @RightId = @RightId,	@StatusId = @Status,	@UserActionId = @UserActionId OUTPUT


		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_SettingsRight', @ShortName = 'SettingsRight', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = '', @RightId = @RightId OUTPUT;
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Settings' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_SettingsRoute', @ShortName = 'SettingsRoute', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = '', @RightId = @RightId OUTPUT;
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Settings' AND 
			  ObjectType = 'Route' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;


		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ManageUnifiedSetting', @ShortName = 'ManageUnifiedSetting', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = '', @RightId = @RightId OUTPUT;
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Manage Unified Settings' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
		
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ViewUnifiedSetting', @ShortName = 'ViewUnifiedSetting', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = '', @RightId = @RightId OUTPUT;
		
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'View Unified Settings' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
		UPDATE #HoldOrgsEditOthersProfile
		  SET PStatus = 1
		WHERE RowNumber = @OrgRowNum;
	END;

GO
DECLARE @RightValueTypeId INT
DECLARE @SettingssRoute int;
DECLARE @EmployeeAccess INT
DECLARE @ManageUnifiedSettings INT
DECLARE @ViewUnifiedSettings INT
DECLARE @DashBoard INT
DECLARE @SettingsRight INT

SELECT  @DashBoard = RightValueTypeId
FROM Enterprise.RightValueType 
WHERE value = 'DefaultSidemenu_Users';


SELECT @SettingssRoute = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_SettingsRoute';

SELECT @ManageUnifiedSettings = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_ManageUnifiedSetting'

SELECT @ViewUnifiedSettings = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_ViewUnifiedSetting'

SELECT  @SettingsRight = RightValueTypeId
FROM Enterprise.RightValueType 
WHERE value = 'Default_SettingsRight';



SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'View Unified Settings');

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @SettingssRoute
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @SettingssRoute );
END;




IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @ViewUnifiedSettings
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @ManageUnifiedSettings );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @SettingsRight
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @SettingsRight );
END;

----->

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Ability to manage Unified Settings');

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @SettingssRoute
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @SettingssRoute );
END;



IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @ManageUnifiedSettings
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @ManageUnifiedSettings );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @SettingsRight
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @SettingsRight );
END;

GO
UPDATE Enterprise.RightValueType SET VisibilityStatusId = 11 , ProductId = 3 
where Value In (N'Default_ActivityLogRight', N'Default_ActivityLogRoute', N'Default_SettingsRight', N'Default_SettingsRoute')
GO
UPDATE enterprise.ACTION
  SET
      actionvaluetypeid = 1
WHERE ActionValueTypeId <> 1;

GO
DECLARE @RightValueTypeId INT;
SELECT @RightValueTypeId = RIghtValueTYpeId
FROM enterprise.rightvaluetype
WHERE VALUE LIKE 'Access to Unified Settings via Support tool';
IF EXISTS
(
    SELECT 1
    FROM Enterprise.[Right]
    WHERE RightValueTypeId = @RightValueTypeId
)
    BEGIN
        DELETE FROM Enterprise.[Right]
        WHERE RightValueTypeId = @RightValueTypeId;
        DELETE FROM Enterprise.RightDependency
        WHERE RIghtValueTypeId = @RightValueTypeId
              OR DependentRightValueTypeId = @RightValueTypeId;
        DELETE FROM Enterprise.RIghtValueType
        WHERE RightValueTYpeId = @RightValueTypeId;
    END;
GO

--activity Log route
DECLARE @ResendInvite int;
DECLARE @Lock INT
DECLARE @OrgRowNum INT;
DECLARE @ActionID INT;
DECLARE @RightID INT;
DECLARE @RoleID INT;
DECLARE @Status INT;
DECLARE @ActionValueID INT;
DECLARE @OrgID INT;
DECLARE @ProductID INT;
DECLARE @ParentActionId INT;
DECLARE @UserActionId INT;
DECLARE @RightCategoryId INT;
DECLARE @PartyId INT;
DECLARE @RightName VARCHAR(100);
DECLARE @RVT INT;
DECLARE @DefaultRoute NVARCHAR(200);
DECLARE @RightValueTypeId INT;
DECLARE @StatusId INT;
DECLARE @PersonaId INT;
DECLARE @FromDate DATETIME;
DECLARE @TRoleId INT;
DECLARE @TRoleName NVARCHAR(500);
DECLARE @TRoleDesc NVARCHAR(500);
DECLARE @TRightId INT;
DECLARE @TRightName NVARCHAR(500);
DECLARE @TRightDesc NVARCHAR(500);
DECLARE @RightCategory INT;
DECLARE @RoleCategory INT;
DECLARE @RoleName NVARCHAR(500);
DECLARE @RoleTypeID INT;
DECLARE @PerosonaP INT;
DECLARE @PartyRowNum INT;


SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Unified Platform';

SELECT @ActionValueID = 1

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'ActivityLog' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'ActivityLog', @ActionTarget = N'Route', @ActionbValueTypeId = @ActionValueID, @Description = 'SupportRoute Route.', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;



SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = 'SideMenu' AND 
	  ObjectType = 'Route' AND 
	  Description = 'SuperUser';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Activity Log' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Activity Log', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'ActivityLog' AND Description = 'SuperUser' AND OBjectType = 'Route'
		 AND ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'ActivityLog', @ActionTarget = N'Route', @ActionbValueTypeId = @ActionValueID, @Description = 'Superuser', @ActionID = @ActionID OUTPUT;
	SELECT @ParentActionId = @ActionID;
END;


IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Activity Log' AND ObjectType = 'Right' AND
		  ParentActionID IS NULL
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Activity Log', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;


IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'View Audit Trail' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'View Audit Trail', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;


IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'View Audit Trail' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'View Audit Trail', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = 'Edit Other User Profile', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;


IF OBJECT_ID('tempdb..#HoldOrgs') IS NOT NULL
BEGIN
	DROP TABLE #HoldOrgs;
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldOrgs
FROM Person.Persona;-- WHERE Person.Persona.OrganizationPartyId = 353

SELECT @Status = StatusType.StatusTypeID
FROM Enterprise.StatusTypeCategoryType
	 JOIN
	 Enterprise.StatusTypeCategory
	 ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification
	 ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType
	 ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
WHERE StatusType.name = 'ALL' AND 
	  StatusTypeCategoryType.Name = 'Security';

SELECT @RightCategoryId = StatusType.StatusTypeID
FROM Enterprise.StatusTypeCategoryType
	 JOIN
	 Enterprise.StatusTypeCategory
	 ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification
	 ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType
	 ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
WHERE StatusType.name = 'System' AND 
	  StatusTypeCategory.Name = 'Right Type' AND 
	  StatusTypeCategoryType.Name = 'Security';






WHILE EXISTS
(
	SELECT 1
	FROM #HoldOrgs
	WHERE PStatus = 0
)
BEGIN
	SELECT TOP 1 @OrgRowNum = Rownumber, @OrgID = OrganizationPartyID
	FROM #HoldOrgs
	WHERE PStatus = 0;

	SELECT @RoleId = RoleId
		FROM Enterprise.Role AS R
			 INNER JOIN
			 Enterprise.RoleValueType AS RR
			 ON RR.RoleValueTypeId = R.RoleValueTypeId
		WHERE RR.Value = 'User Administrator' AND 
			  R.PartyId = @OrgId;

		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = 'Ability to view audit trail on user data', @ShortName = 'ViewAuditTrailUserData', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'Ability to view audit trail on user data', @RightId = @RightId OUTPUT;
		SELECT @ActionID = ActionID FROM Enterprise.Action WHERE ObjectValue = 'ActivityLog' and ObjectType = 'ROUTE' and Description = 'SuperUser'
		SELECT @RightID = RightId FROM Enterprise.[Right] R
		INNER JOIN Enterprise.RightValueType RR on RR.RightValueTypeId = R.RightValueTypeId
			 WHERE Value = 'Ability to view audit trail on user data' and RoleId = @RoleID
		EXEC	[Enterprise].[LinkActionToRights]  @ActionID = @ActionID, @RightId = @RightId,	@StatusId = @Status,	@UserActionId = @UserActionId OUTPUT


		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ActivityLogRight', @ShortName = 'ActivityLogRight', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = '', @RightId = @RightId OUTPUT;
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Activity Log' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ActivityLogRoute', @ShortName = 'ActivityLogRoute', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = '', @RightId = @RightId OUTPUT;
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'ActivityLog' AND 
			  ObjectType = 'Route' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ViewAuditTrailData', @ShortName = 'ViewAuditTrailData', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = '', @RightId = @RightId OUTPUT;
		
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'View Audit Trail' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
		UPDATE #HoldOrgs
		  SET PStatus = 1
		WHERE RowNumber = @OrgRowNum;
	END;

GO
DECLARE @RightValueTypeId INT
DECLARE @ActivityLogsRoute int;
DECLARE @ViewAuditTrail INT
DECLARE @Sidemenu INT
DECLARE @ActivityLogRight INT

SELECT  @Sidemenu = RightValueTypeId
FROM Enterprise.RightValueType 
WHERE value = 'Default_SideMenu_People';


SELECT @ActivityLogsRoute = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_ActivityLogRoute';

SELECT @ViewAuditTrail = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_ViewAuditTrailData'

SELECT  @ActivityLogRight = RightValueTypeId
FROM Enterprise.RightValueType 
WHERE value = 'Default_ActivityLogRight';



SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Ability to view audit trail on user data');

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @ActivityLogsRoute
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @ActivityLogsRoute );
END;



IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @ViewAuditTrail
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @ViewAuditTrail );
END;



IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @ActivityLogRight
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @ActivityLogRight );
END;

----->
GO  
DECLARE @VisibilityStatus INT  
DECLARE @RightTypeId INT         
		
SET @VisibilityStatus = NULL
SELECT @VisibilityStatus = StatusType.StatusTypeID
         FROM Enterprise.StatusTypeCategoryType
              JOIN Enterprise.StatusTypeCategory ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
              JOIN Enterprise.StatusTypeCategoryClassification ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
              JOIN Enterprise.StatusType ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
         WHERE StatusType.name = 'Internal Only'
               AND StatusTypeCategoryType.Name = 'Security';

UPDATE Enterprise.RightValueType
SET ProductId = 3,
	VisibilityStatusId = @VisibilityStatus
WHERE  value = 'View only access to Unified Platform from Support Tool'
GO



DECLARE @VisibilityStatus INT  
DECLARE @RightTypeId INT         
		
SET @VisibilityStatus = NULL
SELECT @VisibilityStatus = StatusType.StatusTypeID
         FROM Enterprise.StatusTypeCategoryType
              JOIN Enterprise.StatusTypeCategory ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
              JOIN Enterprise.StatusTypeCategoryClassification ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
              JOIN Enterprise.StatusType ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
         WHERE StatusType.name = 'ALL'
               AND StatusTypeCategoryType.Name = 'Security';

UPDATE Enterprise.RightValueType
SET ProductId = 3,
	VisibilityStatusId = @VisibilityStatus
WHERE  value In (N'Default_ViewOnlySupportToolAccess', N'Ability to edit password', N'Default_SettingsRight', N'Default_ActivityLogRight', N'Default_ActivityLogRoute', N'Default_OneSiteConversions', N'Access to Leasing & Rents Conversion Tool for OneSite users')
GO
--Migration tool settings
	DECLARE @ProductConfiguration AS ProductConfigurationType;
	DECLARE @ProductID INT;
	DECLARE @LoginURI NVARCHAR(100);
	DECLARE @SigningCertificateThumbprint NVARCHAR(50);
 
	INSERT INTO @ProductConfiguration values ('GetListUsersEndpoint',    'List users End point used by Migration tool', '/api/{0}/users?filter={1}&startRow={2}&resultsperpage={3}')
	INSERT INTO @ProductConfiguration values ('PatchMigrateUsersEndpoint',     'Migrate users End point', '/api/{0}/migrate-users')
	INSERT INTO @ProductConfiguration values ('PutMigrateUserEndpoint',     'Migrate users End point', '{0}/migrate-users')

 
 
	SET @ProductID = 40
	SET @LoginURI = 'http://ilmbeta.slopejet.com/sjilm'
	SET @SigningCertificateThumbprint = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'
 
 
	EXEC Enterprise.ProductConfigurationSetup 
		   @ProductId,
		   @LoginURI,
		   @SigningCertificateThumbprint,
		   @ProductConfiguration
 
	GO
	--------------------------------------------------------------------------------------------------------------------------------------------------------
	DECLARE @ProductConfiguration AS ProductConfigurationType;
	DECLARE @ProductID INT;
	DECLARE @LoginURI NVARCHAR(100);
	DECLARE @SigningCertificateThumbprint NVARCHAR(50);
 
	INSERT INTO @ProductConfiguration values ('GetListUsersEndpoint',    'List users End point used by Migration tool', '/api/{0}/users?filter={1}&startRow={2}&resultsperpage={3}')
	INSERT INTO @ProductConfiguration values ('PatchMigrateUsersEndpoint',     'Migrate users End point', '/api/{0}/migrate-users')
	INSERT INTO @ProductConfiguration values ('PutMigrateUserEndpoint',     'Migrate users End point', '{0}/migrate-users')
 
	SET @ProductID = 41
	SET @LoginURI = 'http://ilmbeta.slopejet.com/sjila'
	SET @SigningCertificateThumbprint = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'
 
 
	EXEC Enterprise.ProductConfigurationSetup 
		   @ProductId,
		   @LoginURI,
		   @SigningCertificateThumbprint,
		   @ProductConfiguration

go
-- New RP System ROle

DECLARE @OrganizationId INT;
DECLARE @RoleId INT;
DECLARE @OrgRowNum INT;
DECLARE @PerRowNum INT;
DECLARE @PerPriv INT;
DECLARE @RoleName VARCHAR(200);
DECLARE @RightID INT;
DECLARE @ActionID INT;
DECLARE @Status INT;
DECLARE @UserActionID INT;
DECLARE @PersonRoleID INT;
DECLARE @Status_Role INT;
DECLARE @Status_Right INT;
DECLARE @HoldUserId INT;
IF OBJECT_ID('tempdb..#HoldOrgs') IS NULL
    BEGIN
        CREATE TABLE #HoldOrgs
(RowNumber           INT IDENTITY(1, 1),
 OrganizationPartyID INT,
 PStatus             BIT DEFAULT 0
);
    END;
BEGIN
    SELECT @Status_Right = ST.StatusTypeId
    FROM Enterprise.StatusTypeCategoryType AS STCT
         JOIN Enterprise.StatusTypeCategory AS STC ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
         JOIN Enterprise.StatusTypeCategoryClassification AS STCC ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
         JOIN Enterprise.StatusType AS ST ON ST.StatusTypeId = STCC.StatusTypeId
    WHERE STC.Name = 'Right Type'
          AND ST.Name = 'System';
    SELECT @Status_Role = ST.StatusTypeId
    FROM Enterprise.StatusTypeCategoryType AS STCT
         JOIN Enterprise.StatusTypeCategory AS STC ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
         JOIN Enterprise.StatusTypeCategoryClassification AS STCC ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
         JOIN Enterprise.StatusType AS ST ON ST.StatusTypeId = STCC.StatusTypeId
    WHERE STC.Name = 'Role Type'
          AND ST.Name = 'System';
    INSERT INTO #HoldOrgs(OrganizationPartyID, PStatus)
           SELECT DISTINCT
                  OrganizationPartyID, 0
           FROM Person.Persona AS P
                INNER JOIN Enterprise.Organization AS O ON P.OrganizationPartyId = O.PartyId;
    WHILE EXISTS
	(
		SELECT 1
		FROM #HoldOrgs
		WHERE PStatus = 0
	)
        BEGIN
            SELECT TOP 1 @OrganizationId = OrganizationPartyID,
                         @OrgRowNum = RowNumber
            FROM #HoldOrgs
            WHERE PStatus = 0;
            IF NOT EXISTS
			(
				SELECT 1
				FROM Enterprise.Role AS R
					 INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
				WHERE value = 'Read only for Unified Platform'
					  AND PartyID = @OrganizationId
			)
                BEGIN
                    EXEC Enterprise.CreateRole
                         @RoleName = N'Read only for Unified Platform',
                         @ShortName = 'ROForUnifiedPlatform',
                         @Description = N'Read only for Unified Platform',
                         @RoleTypeID = 402,
                         @RoleCategoryId = @Status_Role,
                         @PartyID = @OrganizationId,
                         @RoleID = @RoleID OUTPUT;
                    SET @RoleName = 'Read only for Unified Platform';
                    
					SELECT @RoleID = RoleId
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = @RoleName
                          AND PartyId = @OrganizationId;
                    EXECUTE Enterprise.CreateRight
                            @RoleId = @RoleId,
                            @PartyId = @OrganizationId,
                            @ProductId = 3,
                            @RightName = 'Ability to view users',
                            @RightCategoryId = @Status_Right,
                            @RightID = @RightID OUTPUT,
                            @Description = '';
                    SELECT @RightId;
                    

                    
					EXECUTE Enterprise.CreateRight
                            @RoleId = @RoleId,
                            @PartyId = @OrganizationId,
                            @ProductId = 3,
                            @RightName = 'Ability to edit my own profile',
                            @RightCategoryId = @Status_Right,
                            @RightID = @RightID OUTPUT,
                            @Description = '';
                    SELECT @RightId;
                    EXECUTE Enterprise.CreateRight
                            @RoleId = @RoleId,
                            @PartyId = @OrganizationId,
                            @ProductId = 3,
                            @RightName = 'Ability to view roles and rights',
                            @RightCategoryId = @Status_Right,
                            @RightID = @RightID OUTPUT,
                            @Description = '';
                    SELECT @RightId;
					

                    EXECUTE Enterprise.CreateRight
                            @RoleId = @RoleId,
                            @PartyId = @OrganizationId,
                            @ProductId = 3,
                            @RightName = 'Access to Product Learning Portal',
                            @RightCategoryId = @Status_Right,
                            @RightID = @RightID OUTPUT,
                            @Description = '';
                    SELECT @RightId;
                    EXECUTE Enterprise.CreateRight
                            @RoleId = @RoleId,
                            @PartyId = @OrganizationId,
                            @ProductId = 3,
                            @RightName = 'Ability to view audit trail on user data',
                            @RightCategoryId = @Status_Right,
                            @RightID = @RightID OUTPUT,
                            @Description = '';
                    SELECT @RightId;
                    EXECUTE Enterprise.CreateRight
                            @RoleId = @RoleId,
                            @PartyId = @OrganizationId,
                            @ProductId = 3,
                            @RightName = 'Default_SideMenu_Users',
                            @RightCategoryId = @Status_Right,
                            @RightID = @RightID OUTPUT,
                            @Description = '';
                    SELECT @RightId;
                    EXECUTE Enterprise.CreateRight
                            @RoleId = @RoleId,
                            @PartyId = @OrganizationId,
                            @ProductId = 3,
                            @RightName = 'Default_Dashboard_Users',
                            @RightCategoryId = @Status_Right,
                            @RightID = @RightID OUTPUT,
                            @Description = '';
                    SELECT @RightId;
                    
					SELECT @ActionID = ActionID
                    FROM Enterprise.ACTION
                    WHERE ObjectValue = 'SideMenu'
                          AND ObjectType = 'ROUTE'
                          AND Description = 'User'
                    SELECT @RoleID = RoleID
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = 'Read only for Unified Platform'
                          AND partyid = @OrganizationId;
                    SELECT @RightID = RightId
                    FROM Enterprise.[Right] AS R
                         INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                    WHERE Value = 'Default_SideMenu_Users'
                          AND RoleId = @RoleID;
                    EXEC Enterprise.LinkActionToRights
                         @ActionID = @ActionID,
                         @RightId = @RightId,
                         @StatusId = @Status_Right,
                         @UserActionId = @UserActionId OUTPUT;

                    SELECT @ActionID = ActionID
                    FROM Enterprise.ACTION
                    WHERE ObjectValue = 'Dashboard'
                          AND ObjectType = 'ROUTE'
                          AND Description = 'User';
                    SELECT @RoleID = RoleID
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = 'Read only for Unified Platform'
                          AND partyid = @OrganizationId;
                    SELECT @RightID = RightId
                    FROM Enterprise.[Right] AS R
                         INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                    WHERE Value = 'Default_dashboard_Users'
                          AND RoleId = @RoleID;
                    EXEC Enterprise.LinkActionToRights
                         @ActionID = @ActionID,
                         @RightId = @RightId,
                         @StatusId = @Status_Right,
                         @UserActionId = @UserActionId OUTPUT;
						 
					SELECT @ActionID = ActionID
                    FROM Enterprise.ACTION
                    WHERE ObjectValue = 'EditUser'
                          AND ObjectType = 'ROUTE'
                          AND Description = 'Edit User Route'
						  AND ParentActionId IS NULL
 					SELECT @RoleID = RoleID
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = 'Read only for Unified Platform'
                          AND partyid = @OrganizationId;
                    SELECT @RightID = RightId
                    FROM Enterprise.[Right] AS R
                         INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                    WHERE Value = 'Ability to edit my own profile'
                          AND RoleId = @RoleID;
					EXEC Enterprise.LinkActionToRights
                         @ActionID = @ActionID,
                         @RightId = @RightId,
                         @StatusId = @Status_Right,
                         @UserActionId = @UserActionId OUTPUT;

					SELECT @ActionID = ActionID
                    FROM Enterprise.ACTION
                    WHERE ObjectValue = 'UsersList'
                          AND ObjectType = 'ROUTE'
                          AND Description = 'User';
                    SELECT @RoleID = RoleID
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = 'Read only for Unified Platform'
                          AND partyid = @OrganizationId;
                    SELECT @RightID = RightId
                    FROM Enterprise.[Right] AS R
                         INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                    WHERE Value = 'Ability to view users'
                          AND RoleId = @RoleID;
					EXEC Enterprise.LinkActionToRights
                         @ActionID = @ActionID,
                         @RightId = @RightId,
                         @StatusId = @Status_Right,
                         @UserActionId = @UserActionId OUTPUT;

					SELECT @ActionId = ActionID
                    FROM Enterprise.ACTION
                    WHERE ObjectValue = 'EditUser'
                          AND ObjectType = 'ROUTE'
                          AND Description = 'Edit User Route';
                    SELECT @RoleID = RoleID
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = 'Read only for Unified Platform'
                          AND partyid = @OrganizationId;
                    SELECT @RightID = RightId
                    FROM Enterprise.[Right] AS R
                         INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                    WHERE Value = 'Ability to view audit trail on user data'
                          AND RoleId = @RoleID;
					EXEC Enterprise.LinkActionToRights
                         @ActionID = @ActionID,
                         @RightId = @RightId,
                         @StatusId = @Status_Right,
                         @UserActionId = @UserActionId OUTPUT;

					SELECT @ActionId = ActionID
                    FROM Enterprise.ACTION
                    WHERE ObjectValue = 'RolesAndRights'
                          AND ObjectType = 'ROUTE'
                          AND Description = 'User';
                    SELECT @RoleID = RoleID
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = 'Read only for Unified Platform'
                          AND partyid = @OrganizationId;
                    SELECT @RightID = RightId
                    FROM Enterprise.[Right] AS R
                         INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                    WHERE Value = 'Ability to view roles and rights'
                          AND RoleId = @RoleID;
					EXEC Enterprise.LinkActionToRights
                         @ActionID = @ActionID,
                         @RightId = @RightId,
                         @StatusId = @Status_Right,
                         @UserActionId = @UserActionId OUTPUT;
						
					SELECT @ActionId = ActionID
                    FROM Enterprise.ACTION
                    WHERE ObjectValue = 'Dashboard'
                          AND ObjectType = 'ROUTE'
                          AND Description = 'User';
                    SELECT @RoleID = RoleID
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = 'Read only for Unified Platform'
                          AND partyid = @OrganizationId;
                    SELECT @RightID = RightId
                    FROM Enterprise.[Right] AS R
                         INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                    WHERE Value = 'Access to Product Learning Portal'
                          AND RoleId = @RoleID;
					EXEC Enterprise.LinkActionToRights
                         @ActionID = @ActionID,
                         @RightId = @RightId,
                         @StatusId = @Status_Right,
                         @UserActionId = @UserActionId OUTPUT;

                END;
            UPDATE #HoldOrgs
              SET
                  PStatus = 1
            WHERE RowNumber = @OrgRowNum;
        END;
END;

GO