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
        DECLARE @ProductIdList TABLE(ProductId INT);
        DECLARE @CompanyIdList TABLE(CompanyId INT);
        DECLARE @ProductCount INT= 1;
        DECLARE @CompanyIdCount INT= 1;
        CREATE TABLE #UserList
        (UserId        BIGINT, 
         LoginName     NVARCHAR(255), 
         FirstName     NVARCHAR(50), 
         LastName      NVARCHAR(50), 
         AddressString NVARCHAR(255)
        );
        INSERT INTO @ProductIdList(ProductId)
               SELECT *
               FROM STRING_SPLIT(@ProductId, ',');
        INSERT INTO @CompanyIdList(CompanyId)
        (
            SELECT *
            FROM STRING_SPLIT(@companyid, ',')
        );
        IF
        (
            SELECT COUNT(*)
            FROM @ProductIdList
        ) = 0
            BEGIN
                SET @ProductCount = NULL;
        END;
        IF
        (
            SELECT COUNT(*)
            FROM @CompanyIdList
        ) = 0
            BEGIN
                SET @CompanyIdCount = NULL;
        END;
        SELECT @RowsPerPage = CASE
                                  WHEN @RowsPerPage <= 0
                                  THEN 2147483647
                                  ELSE @RowsPerPage
                              END;
        WITH Products
             AS (SELECT DISTINCT 
                        p.PersonaID, 
                        pp.ProductId
                 FROM Person.Persona AS p
                      INNER JOIN Ident.UserLoginPersona AS ULP ON ULP.UserLoginPersonaId = p.UserLoginPersonaId
                      INNER JOIN Enterprise.PersonaConfiguration AS pec ON p.PersonaId = pec.PersonaId
                      INNER JOIN Enterprise.ProductConfiguration AS prc ON pec.ConfigurationId = prc.ConfigurationId
                      INNER JOIN Enterprise.Product AS pp ON pp.ProductId = pec.ProductId
                      INNER JOIN Enterprise.ProductSetting AS ps ON prc.ProductSettingId = ps.ProductSettingId
                                                                    AND ps.Value = '8'
                      INNER JOIN Enterprise.ProductSettingType AS pst ON ps.ProductSettingTypeId = pst.ProductSettingTypeId
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
                      AND ((@ProductCount IS NULL)
                           OR pp.ProductId IN
                 (
                     SELECT *
                     FROM @ProductIdList
                 ))),
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
                 FROM Enterprise.ContactMechanismUsage AS cmu
                      JOIN Enterprise.PartyContactMechanism AS pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
                      JOIN Enterprise.ContactMechanism AS cm ON cm.ContactMechanismID = pcm.ContactMechanismId
                      JOIN Enterprise.ElectronicAddress AS ea ON ea.ContactMechanismID = cm.ContactMechanismID
                      JOIN Enterprise.Party AS p ON p.PartyId = pcm.PartyId
                                                    AND (pcm.ThruDate IS NULL
                                                         OR pcm.ThruDate > GETUTCDATE())),
             Users
             AS (SELECT ul.UserId, 
                        ul.LoginName, 
                        p.FirstName, 
                        p.LastName, 
                        e.AddressString 
                 -- ,pa.realpageid 'RealPageId'						

                 FROM ident.UserLogin AS ul
                      INNER JOIN ident.UserLoginPersona AS ulp ON ul.UserId = ulp.UserLoginId
                      INNER JOIN person.Persona AS p2 ON ulp.UserLoginPersonaId = p2.UserLoginPersonaId
                      INNER JOIN Person.Person AS p ON ul.PersonPartyId = p.partyid
                      INNER JOIN Enterprise.Party AS pa ON pa.partyid = p.PartyId
                      INNER JOIN Products AS cp ON cp.PersonaId = p2.PersonaId
                      LEFT JOIN Ident.SamlUserAttribute AS sua ON sua.PersonaId = p2.PersonaId
                                                                  AND sua.ProductId = cp.ProductId
                                                                  AND sua.SamlAttributeId = 1                      
                      INNER JOIN enterprise.VW_DataImportMapping VDIM ON VDIM.PartyID = ULP.OrganizationPartyId
                      LEFT JOIN Emails AS e ON e.partyid = pa.partyid
                 WHERE ulp.StatusTypeId = 1
                       AND ((@CompanyIdCount IS NULL)
                            OR VDIM.CompanyMasterId IN
                 (
                     SELECT *
                     FROM @CompanyIdList AS cil
                 )))
             INSERT INTO #UserList
             (UserId, 
              LoginName, 
              FirstName, 
              LastName
             )
                    SELECT UserId, 
                           LoginName, 
                           FirstName, 
                           LastName
                    FROM Users AS u;
        IF EXISTS
        (
            SELECT ProductId
            FROM @ProductIdList
            WHERE [@ProductIdList].ProductId IN(45)
        )
            BEGIN
                INSERT INTO #UserList
                (UserId, 
                 LoginName, 
                 FirstName, 
                 LastName
                )
                       SELECT ul.UserId, 
                              ul.LoginName, 
                              pp.FirstName, 
                              pp.LastName
                       FROM Ident.UserLogin ul
                            INNER JOIN Ident.UserLoginPersona ulp ON ul.UserId = ulp.UserLoginId
                            INNER JOIN Person.Persona p ON ulp.UserLoginPersonaId = p.UserLoginPersonaId
                            INNER JOIN Person.Person AS pp ON ul.PersonPartyId = pp.partyid
                            INNER JOIN Enterprise.Party pa ON pa.partyid = pp.PartyId
                            INNER JOIN Enterprise.PersonaPrivilege ppv ON p.PersonaId = ppv.PersonaId
                            --INNER JOIN Enterprise.PersonaConfiguration pc ON p.PersonaId = pc.PersonaId
                            INNER JOIN Enterprise.Role r ON R.RoleID = ppv.RoleID
                            INNER JOIN Enterprise.[Right] r2 ON r.RoleID = r2.RoleID
                            INNER JOIN Enterprise.RightValueType rvt ON r2.RightValueTypeId = rvt.RightValueTypeId							
                            INNER JOIN enterprise.VW_DataImportMapping VDIM ON VDIM.PartyID = ULP.OrganizationPartyId
                            LEFT JOIN
                       (
                           SELECT p.PartyId, 
                                  ea.ElectronicAddressString AS AddressString
                           FROM Enterprise.ContactMechanismUsage AS cmu
                                JOIN Enterprise.PartyContactMechanism AS pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
                                JOIN Enterprise.ContactMechanism AS cm ON cm.ContactMechanismID = pcm.ContactMechanismId
                                JOIN Enterprise.ElectronicAddress AS ea ON ea.ContactMechanismID = cm.ContactMechanismID
                                JOIN Enterprise.Party AS p ON p.PartyId = pcm.PartyId
                                                              AND (pcm.ThruDate IS NULL
                                                                   OR pcm.ThruDate > GETUTCDATE())
                       ) Emails ON Emails.PartyId = pa.PartyId
                       WHERE ulp.StatusTypeId = 1
                             AND --pc.ProductId IN (SELECT ProductId FROM @ProductIdList pil WHERE pil.ProductId IN (45)) AND
                             rvt.ShortName IN
                       (N'ViewCIMPLQuestions', N'EmployeeViewCIMPLQuestions'
                       )
                             AND P.PersonaId NOT IN
                       (
                           SELECT pe.PersonaId
                           FROM Enterprise.MasterConfigurationType mct
                                INNER JOIN Enterprise.MasterSettingType MST ON mst.MasterConfigurationTypeId = mct.MasterCOnfigurationTypeId
                                INNER JOIN Enterprise.MasterSetting ms ON ms.MasterSettingTypeId = mst.MasterSettingTYpeId
                                INNER JOIN Enterprise.Party p ON CONVERT(NVARCHAR(40), p.RealPageId) = ms.Value
                                INNER JOIN ident.UserLogin ul ON UL.PersonPartyId = p.PartyId
                                INNER JOIN Ident.UserLoginPersona ulp ON ul.UserId = ulp.UserLoginId
                                INNER JOIN Person.Persona pe ON pe.UserLoginPersonaId = ulp.UserLoginPersonaId
                           WHERE mct.Name = 'Organization'
                                 AND mst.Name = 'RealPageEmployeeAccessID'
                       )  AND ((@CompanyIdCount IS NULL)
                            OR VDIM.CompanyMasterId IN
						 (
							 SELECT *
							 FROM @CompanyIdList AS cil
						 ));
        END;
        SELECT DISTINCT 
               UserId, 
               LoginName, 
               FirstName, 
               LastName
        FROM #UserList ul
        ORDER BY UserId
        OFFSET((@PageNumber - 1) * @RowsPerPage) ROWS FETCH NEXT(@RowsPerPage) ROWS ONLY;
        --WHERE ProductId IS NOT NULL

    END;