--EXEC [Person].[ListUsersWithCompanyId_VER2]
CREATE PROCEDURE [Person].[ListUsersWithCompanyId_Ver2]
(
    @OrgPartyIdId BIGINT,
    @Source NVARCHAR(50) = 'BlueBook',
    @ProductId NVARCHAR(200) = NULL,
    @RowsPerPage INT = 0,
    @PageNumber INT = 1,
    @companyDomain NVARCHAR(20) = 'primary'
)
AS
BEGIN
    DECLARE @domainId INT = 0;
    DECLARE @Now DATETIME = GETUTCDATE();
    Declare @RealpageEmployeeOrgPartyID BIGINT = 0;
    
	select @RealpageEmployeeOrgPartyID = PartyId from enterprise.Party where RealpageId = '0d018e46-c20e-477d-aded-4e5a35fb8f99';

    DECLARE @ProductIdList TABLE
    (
        ProductId INT
    );
    DECLARE @ProductIdRightList TABLE
    (
        ProductId INT
    );

    DECLARE @ProductCount INT = 1;
    CREATE TABLE #UserList
    (
        UserId BIGINT,
        LoginName NVARCHAR(255),
        FirstName NVARCHAR(50),
        LastName NVARCHAR(50),
        AddressString NVARCHAR(255),
        PersonaId BIGINT,
        PreferredPhoneNumber VARCHAR(30),
        Email VARCHAR(255)
    );
    CREATE NONCLUSTERED INDEX [NC_Uerlist_userID]
    ON #UserList ([UserId] ASC);

    INSERT INTO @ProductIdList
    (
        ProductId
    )
    SELECT *
    FROM STRING_SPLIT(@ProductId, ',');

    IF
    (
        SELECT COUNT(*)FROM @ProductIdList
    ) = 0
    BEGIN
        SET @ProductCount = NULL;
    END;

    SELECT @RowsPerPage = CASE
                              WHEN @RowsPerPage <= 0 THEN
                                  2147483647
                              ELSE
                                  @RowsPerPage
                          END;

    --Preferred mobile number logic    
    DECLARE @ContactPreference TABLE
    (
        PartyId INT,
        PreferredPhoneNumber VARCHAR(30)
    );
    INSERT INTO @ContactPreference
    (
        PartyId,
        PreferredPhoneNumber
    )
    SELECT pcm.PartyId AS PartyId,
           ISNULL(tm.CountryCode, '') + tm.AreaCode + tm.PhoneNumber
    FROM Enterprise.TelecommunicationsNumber tm
        INNER JOIN Enterprise.PartyContactMechanism pcm
            ON tm.ContactMechanismID = pcm.ContactMechanismId
        INNER JOIN Enterprise.[ContactMechanismPreference] CMP
            ON CMP.ContactMechanismID = pcm.ContactMechanismId
               AND
               (
                   pcm.ThruDate IS NULL
                   OR pcm.ThruDate > GETUTCDATE()
               )
        JOIN Ident.UserLogin ul
            ON ul.PersonPartyId = pcm.PartyId
        JOIN Ident.UserLoginPersona ulp
            ON ulp.UserLoginId = ul.UserId
    WHERE ulp.OrganizationPartyId = @OrgPartyIdId; /*OPTION (RECOMPILE)*/

    --Notification Email    
    DECLARE @NotificationEmail TABLE
    (
        PartyId BIGINT,
        Email VARCHAR(255)
    );
    INSERT INTO @NotificationEmail
    (
        PartyId,
        Email
    )
    SELECT p.PartyId,
           ea.ElectronicAddressString AS NotificationEmail
    FROM Enterprise.ContactMechanismUsage cmu
        INNER JOIN Enterprise.PartyContactMechanism pcm
            ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
        INNER JOIN Enterprise.ContactMechanism cm
            ON cm.ContactMechanismID = pcm.ContactMechanismId
        INNER JOIN Enterprise.ElectronicAddress ea
            ON ea.ContactMechanismID = cm.ContactMechanismID
        INNER JOIN Enterprise.Party p
            ON p.PartyId = pcm.PartyId
        INNER JOIN Ident.UserLogin ul
            ON ul.PersonPartyId = p.PartyId
        INNER JOIN Ident.UserLoginPersona ulp
            ON ulp.UserLoginId = ul.UserId
    WHERE (
              pcm.ThruDate IS NULL
              OR pcm.ThruDate > GETUTCDATE()
          )
          AND cmu.ContactMechanismUsageTypeID = 301
          AND ulp.OrganizationPartyId = @OrgPartyIdId;

    ;WITH Products
    AS (SELECT p.PersonaId,
               pec.ProductId
        FROM Person.Persona AS p
            INNER JOIN Ident.UserLoginPersona AS ULP
                ON ULP.UserLoginPersonaId = p.UserLoginPersonaId
                   AND ULP.OrganizationPartyId = @OrgPartyIdId
            INNER JOIN Enterprise.PersonaConfiguration AS pec
                ON p.PersonaId = pec.PersonaId
                   AND pec.StatusTypeId = 8
        WHERE (
                  (@Now
              BETWEEN pec.FromDate AND pec.ThruDate
                  )
                  OR
                  (
                      @Now >= pec.FromDate
                      AND pec.ThruDate IS NULL
                  )
              )
              AND pec.ProductId NOT IN ( 14, 19, 25, 34 ) --Client Portal, Product Learning Portal, Self-provisioning portal, Benchmarking   
              AND
              (
                  (@ProductCount IS NULL)
                  OR pec.ProductId IN
                     (
                         SELECT * FROM @ProductIdList
                     )
              )),
          Users
    AS (SELECT ul.UserId,
               ul.LoginName,
               p.FirstName,
               p.LastName,
               p2.PersonaId,
               CTPREF.PreferredPhoneNumber,
               ne.Email
        FROM Ident.UserLogin AS ul
            INNER JOIN Ident.UserLoginPersona AS ulp
                ON ul.UserId = ulp.UserLoginId
            INNER JOIN Person.Persona AS p2
                ON ulp.UserLoginPersonaId = p2.UserLoginPersonaId
            INNER JOIN Person.Person AS p
                ON ul.PersonPartyId = p.PartyId
            INNER JOIN Enterprise.Party AS pa
                ON pa.PartyId = p.PartyId
            INNER JOIN Products AS cp
                ON cp.PersonaId = p2.PersonaId
            LEFT JOIN Ident.SamlUserAttribute AS sua
                ON sua.PersonaId = p2.PersonaId
                   AND sua.ProductId = cp.ProductId
                   AND sua.SamlAttributeId = 1
            LEFT OUTER JOIN @ContactPreference AS CTPREF
                ON CTPREF.PartyId = pa.PartyId
            LEFT OUTER JOIN @NotificationEmail ne
                ON ne.PartyId = p.PartyId
            LEFT OUTER JOIN enterprise.OrganizationAdminUser oau
			    ON oau.UserLoginPersonaId = ulp.UserLoginPersonaId 
        WHERE ulp.StatusTypeId = 1
              AND ulp.OrganizationPartyId = @OrgPartyIdId
              AND( ulp.IsRPEmployee <> 1 or ulp.OrganizationPartyId =@RealpageEmployeeOrgPartyID)
		      AND oau.UserLoginPersonaId IS NULL )

    --- Add the users that UL is not thier user management     
    INSERT INTO #UserList
    (
        UserId,
        LoginName,
        FirstName,
        LastName,
        PersonaId,
        PreferredPhoneNumber,
        Email
    )
    SELECT UserId,
           LoginName,
           FirstName,
           LastName,
           PersonaId,
           PreferredPhoneNumber,
           Email
    FROM Users AS u
    OPTION (RECOMPILE);

    --- Add the users that UL is the users management    
    INSERT INTO @ProductIdRightList
    (
        ProductId
    )
    SELECT ProductId
    FROM @ProductIdList
    WHERE [@ProductIdList].ProductId IN
          (
              SELECT ProductId FROM Enterprise.ProductRight
          );

    IF EXISTS (SELECT TOP 1 1 FROM @ProductIdRightList)
    BEGIN
        INSERT INTO #UserList
        (
            UserId,
            LoginName,
            FirstName,
            LastName,
            PersonaId,
            PreferredPhoneNumber,
            Email
        )
        SELECT ul.UserId,
               ul.LoginName,
               pp.FirstName,
               pp.LastName,
               p.PersonaId,
               CTPREF.PreferredPhoneNumber,
               ne.Email
        FROM Ident.UserLogin ul
            INNER JOIN Ident.UserLoginPersona ulp
                ON ul.UserId = ulp.UserLoginId
            INNER JOIN Person.Persona p
                ON ulp.UserLoginPersonaId = p.UserLoginPersonaId
            INNER JOIN Person.Person AS pp
                ON ul.PersonPartyId = pp.PartyId
            INNER JOIN Enterprise.Party pa
                ON pa.PartyId = pp.PartyId
            INNER JOIN [Security].[PersonaRole] AS pr
                ON p.PersonaId = pr.PersonaId
            INNER JOIN [Security].[Role] AS r
                ON pr.RoleId = r.RoleId
            INNER JOIN [Security].[RoleRight] AS rr
                ON r.RoleId = rr.RoleId
            INNER JOIN [Security].[Right] AS r2
                ON rr.RightId = r2.RightId
            INNER JOIN Enterprise.ProductRight AS prt
                ON r2.TargetProductId = prt.ProductId
            LEFT OUTER JOIN @ContactPreference AS CTPREF
                ON CTPREF.PartyId = pa.PartyId
            LEFT OUTER JOIN @NotificationEmail ne
                ON ne.PartyId = pp.PartyId  
            LEFT OUTER JOIN enterprise.OrganizationAdminUser oau
			    ON oau.UserLoginPersonaId = ulp.UserLoginPersonaId 
        WHERE ulp.StatusTypeId = 1
              AND prt.ProductId IN
                  (
                      SELECT ProductId FROM @ProductIdRightList
                  )
              AND p.PersonaId NOT IN
                  (
                      SELECT PE.PersonaId
                      FROM Enterprise.OrganizationAdminUser OAU
                          INNER JOIN Ident.UserLoginPersona ULP
                              ON OAU.UserLoginPersonaId = ULP.UserLoginPersonaId
                          INNER JOIN Person.Persona PE
                              ON PE.UserLoginPersonaId = ULP.UserLoginPersonaId
                  )
              AND ulp.OrganizationPartyId = @OrgPartyIdId
              AND( ulp.IsRPEmployee <> 1 or ulp.OrganizationPartyId =@RealpageEmployeeOrgPartyID)
		      AND oau.UserLoginPersonaId IS NULL 
        OPTION (RECOMPILE);
    END;

    ;WITH totalusers (UserId, LoginName, FirstName, LastName, PersonaId, PreferredPhoneNumber, Email)
    AS (SELECT DISTINCT
               UserId,
               LoginName,
               FirstName,
               LastName,
               PersonaId,
               PreferredPhoneNumber,
               Email
        FROM #UserList ul)
    SELECT UserId,
           LoginName,
           FirstName,
           LastName,
           PersonaId,
           PreferredPhoneNumber,
           COUNT(1) OVER () AS TotalRecords,
           Email
    FROM totalusers
    ORDER BY UserId OFFSET ((@PageNumber - 1) * @RowsPerPage) ROWS FETCH NEXT (@RowsPerPage) ROWS ONLY
    OPTION (RECOMPILE);
END;
