CREATE OR ALTER PROCEDURE [Person].[ListPersons_Ver04] (
	@RealPageId uniqueidentifier = NULL,
	@ParentPartyRoleTypeId int = NULL,
	@UserListFilterType tinyint = 0,
	@AssignedProducts nvarchar(max),
	@FilterBy nvarchar(max),
	@SortBy nvarchar(max),
	@RowsPerPage int = 0,
	@PageNumber int = 1
)
AS
BEGIN
	DECLARE @PartyId bigint,
		@NOW datetime= GETUTCDATE(),
		@sortOrder nvarchar(128),
		@sortDirection nvarchar(4),
		@sortValue int = 100,
		@filterName nvarchar(512),
		@filterProductId int = NULL,
		@filterPartyRoleTypeId int = NULL,
		@minSequence smallint,
		@csvAssignedProducts varchar(max),
		@csvStatus varchar(max),
		@ProductSettingTypeId int,
		@OffsetMinutes smallint;  

	DECLARE @filterStatus TABLE (
		StatusTypeId int PRIMARY KEY
	)

	DECLARE @HoldPersona TABLE (
		PersonaId bigint
	)

	DECLARE @AssignedProductIds TABLE (
		ProductId int PRIMARY KEY
	)

	DECLARE @tblFilterBy TABLE (
		ColumnName varchar(128),
		SearchValue varchar(max)
	)

	DECLARE @UserLoginPersona TABLE (
		PersonaId bigint,
		PersonPartyId bigint,
		UserId bigint,
		UserLoginPersonaId bigint,
		LoginName varchar(255),
		LastLogin datetime,
		FromDate datetime,
		ThruDate datetime,
		IdentityProviderTypeId int,
		StatusId int,
		StatusName varchar(50),
		StatusThruDate datetime
	)

	DECLARE @PersonaProduct TABLE (
		PersonaId bigint,
		ProductId bigint
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

	SELECT	@ProductSettingTypeId = ProductSettingTypeId
	FROM		Enterprise.ProductSettingType
	WHERE	Name = 'ProductStatus'

	SELECT	@csvAssignedProducts = ColumnValue
	FROM	OPENJSON (JSON_QUERY(@AssignedProducts, '$.assignedProducts'))
	WITH	(
					ColumnName nvarchar(max) '$.ColumnName',
					ColumnValue nvarchar(max) '$.SearchValue'
				)

	INSERT INTO @AssignedProductIds (
		ProductId
	)
	SELECT	CONVERT(int, value)
	FROM		STRING_SPLIT(@csvAssignedProducts, ',');

	SELECT	@SortValue =
			CASE ColumnName
				WHEN N'InitialSort' THEN 100
				WHEN N'Name' THEN 100
				WHEN N'Products' THEN 101
				WHEN N'LastLogin' THEN 102
				WHEN N'LoginName' THEN 103
				WHEN N'Status' THEN 104
				ELSE 100
			END * CASE SortDirection WHEN N'ASC' THEN 1 ELSE -1 END 
	FROM	OPENJSON (JSON_QUERY(@SortBy, '$.sortBy'))
	WITH	(
					ColumnName nvarchar(max) '$.ColumnName',
					SortDirection nvarchar(max) '$.SortDirection'
				)
	WHERE	ISJSON(@SortBy) > 0;

	INSERT INTO @tblFilterBy (
		ColumnName,
		SearchValue
	)
	SELECT	ColumnName,
					SearchValue
	FROM	OPENJSON (JSON_QUERY(@FilterBy, '$.filterBy'))
	WITH	(
					ColumnName nvarchar(max) '$.ColumnName',
					SearchValue nvarchar(max) '$.SearchValue'
				)
	WHERE	ISJSON(@FilterBy) > 0

	SELECT	@OffsetMinutes = SearchValue
	FROM		@tblFilterBy
	WHERE	ColumnName = 'OffsetMinutes'

	SET @OffsetMinutes = ISNULL(@OffsetMinutes, 0)

	SELECT	@filterName = SearchValue
	FROM		@tblFilterBy
	WHERE	ColumnName = 'Name'
	AND			SearchValue NOT IN ( '%', '')

	SELECT	@filterProductId = CONVERT(int, SearchValue)
	FROM		@tblFilterBy
	WHERE	ColumnName = 'ProductId'
	AND			ISNUMERIC(SearchValue) = 1

	SELECT	@filterPartyRoleTypeId = CONVERT(int, SearchValue)
	FROM		@tblFilterBy
	WHERE	ColumnName = 'UserType'
	AND			ISNUMERIC(SearchValue) = 1

	SELECT	@csvStatus = SearchValue
	FROM		@tblFilterBy
	WHERE	ColumnName = 'Status'
	AND			SearchValue NOT IN ( '%', '')

	IF (LEN(@csvStatus) > 0)
	BEGIN
		INSERT INTO @filterStatus (
			StatusTypeId
		)
		SELECT	CONVERT(int, value)
		FROM		STRING_SPLIT(@csvStatus, ',');
	END

	SELECT	@PartyId = PartyId
	FROM		Enterprise.Party
	WHERE	RealPageId = @RealPageId
	
	SELECT @minSequence = MIN(Sequence)
	FROM  [CustomField].[Field]
	WHERE	OrganizationId = @PartyId
	AND			Enabled = 1

	IF (@UserListFilterType IN (1, 2))
	BEGIN
		INSERT INTO @HoldPersona (
			PersonaId
		)
		SELECT	pe.PersonaId 
		FROM	Enterprise.MasterConfigurationType mct
					INNER JOIN Enterprise.MasterSettingType mst ON mst.MasterConfigurationTypeId = mct.MasterCOnfigurationTypeId
					INNER JOIN Enterprise.MasterSetting ms ON ms.MasterSettingTypeId = mst.MasterSettingTYpeId
					INNER JOIN Enterprise.Party p ON p.RealPageId = ms.Value
					INNER JOIN Ident.UserLogin ul ON ul.PersonPartyId = p.PartyId
					INNER JOIN Ident.UserLoginPersona ulp ON ulp.UserLoginId = ul.UserId
					INNER JOIN Person.Persona pe ON pe.UserLoginPersonaId = ulp.UserLoginPersonaId
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
		FROM	Person.Persona p
					INNER JOIN Ident.UserLoginPersona ulp ON p.UserLoginPersonaId = ulp.UserLoginPersonaId
					INNER JOIN Ident.UserLogin ul ON ulp.UserLoginId = ul.UserId
					INNER JOIN Enterprise.PartyRelationship pr ON pr.PartyIdFrom = ul.PersonPartyId
					INNER JOIN Enterprise.RoleType rt ON rt.PartyRoleTYpeId = pr.RoleTypeIdFrom
		WHERE	rt.Name = 'SuperUser'
		AND		ulp.OrganizationPartyId = @PartyId
	END

	IF (@UserListFilterType = 0)	
	BEGIN	
		INSERT INTO @HoldPersona (
			PersonaId
		)
		SELECT  0 
	END;

	INSERT INTO @UserLoginPersona (
		PersonaId,
		PersonPartyId,
		UserId,
		UserLoginPersonaId,
		LoginName,
		LastLogin,
		FromDate,
		ThruDate,
		IdentityProviderTypeId,
		StatusId,
		StatusName,
		StatusThruDate
	)
	SELECT	pe.PersonaId,
				iul.PersonPartyId,
				iul.UserId,
				iulp.UserLoginPersonaId,
				iul.LoginName,
				CASE
					WHEN DATEDIFF(day, iul.LastLoginDate, '12/31/9999') = 0 THEN iul.LastLoginDate
					ELSE DATEADD(minute, @OffsetMinutes, iul.LastLoginDate)
				END AS 'LastLoginDate',
				CASE
					WHEN DATEDIFF(day, iulp.FromDate, '12/31/9999') = 0 THEN iulp.FromDate
					ELSE DATEADD(minute, @OffsetMinutes, iulp.FromDate)
				END AS 'FromDate',
				CASE
					WHEN DATEDIFF(day, iulp.ThruDate, '12/31/9999') = 0 THEN iulp.ThruDate
					ELSE DATEADD(minute, @OffsetMinutes, iulp.ThruDate)
				END AS 'ThruDate',
				iul.IdentityProviderTypeId,
				iulp.StatusTypeId,
				CASE
					WHEN ((iulp.StatusTypeId = 12) AND (iul.LastLoginDate IS NULL)) THEN 'Pending'
					WHEN ((iulp.StatusTypeId = 12) AND (iul.LastLoginDate IS NOT NULL)) THEN 'Active'
					ELSE est.Name
				END AS 'StatusName',
				CASE
					WHEN DATEDIFF(day, iulp.StatusThruDate, '12/31/9999') = 0 THEN iulp.StatusThruDate
					ELSE DATEADD(minute, @OffsetMinutes, iulp.StatusThruDate)
				END AS 'StatusThruDate'
	FROM	Person.Persona pe
				INNER JOIN Ident.UserLoginPersona iulp ON (pe.UserLoginPersonaId = iulp.UserLoginPersonaId)
				INNER JOIN Ident.UserLogin iul ON iulp.UserLoginId = iul.UserId
				INNER JOIN Enterprise.StatusType est ON iulp.StatusTypeId = est.StatusTypeId
				LEFT OUTER JOIN @filterStatus fs ON (est.StatusTypeId = fs.StatusTypeId)
	WHERE	iulp.OrganizationPartyId = @PartyId
	AND		((@csvStatus IS NULL) OR (NOT fs.StatusTypeId IS NULL))
	AND		pe.PersonaId  NOT IN ( SELECT ISNULL(PersonaId, 0) FROM @HoldPersona)
	AND		(
					pe.PersonaId IN
					(
						SELECT	PersonaID
						FROM		@PersonaProduct
						WHERE	ProductId = @filterProductId
					)
					OR @filterProductId IS NULL
				)

	INSERT INTO @PersonaProduct (
		PersonaId,
		ProductId
	)
	SELECT	ulp.PersonaID,
				pec.ProductId
	FROM	@UserLoginPersona ulp
		INNER JOIN Enterprise.PersonaConfiguration pec ON ulp.PersonaId = pec.PersonaId
		INNER JOIN Enterprise.ProductConfiguration prc ON pec.ConfigurationId = prc.ConfigurationId
		INNER JOIN Enterprise.ProductSetting ps ON (prc.ProductSettingId = ps.ProductSettingId AND ps.ProductSettingTypeId = @ProductSettingTypeId AND ps.Value = '8')
	WHERE	((@NOW BETWEEN pec.FromDate AND pec.ThruDate) OR (@NOW >= pec.FromDate AND pec.ThruDate IS NULL))
	AND		((@NOW BETWEEN prc.FromDate AND prc.ThruDate) OR (@NOW >= prc.FromDate AND prc.ThruDate IS NULL))
	AND		((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))
	AND		pec.ProductId NOT IN (14, 19, 24, 25, 34, 39); --Client Portal, Product Learning Portal, Black Book, Self-provisioning portal, Benchmarking, Integration Marketplace

	WITH cteProductCount
	(
		PersonaId,
		ProductCount
	)
	AS
	(
		SELECT	pp.PersonaId,
					COUNT(pp.ProductId)
		FROM	@PersonaProduct pp
					INNER JOIN @AssignedProductIds ap ON (ap.ProductId = pp.ProductId)
		GROUP BY pp.PersonaId
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
					ulp.UserId,
					ulp.LoginName,
					ulp.LastLogin,
					ulp.FromDate,
					ulp.ThruDate,
					ulp.StatusId,
					ulp.StatusName,
					ulp.StatusThruDate,
					CASE
						WHEN ipt.Name = 'ID3' THEN 0
						ELSE 1
					END AS 'Is3rdPartyIDP',
					ISNULL(pct.ProductCount, 0) AS Products,
					0 AS Properties,
					ISNULL(rt.Name, '') AS UserType,
					prs.RoleTypeIdFrom AS PartyRoleTypeId,
					COUNT(1) OVER () AS TotalRecords,
					CASE @sortValue
						WHEN 100 THEN ROW_NUMBER() OVER (ORDER BY p.FirstName + ' ' + p.LastName ASC)
						WHEN 101 THEN ROW_NUMBER() OVER (ORDER BY pct.ProductCount ASC, p.FirstName + ' ' + p.LastName ASC)
						WHEN 102 THEN ROW_NUMBER() OVER (ORDER BY ulp.LastLogin ASC, p.FirstName + ' ' + p.LastName ASC)
						WHEN 103 THEN ROW_NUMBER() OVER (ORDER BY ulp.LoginName ASC, p.FirstName + ' ' + p.LastName ASC)
						WHEN 104 THEN ROW_NUMBER() OVER (ORDER BY ulp.StatusName ASC, p.FirstName + ' ' + p.LastName ASC)
						WHEN -100 THEN ROW_NUMBER() OVER (ORDER BY p.FirstName + ' ' + p.LastName DESC)
						WHEN -101 THEN ROW_NUMBER() OVER (ORDER BY pct.ProductCount DESC, p.FirstName + ' ' + p.LastName DESC)
						WHEN -102 THEN ROW_NUMBER() OVER (ORDER BY ulp.LastLogin DESC, p.FirstName + ' ' + p.LastName DESC)
						WHEN -103 THEN ROW_NUMBER() OVER (ORDER BY ulp.LoginName DESC, p.FirstName + ' ' + p.LastName DESC)
						WHEN -104 THEN ROW_NUMBER() OVER (ORDER BY ulp.StatusName DESC, p.FirstName + ' ' + p.LastName DESC)
					END AS RowNumber
		FROM	@UserLoginPersona ulp
					INNER JOIN Person.Person p ON p.PartyId = ulp.PersonPartyId
					INNER JOIN Enterprise.Party pa ON p.PartyId = pa.PartyID
					INNER JOIN Enterprise.PartyRelationship prs ON prs.PartyIdFrom = ulp.PersonPartyId AND prs.PartyIdTo = @PartyId
					INNER JOIN Enterprise.RelationshipType rst ON rst.RelationshipTypeId = prs.PartyRelationshipTypeId
					INNER JOIN Enterprise.RoleType rt ON (rt.PartyRoleTypeId = rst.RoleTypeIdValidFrom)
					INNER JOIN Ident.IdentityProviderType ipt ON ulp.IdentityProviderTypeId = ipt.IdentityProviderTypeId
					LEFT OUTER JOIN cteProductCount pct ON pct.PersonaId = ulp.PersonaId
					LEFT OUTER JOIN cteCustomFields cf ON (cf.UserLoginPersonaId = ulp.UserLoginPersonaId)
		WHERE		(
								(@filterName IS NULL)
								OR (CHARINDEX(@filterName, FirstName + ' ' + LastName, 1) > 0)
								OR (CHARINDEX(@filterName, ulp.LoginName, 1) > 0)
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
				PartyRoleTypeId
	FROM	cteUsersFinal
	ORDER BY RowNumber
	OFFSET ((@PageNumber - 1) * @RowsPerPage) ROWS
	FETCH NEXT(@RowsPerPage) ROWS ONLY
	OPTION (RECOMPILE)
END;
