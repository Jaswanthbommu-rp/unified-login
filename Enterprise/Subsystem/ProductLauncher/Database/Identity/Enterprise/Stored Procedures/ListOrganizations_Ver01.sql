CREATE PROCEDURE [Enterprise].[ListOrganizations_Ver01] 
(
    @RealPageId UNIQUEIDENTIFIER = NULL,  
    @Filter VARCHAR(100) = NULL,
    @OrganizationTypeIds VARCHAR(MAX) = NULL
)
AS  
BEGIN 
	IF @Filter = ''
	BEGIN
		SET @Filter = NULL
	END

    -- Convert @OrganizationIds to a table variable
    DECLARE @OrgTypeIdsTable TABLE (OrgTypeId INT)
    IF @OrganizationTypeIds IS NOT NULL AND @OrganizationTypeIds <> ''
    BEGIN
        INSERT INTO @OrgTypeIdsTable (OrgTypeId)
        SELECT value FROM STRING_SPLIT(@OrganizationTypeIds, ',')
    END
	
	IF (@RealPageId IS NOT NULL)
	BEGIN

		SELECT 
			O.PartyId,  
			O.[Name],  
			O.IsActive,  
			P.RealPageId 'OrganizationRealPageId',  
			ISNULL(D.MasterId, 0)  AS 'BooksMasterId',  
			ISNULL(D.CompanyMasterId, 0)  AS 'BooksCustomerMasterId',  
			'RealPageEmployeeAccessID' AS 'SettingName',  
			P2.RealPageId AS 'PersonRealPageId',
			OD.[Name] AS Domain
		FROM 
		Enterprise.Organization O WITH (NOLOCK) 
		INNER JOIN Enterprise.OrganizationDomain OD WITH (NOLOCK) ON O.OrganizationDomainId = OD.OrganizationDomainId	
		INNER JOIN Enterprise.Party P WITH (NOLOCK) ON O.PartyId = P.PartyId  
		INNER JOIN Enterprise.VW_DataImportMapping D WITH (NOLOCK) ON O.PartyId = D.PartyId  
		INNER JOIN Enterprise.OrganizationAdminUser OAU WITH (NOLOCK) ON OAU.OrganizationPartyId = O.PartyId  
		INNER JOIN Ident.UserLoginPersona ULP WITH (NOLOCK) ON oau.UserLoginPersonaId = ulp.UserLoginPersonaId  
		INNER JOIN Ident.UserLogin UL WITH (NOLOCK) ON UL.UserId = ulp.UserLoginId  
		INNER JOIN Enterprise.Party P2 WITH (NOLOCK) ON P2.PartyId = UL.PersonPartyId  
		WHERE 
			P.RealPageId = @RealPageId
            AND (
                @OrganizationTypeIds IS NULL OR @OrganizationTypeIds = '' 
                OR O.OrganizationTypeId IN (SELECT OrgTypeId FROM @OrgTypeIdsTable)
            )
		ORDER BY 
			O.[Name]  

	END

	ELSE IF (@Filter IS NOT NULL)
	BEGIN
		
		SELECT 
			O.PartyId,  
			O.[Name],  
			O.IsActive,  
			P.RealPageId 'OrganizationRealPageId',  
			ISNULL(D.MasterId, 0)  AS 'BooksMasterId',  
			ISNULL(D.CompanyMasterId, 0)  AS 'BooksCustomerMasterId',  
			'RealPageEmployeeAccessID' AS 'SettingName',  
			P2.RealPageId AS 'PersonRealPageId',
			OD.[Name] AS Domain
		FROM 
		Enterprise.Organization O WITH (NOLOCK) 
		INNER JOIN Enterprise.OrganizationDomain OD WITH (NOLOCK) ON O.OrganizationDomainId = OD.OrganizationDomainId
		INNER JOIN Enterprise.Party P WITH (NOLOCK) ON O.PartyId = P.PartyId  
		INNER JOIN Enterprise.VW_DataImportMapping D WITH (NOLOCK) ON O.PartyId = D.PartyId  
		INNER JOIN Enterprise.OrganizationAdminUser OAU WITH (NOLOCK) ON OAU.OrganizationPartyId = O.PartyId  
		INNER JOIN Ident.UserLoginPersona ULP WITH (NOLOCK) ON oau.UserLoginPersonaId = ulp.UserLoginPersonaId  
		INNER JOIN Ident.UserLogin UL WITH (NOLOCK) ON UL.UserId = ulp.UserLoginId  
		INNER JOIN Enterprise.Party P2 WITH (NOLOCK) ON P2.PartyId = UL.PersonPartyId  
		WHERE 
			O.[Name] LIKE '%'+@Filter+'%'
            AND (
                @OrganizationTypeIds IS NULL OR @OrganizationTypeIds = '' 
                OR O.OrganizationTypeId IN (SELECT OrgTypeId FROM @OrgTypeIdsTable)
            )
		ORDER BY 
			O.[Name] 

	END

	ELSE 
	BEGIN

		SELECT 
			O.PartyId,  
			O.[Name],  
			O.IsActive,  
			P.RealPageId 'OrganizationRealPageId',  
			ISNULL(D.MasterId, 0)  AS 'BooksMasterId',  
			ISNULL(D.CompanyMasterId, 0)  AS 'BooksCustomerMasterId',  
			'RealPageEmployeeAccessID' AS 'SettingName',  
			P2.RealPageId AS 'PersonRealPageId',
			OD.[Name] AS Domain
		FROM 
		Enterprise.Organization O WITH (NOLOCK) 
		INNER JOIN Enterprise.OrganizationDomain OD WITH (NOLOCK) ON O.OrganizationDomainId = OD.OrganizationDomainId
		INNER JOIN Enterprise.Party P WITH (NOLOCK) ON O.PartyId = P.PartyId  
		INNER JOIN Enterprise.VW_DataImportMapping D WITH (NOLOCK) ON O.PartyId = D.PartyId  
		INNER JOIN Enterprise.OrganizationAdminUser OAU WITH (NOLOCK) ON OAU.OrganizationPartyId = O.PartyId  
		INNER JOIN Ident.UserLoginPersona ULP WITH (NOLOCK) ON oau.UserLoginPersonaId = ulp.UserLoginPersonaId  
		INNER JOIN Ident.UserLogin UL WITH (NOLOCK) ON UL.UserId = ulp.UserLoginId  
		INNER JOIN Enterprise.Party P2 WITH (NOLOCK) ON P2.PartyId = UL.PersonPartyId  
		WHERE 
            (@OrganizationTypeIds IS NULL OR @OrganizationTypeIds = '' 
                OR O.OrganizationTypeId IN (SELECT OrgTypeId FROM @OrgTypeIdsTable)
            )
		ORDER BY 
			O.[Name]  

	END

END;