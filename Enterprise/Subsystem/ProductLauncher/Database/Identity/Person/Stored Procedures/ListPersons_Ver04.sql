CREATE PROCEDURE [Person].[ListPersons_Ver04] (
	@RealPageId uniqueidentifier = NULL,
	@ParentPartyRoleTypeId int = NULL,
	@UserListFilterType tinyint = 0,
	@AssignedProducts nvarchar(max), --{"assignedProducts":[{"ColumName":"ProductId","SearchValue":"1,1,3,8,9,14,16,19,21,27,28,36,37,45,46"}]}
	@FilterBy nvarchar(max), --'{"filterBy":[{"ColumnName":"Status","SearchValue":"1,2,3,4,5"}]}'
	@SortBy nvarchar(max), --'{"sortBy":[{"ColumnName":"Name","SortDirection":"DESC"}]}'
	@RowsPerPage int = 0,
	@PageNumber int = 1
)
AS
BEGIN
	DECLARE @PartyId bigint,
		@NOW datetime= GETUTCDATE();

	DECLARE @sortOrder nvarchar(128),
		@sortDirection nvarchar(4),
		@sortValue int = 100,
		@filterName nvarchar(512),
		@filterProductId int = NULL,
		@filterPartyRoleTypeId int = NULL,
		@filterStatusTypeId int = 0,
		@minSequence smallint,
		@csvAssignedProducts varchar(max),
		@csvStatus varchar(max)

	DECLARE @filterStatus TABLE (
		StatusTypeId int PRIMARY KEY
	)

	DECLARE @HoldPersona TABLE (
		PersonaId bigint
	)

	DECLARE @AssignedProductIds TABLE (
		ProductId int PRIMARY KEY
	)

	SELECT	@RowsPerPage = CASE
		WHEN @RowsPerPage <= 0 THEN 2147483647
		ELSE @RowsPerPage
	END; 

	IF (ISJSON(@SortBy) = 0)
	BEGIN
		SET @SortBy = NULL
	END

	IF (ISJSON(@FilterBy) = 0)
	BEGIN
		SET @FilterBy = NULL
	END

	SELECT	@csvAssignedProducts = ColumnValue
	FROM	OPENJSON (JSON_QUERY(@AssignedProducts, '$.assignedProducts'))
	WITH	(
					ColumnName nvarchar(max) '$.ColumnName',
					ColumnValue nvarchar(max) '$.SearchValue'
				)

	INSERT INTO @AssignedProductIds (
		ProductId
	)
	SELECT CONVERT(int, value) FROM STRING_SPLIT(@csvAssignedProducts, ',');

	SELECT	@SortValue =
		CASE SortDirection
			WHEN N'ASC' THEN
				CASE ColumnName
					WHEN N'InitialSort' THEN 100
					WHEN N'Name' THEN 100
					WHEN N'Products' THEN 101
					WHEN N'LastLogin' THEN 102
					WHEN N'LoginName' THEN 103
					WHEN N'Status' THEN 104
					ELSE 100
				END
			WHEN N'DESC' THEN
				CASE ColumnName
					WHEN N'InitialSort' THEN -100
					WHEN N'Name' THEN -100
					WHEN N'Products' THEN -101
					WHEN N'LastLogin' THEN -102
					WHEN N'LoginName' THEN -103
					WHEN N'Status' THEN -104
					ELSE -100
				END
		END
	FROM	OPENJSON (JSON_QUERY(@SortBy, '$.sortBy'))
	WITH	(
					ColumnName nvarchar(max) '$.ColumnName',
					SortDirection nvarchar(max) '$.SortDirection'
				)
	WHERE	ISJSON(@SortBy) > 0;

	SELECT	@filterName = SearchValue
	FROM	OPENJSON (JSON_QUERY(@FilterBy, '$.filterBy'))
	WITH	(
					ColumnName nvarchar(max) '$.ColumnName',
					SearchValue nvarchar(max) '$.SearchValue'
				)
	WHERE	ISJSON(@FilterBy) > 0
	AND			ColumnName = 'Name'
	AND			SearchValue NOT IN ( '%', '')

	SELECT	@filterProductId = CONVERT(int, SearchValue)
	FROM	OPENJSON (JSON_QUERY(@FilterBy, '$.filterBy'))
	WITH	(
					ColumnName nvarchar(max) '$.ColumnName',
					SearchValue nvarchar(max) '$.SearchValue'
				)
	WHERE	ISJSON(@FilterBy) > 0
	AND			ColumnName = 'ProductId'
	AND			ISNUMERIC(SearchValue) = 1

	SELECT	@filterPartyRoleTypeId = CONVERT(int, SearchValue)
	FROM	OPENJSON (JSON_QUERY(@FilterBy, '$.filterBy'))
	WITH	(
					ColumnName nvarchar(max) '$.ColumnName',
					SearchValue nvarchar(max) '$.SearchValue'
				)
	WHERE	ISJSON(@FilterBy) > 0
	AND			ColumnName = 'UserType'
	AND			ISNUMERIC(SearchValue) = 1

	SELECT	@csvStatus = SearchValue
	FROM	OPENJSON (JSON_QUERY(@FilterBy, '$.filterBy'))
	WITH	(
					ColumnName nvarchar(max) '$.ColumnName',
					SearchValue nvarchar(max) '$.SearchValue'
				)
	WHERE	ISJSON(@FilterBy) > 0
	AND			ColumnName = 'Status'
	AND			SearchValue NOT IN ( '%', '')

	INSERT INTO @filterStatus (
		StatusTypeId
	)
	SELECT CONVERT(int, value) FROM STRING_SPLIT(@csvStatus, ',');

	SELECT	@filterStatusTypeId = COUNT(StatusTypeId)
	FROM		@filterStatus
	WHERE	StatusTypeId > 0

	SELECT	@PartyId = PartyId
	FROM	Enterprise.Party
	WHERE	RealPageId = @RealPageId
	
	SELECT @minSequence = MIN(Sequence)
	FROM  [CustomField].[Field]
	WHERE	OrganizationId = @PartyId
	AND			Enabled = 1
	GROUP BY OrganizationId;
         
	IF (@UserListFilterType IN (1, 2))
	BEGIN
		INSERT INTO @HoldPersona (
			PersonaId
		)
		SELECT	pe.PersonaId 
		FROM	Enterprise.MasterConfigurationType mct
					INNER JOIN Enterprise.MasterSettingType MST ON mst.MasterConfigurationTypeId = mct.MasterCOnfigurationTypeId
					INNER JOIN Enterprise.MasterSetting ms ON ms.MasterSettingTypeId = mst.MasterSettingTYpeId
					INNER JOIN Enterprise.Party p ON p.RealPageId = ms.Value
					INNER JOIN Ident.UserLogin UL ON UL.PersonPartyId = P.PartyId
					INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId
					INNER JOIN Person.Persona pe ON PE.UserLoginPersonaId = ULP.USerLoginPersonaId
		WHERE	mct.Name = 'Organization'
		AND		mst.Name = 'RealPageEmployeeAccessID'
		AND		ULP.OrganizationPartyId = @PartyId
	END

	IF (@UserListFilterType = 2)
	BEGIN
		INSERT INTO @HoldPersona (
			PersonaId
		)
		SELECT	P.PersonaId
		FROM	Person.Persona P
					INNER JOIN Ident.UserLoginPersona ULP on P.UserLoginPersonaId = ULP.UserLoginPersonaId
					INNER JOIN Ident.UserLogin UL ON ULP.UserLoginId = UL.UserId
					INNER JOIN Enterprise.PartyRelationship PR ON PR.PartyIdFrom = UL.PersonPartyId
					INNER JOIN Enterprise.RoleType RT ON RT.PartyRoleTYpeId = PR.RoleTypeIdFrom
		WHERE	RT.Name = 'SuperUser'
		AND		ULP.OrganizationPartyId = @PartyId
	END

	IF (@UserListFilterType = 0)	
	BEGIN	
		INSERT INTO @HoldPersona (
			PersonaId
		)
		SELECT  0 
	END;

	WITH ctePersonaProduct
	(
		PersonaId,
		ProductId
	)
	AS
	(
		SELECT	p.PersonaID,
					pec.ProductId
		FROM	Person.Persona p
			INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = p.UserLoginPersonaId
			INNER JOIN Enterprise.PersonaConfiguration pec ON p.PersonaId = pec.PersonaId
			INNER JOIN Enterprise.ProductConfiguration prc ON pec.ConfigurationId = prc.ConfigurationId
			INNER JOIN Enterprise.ProductSetting ps ON prc.ProductSettingId = ps.ProductSettingId AND ps.Value = '8'
			INNER JOIN Enterprise.ProductSettingType pst ON ps.ProductSettingTypeId = pst.ProductSettingTypeId AND pst.Name = 'ProductStatus'
		WHERE	((@NOW BETWEEN pec.FromDate AND pec.ThruDate) OR (@NOW >= pec.FromDate AND pec.ThruDate IS NULL))
		AND		((@NOW BETWEEN prc.FromDate AND prc.ThruDate) OR (@NOW >= prc.FromDate AND prc.ThruDate IS NULL))
		AND		((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))
		AND		pec.ProductId NOT IN (14, 19, 24, 25, 34, 39) --Client Portal, Product Learning Portal, Black Book, Self-provisioning portal, Benchmarking, Integration Marketplace
		AND		ULP.OrganizationPartyId = @PartyId
	),
	cteProductCount
	(
		PersonaId,
		ProductCount
	)
	AS
	(
		SELECT	cte.PersonaId,
					COUNT(cte.ProductId) AS ProductCount
		FROM	ctePersonaProduct cte
					INNER JOIN @AssignedProductIds ap ON (ap.ProductId = cte.ProductId)
		GROUP BY cte.PersonaId
	),
	cteSettings
	(
		AttributeId,
		TimeZone
	)
	AS
	(
		SELECT	mc.AttributeId,
					ms.Value
		FROM	Enterprise.MasterSettingType mst
					INNER JOIN Enterprise.MasterSetting ms ON mst.MasterSettingTypeId = ms.MasterSettingTypeId
					INNER JOIN Enterprise.MasterConfigurationSetting mcs ON mcs.MasterSettingId = ms.MasterSettingId
					INNER JOIN Enterprise.MasterConfiguration mc ON mc.MasterConfigurationId = mcs.MasterConfigurationId
					INNER JOIN Ident.UserLogin iul ON iul.UserId = mc.AttributeId
					INNER JOIN Ident.UserLoginPersona ulp on ulp.UserLoginId = iul.UserId
		WHERE	
				ulp.OrganizationPartyId = @PartyId
		AND		mst.MasterConfigurationTypeId = 3 --'UserLogin'
		AND		mst.Name = 'TimeZone'
	),
	cteCustomFields
	(
		Id,
		FieldName,
		FieldValue,
		UserId,
		UserLoginPersonaId
	)
	AS
	(
		SELECT	cff.FieldId AS 'Id',
					cff.Name AS 'FieldName',
					cffv.Value AS 'FieldValue',
					ULP.UserLoginID AS 'UserId',
					ULP.UserLoginPersonaID AS 'UserLoginPersonaId'
		FROM	[CustomField].[Field] cff
					INNER JOIN [CustomField].[FieldValue] cffv ON (cff.Enabled = 1 AND cff.FieldId = cffv.FieldId)
					INNER JOIN Ident.UserLoginPersona ULP ON cffv.UserLoginPersonaId = ULP.UserLoginPersonaId
		WHERE		cff.OrganizationId = @PartyId
		AND				cff.Sequence = @minSequence
		AND				((NOT cffv.Value IS NULL) OR (LEN(cffv.Value) = 0))
	),
	cteUserLogin
	(
		PersonaId,
		PersonPartyId,
		UserId,
		LoginName,
		LastLogin,
		FromDate,
		ThruDate,
		IdentityProviderTypeId,
		StatusId,
		StatusName,
		StatusThruDate
	)
	AS
	(
		SELECT		pe.PersonaId,
					ul.PersonPartyId,
					ul.UserId,
					ul.LoginName,
					ul.LastLoginDate,
					iulp.FromDate,
					iulp.ThruDate,
					ul.IdentityProviderTypeId,
					iulp.StatusTypeId,
					CASE
						WHEN ((iulp.StatusTypeId = 12) AND (ul.LastLoginDate IS NULL)) THEN 'Pending'
						WHEN ((iulp.StatusTypeId = 12) AND (ul.LastLoginDate IS NOT NULL)) THEN 'Active'
						ELSE est.Name
					END AS 'StatusName',
					iulp.StatusThruDate
		FROM	Person.Persona pe
					INNER JOIN Ident.UserLoginPersona iulp ON (pe.UserLoginPersonaId = iulp.UserLoginPersonaId)
					INNER JOIN Ident.UserLogin ul ON iulp.UserLoginId = ul.UserId
					INNER JOIN Enterprise.StatusType est ON iulp.StatusTypeId = est.StatusTypeId
					LEFT OUTER JOIN @filterStatus fs ON (est.StatusTypeId = fs.StatusTypeId)
		WHERE	iulp.OrganizationPartyId = @PartyId
		AND		pe.personaId  NOT IN ( SELECT ISNULL(PersonaId, 0) FROM @HoldPersona)
		AND		(
						pe.PersonaId IN
						(
							SELECT	PersonaID
							FROM	ctePersonaProduct
							WHERE	ProductId = @filterProductId
						)
						OR @filterProductId IS NULL
					)
		AND		((@filterStatusTypeId = 0) OR (NOT fs.StatusTypeId IS NULL))
		--AND		((@NOW BETWEEN iulp.FromDate AND iulp.ThruDate) OR (@NOW >= iulp.FromDate AND iulp.ThruDate IS NULL))
	),
 	cteUsersFinal
	(
		RealPageID,
		PartyId,
		FirstName,
		MiddleName,
		LastName,
		Title,
		Suffix,
		CustomField,
		UserId,
		LoginName,
		LastLogin,
		FromDate,
		ThruDate,
		StatusId,
		StatusName,
		StatusThruDate,
		Is3rdPartyIDP,
		Products,
		Properties,
		UserType,
		PartyRoleTypeId,
		TimeZoneOffset,
		TotalRecords,
		RowNumber
	)
	AS
	(
		SELECT DISTINCT
					pa.RealpageId AS RealPageID,
					p.PartyId,
					p.FirstName,
					p.MiddleName,
					p.LastName,
					p.Title,
					p.Suffix,
					CASE
						WHEN cf.FieldValue IS NULL THEN ''
						ELSE cf.FieldValue
					END AS 'CustomFieldValue',
					cteul.UserId,
					cteul.LoginName,
					cteul.LastLogin,
					cteul.FromDate,
					cteul.ThruDate,
					cteul.StatusId,
					cteul.StatusName,
					cteul.StatusThruDate,
					CASE
						WHEN ipt.Name = 'ID3' THEN 0
						ELSE 1
					END AS 'Is3rdPartyIDP',
					ISNULL(pct.ProductCount, 0) AS Products,
					0 AS Properties,
					ISNULL(rt.Name, '') AS UserType,
					prs.RoleTypeIdFrom AS PartyRoleTypeId,
					s.TimeZone AS 'TimeZoneOffset',
					COUNT(1) OVER () AS TotalRecords,
					CASE @sortValue
						WHEN 100 THEN ROW_NUMBER() OVER (ORDER BY p.FirstName + ' ' + p.LastName ASC)
						WHEN 101 THEN ROW_NUMBER() OVER (ORDER BY pct.ProductCount ASC, p.FirstName + ' ' + p.LastName ASC)
						WHEN 102 THEN ROW_NUMBER() OVER (ORDER BY cteul.LastLogin ASC, p.FirstName + ' ' + p.LastName ASC)
						WHEN 103 THEN ROW_NUMBER() OVER (ORDER BY cteul.LoginName ASC, p.FirstName + ' ' + p.LastName ASC)
						WHEN 104 THEN ROW_NUMBER() OVER (ORDER BY cteul.StatusName ASC, p.FirstName + ' ' + p.LastName ASC)
						WHEN -100 THEN ROW_NUMBER() OVER (ORDER BY p.FirstName + ' ' + p.LastName DESC)
						WHEN -101 THEN ROW_NUMBER() OVER (ORDER BY pct.ProductCount DESC, p.FirstName + ' ' + p.LastName DESC)
						WHEN -102 THEN ROW_NUMBER() OVER (ORDER BY cteul.LastLogin DESC, p.FirstName + ' ' + p.LastName DESC)
						WHEN -103 THEN ROW_NUMBER() OVER (ORDER BY cteul.LoginName DESC, p.FirstName + ' ' + p.LastName DESC)
						WHEN -104 THEN ROW_NUMBER() OVER (ORDER BY cteul.StatusName DESC, p.FirstName + ' ' + p.LastName DESC)
					END AS RowNumber
		FROM	cteUserLogin cteul
					INNER JOIN Person.Person p ON p.PartyId = cteul.PersonPartyId
					INNER JOIN Ident.UserLogin UL ON UL.PersonPartyId = p.PartyId
					INNER JOIN Ident.UserLoginPersona ULP ON UL.UserId = ULP.UserLoginId AND ULP.OrganizationPartyId = @PartyId
					INNER JOIN Enterprise.Party pa ON p.PartyId = pa.PartyID
					INNER JOIN Enterprise.PartyRelationship prs ON prs.PartyIdFrom = cteul.PersonPartyId AND prs.PartyIdTo = ulp.OrganizationPartyId
					INNER JOIN Enterprise.RelationshipType rst ON rst.RelationshipTypeId = prs.PartyRelationshipTypeId
					INNER JOIN Enterprise.RoleType rt ON (rt.PartyRoleTypeId = rst.RoleTypeIdValidFrom)
					INNER JOIN Ident.IdentityProviderType ipt ON cteul.IdentityProviderTypeId = ipt.IdentityProviderTypeId
					LEFT OUTER JOIN cteProductCount pct ON pct.PersonaId = cteul.PersonaId
					LEFT OUTER JOIN cteSettings s ON (s.AttributeId = cteul.UserId)
					LEFT OUTER JOIN cteCustomFields cf ON (cf.UserLoginPersonaId = ULP.UserLoginPersonaId AND cf.UserId = ULP.UserLoginId)
		WHERE		(
								(@filterName IS NULL)
								OR (CHARINDEX(@filterName, FirstName + ' ' + LastName, 1) > 0)
								OR (CHARINDEX(@filterName, cteul.LoginName, 1) > 0)
								OR (CHARINDEX(@filterName, cf.FieldValue, 1) > 0)
							)
		AND		((@NOW BETWEEN prs.FromDate AND prs.ThruDate) OR (@NOW >= prs.FromDate AND prs.ThruDate IS NULL))
		AND		((@ParentPartyRoleTypeId IS NULL) OR (rt.ParentPartyRoleTypeId = @ParentPartyRoleTypeId))
		AND		((@filterPartyRoleTypeId IS NULL) OR (prs.RoleTypeIdFrom = @filterPartyRoleTypeId))
	)

	SELECT	TotalRecords,
				RealPageID,
				PartyId,
				FirstName,
				MiddleName,
				LastName,
				Title,
				Suffix,
				CustomField,
				UserId,
				LoginName,
				LastLogin,
				FromDate,
				ThruDate,
				StatusId,
				StatusName,
				StatusThruDate,
				Is3rdPartyIDP,
				Products,
				Properties,
				UserType,
				PartyRoleTypeId,
				TimeZoneOffset
	FROM	cteUsersFinal
	ORDER BY RowNumber
	OFFSET ((@PageNumber - 1) * @RowsPerPage) ROWS
	FETCH NEXT(@RowsPerPage) ROWS ONLY
END;
