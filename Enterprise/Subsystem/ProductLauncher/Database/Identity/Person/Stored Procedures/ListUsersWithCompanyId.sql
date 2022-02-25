CREATE PROCEDURE [Person].[ListUsersWithCompanyId] 
(@CompanyId   VARCHAR(100) = NULL, 
 @Source      VARCHAR(50)  = 'BlueBook', 
 @ProductId   VARCHAR(200) = NULL, 
 @RowsPerPage INT           = 0, 
 @PageNumber  INT           = 1
)
AS
    BEGIN
        DECLARE @Now DATETIME= GETUTCDATE();
        DECLARE @ProductIdList TABLE(ProductId INT);
        DECLARE @CompanyIdList TABLE(CompanyId NVARCHAR(100));
        DECLARE @CompanyPartyIdList TABLE(PartyId BIGINT);
        CREATE TABLE #UserList
        (UserId        BIGINT, 
         LoginName     NVARCHAR(255), 
         FirstName     NVARCHAR(50), 
         LastName      NVARCHAR(50), 
         AddressString NVARCHAR(255),
		 PersonaId	   BIGINT
        );

        INSERT INTO @ProductIdList(ProductId)
               SELECT value
               FROM STRING_SPLIT(@ProductId, ',');
        INSERT INTO @CompanyIdList(CompanyId)
        (
            SELECT value
            FROM STRING_SPLIT(@companyid, ',')
        );
        IF
        (
            SELECT COUNT(*)
            FROM @ProductIdList
        ) = 0
            BEGIN
                --SET @ProductCount = 0;
				INSERT INTO @ProductIdList ( ProductId )
					SELECT productid FROM enterprise.Product WHERE ProductId NOT IN (14, 19, 24, 25, 34, 39, 45)
        END;

        IF
        ( SELECT COUNT(*) FROM @CompanyIdList ) = 0
        BEGIN
			IF ( SELECT COUNT(*) FROM @ProductIdList ) = 0
			BEGIN
				insert INTO @CompanyPartyIdList ( PartyId )
					SELECT partyid from enterprise.Organization
			END
			ELSE
            BEGIN
				insert INTO @CompanyPartyIdList ( PartyId )
					SELECT OP.Partyid FROM enterprise.OrganizationProduct OP
						INNER JOIN @ProductIdList PL ON PL.ProductId = OP.ProductId
						WHERE OP.ThruDate IS NULL
			END
		END
		ELSE
		BEGIN
			insert into @CompanyPartyIdList ( PartyId )
				SELECT dm.PartyId FROM Enterprise.DataImportMapping dm
					INNER JOIN @CompanyIdList cl ON cl.CompanyId = dm.SourceId and dm.DataImportApplicationId = '2'
		END

        SELECT @RowsPerPage = CASE
                                  WHEN @RowsPerPage <= 0
                                  THEN 2147483647
                                  ELSE @RowsPerPage
                              END;
							  
        ;WITH Products
             AS (SELECT DISTINCT 
                        p.PersonaID, 
                        pec.ProductId
                 FROM Person.Persona AS p
                      INNER JOIN Ident.UserLoginPersona AS ULP ON ULP.UserLoginPersonaId = p.UserLoginPersonaId
                      INNER JOIN @CompanyPartyIdList cpl ON cpl.PartyId = ulp.OrganizationPartyId
					  INNER JOIN Enterprise.PersonaConfiguration AS pec ON p.PersonaId = pec.PersonaId 
                 WHERE
				 ((@NOW BETWEEN pec.FromDate AND pec.ThruDate)
                       OR (@NOW >= pec.FromDate
                           AND pec.ThruDate IS NULL))
					  AND pec.StatusTypeId = '8'
                      AND pec.ProductId NOT IN (14, 19, 24, 25, 34, 39) --Client Portal, Product Learning Portal, Black Book, Self-provisioning portal, Benchmarking, Integration Marketplace
                      AND --((1 = CASE WHEN @ProductCount = 0 THEN 1 ELSE 0 END)
                           --OR 
						   pec.ProductId IN
							(
								SELECT *
								FROM @ProductIdList
							)--)
					 )
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
             ,Emails
             AS (SELECT p.PartyId, 
                        ea.ElectronicAddressString AS AddressString
                 FROM Enterprise.ContactMechanismUsage AS cmu
                      JOIN Enterprise.PartyContactMechanism AS pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
                      JOIN Enterprise.ContactMechanism AS cm ON cm.ContactMechanismID = pcm.ContactMechanismId
                      JOIN Enterprise.ElectronicAddress AS ea ON ea.ContactMechanismID = cm.ContactMechanismID
                      JOIN Enterprise.Party AS p ON p.PartyId = pcm.PartyId
                                                    AND (pcm.ThruDate IS NULL
                                                         OR pcm.ThruDate > GETUTCDATE()))
             ,Users
             AS (SELECT ul.UserId, 
                        ul.LoginName, 
                        p.FirstName, 
                        p.LastName, 
                        e.AddressString,
						cp.PersonaId
                 -- ,pa.realpageid 'RealPageId'						

                 FROM ident.UserLogin AS ul
                      INNER JOIN ident.UserLoginPersona AS ulp ON ul.UserId = ulp.UserLoginId
                      INNER JOIN @CompanyPartyIdList cpl ON cpl.PartyId = ulp.OrganizationPartyId
					  INNER JOIN person.Persona AS p2 ON ulp.UserLoginPersonaId = p2.UserLoginPersonaId
                      INNER JOIN Person.Person AS p ON ul.PersonPartyId = p.partyid
                      INNER JOIN Enterprise.Party AS pa ON pa.partyid = p.PartyId
                      INNER JOIN Products AS cp ON cp.PersonaId = p2.PersonaId
                      LEFT JOIN Ident.SamlUserAttribute AS sua ON sua.PersonaId = p2.PersonaId
                                                                  AND sua.ProductId = cp.ProductId
                                                                  AND sua.SamlAttributeId = 1                      
                      LEFT JOIN Emails AS e ON e.partyid = pa.partyid
                 WHERE ulp.StatusTypeId = 1
				)
             INSERT INTO #UserList
             (UserId, 
              LoginName, 
              FirstName, 
              LastName,
			  PersonaId
             )
                    SELECT UserId, 
                           LoginName, 
                           FirstName, 
                           LastName,
						   PersonaId
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
                 LastName,
				 PersonaId
                )
                       SELECT ul.UserId, 
                              ul.LoginName, 
                              pp.FirstName, 
                              pp.LastName,
							  p.PersonaId
                       FROM Ident.UserLogin ul
                            INNER JOIN Ident.UserLoginPersona ulp ON ul.UserId = ulp.UserLoginId
                            INNER JOIN @CompanyPartyIdList cpl ON cpl.PartyId = ulp.OrganizationPartyId
							INNER JOIN Person.Persona p ON ulp.UserLoginPersonaId = p.UserLoginPersonaId
                            INNER JOIN Person.Person AS pp ON ul.PersonPartyId = pp.partyid
                            INNER JOIN Enterprise.Party pa ON pa.partyid = pp.PartyId
                            INNER JOIN Enterprise.PersonaPrivilege ppv ON p.PersonaId = ppv.PersonaId
                            INNER JOIN Enterprise.Role r ON R.RoleID = ppv.RoleID
                            INNER JOIN Enterprise.[Right] r2 ON r.RoleID = r2.RoleID
                            INNER JOIN Enterprise.RightValueType rvt ON r2.RightValueTypeId = rvt.RightValueTypeId							
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
                           SELECT	pe.PersonaId
							FROM Enterprise.OrganizationAdminUser OAU
								INNER JOIN Ident.UserLoginPersona ULP ON OAU.UserLoginPersonaId = ULP.UserLoginPersonaId
								INNER JOIN Person.Persona PE ON PE.UserLoginPersonaId = ULP.UserLoginPersonaId
                       );
        END;

        SELECT DISTINCT 
               UserId, 
               LoginName, 
               FirstName, 
               LastName,
			   PersonaId
        FROM #UserList ul
        ORDER BY UserId
        OFFSET((@PageNumber - 1) * @RowsPerPage) ROWS FETCH NEXT(@RowsPerPage) ROWS ONLY;
        --WHERE ProductId IS NOT NULL

    END;
