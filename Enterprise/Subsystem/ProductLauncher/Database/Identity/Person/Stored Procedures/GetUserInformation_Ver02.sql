CREATE PROCEDURE [Person].[GetUserInformation_Ver02] (
	@OrganizationId bigint,
	@ProductIds Enterprise.ProductIdType READONLY,
	@RealPageId uniqueidentifier = NULL,
	@StatusTypeId int =0,
	@Name nvarchar(50) = NULL,
	@RowsPerPage int = 0,
	@PageNumber int = 1
)
AS
BEGIN
	DECLARE @NOW DATETIME = GETUTCDATE();

	IF NOT EXISTS(SELECT TOP 1 ProductId FROM @ProductIds)
	BEGIN
		SELECT	0 AS Id,
					'Target ProductId list is empty.';
		RETURN;
	END;

	SELECT	@RowsPerPage = CASE
				WHEN @RowsPerPage <= 0 THEN 2147483647
				ELSE @RowsPerPage
			END;

	SELECT	cff.[FieldId]
				,cff.[OrganizationId]
				,cff.[Enabled]
				,cff.[Name]
				,cff.[Description]
				,cff.[FieldTypeId]
				,cfft.[Name] AS 'FieldTypeName'
				,cff.[Required]
				,cff.[ReadOnly]
				,cff.[DefaultValue]
				,cff.[SyncField]
				,cff.[Sequence]
				,cff.[HelpText]
				,cff.MinCharLength
				,cff.MaxCharLength
				,cffv.FieldValueId
				,ULP.UserLoginID
				,cffv.[Value]
	INTO #ListCustomFields
	FROM	[CustomField].[Field] cff
				INNER JOIN [CustomField].[FieldType] cfft ON (cff.FieldTypeId = cfft.FieldTypeId)
				INNER JOIN [CustomField].[FieldValue] cffv ON (cff.FieldId = cffv.FieldId)
				INNER JOIN Ident.UserLoginPersona ULP ON cffv.UserLoginPersonaId = ULP.UserLoginPersonaId
	WHERE	cff.OrganizationId = @OrganizationId
	AND		cff.[Enabled] = 1
	
	;WITH AssignedRole
	(  
		PersonaId,  
		[Value]  
	)  
	AS  
	(  
	  SELECT  
		pep.PersonaId,
		ro.RoleName 
	  FROM [Security].PersonaRole pep
		INNER JOIN [Security].[Role] ro ON ro.RoleId = pep.RoleId  
	  WHERE ro.ProductId = 3 
	  AND ro.OrgPartyID = @OrganizationId
	),
	cteUserProductSAML
	(
		PersonaId,
		ProductId,
		productCode,
		userName,
		id
	)
	AS
	(
		SELECT	PersonaId,
					ProductId,
					BooksProductCode,
					ProductUserName,
					UserId
		FROM	(
					SELECT p.PersonaId,
								ep.ProductId,
								ep.BooksProductCode,
								sa.Name,
								sua.Value
					FROM Person.Persona p
								INNER JOIN Ident.UserLoginPersona ULP ON p.UserLoginPersonaId = ULP.UserLoginPersonaId
								INNER JOIN Enterprise.PersonaConfiguration pec ON p.PersonaId = pec.PersonaId AND pec.StatusTypeId = 8
								INNER JOIN Enterprise.Product ep ON ep.ProductId = pec.ProductId
								INNER JOIN Ident.SamlUserAttribute sua ON(p.PersonaId = sua.PersonaId AND sua.ProductId = pec.ProductId)
								INNER JOIN Ident.SamlAttribute sa ON(sua.SamlAttributeId = sa.SamlAttributeId)
								INNER JOIN @ProductIds udttp ON pec.ProductId = udttp.ProductId
					WHERE 
							pec.ThruDate IS null
					AND		p.ThruDate IS NULL
					AND		sa.Name IN('productUsername', 'UserId')
					AND		ULP.OrganizationPartyId = @OrganizationId

		) AS T PIVOT(MAX(T.Value) FOR T.Name IN([productUsername], [UserId])) AS p
	),
	cteUsersFinal
	(
		CompanyName,
		FirstName,
		MiddleName,
		LastName,
		LoginName,
		Email,
		UserType,
		RoleName,
		CustomFields,
		OrganizationId,
		UserId,
		PersonaId,
		UserEffectiveDate,
		LastLogin,
		UserExpirationDate,
		IsExternalIdp,
		[Status],
		StatusFromDate,
		StatusThruDate,
		UserRealPageId,
		Product,
		TotalRecords,
		EmployeeId
	)
	AS
	(
		SELECT	o.[Name] AS 'CompanyName',
					pe.FirstName AS 'FirstName',
					ISNULL(pe.MiddleName, '') AS 'MiddleName',
					pe.LastName AS 'LastName',
					ul.LoginName AS 'LoginName',
					ISNULL(ne.NotificationEmail, '') AS 'Email',
					CASE
						WHEN rt.PartyRoleTypeId = 401 THEN 'Regular User'
						WHEN rt.PartyRoleTypeId = 402 THEN 'RealPage System Administrator'
						WHEN rt.PartyRoleTypeId = 403 THEN 'RealPage Employee'
						WHEN rt.PartyRoleTypeId = 404 THEN 'Regular User (No Email)'
						WHEN rt.PartyRoleTypeId = 405 THEN 'External User'
					END AS 'UserType',
					ar.[Value] AS 'RoleName',
					(
						SELECT	FieldId,
									OrganizationId,
									[Enabled],
									[Name],
									[Description],
									FieldTypeId,
									FieldTypeName,
									[Required],
									[ReadOnly],
									DefaultValue,
									SyncField,
									[Sequence],
									HelpText,
									MinCharLength,
									MaxCharLength,
									FieldValueId,
									UserLoginID,
									[Value]
						FROM	#ListCustomFields
						WHERE		UserLoginId = ul.UserId
						ORDER BY FieldId
						FOR JSON AUTO, INCLUDE_NULL_VALUES
					) AS 'CustomFields',
					ULP.OrganizationPartyId AS 'OrganizationId',
					ul.UserId AS 'UserId',
					p.PersonaId AS 'PersonaId',
					ul.CreateDate AS 'UserEffectiveDate',
					ULP.LastLoginDate AS 'LastLogin',
					ULP.Thrudate AS 'UserExpirationDate',
					CASE
						WHEN ipt.[Name] = 'ID3' THEN 0
						ELSE 1
					END AS 'IsExternalIdp',
					st.Name AS 'Status',
					ULP.FromDate AS 'StatusFromDate',
					ULP.StatusThruDate AS 'StatusThruDate',
					pa.RealPageId AS 'UserRealPageId',
					(SELECT productCode, userName, id FROM cteUserProductSAML WHERE PersonaId = p.PersonaId FOR JSON PATH) AS 'Product',
					COUNT(1) OVER () AS TotalRecords,
					ue.Employee
		FROM	Ident.UserLogin ul
				INNER JOIN Enterprise.Party pa ON pa.PartyId = ul.PersonPartyId
				INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = ul.UserId
				INNER JOIN Person.Persona p ON p.UserLoginPersonaId = ULP.UserLoginPersonaId AND ulp.OrganizationPartyId = @OrganizationId
				INNER JOIN Enterprise.Organization o ON o.PartyId = ULP.OrganizationPartyId
				INNER JOIN Person.Person pe ON pe.PartyId = ul.PersonPartyId
				--INNER JOIN Enterprise.PartyRelationship pr ON pr.PartyIdFrom = pe.PartyId
				INNER JOIN Enterprise.PartyRelationship pr ON pr.PartyIdFrom = ul.PersonPartyId AND pr.PartyIdTo = ulp.OrganizationPartyId
				INNER JOIN Enterprise.RoleType rt ON rt.PartyRoleTypeId = pr.RoleTypeIdFrom
				INNER JOIN Ident.IdentityProviderType ipt ON ipt.IdentityProviderTypeId = ul.IdentityProviderTypeId
				LEFT JOIN AssignedRole ar ON ar.PersonaId = p.PersonaId
				INNER JOIN Enterprise.StatusType st ON st.StatusTYpeId = ULP.StatusTypeId 
				LEFT OUTER JOIN
				(
					SELECT	p.PartyId,
								ea.ElectronicAddressString AS NotificationEmail
					FROM	Enterprise.ContactMechanismUsage cmu
								INNER JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
								INNER JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
								INNER JOIN Enterprise.ElectronicAddress ea ON ea.ContactMechanismID = cm.ContactMechanismID
								INNER JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
								INNER JOIN ident.UserLogin ul ON ul.PersonPartyId = p.PartyId
								INNER JOIN ident.UserLoginPersona ulp ON ulp.UserLoginId = ul.UserId AND ulp.OrganizationPartyId = @OrganizationId
					WHERE	
						(pcm.ThruDate IS NULL OR pcm.ThruDate > @NOW)
					AND			cmu.ContactMechanismUsageTypeID = 301
				) ne ON ne.PartyId = pe.PartyId
			LEFT OUTER JOIN #ListCustomFields cf ON (cf.UserLoginID = ul.UserId)
			LEFT OUTER JOIN [Enterprise].[UserEmployeeId] AS ue ON ulp.UserLoginPersonaId = ue.UserLoginPersonaId
		WHERE	pr.RoleTypeIdFrom >= 400
		AND		(
			(CHARINDEX(@Name, pe.FirstName, 1) > 0)
			OR (CHARINDEX(@Name, pe.LastName, 1) > 0)
			OR (CHARINDEX(@Name, ul.LoginName, 1) > 0)
			OR (CHARINDEX(@Name, ne.NotificationEmail, 1) > 0)
			OR @Name = pe.FirstName + ' ' + pe.LastName
			OR (CHARINDEX(@Name, cf.[Value], 1) > 0)
			OR @Name IS NULL
		)
		AND		(o.PartyId = @OrganizationId )
		AND		((@RealPageId IS NULL) OR (pa.RealPageId =@RealPageId))
		AND		ULP.UserLoginPersonaId NOT IN (SELECT UserLoginPersonaId FROM Enterprise.OrganizationAdminUser WHERE OrganizationPartyId = @OrganizationId)
		AND		((@NOW BETWEEN pr.FromDate AND pr.ThruDate) OR (@NOW >= pr.FromDate AND pr.ThruDate IS NULL))
		AND		pr.ThruDate IS null AND ulp.IsRPEmployee = 0
		AND		(ULP.StatusTypeId = @StatusTypeId or @StatusTypeId=0)
	)

	SELECT distinct	CompanyName,
				FirstName,
				MiddleName,
				LastName,
				LoginName,
				Email,
				UserType,
				CASE
					WHEN CustomFields IS NULL THEN ''
					ELSE CustomFields
				END AS 'CustomFields',
				OrganizationId,
				UserId,
				PersonaId,
				UserEffectiveDate,
				LastLogin,
				UserExpirationDate,
				IsExternalIdp,
				[Status],
				StatusFromDate,
				StatusThruDate,
				UserRealPageId,
				Product,
				TotalRecords,
				EmployeeId
	FROM	cteUsersFinal
	ORDER BY UserId
	OFFSET((@PageNumber - 1) * @RowsPerPage)
	ROWS FETCH NEXT(@RowsPerPage) ROWS ONLY
	OPTION (RECOMPILE)
	
	DROP TABLE #ListCustomFields
END;
GO