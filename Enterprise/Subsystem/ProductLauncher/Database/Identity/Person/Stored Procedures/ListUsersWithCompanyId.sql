CREATE PROCEDURE [Person].[ListUsersWithCompanyId]
(@CompanyId   NVARCHAR(100) = NULL, 
 @Source      NVARCHAR(50)  = 'BlueBook', 
 @ProductId   NVARCHAR(200) = NULL, 
 @RowsPerPage INT           = 0, 
 @PageNumber  INT           = 1
)
AS
    BEGIN
        DECLARE @Now DATETIME= GETUTCDATE();
        SELECT @RowsPerPage = CASE
                                  WHEN @RowsPerPage <= 0
                                  THEN 2147483647
                                  ELSE @RowsPerPage
                              END;
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
             --ConcatProducts
             --AS (SELECT DISTINCT 
             --           P2.PersonaId, 
             --           SUBSTRING(
             --    (
             --        SELECT ',' + CONVERT(NVARCHAR(100), PR1.ProductId) AS [text()]
             --        FROM Products PR1
             --        WHERE PR1.PersonaId = P2.PersonaId
             --        ORDER BY PR1.PersonaId FOR XML PATH('')
             --    ), 2, 1000) ProductId
             --    FROM Products P2),
             Emails
             AS (SELECT p.PartyId, 
                        ea.ElectronicAddressString AS AddressString
                 FROM Enterprise.ContactMechanismUsage cmu
                      JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
                      JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
                      JOIN Enterprise.ElectronicAddress ea ON ea.ContactMechanismID = cm.ContactMechanismID
                      JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
                                                 AND (pcm.ThruDate IS NULL
                                                      OR pcm.ThruDate > GETUTCDATE())),
             Users
             AS (SELECT ul.UserId, 
                        ul.LoginName, 
                        p.FirstName, 
                        p.LastName, 
                        e.AddressString 
                 -- ,pa.realpageid 'RealPageId'						

                 FROM ident.UserLogin ul
                      INNER JOIN ident.UserLoginPersona ulp ON ul.UserId = ulp.UserLoginId
                      INNER JOIN person.Persona p2 ON ulp.UserLoginPersonaId = p2.UserLoginPersonaId
                      INNER JOIN Person.Person p ON ul.PersonPartyId = p.partyid
                      INNER JOIN Enterprise.Party pa ON pa.partyid = p.PartyId
                      INNER JOIN Products cp ON cp.PersonaId = p2.PersonaId
                      INNER JOIN Ident.SamlUserAttribute sua ON sua.PersonaId = p2.PersonaId
                                                                AND sua.ProductId = cp.ProductId
                                                                AND sua.SamlAttributeId = 1
                      INNER JOIN Enterprise.DataImportMapping dim ON ULP.OrganizationPartyId = dim.PartyId
                      INNER JOIN Enterprise.DataImportApplication dia ON dim.DataImportApplicationId = dia.DataImportApplicationId
                      LEFT JOIN Emails e ON e.partyid = pa.partyid
                 WHERE(
                 (
                     SELECT COUNT(*)
                     FROM STRING_SPLIT(@companyid, ',')
                 ) = 0)
                      OR dim.sourceId IN
                 (
                     SELECT *
                     FROM STRING_SPLIT(@companyid, ',')
                 ))
             SELECT DISTINCT 
                    UserId, 
                    LoginName, 
                    FirstName, 
                    LastName
             FROM Users u
             ORDER BY UserId
             OFFSET((@PageNumber - 1) * @RowsPerPage) ROWS FETCH NEXT(@RowsPerPage) ROWS ONLY;
        --WHERE ProductId IS NOT NULL;
    END;