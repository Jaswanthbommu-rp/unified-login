CREATE PROCEDURE [Person].[ListUsersWithCompanyId_Ver3] 
(@CompanyId   INT, 
 @Source      NVARCHAR(50)  = 'BlueBook', 
 @ProductId   NVARCHAR(200) = NULL, 
 @RowsPerPage INT           = 0, 
 @PageNumber  INT           = 1,
 @Roles		  NVARCHAR(1000) = NULL,
 @Rights	  NVARCHAR(1000) = NULL,
 @PropertyId  NVARCHAR(100) = NULL
)AS
BEGIN
	
	DECLARE @Now DATETIME= GETUTCDATE();
	DECLARE @RoleList TABLE(RoleShortName NVARCHAR(255));
	DECLARE @RightList TABLE(RightName NVARCHAR(255));
	DECLARE @ProductCount INT= 1;
	DECLARE @RoleCount INT= 1;
	DECLARE @RightCount INT= 1;
	DECLARE @OrganizationPartyId BIGINT
	DECLARE @ProductIds Enterprise.ProductIdType

	DECLARE @ProductsList2 TABLE
	(
		PersonaId			BIGINT,
		ProductId			INT,
		TargetProductId		INT
	)

	CREATE TABLE #UserList
	(
		UserId        BIGINT, 
		LoginName     NVARCHAR(255), 
		FirstName     NVARCHAR(50), 
		LastName      NVARCHAR(50), 
		AddressString NVARCHAR(255),
		PersonaId     BIGINT
	);

	INSERT INTO @ProductIds(ProductId)
    (
		SELECT *
		FROM STRING_SPLIT(@ProductId, ',')
		WHERE value IN ('45','39','26','56','3')
	);

	INSERT INTO @RoleList(RoleShortName)
    (
        SELECT *
        FROM STRING_SPLIT(@Roles, ',')
    );

	INSERT INTO @RightList(RightName)
    (
        SELECT *
        FROM STRING_SPLIT(@Rights, ',')
    );

	IF (SELECT COUNT(*)
        FROM @ProductIds) = 0
	BEGIN
		SET @ProductCount = NULL;
    END;

	IF(SELECT COUNT(*)
        FROM @RoleList) = 0
    BEGIN
		SET @RoleCount = NULL;
    END;

	IF(SELECT COUNT(*)
        FROM @RightList) = 0
    BEGIN
		SET @RightCount = NULL;
    END;

    SELECT 
		@RowsPerPage = CASE
						WHEN @RowsPerPage <= 0
                        THEN 2147483647
                        ELSE @RowsPerPage
    END;

	IF EXISTS (SELECT TOP 1 ProductId
				FROM @ProductIds)
    BEGIN
		
		SELECT
			@OrganizationPartyId = dim.PartyId
		FROM Enterprise.DataImportMapping AS dim
		WHERE
			dim.SourceId = @companyid;
		INSERT INTO @ProductsList2
			EXEC [Security].[GetPersonaProductsByOrganizationPartyId] @ProductIds = @ProductIds, @OrganizationPartyId = @OrganizationPartyId;
		;WITH Users
		AS 
			(
			
			(SELECT ul.UserId, 
				ul.LoginName, 
				p2.FirstName, 
				p2.LastName, 
				p.PersonaId
                        
			FROM @ProductsList2 AS cp
				INNER JOIN Person.Persona AS p ON cp.PersonaId = p.PersonaId
				INNER JOIN [Security].[PersonaRole] AS pr ON p.PersonaId = pr.PersonaId
				INNER JOIN [Security].[Role] AS r ON pr.RoleId = r.RoleId AND cp.ProductId = r.ProductId
				INNER JOIN [Security].[RoleRight] AS rr ON r.RoleId = rr.RoleId
				INNER JOIN [Security].[Right] AS r2 ON rr.RightId = r2.RightId
				INNER JOIN Ident.UserLoginPersona AS ulp ON p.UserLoginPersonaId = ulp.UserLoginPersonaId
				INNER JOIN ident.UserLogin AS ul ON ulp.UserLoginId = ul.UserId
				INNER JOIN Person.Person AS p2 ON ul.PersonPartyId = p2.PartyId
				INNER JOIN Enterprise.PersonaConfiguration AS pc ON p.PersonaId = pc.PersonaId 
			
			WHERE 
				ulp.StatusTypeId = 1
				AND ulp.OrganizationPartyId = @OrganizationPartyId
				AND (@RoleCount IS NULL OR r.ShortName IN (SELECT RoleShortName FROM @RoleList))
				AND (@RightCount IS NULL OR r2.RightName IN (SELECT RightName FROM @RightList))

				AND P.PersonaId NOT IN
				(
					SELECT pe.PersonaId
					FROM Enterprise.MasterConfigurationType mct
						INNER JOIN Enterprise.MasterSettingType MST ON mst.MasterConfigurationTypeId = mct.MasterCOnfigurationTypeId
						INNER JOIN Enterprise.MasterSetting ms ON ms.MasterSettingTypeId = mst.MasterSettingTYpeId
						INNER JOIN Enterprise.Party p ON CONVERT(NVARCHAR(40), p.RealPageId) = ms.Value
						INNER JOIN ident.UserLogin ul ON UL.PersonPartyId = p.PartyId
						INNER JOIN Ident.UserLoginPersona ulp ON ul.UserId = ulp.UserLoginId
						INNER JOIN Person.Persona pe ON pe.UserLoginPersonaId = ulp.UserLoginPersonaId
					WHERE
						mct.Name = 'Organization'
						AND mst.Name = 'RealPageEmployeeAccessID')

			UNION
			SELECT ul.UserId, 
				ul.LoginName, 
				p2.FirstName, 
				p2.LastName, 
				p.PersonaId
                        
			FROM @ProductsList2 AS cp
				INNER JOIN Person.Persona AS p ON CP.PersonaId = p.PersonaId
				INNER JOIN [Security].[PersonaRole] AS pr ON p.PersonaId = pr.PersonaId
				INNER JOIN [Security].[Role] AS r ON pr.RoleId = r.RoleId
				INNER JOIN [Security].[RoleRight] AS rr ON r.RoleId = rr.RoleId
				INNER JOIN [Security].[Right] AS r2 ON rr.RightId = r2.RightId 
				
				INNER JOIN Ident.UserLoginPersona AS ulp ON p.UserLoginPersonaId = ulp.UserLoginPersonaId
				INNER JOIN ident.UserLogin AS ul ON ulp.UserLoginId = ul.UserId
				INNER JOIN Person.Person AS p2 ON ul.PersonPartyId = p2.PartyId
				INNER JOIN Enterprise.productright AS pc ON r2.TargetProductId = pc.ProductId 
			
			WHERE
				r2.TargetProductId IN (SELECT TargetProductId FROM @ProductsList2 )
				AND r2.TargetProductId <> r2.ProductId 
				AND ulp.OrganizationPartyId = @OrganizationPartyId
				AND ulp.StatusTypeId = 1
				AND (@RoleCount IS NULL OR r.ShortName IN (SELECT * FROM @RoleList))
				AND (@RightCount IS NULL OR r2.RightName IN (SELECT RightName FROM @RightList))
				AND P.PersonaId NOT IN
				(
					SELECT pe.PersonaId
					FROM Enterprise.MasterConfigurationType mct
						INNER JOIN Enterprise.MasterSettingType MST ON mst.MasterConfigurationTypeId = mct.MasterCOnfigurationTypeId
						INNER JOIN Enterprise.MasterSetting ms ON ms.MasterSettingTypeId = mst.MasterSettingTYpeId
						INNER JOIN Enterprise.Party p ON CONVERT(NVARCHAR(40), p.RealPageId) = ms.Value
						INNER JOIN ident.UserLogin ul ON UL.PersonPartyId = p.PartyId
						INNER JOIN Ident.UserLoginPersona ulp ON ul.UserId = ulp.UserLoginId
						INNER JOIN Person.Persona pe ON pe.UserLoginPersonaId = ulp.UserLoginPersonaId
					WHERE
						mct.Name = 'Organization'
						AND mst.Name = 'RealPageEmployeeAccessID')

				
			))

		INSERT INTO #UserList
		(UserId, 
		LoginName, 
		FirstName, 
		LastName,
		PersonaId)
			SELECT
				UserId, 
				LoginName, 
				FirstName, 
				LastName,
				PersonaId
			FROM Users AS u;

	END

	;with totalusers 
		(UserId, 
        LoginName, 
        FirstName, 
        LastName,
		PersonaId) AS 
		( SELECT DISTINCT		   
			UserId, 
			LoginName, 
			FirstName, 
			LastName,
			PersonaId
		FROM #UserList ul)

		SELECT
			UserId, 
            LoginName, 
            FirstName, 
            LastName,
			PersonaId,
			COUNT(1) OVER() AS TotalRecords
		FROM totalusers		   
        
        ORDER BY UserId
        OFFSET((@PageNumber - 1) * @RowsPerPage) ROWS FETCH NEXT(@RowsPerPage) ROWS ONLY;

END;