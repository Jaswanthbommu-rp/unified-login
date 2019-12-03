/*
EXECUTE Person.ListUsersWithCompanyId @CompanyId = 379, @ProductId = 'CD,LS,OPS'
EXECUTE Person.ListUsersWithCompanyId @CompanyId = 4638, @ProductId = '1,2,3'
EXECUTE Person.ListUsersWithCompanyId @CompanyId = '379,4638',  @ProductId = 'CD,LS,OPS'

*/

Create PROCEDURE Enterprise.ListProductUsersForOrganization
(@OrgPartyId   NVARCHAR(100)  = NULL, 
 @ProductId NVARCHAR(200) = NULL
)
AS
    BEGIN
        DECLARE @Now DATETIME= GETUTCDATE();
        WITH Products
             AS (SELECT DISTINCT 
						p.PersonaID, 
                        pp.ProductId
                 FROM Person.Persona p
                      INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = p.UserLoginPersonaId
                      INNER JOIN Enterprise.PersonaConfiguration pec ON p.PersonaId = pec.PersonaId
                      INNER JOIN Enterprise.ProductConfiguration prc ON pec.ConfigurationId = prc.ConfigurationId
                      INNER JOIN Enterprise.Product pp ON pp.ProductId = pec.ProductId
                      INNER JOIN Enterprise.ProductSetting ps ON prc.ProductSettingId = ps.ProductSettingId
                                                                 AND ps.Value = '8'
                      INNER JOIN Enterprise.ProductSettingType pst ON ps.ProductSettingTypeId = pst.ProductSettingTypeId
                                                                      AND pst.Name = 'ProductStatus'					  
                 WHERE((@NOW BETWEEN pec.FromDate AND pec.ThruDate)
                       OR (@NOW >= pec.FromDate
                           AND pec.ThruDate IS NULL))
                      AND ((@NOW BETWEEN prc.FromDate AND prc.ThruDate)
                           OR (@NOW >= prc.FromDate
                               AND prc.ThruDate IS NULL))
                      AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate)
                           OR (@NOW >= ps.FromDate
                               AND ps.ThruDate IS NULL))
                      AND pec.ProductId NOT IN(14, 19, 24, 25, 34, 39) --Client Portal, Product Learning Portal, Black Book, Self-provisioning portal, Benchmarking, Integration Marketplace
                      AND (
							 (
								 SELECT COUNT(*)
								 FROM STRING_SPLIT(@ProductId, ',')
							 ) = 0)
								  OR pp.ProductId IN
							 (
								 SELECT *
								 FROM STRING_SPLIT(@ProductId, ',')
							 )),
             ConcatProducts
             AS (SELECT DISTINCT 
                        P2.PersonaId, 
                        SUBSTRING(
                 (
                     SELECT ',' + CONVERT(NVARCHAR(100), PR1.ProductId) AS [text()]
                     FROM Products PR1
                     WHERE PR1.PersonaId = P2.PersonaId
                     ORDER BY PR1.PersonaId FOR XML PATH('')
                 ), 2, 1000) ProductId
                 FROM Products P2),
             Users
             AS (SELECT ul.UserId, 
                        ul.LoginName, 
                        p.FirstName, 
                        p.LastName,                        
                        pa.realpageid 'RealPageId', 
                        dim.SourceId, 
                        cp.ProductId,
						sua.Value 'ProductUserName'
                FROM ident.UserLogin ul
                      INNER JOIN ident.UserLoginPersona ulp ON ul.UserId = ulp.UserLoginId
                      INNER JOIN person.Persona p2 ON ulp.UserLoginPersonaId = p2.UserLoginPersonaId
                      INNER JOIN Person.Person p ON ul.PersonPartyId = p.partyid
                      INNER JOIN Enterprise.Party pa ON pa.partyid = p.PartyId
                      INNER JOIN ConcatProducts cp ON cp.PersonaId = p2.PersonaId
					  INNER JOIN Ident.SamlUserAttribute sua ON sua.PersonaId = p2.PersonaId 
					  AND sua.ProductId =  cp.ProductId	  AND sua.SamlAttributeId = 1
                      INNER JOIN Enterprise.DataImportMapping dim ON ULP.OrganizationPartyId = dim.PartyId
                      INNER JOIN Enterprise.DataImportApplication dia ON dim.DataImportApplicationId = dia.DataImportApplicationId					 
                      
                 WHERE
				 (
							 (
								 SELECT COUNT(*)
								 FROM STRING_SPLIT(@OrgPartyId, ',')
							 ) = 0)
								  OR dim.PartyId  IN
							 (
								 SELECT *
								 FROM STRING_SPLIT(@OrgPartyId, ',')
							 ))
             SELECT UserId, 
                    LoginName, 
                    FirstName, 
                    LastName,                  
                    ProductId,
					ProductUserName
             FROM Users u
             WHERE ProductId IS NOT NULL;
    END;