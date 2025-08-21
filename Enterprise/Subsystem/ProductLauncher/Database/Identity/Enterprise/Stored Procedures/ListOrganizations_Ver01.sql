CREATE PROCEDURE [Enterprise].[ListOrganizations_Ver01] 
(
    @RealPageId UNIQUEIDENTIFIER = NULL, 
    @CompanyRealPageIds VARCHAR(MAX) = NULL,
    @Filter VARCHAR(100) = NULL,
    @OrganizationTypeIds VARCHAR(MAX) = NULL,
	@CompanyPartyIds VARCHAR(MAX) = NULL  
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

    ELSE IF (@CompanyRealPageIds IS NOT NULL)
    BEGIN
       DECLARE @RealPageIdTable TABLE (RealPageId UNIQUEIDENTIFIER);
    IF @CompanyRealPageIds <> ''
    BEGIN
        INSERT INTO @RealPageIdTable (RealPageId)
        SELECT TRY_CAST(value AS UNIQUEIDENTIFIER)
        FROM STRING_SPLIT(@CompanyRealPageIds, ',')
        WHERE TRY_CAST(value AS UNIQUEIDENTIFIER) IS NOT NULL;
    END
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
            INNER JOIN @RealPageIdTable PRT ON P.RealPageId = PRT.RealPageId
        WHERE 
                @OrganizationTypeIds IS NULL OR @OrganizationTypeIds = '' 
                OR O.OrganizationTypeId IN (SELECT OrgTypeId FROM @OrgTypeIdsTable)
        ORDER BY 
            O.[Name]  
    END
   
    ELSE IF (@CompanyPartyIds IS NOT NULL)
    BEGIN
    DECLARE @PartyIdsTable TABLE (PartyId BIGINT)
 IF @CompanyPartyIds <> ''
 BEGIN
     INSERT INTO @PartyIdsTable (PartyId)
     SELECT TRY_CAST(value AS BIGINT)
     FROM STRING_SPLIT(@CompanyPartyIds, ',')
     WHERE TRY_CAST(value AS BIGINT) IS NOT NULL
 END
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
		INNER JOIN @PartyIdsTable CPT ON O.PartyId = CPT.PartyId
        WHERE 
                @OrganizationTypeIds IS NULL OR @OrganizationTypeIds = '' 
                OR O.OrganizationTypeId IN (SELECT OrgTypeId FROM @OrgTypeIdsTable)
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