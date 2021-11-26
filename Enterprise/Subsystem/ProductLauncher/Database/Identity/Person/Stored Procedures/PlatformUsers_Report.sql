
CREATE PROCEDURE [Person].[PlatformUsers_Report] 
(
	  @CompanyId UNIQUEIDENTIFIER , 
	  @Products NVARCHAR(MAX) = NULL, 
	  @UserType  NVARCHAR(MAX) = NULL, 
	  @UserStatus NVARCHAR(MAX) = NULL, 
	  @PlatformRole NVARCHAR(MAX) = NULL, 
	  @SortBy  NVARCHAR(MAX) = NULL 
)
AS
BEGIN

	DECLARE @PartyId bigint,
		@NOW datetime= GETUTCDATE(),
		@sortValue int = 100,
		@filterProductId int = NULL,
		@minSequence smallint,
		@ProductSettingTypeId int

	DECLARE @filterEnterpriseRole TABLE (
		RoleTemplateId int PRIMARY KEY
	)
	

	DECLARE @AssignedProductIds TABLE (
		ProductId int PRIMARY KEY
	)


	CREATE TABLE #PersonaProduct(
		PersonaId bigint,
		ProductId bigint
	)

	
	SELECT	@ProductSettingTypeId = ProductSettingTypeId
	FROM		Enterprise.ProductSettingType
	WHERE	Name = 'ProductStatus'


	SELECT	@SortValue =
			CASE 
				WHEN @SortBy = N'InitialSort' THEN 100
				WHEN @SortBy = N'Name' THEN 100
				WHEN @SortBy = N'Products' THEN 101
				WHEN @SortBy = N'LastLogin' THEN 102
				WHEN @SortBy = N'LoginName' THEN 103
				WHEN @SortBy = N'Status' THEN 104
				WHEN @SortBy = N'EmployeeId' THEN 105
				WHEN @SortBy = N'EnterpriseRoleName' THEN 106
				ELSE 100
			END --* CASE SortDirection WHEN N'ASC' THEN 1 ELSE -1 END 

	   
	SELECT	@PartyId = PartyId
	FROM		Enterprise.Party
	WHERE	RealPageId = @CompanyId

	INSERT INTO @AssignedProductIds(ProductId)
	SELECT ProductId
	FROM Enterprise.Product 
	WHERE 
	(
	@Products IS NULL OR @Products = ' ALL' OR
	[Name] IN (SELECT [value]	FROM STRING_SPLIT(@Products, ','))
	)

	--only works for single product selection
	IF (ISNULL(@Products,'') <> '' AND @Products <> ' ALL')
	BEGIN

		SELECT @filterProductId =  ProductId
		FROM Enterprise.Product 
		WHERE [Name] = @Products

	END


	IF(@filterProductId = 37) -- 37 Property Photos product Id
	BEGIN

		SET @filterProductId = 9  -- Marketing Center product Id
	
		INSERT INTO #PersonaProduct (
		PersonaId,
		ProductId)
	
	   SELECT	p.PersonaID,
			pec.ProductId
		FROM	
			Person.Persona p
			INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = p.UserLoginPersonaId
			INNER JOIN Enterprise.PersonaConfiguration pec ON p.PersonaId = pec.PersonaId
			INNER JOIN Enterprise.ProductConfiguration prc ON pec.ConfigurationId = prc.ConfigurationId
			INNER JOIN Enterprise.ProductSetting ps ON prc.ProductSettingId = ps.ProductSettingId AND ps.Value = '8' AND ps.ProductSettingTypeId = @ProductSettingTypeId
			INNER JOIN [security].PersonaRole sp on sp.PersonaId = p.PersonaId 
			INNER JOIN [Security].RoleRight srr on srr.RoleId = sp.RoleId 
			INNER JOIN [Security].[Right] sr on sr.RightId = srr.RightId AND sr.RightName = 'AccessPropertyPhotos' --- Access to Property Photos (requires Marketing Center access)
		WHERE
				ULP.OrganizationPartyId = @PartyId
		AND		pec.ProductId = 9
		AND		((@NOW >= p.FromDate AND p.ThruDate IS NULL) OR (@NOW BETWEEN p.FromDate AND p.ThruDate))
        AND     ((@NOW >= pec.FromDate AND pec.ThruDate IS NULL) OR (@NOW BETWEEN pec.FromDate AND pec.ThruDate))
        AND     ((@NOW >= prc.FromDate AND prc.ThruDate IS NULL) OR (@NOW BETWEEN prc.FromDate AND prc.ThruDate))
        AND     ((@NOW >= ps.FromDate AND ps.ThruDate IS NULL) OR (@NOW BETWEEN ps.FromDate AND ps.ThruDate))	
	END
	ELSE
	BEGIN

		INSERT INTO #PersonaProduct 
		(
			PersonaId,
			ProductId
		)
		SELECT	p.PersonaID,
			pec.ProductId
		FROM	
			Person.Persona p
			INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = p.UserLoginPersonaId
			INNER JOIN Enterprise.PersonaConfiguration pec ON p.PersonaId = pec.PersonaId
			INNER JOIN Enterprise.ProductConfiguration prc ON pec.ConfigurationId = prc.ConfigurationId
			INNER JOIN Enterprise.ProductSetting ps ON prc.ProductSettingId = ps.ProductSettingId AND ps.Value = '8' AND ps.ProductSettingTypeId = @ProductSettingTypeId
		WHERE
				ULP.OrganizationPartyId = @PartyId
		AND		pec.ProductId NOT IN (14, 19, 24, 25, 34, 39) --Client Portal, Product Learning Portal, Black Book, Self-provisioning portal, Benchmarking, Integration Marketplace
		AND		((@NOW >= p.FromDate AND p.ThruDate IS NULL) OR (@NOW BETWEEN p.FromDate AND p.ThruDate))
		AND     ((@NOW >= pec.FromDate AND pec.ThruDate IS NULL) OR (@NOW BETWEEN pec.FromDate AND pec.ThruDate))
		AND     ((@NOW >= prc.FromDate AND prc.ThruDate IS NULL) OR (@NOW BETWEEN prc.FromDate AND prc.ThruDate))
		AND     ((@NOW >= ps.FromDate AND ps.ThruDate IS NULL) OR (@NOW BETWEEN ps.FromDate AND ps.ThruDate))
						

	END

	DROP INDEX IF EXISTS [NCI_Temp_PersonaProduct_ProductId] ON [dbo].[#PersonaProduct]
	CREATE NONCLUSTERED INDEX [NCI_Temp_PersonaProduct_ProductId]	ON [dbo].[#PersonaProduct] ([ProductId]) INCLUDE ([PersonaId])
	
	DROP TABLE IF EXISTS #CustomFields

	SELECT  Id,UserLoginPersonaId,FieldValue,[Enabled],[Name],[Value],[Sequence]
	INTO #CustomFields
	From (
		Select sr.[SettingTableRowId] AS 'Id',
			   st.[PartyId] AS 'OrganizationId',
			   srv.UserLoginPersonaId 'UserLoginPersonaId',
			   srv.Value 'FieldValue',
			   [TableColumnName],
			   [TableColumnValue]
		from [Settings].[SettingTableColumn] stc
		join [Settings].[SettingTableRow] sr on
			stc.[SettingTableRowId] = sr.[SettingTableRowId]
		join Settings.SettingTableRowValue SRV on
			srv.SettingTableRowId = sr.SettingTableRowId
		join [Settings].[SettingTable] st on
			st.[SettingTableId] = sr.[SettingTableId]
		where st.[PartyId] = @PartyId) As SourceTable
	PIVOT
	(
		MIN([TableColumnValue])
		FOR [TableColumnName] IN ([Enabled],[Name],[Value],[Sequence])
	) AS PivotOutput

	Delete from #CustomFields 
	Where Enabled = 0
	AND ((FieldValue IS NOT NULL) OR (LEN(FieldValue) > 0) )  

	SELECT @minSequence = MIN(Sequence)
	FROM  #CustomFields

	Delete from #CustomFields 
	Where [Sequence] <> @minSequence


	DROP TABLE IF EXISTS #UserLogin

	CREATE TABLE #UserLogin
	(
		PersonaId BIGINT PRIMARY KEY,  
		UserLoginPersonaId BIGINT NULL,
		PersonPartyId BIGINT NULL,  
		UserId BIGINT NULL,  
		LoginName VARCHAR(255) NULL,  
		LastLogin DATETIME NULL,  
		FromDate DATETIME NULL,  
		ThruDate DATETIME NULL,  
		IdentityProviderTypeId INT NULL,  
		StatusId INT,  
		StatusName VARCHAR(50) NULL,
		StatusThruDate DATETIME NULL,  
		PasswordModifiedDate smalldatetime NULL, 
		MFAFlag VARCHAR(16) NULL 
	)

	INSERT INTO #UserLogin
	(
	PersonaId,UserLoginPersonaId,PersonPartyId,UserId,LoginName,LastLogin,FromDate,ThruDate,IdentityProviderTypeId
	,StatusId,StatusName,StatusThruDate,PasswordModifiedDate,MFAFlag
	)
	SELECT * FROM (
	SELECT   
		pe.PersonaId, 
		iulp.UserLoginPersonaId, 
		ul.PersonPartyId,  
		ul.UserId,  
		ul.LoginName,  
		ul.LastLoginDate AS LastLogin,  
		iulp.FromDate,  
		iulp.ThruDate,  
		ul.IdentityProviderTypeId,  
		iulp.StatusTypeId AS StatusId,  
		CASE  
		WHEN ((iulp.StatusTypeId = 12) AND (ul.LastLoginDate IS NULL)) THEN 'Pending'  
		WHEN ((iulp.StatusTypeId = 12) AND (ul.LastLoginDate IS NOT NULL)) THEN 'Active'  
		ELSE est.Name  
		END AS 'StatusName',  
		iulp.StatusThruDate,  
		ul.PasswordModifiedDate,
		CASE     
		 WHEN ul.TwoFactorEnabled = 1 THEN 'Yes'    
		 WHEN ul.TwoFactorEnabled = 0 THEN 'No'    
		END AS MFAFlag   
	FROM Person.Persona pe  
		INNER JOIN Ident.UserLoginPersona iulp ON (pe.UserLoginPersonaId = iulp.UserLoginPersonaId)  
		INNER JOIN Ident.UserLogin ul ON iulp.UserLoginId = ul.UserId  
		INNER JOIN Enterprise.StatusType est ON iulp.StatusTypeId = est.StatusTypeId  
		--LEFT OUTER JOIN @filterStatus fs ON (est.StatusTypeId = fs.StatusTypeId)  
	WHERE iulp.OrganizationPartyId = @PartyId  AND iulp.IsRPEmployee = 0
	AND  (  
		pe.PersonaId IN  
		(  
		SELECT PersonaID  
		FROM #PersonaProduct  T
		WHERE 
			(
			@Products IS NULL OR @Products = ' ALL' OR
			T.ProductId IN (SELECT ProductId FROM @AssignedProductIds)
			)
		)
	)  
	)X
	WHERE
		(
		StatusName IN (SELECT [value] FROM STRING_SPLIT(@UserStatus, ','))
		OR @UserStatus IS NULL
		OR @UserStatus = ' ALL'
		)

	DROP TABLE IF EXISTS #UserEnterpriseRole

	CREATE TABLE #UserEnterpriseRole
	(
		PersonaId BIGINT PRIMARY KEY,  
		RoleTemplateId INT NULL,
		EnterpriseRoleName Varchar(256) NULL
	)

	INSERT INTO #UserEnterpriseRole
	SELECT UL.PersonaId, RTUM.RoleTemplateId, SRT.RoleTemplateName
	FROM #UserLogin UL
	INNER JOIN [Security].[RoleTemplateUserMapping] RTUM  ON UL.PersonaId  = RTUM.PersonaId
	INNER JOIN [Security].[RoleTemplate] SRT ON SRT.RoleTemplateId = RTUM.RoleTemplateId
	Where SRT.PartyID = @PartyId

	DROP TABLE IF EXISTS #ProductCount

	CREATE TABLE #ProductCount
	(
	PersonaId INT NOT NULL PRIMARY KEY,
	ProductCount INT NOT NULL
	)

	INSERT INTO #ProductCount
	(
		PersonaId,ProductCount
	)
	SELECT 
		pp.PersonaId,  
		COUNT(pp.ProductId) AS ProductCount
	FROM #PersonaProduct pp  
	INNER JOIN @AssignedProductIds ap ON (ap.ProductId = pp.ProductId)  	
	GROUP BY pp.PersonaId  

	DROP TABLE IF EXISTS #PartyNotificationEmails

	CREATE TABLE #PartyNotificationEmails
	(
		PartyId BIGINT PRIMARY KEY,
		NotificationEmail NVARCHAR(MAX)
	)

	;WITH CTE AS 
	(
		SELECT DISTINCT PartyId,EA.ElectronicAddressString AS NotificationEmail
		FROm Enterprise.PartyContactMechanism pcm
		LEFT JOIN Enterprise.ElectronicAddress EA ON PCM.ContactMechanismId=EA.ContactMechanismID
		WHERE ThruDate > GETUTCDATE() 
		AND ISNULL(EA.ElectronicAddressString,'') <> ''
	)
	INSERT INTO #PartyNotificationEmails
	SELECT DISTINCT 
	C.PartyId,
	STUFF((
		SELECT ', '+C1.NotificationEmail
		FROM
		CTE C1
		WHERE
		C.PartyId = C1.PartyId
		FOR XML PATH('')),1,1,'') AS NotificationEmail

	FROM
		CTE C

	DROP INDEX IF EXISTS [NCI_cteUserLogin_PersonPartyId] ON [dbo].[#UserLogin]
	CREATE NONCLUSTERED INDEX [NCI_cteUserLogin_PersonPartyId]  ON [dbo].[#UserLogin] ([PersonPartyId])
	INCLUDE ([UserLoginPersonaId],[PersonaId],[UserId],[LoginName],[LastLogin],[FromDate],[ThruDate],[IdentityProviderTypeId],[StatusId],[StatusName],[StatusThruDate],[PasswordModifiedDate])

	

	;WITH CTE AS     
	 (    
	 	
		SELECT DISTINCT    
		 ulp.UserId,R.RoleName AS PlatformRole    
		FROM  #UserLogin ulp     
		LEFT JOIN [Security].[PersonaRole] PR ON ulp.PersonaId = PR.PersonaId    
		LEFT JOIN [Security].[Role] R ON PR.RoleId = R.RoleId  
		WHERE
			(@PlatformRole IS NULL OR @PlatformRole = ' ALL' OR @PlatformRole LIKE '%'+R.RoleName+'%')
	)     
	,CTE1 AS    
	(    
		SELECT DISTINCT     
		 C.UserId,    
		 STUFF((SELECT ', '+C1.PlatformRole     
		   FROM CTE C1     
		   WHERE C.UserId = C1.UserId     
		   FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 1, '') AS PlatformRoles    
		   FROM    
		 CTE C    
	 ) 
	 
	 ,cteUsersFinal
	(
		FirstName,
		MiddleName,
		LastName,
		EmployeeId,
		Title,
		Suffix,
		CustomField,
		UserId,
		LoginName,
		LastLogin,
		User_Effective,
		User_Expires,
		[Status],
		StatusThruDate,
		Is3rdPartyIDP,
		Products,
		UserType,
		PasswordModifiedDate,
		EntepriseRoleName,
		RoleTemplateId,
		RowNumber,
		NotificationEmail,
		MFAFlag,
		PlatformRoles
	) AS 
	(
		  SELECT 
			 p.FirstName,  
			 p.MiddleName,  
			 p.LastName,  
			 UE.Employee as EmployeeId,  
			 p.Title,  
			 p.Suffix,  
			 CASE  
			  WHEN cf.FieldValue IS NULL THEN ''  
			  ELSE cf.FieldValue  
			 END AS 'CustomFieldValue',  
			 ulp.UserId,  
			 ulp.LoginName,  
			 ulp.LastLogin,  
			 ulp.FromDate AS User_Effective,  
			 ulp.ThruDate AS User_Expires,  
			 ulp.StatusName AS [Status],  
			 ulp.StatusThruDate,  
			 CASE  
			  WHEN ipt.Name = 'ID3' THEN 'No'--0  
			  ELSE 'Yes'--1  
			 END AS 'Is3rdPartyIDP',  
			ISNULL(pct.ProductCount, 0) AS Products,  
			 ISNULL(rt.Name, '') AS UserType,  
			 ulp.PasswordModifiedDate, 
			 UER.EnterpriseRoleName,
			 UER.RoleTemplateId,
			 CASE @sortValue  
				  WHEN 100 THEN ROW_NUMBER() OVER (ORDER BY p.FirstName + ' ' + p.LastName ASC)  
				 WHEN 101 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(pct.ProductCount,0) ASC, p.FirstName + ' ' + p.LastName ASC)  
				  WHEN 102 THEN ROW_NUMBER() OVER (ORDER BY ulp.LastLogin ASC, p.FirstName + ' ' + p.LastName ASC)  
				  WHEN 103 THEN ROW_NUMBER() OVER (ORDER BY ulp.LoginName ASC, p.FirstName + ' ' + p.LastName ASC)  
				  WHEN 104 THEN ROW_NUMBER() OVER (ORDER BY ulp.StatusName ASC, p.FirstName + ' ' + p.LastName ASC)  
				  WHEN 105 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(UE.Employee,'') ASC, p.FirstName + ' ' + p.LastName ASC)
				  WHEN 106 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(UER.EnterpriseRoleName,'') ASC, p.FirstName + ' ' + p.LastName ASC) 
				  WHEN -100 THEN ROW_NUMBER() OVER (ORDER BY p.FirstName + ' ' + p.LastName DESC)  
				 WHEN -101 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(pct.ProductCount,0)  DESC, p.FirstName + ' ' + p.LastName DESC)  
				  WHEN -102 THEN ROW_NUMBER() OVER (ORDER BY ulp.LastLogin DESC, p.FirstName + ' ' + p.LastName DESC)  
				  WHEN -103 THEN ROW_NUMBER() OVER (ORDER BY ulp.LoginName DESC, p.FirstName + ' ' + p.LastName DESC)  
				  WHEN -104 THEN ROW_NUMBER() OVER (ORDER BY ulp.StatusName DESC, p.FirstName + ' ' + p.LastName DESC)  
				  WHEN -105 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(UE.Employee,'') DESC, p.FirstName + ' ' + p.LastName DESC)  
				  WHEN -106 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(UER.EnterpriseRoleName,'') DESC, p.FirstName + ' ' + p.LastName DESC) 
				 END AS RowNumber,  
			  PNE.NotificationEmail,
			  ulp.MFAFlag,    
			  C.PlatformRoles
		  FROM #UserLogin ulp  
			 INNER JOIN Person.Person p ON p.PartyId = ulp.PersonPartyId  
			 INNER JOIN Enterprise.Party pa ON p.PartyId = pa.PartyID  
			 LEFT JOIN #PartyNotificationEmails PNE  ON ulp.PersonPartyId = PNE.PartyId
			 INNER JOIN Enterprise.PartyRelationship prs ON prs.PartyIdFrom = ulp.PersonPartyId AND prs.PartyIdTo = @PartyId  
			 INNER JOIN Enterprise.RelationshipType rst ON rst.RelationshipTypeId = prs.PartyRelationshipTypeId  
			 INNER JOIN Enterprise.RoleType rt ON (rt.PartyRoleTypeId = rst.RoleTypeIdValidFrom)  
			 INNER JOIN Ident.IdentityProviderType ipt ON ulp.IdentityProviderTypeId = ipt.IdentityProviderTypeId  
			 LEFT OUTER JOIN #ProductCount pct ON pct.PersonaId = ulp.PersonaId  
			 LEFT OUTER JOIN #CustomFields cf ON (cf.UserLoginPersonaId = ulp.UserLoginPersonaId)  
			 LEFT OUTER JOIN Enterprise.UserEmployeeId UE ON ulp.UserLoginPersonaId = UE.UserLoginPersonaId  
			 LEFT OUTER JOIN #UserEnterpriseRole UER  ON ulp.PersonaId  = UER.PersonaId
			 LEFT JOIN CTE1 C ON ulp.UserId = C.UserId 
		  WHERE  
		  ((@NOW BETWEEN prs.FromDate AND prs.ThruDate) OR (@NOW >= prs.FromDate AND prs.ThruDate IS NULL))  
		  AND (@UserType IS NULL OR @UserType = ' ALL' OR rt.[Name] IN (SELECT [value]	FROM	STRING_SPLIT(@UserType, ',')))
		  AND (@PlatformRole IS NULL OR @PlatformRole = ' ALL' OR C.UserId IS NOT NULL)
	)   
	SELECT	
		FirstName,
		MiddleName,
		LastName,
		EmployeeId,
		Title,
		Suffix,
		CustomField,
		EntepriseRoleName,
		RoleTemplateId,
		UserId,
		LoginName,
		LastLogin,
		User_Effective,
		User_Expires,
		[Status],
		StatusThruDate,
		Is3rdPartyIDP,
		PasswordModifiedDate,	
		Products,
		UserType,
		NotificationEmail,
		MFAFlag,
		PlatformRoles
	
	FROM	cteUsersFinal C
	ORDER BY RowNumber
	OPTION (RECOMPILE)

	DROP INDEX IF EXISTS [NCI_cteUserLogin_PersonPartyId] ON [dbo].[#UserLogin]
	DROP TABLE IF EXISTS #ProductCount	
	DROP TABLE IF EXISTS #UserLogin
	DROP TABLE IF EXISTS #PersonaProduct
	DROP TABLE IF EXISTS #PartyContactMechanism
	DROP TABLE IF EXISTS #ProductEnabled
	DROP TABLE IF EXISTS ##Temp_ProductEnabled

END;