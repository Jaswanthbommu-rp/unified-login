CREATE PROCEDURE [Enterprise].[ListOrganizations_Ver01] 
    @RealPageId UNIQUEIDENTIFIER = NULL,
    @Filter VARCHAR(100) = NULL
AS
     BEGIN
         SELECT O.PartyId,
                O.Name,
                O.IsActive,
                P.RealPageId 'OrganizationRealPageId',
                COALESCE(ISNULL(D.MasterId, 0),0)  AS 'BooksMasterId',
				COALESCE(ISNULL(D.CompanyMasterId, 0), 0)  AS 'BooksCustomerMasterId',
                'RealPageEmployeeAccessID' AS 'SettingName',
                P2.RealPageId AS 'PersonRealPageId',
                UL.LoginName,
				OD.Name as Domain
         FROM Enterprise.Organization O
              INNER JOIN Enterprise.Party P ON O.PartyId = P.PartyId
              INNER JOIN Enterprise.VW_DataImportMapping D ON O.PartyId = D.PartyId
			  INNER JOIN Enterprise.OrganizationDomain OD on O.OrganizationDomainId = OD.OrganizationDomainId
              INNER JOIN Enterprise.OrganizationAdminUser OAU ON OAU.OrganizationPartyId = O.PartyId
			  INNER JOIN Ident.UserLoginPersona ULP ON oau.UserLoginPersonaId = ulp.UserLoginPersonaId
			  INNER JOIN Ident.UserLogin UL ON UL.UserId = ulp.UserLoginId
			  INNER JOIN Enterprise.Party P2 ON P2.PartyId = UL.PersonPartyId
               AND (@Filter IS NULL OR o.Name like '%'+@Filter+'%')
			   AND (@RealPageID IS NULL OR P.RealPageId = @RealPageId)
		order by O.Name
     END; 