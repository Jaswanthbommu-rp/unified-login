
/*
EXECUTE Enterprise.ListProductUsersForOrganization @OrgPartyId = 6768, @ProductId = 'CD,LS,OPS'
EXECUTE Enterprise.ListProductUsersForOrganization @OrgPartyId = 6768, @ProductId = '4'
*/

Create PROCEDURE Enterprise.ListProductUsersForOrganization
(@OrgPartyId INT, 
 @ProductId  NVARCHAR(100)
)
AS
    BEGIN
        DECLARE @BlueBookId INT, @Now DATETIME= GETUTCDATE();
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
             Users
             AS (SELECT ul.UserId, 
                        ul.LoginName, 
                        p.FirstName, 
                        p.LastName, 
                        cp.ProductId, 
                        sua.Value 'ProductUserName'
                 FROM ident.UserLogin ul
                      INNER JOIN ident.UserLoginPersona ulp ON ul.UserId = ulp.UserLoginId
                      INNER JOIN person.Persona p2 ON ulp.UserLoginPersonaId = p2.UserLoginPersonaId
                      INNER JOIN Person.Person p ON ul.PersonPartyId = p.partyid
                      INNER JOIN Products cp ON cp.PersonaId = p2.PersonaId
                      INNER JOIN Ident.SamlUserAttribute sua ON sua.PersonaId = cp.PersonaId
                                                                AND sua.SamlAttributeId = 1
                                                                AND sua.ProductId IN(SELECT *
                                                                                     FROM STRING_SPLIT(@ProductId, ','))
                      INNER JOIN Enterprise.DataImportMapping dim ON ULP.OrganizationPartyId = dim.PartyId
                      INNER JOIN Enterprise.DataImportApplication dia ON dim.DataImportApplicationId = dia.DataImportApplicationId
                 WHERE dia.Name = 'Bluebook'
                       AND dim.PartyId = @OrgPartyId)
             SELECT UserId, 
                    LoginName, 
                    FirstName, 
                    LastName, 
                    ProductId, 
                    ProductUserName
             FROM Users u
             WHERE ProductId IS NOT NULL;
    END;
