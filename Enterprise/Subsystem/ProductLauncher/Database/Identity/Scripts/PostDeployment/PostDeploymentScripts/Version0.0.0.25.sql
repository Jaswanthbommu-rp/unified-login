--Ident.UserLogin, Person.Persona, ContactMechanismID Chnages
--Populate UserId in Persona Table.
UPDATE A
  SET
      A.UserId = B.UserId
FROM Person.Persona A
     INNER JOIN [Enterprise].[PersonaIdentityUserLogin] B ON A.PersonaId = B.PersonaID;

--declaration Block
DECLARE @IdentityProviderSettingTypeID INT;
DECLARE @DBName VARCHAR(100);
DECLARE @ServerName VARCHAR(100);
DECLARE @Realpageid UNIQUEIDENTIFIER;
DECLARE @identityProviderTypeId INT;
DECLARE @ContactMechanismId INT;
SELECT *
FROM ident.identityprovidertype;
SELECT *
FROM [Ident].[IdentityProviderSettingType];
SELECT *
FROM [Ident].[IdentityProviderSetting];
SELECT DISTINCT
       name
FROM [Ident].[IdentityProviderSettingType];
EXEC Person.CreateContactMechanism
     @ContactMechanismId = @ContactMechanismId OUTPUT;
EXECUTE [Ident].[CreateIdentityProviderType]
        @Name = 'OktaCamden',
        @Description = 'Okta Provider For Camden',
        @ContactMechanismId = @ContactMechanismId;
SET @ContactMechanismId = NULL;
EXEC Person.CreateContactMechanism
     @ContactMechanismId = @ContactMechanismId OUTPUT;
EXECUTE [Ident].[CreateIdentityProviderType]
        @Name = 'OktaAmli',
        @Description = 'Okta Provider For Amli',
        @ContactMechanismId = @ContactMechanismId;
SELECT @IdentityProviderTypeId = IdentityProviderTypeId
FROM Ident.IdentityProviderType
WHERE Name = 'OktaCamden';
EXECUTE [Ident].[CreateIdentityProviderSettingType]
        @IdentityProviderTypeId = @IdentityProviderTypeId,
        @Name = 'AuthenticationMode';
EXECUTE [Ident].[CreateIdentityProviderSettingType]
        @IdentityProviderTypeId = @IdentityProviderTypeId,
        @Name = 'AuthenticationType';
EXECUTE [Ident].[CreateIdentityProviderSettingType]
        @IdentityProviderTypeId = @IdentityProviderTypeId,
        @Name = 'AuthorityUri';
EXECUTE [Ident].[CreateIdentityProviderSettingType]
        @IdentityProviderTypeId = @IdentityProviderTypeId,
        @Name = 'Caption';
EXECUTE [Ident].[CreateIdentityProviderSettingType]
        @IdentityProviderTypeId = @IdentityProviderTypeId,
        @Name = 'ClientSecret';
EXECUTE [Ident].[CreateIdentityProviderSettingType]
        @IdentityProviderTypeId = @IdentityProviderTypeId,
        @Name = 'OktaEntityId';
EXECUTE [Ident].[CreateIdentityProviderSettingType]
        @IdentityProviderTypeId = @IdentityProviderTypeId,
        @Name = 'OktaMetadataLocation';
EXECUTE [Ident].[CreateIdentityProviderSettingType]
        @IdentityProviderTypeId = @IdentityProviderTypeId,
        @Name = 'PostLogoutRedirectUri';
EXECUTE [Ident].[CreateIdentityProviderSettingType]
        @IdentityProviderTypeId = @IdentityProviderTypeId,
        @Name = 'ProviderClientId';
EXECUTE [Ident].[CreateIdentityProviderSettingType]
        @IdentityProviderTypeId = @IdentityProviderTypeId,
        @Name = 'RedirectUri';
EXECUTE [Ident].[CreateIdentityProviderSettingType]
        @IdentityProviderTypeId = @IdentityProviderTypeId,
        @Name = 'Scope';
EXECUTE [Ident].[CreateIdentityProviderSettingType]
        @IdentityProviderTypeId = @IdentityProviderTypeId,
        @Name = 'TokenValidationAuthenticationType';
EXECUTE [Ident].[CreateIdentityProviderSettingType]
        @IdentityProviderTypeId = @IdentityProviderTypeId,
        @Name = 'ValidateIssuer';
SET @IdentityProviderTypeId = NULL;
SELECT @IdentityProviderTypeId = IdentityProviderTypeId
FROM Ident.IdentityProviderType
WHERE Name = 'OktaAmli';
EXECUTE [Ident].[CreateIdentityProviderSettingType]
        @IdentityProviderTypeId = @IdentityProviderTypeId,
        @Name = 'AuthenticationMode';
EXECUTE [Ident].[CreateIdentityProviderSettingType]
        @IdentityProviderTypeId = @IdentityProviderTypeId,
        @Name = 'AuthenticationType';
EXECUTE [Ident].[CreateIdentityProviderSettingType]
        @IdentityProviderTypeId = @IdentityProviderTypeId,
        @Name = 'AuthorityUri';
EXECUTE [Ident].[CreateIdentityProviderSettingType]
        @IdentityProviderTypeId = @IdentityProviderTypeId,
        @Name = 'Caption';
EXECUTE [Ident].[CreateIdentityProviderSettingType]
        @IdentityProviderTypeId = @IdentityProviderTypeId,
        @Name = 'ClientSecret';
EXECUTE [Ident].[CreateIdentityProviderSettingType]
        @IdentityProviderTypeId = @IdentityProviderTypeId,
        @Name = 'OktaEntityId';
EXECUTE [Ident].[CreateIdentityProviderSettingType]
        @IdentityProviderTypeId = @IdentityProviderTypeId,
        @Name = 'OktaMetadataLocation';
EXECUTE [Ident].[CreateIdentityProviderSettingType]
        @IdentityProviderTypeId = @IdentityProviderTypeId,
        @Name = 'PostLogoutRedirectUri';
EXECUTE [Ident].[CreateIdentityProviderSettingType]
        @IdentityProviderTypeId = @IdentityProviderTypeId,
        @Name = 'ProviderClientId';
EXECUTE [Ident].[CreateIdentityProviderSettingType]
        @IdentityProviderTypeId = @IdentityProviderTypeId,
        @Name = 'RedirectUri';
EXECUTE [Ident].[CreateIdentityProviderSettingType]
        @IdentityProviderTypeId = @IdentityProviderTypeId,
        @Name = 'Scope';
EXECUTE [Ident].[CreateIdentityProviderSettingType]
        @IdentityProviderTypeId = @IdentityProviderTypeId,
        @Name = 'TokenValidationAuthenticationType';
EXECUTE [Ident].[CreateIdentityProviderSettingType]
        @IdentityProviderTypeId = @IdentityProviderTypeId,
        @Name = 'ValidateIssuer';
SELECT @ServerName = @@SERVERNAME;
SET @DBName = 'IDENTITY';
IF(@DBName = 'IDENTITY'
   AND @ServerName = 'RCDUSODBSQL001')
    BEGIN
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'AuthenticationType');
        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'oktacamden'
        ),
        (@IdentityProviderSettingTypeID,
         'Local'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'Caption');
        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'Sign in with Okta'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'AuthorityUri');
        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'https://identitydev.corp.realpage.com/identity/camden'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'RedirectUri');
        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'https://landing.local/auth.aspx?idp=oktacamden'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'AuthenticationMode');
        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         '0'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'ValidateIssuer');
        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         '1'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'OktaMetadataLocation');
        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'https://dev-489082.oktapreview.com/app/exk8ypmtyrcEEXYKi0h7/sso/saml/metadata'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'OktaEntityId'); --, N'', N'', N'', N'', N'', N'')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'exk8ypmtyrcEEXYKi0h7'
        );
	   --End OctaCamden

	   --Start Amli

        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'AuthenticationType'); --, N'Caption', N'AuthorityUri', N'RedirectUri', N'AuthenticationMode', N'ValidateIssuer', N'OktaMetadataLocation')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'OktaAmli'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'Caption'); --, N'', N'AuthorityUri', N'RedirectUri', N'AuthenticationMode', N'ValidateIssuer', N'OktaMetadataLocation')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'Sign in with Okta'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'AuthorityUri'); --, N'', N'', N'RedirectUri', N'AuthenticationMode', N'ValidateIssuer', N'OktaMetadataLocation')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'https://identitydev.corp.realpage.com/identity/client1'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'RedirectUri'); --, N'', N'', N'', N'AuthenticationMode', N'ValidateIssuer', N'OktaMetadataLocation')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'https://landing.local/auth.aspx?idp=oktaamli'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'AuthenticationMode'); --, N'', N'', N'', N'', N'ValidateIssuer', N'OktaMetadataLocation')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         '1'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'ValidateIssuer'); --, N'', N'', N'', N'', N'', N'OktaMetadataLocation')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         '1'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'OktaMetadataLocation'); --, N'', N'', N'', N'', N'', N'')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'https://dev-489082.oktapreview.com/app/exk8y9pu8jOUTyViW0h7/sso/saml/metadata'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'OktaEntityId'); --, N'', N'', N'', N'', N'', N'')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'exk8y9pu8jOUTyViW0h7'
        );
--EndAmli

        UPDATE A
          SET
              A.UserId = B.UserId
        FROM Person.Persona A
             INNER JOIN [Enterprise].[PersonaIdentityUserLogin] B ON A.PersonaId = B.PersonaID;
        UPDATE a
          SET
              a.IdentityProviderTypeId = CASE b.contactmechanismid
                                             WHEN 77
                                             THEN 7
                                             WHEN 46
                                             THEN 4
                                             WHEN 47
                                             THEN 6
                                             WHEN 45
                                             THEN 5
                                         END
        FROM Ident.UserLogin a
             INNER JOIN [Enterprise].[PersonaIdentityUserLogin] b ON a.userid = b.userid;


--UPDATE IDENTITYPROVIDERTYPE Table with COntactMechanismId

        UPDATE a
          SET
              a.ContactMechaNIsmId = d.COntactMechanismId
        FROM Ident.IdentityProviderType a
             INNER JOIN Ident.IdentityProviderSettingType b ON a.IdentityProviderTypeId = b.IdentityProviderTypeId
             INNER JOIN Ident.IdentityProviderSetting c ON b.IdentityProviderSettingTypeId = c.IdentityProviderSettingTypeId
             INNER JOIN ident.ContactMechanismIdentity d ON d.IdentityProviderSettingId = c.IdentityProviderSettingId
        WHERE B.Name = 'AuthenticationType'
              AND a.Name NOT IN('OktaCamden', 'OktaAmli');

--UPDATE ORGANIZATION TABLE WITH IdentityProviderTypeId



        DECLARE rpguid CURSOR
        FOR
            SELECT p.realpageid
            FROM Enterprise.Organization O
                 INNER JOIN enterprise.party p ON o.partyid = p.partyid;
        OPEN rpguid;
        FETCH rpguid INTO @realpageid;
        WHILE @@FETCH_STATUS = 0
            BEGIN
                UPDATE o
                  SET
                      o.IdentityProviderTypeId = CASE cmi.contactmechanismid
                                                     WHEN 77
                                                     THEN 7
                                                     WHEN 46
                                                     THEN 4
                                                     WHEN 47
                                                     THEN 6
                                                     WHEN 45
                                                     THEN 5
                                                     ELSE 4
                                                 END
                FROM Enterprise.PartyContactMechanism pcm
                     INNER JOIN Enterprise.Organization o ON(pcm.PartyId = o.PartyId)
                     INNER JOIN Enterprise.Party p ON(o.PartyId = p.PartyId)
                     INNER JOIN Ident.ContactMechanismIdentity cmi ON(pcm.ContactMechanismId = cmi.ContactMechanismId)
                     INNER JOIN Ident.IdentityProviderSetting ips ON ips.IdentityProviderSettingId = cmi.IdentityProviderSettingId
                     INNER JOIN Ident.IdentityProviderSettingType ipst ON ipst.IdentityProviderSettingTypeId = ips.IdentityProviderSettingTypeId
                     INNER JOIN Ident.IdentityProviderType ipt ON ipt.IdentityProviderTypeId = ipst.IdentityProviderTypeId
                WHERE ipst.Name = 'AuthenticationType'
                      AND ips.value = 'local'
                      AND p.RealPageId = @RealPageId;
                UPDATE o
                  SET
                      o.IdentityProviderTypeId = CASE cmi.contactmechanismid
                                                     WHEN 77
                                                     THEN 7
                                                     WHEN 46
                                                     THEN 4
                                                     WHEN 47
                                                     THEN 6
                                                     WHEN 45
                                                     THEN 5
                                                     ELSE 4
                                                 END
                FROM Enterprise.PartyContactMechanism pcm
                     INNER JOIN Enterprise.Organization o ON(pcm.PartyId = o.PartyId)
                     INNER JOIN Enterprise.Party p ON(o.PartyId = p.PartyId)
                     INNER JOIN Ident.ContactMechanismIdentity cmi ON(pcm.ContactMechanismId = cmi.ContactMechanismId)
                     INNER JOIN Ident.IdentityProviderSetting ips ON ips.IdentityProviderSettingId = cmi.IdentityProviderSettingId
                     INNER JOIN Ident.IdentityProviderSettingType ipst ON ipst.IdentityProviderSettingTypeId = ips.IdentityProviderSettingTypeId
                     INNER JOIN Ident.IdentityProviderType ipt ON ipt.IdentityProviderTypeId = ipst.IdentityProviderTypeId
                WHERE ipst.Name = 'AuthenticationType'
                      AND ips.value <> 'local'
                      AND p.RealPageId = @RealPageId;
                FETCH rpguid INTO @realpageid;
END;
        CLOSE rpguid;
        DEALLOCATE rpguid;
END;
----------------------------------------------------------------------
IF(@DBName = 'IDENTITY'
   AND @ServerName = 'RCTUSODBSQL001')
    BEGIN
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'AuthenticationType');
        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'oktacamden'
        ),
        (@IdentityProviderSettingTypeID,
         'Local'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'Caption');
        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'Sign in with Okta'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'AuthorityUri');
        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'https://mylqa.corp.realpage.com/identity/camden'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'RedirectUri');
        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'https://myqa.corp.realpage.com/auth.aspx?idp=oktacamden'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'AuthenticationMode');
        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         '0'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'ValidateIssuer');
        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         '1'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'OktaMetadataLocation');
        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'https://dev-489082.oktapreview.com/app/exkb4netl0RgB6bun0h7/sso/saml/metadata'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'OktaEntityId'); --, N'', N'', N'', N'', N'', N'')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'exkb4netl0RgB6bun0h7'
        );
	   --End OctaCamden

	   --Start Amli

        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'AuthenticationType'); --, N'Caption', N'AuthorityUri', N'RedirectUri', N'AuthenticationMode', N'ValidateIssuer', N'OktaMetadataLocation')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'OktaAmli'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'Caption'); --, N'', N'AuthorityUri', N'RedirectUri', N'AuthenticationMode', N'ValidateIssuer', N'OktaMetadataLocation')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'Sign in with Okta'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'AuthorityUri'); --, N'', N'', N'RedirectUri', N'AuthenticationMode', N'ValidateIssuer', N'OktaMetadataLocation')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'https://identitydev.corp.realpage.com/identity/client1'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'RedirectUri'); --, N'', N'', N'', N'AuthenticationMode', N'ValidateIssuer', N'OktaMetadataLocation')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'https://landing.local/auth.aspx?idp=oktaamli'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'AuthenticationMode'); --, N'', N'', N'', N'', N'ValidateIssuer', N'OktaMetadataLocation')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         '1'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'ValidateIssuer'); --, N'', N'', N'', N'', N'', N'OktaMetadataLocation')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         '1'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'OktaMetadataLocation'); --, N'', N'', N'', N'', N'', N'')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'https://dev-489082.oktapreview.com/app/exk8y9pu8jOUTyViW0h7/sso/saml/metadata'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'OktaEntityId'); --, N'', N'', N'', N'', N'', N'')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'exk8y9pu8jOUTyViW0h7'
        );
        
--EndAmli

        UPDATE A
          SET
              A.UserId = B.UserId
        FROM Person.Persona A
             INNER JOIN [Enterprise].[PersonaIdentityUserLogin] B ON A.PersonaId = B.PersonaID;
        UPDATE a
          SET
              a.IdentityProviderTypeId = CASE b.contactmechanismid
                                             WHEN 77
                                             THEN 7
                                             WHEN 46
                                             THEN 4
                                             WHEN 47
                                             THEN 6
                                             WHEN 45
                                             THEN 5
                                             WHEN 48
                                             THEN 6
                                         END
        FROM Ident.UserLogin a
             INNER JOIN [Enterprise].[PersonaIdentityUserLogin] b ON a.userid = b.userid;


--UPDATE IDENTITYPROVIDERTYPE Table with COntactMechanismId

        UPDATE a
          SET
              a.ContactMechaNIsmId = d.COntactMechanismId
        FROM Ident.IdentityProviderType a
             INNER JOIN Ident.IdentityProviderSettingType b ON a.IdentityProviderTypeId = b.IdentityProviderTypeId
             INNER JOIN Ident.IdentityProviderSetting c ON b.IdentityProviderSettingTypeId = c.IdentityProviderSettingTypeId
             INNER JOIN ident.ContactMechanismIdentity d ON d.IdentityProviderSettingId = c.IdentityProviderSettingId
        WHERE B.Name = 'AuthenticationType'
              AND a.Name NOT IN('OktaCamden', 'OktaAmli');

--UPDATE ORGANIZATION TABLE WITH IdentityProviderTypeId



        DECLARE rpguid CURSOR
        FOR
            SELECT p.realpageid
            FROM Enterprise.Organization O
                 INNER JOIN enterprise.party p ON o.partyid = p.partyid;
        OPEN rpguid;
        FETCH rpguid INTO @realpageid;
        WHILE @@FETCH_STATUS = 0
            BEGIN
                UPDATE o
                  SET
                      o.IdentityProviderTypeId = CASE cmi.contactmechanismid
                                                     WHEN 77
                                                     THEN 7
                                                     WHEN 46
                                                     THEN 4
                                                     WHEN 47
                                                     THEN 6
                                                     WHEN 45
                                                     THEN 5
                                                     ELSE 4
                                                 END
                FROM Enterprise.PartyContactMechanism pcm
                     INNER JOIN Enterprise.Organization o ON(pcm.PartyId = o.PartyId)
                     INNER JOIN Enterprise.Party p ON(o.PartyId = p.PartyId)
                     INNER JOIN Ident.ContactMechanismIdentity cmi ON(pcm.ContactMechanismId = cmi.ContactMechanismId)
                     INNER JOIN Ident.IdentityProviderSetting ips ON ips.IdentityProviderSettingId = cmi.IdentityProviderSettingId
                     INNER JOIN Ident.IdentityProviderSettingType ipst ON ipst.IdentityProviderSettingTypeId = ips.IdentityProviderSettingTypeId
                     INNER JOIN Ident.IdentityProviderType ipt ON ipt.IdentityProviderTypeId = ipst.IdentityProviderTypeId
                WHERE ipst.Name = 'AuthenticationType'
                      AND ips.value = 'local'
                      AND p.RealPageId = @RealPageId;
                UPDATE o
                  SET
                      o.IdentityProviderTypeId = CASE cmi.contactmechanismid
                                                     WHEN 77
                                                     THEN 7
                                                     WHEN 46
                                                     THEN 4
                                                     WHEN 47
                                                     THEN 6
                                                     WHEN 45
                                                     THEN 5
                                                     ELSE 4
                                                 END
                FROM Enterprise.PartyContactMechanism pcm
                     INNER JOIN Enterprise.Organization o ON(pcm.PartyId = o.PartyId)
                     INNER JOIN Enterprise.Party p ON(o.PartyId = p.PartyId)
                     INNER JOIN Ident.ContactMechanismIdentity cmi ON(pcm.ContactMechanismId = cmi.ContactMechanismId)
                     INNER JOIN Ident.IdentityProviderSetting ips ON ips.IdentityProviderSettingId = cmi.IdentityProviderSettingId
                     INNER JOIN Ident.IdentityProviderSettingType ipst ON ipst.IdentityProviderSettingTypeId = ips.IdentityProviderSettingTypeId
                     INNER JOIN Ident.IdentityProviderType ipt ON ipt.IdentityProviderTypeId = ipst.IdentityProviderTypeId
                WHERE ipst.Name = 'AuthenticationType'
                      AND ips.value <> 'local'
                      AND p.RealPageId = @RealPageId;
                FETCH rpguid INTO @realpageid;
END;
        CLOSE rpguid;
        DEALLOCATE rpguid;
END;
------------------------------------
IF(@DBName = 'IDENTITY'
   AND @ServerName = 'RCQUSODBSQL001')
    BEGIN
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'AuthenticationType');
        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'oktacamden'
        ),
        (@IdentityProviderSettingTypeID,
         'Local'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'Caption');
        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'Sign in with Okta'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'AuthorityUri');
        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'https://mylsat.realpage.com/identity/camden'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'RedirectUri');
        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'https://mysat.realpage.com/auth.aspx?idp=oktacamden'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'AuthenticationMode');
        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         '0'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'ValidateIssuer');
        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         '1'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'OktaMetadataLocation');
        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'https://dev-489082.oktapreview.com/app/exkab9ky9uSUFy41e0h7/sso/saml/metadata'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'OktaEntityId'); --, N'', N'', N'', N'', N'', N'')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'exkab9ky9uSUFy41e0h7'
        );
	   --End OctaCamden

	   --Start Amli

        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'AuthenticationType'); --, N'Caption', N'AuthorityUri', N'RedirectUri', N'AuthenticationMode', N'ValidateIssuer', N'OktaMetadataLocation')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'OktaAmli'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'Caption'); --, N'', N'AuthorityUri', N'RedirectUri', N'AuthenticationMode', N'ValidateIssuer', N'OktaMetadataLocation')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'Sign in with Okta'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'AuthorityUri'); --, N'', N'', N'RedirectUri', N'AuthenticationMode', N'ValidateIssuer', N'OktaMetadataLocation')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'https://identitydev.corp.realpage.com/identity/client1'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'RedirectUri'); --, N'', N'', N'', N'AuthenticationMode', N'ValidateIssuer', N'OktaMetadataLocation')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'https://mysat.realpage.com/auth.aspx?idp=oktaamli'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'AuthenticationMode'); --, N'', N'', N'', N'', N'ValidateIssuer', N'OktaMetadataLocation')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         '1'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'ValidateIssuer'); --, N'', N'', N'', N'', N'', N'OktaMetadataLocation')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         '1'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'OktaMetadataLocation'); --, N'', N'', N'', N'', N'', N'')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'https://dev-489082.oktapreview.com/app/exk8y9pu8jOUTyViW0h7/sso/saml/metadata'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'OktaEntityId'); --, N'', N'', N'', N'', N'', N'')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'exk8y9pu8jOUTyViW0h7'
        );
        
--EndAmli

        UPDATE A
          SET
              A.UserId = B.UserId
        FROM Person.Persona A
             INNER JOIN [Enterprise].[PersonaIdentityUserLogin] B ON A.PersonaId = B.PersonaID;
        UPDATE a
          SET
              a.IdentityProviderTypeId = CASE b.contactmechanismid
                                             WHEN 77
                                             THEN 7
                                             WHEN 46
                                             THEN 4
                                             WHEN 47
                                             THEN 6
                                             WHEN 45
                                             THEN 5
                                             WHEN 48
                                             THEN 6
                                         END
        FROM Ident.UserLogin a
             INNER JOIN [Enterprise].[PersonaIdentityUserLogin] b ON a.userid = b.userid;


--UPDATE IDENTITYPROVIDERTYPE Table with COntactMechanismId

        UPDATE a
          SET
              a.ContactMechaNIsmId = d.COntactMechanismId
        FROM Ident.IdentityProviderType a
             INNER JOIN Ident.IdentityProviderSettingType b ON a.IdentityProviderTypeId = b.IdentityProviderTypeId
             INNER JOIN Ident.IdentityProviderSetting c ON b.IdentityProviderSettingTypeId = c.IdentityProviderSettingTypeId
             INNER JOIN ident.ContactMechanismIdentity d ON d.IdentityProviderSettingId = c.IdentityProviderSettingId
        WHERE B.Name = 'AuthenticationType'
              AND a.Name NOT IN('OktaCamden', 'OktaAmli');

--UPDATE ORGANIZATION TABLE WITH IdentityProviderTypeId



        DECLARE rpguid CURSOR
        FOR
            SELECT p.realpageid
            FROM Enterprise.Organization O
                 INNER JOIN enterprise.party p ON o.partyid = p.partyid;
        OPEN rpguid;
        FETCH rpguid INTO @realpageid;
        WHILE @@FETCH_STATUS = 0
            BEGIN
                UPDATE o
                  SET
                      o.IdentityProviderTypeId = CASE cmi.contactmechanismid
                                                     WHEN 77
                                                     THEN 7
                                                     WHEN 46
                                                     THEN 4
                                                     WHEN 47
                                                     THEN 6
                                                     WHEN 45
                                                     THEN 5
                                                     ELSE 4
                                                 END
                FROM Enterprise.PartyContactMechanism pcm
                     INNER JOIN Enterprise.Organization o ON(pcm.PartyId = o.PartyId)
                     INNER JOIN Enterprise.Party p ON(o.PartyId = p.PartyId)
                     INNER JOIN Ident.ContactMechanismIdentity cmi ON(pcm.ContactMechanismId = cmi.ContactMechanismId)
                     INNER JOIN Ident.IdentityProviderSetting ips ON ips.IdentityProviderSettingId = cmi.IdentityProviderSettingId
                     INNER JOIN Ident.IdentityProviderSettingType ipst ON ipst.IdentityProviderSettingTypeId = ips.IdentityProviderSettingTypeId
                     INNER JOIN Ident.IdentityProviderType ipt ON ipt.IdentityProviderTypeId = ipst.IdentityProviderTypeId
                WHERE ipst.Name = 'AuthenticationType'
                      AND ips.value = 'local'
                      AND p.RealPageId = @RealPageId;
                UPDATE o
                  SET
                      o.IdentityProviderTypeId = CASE cmi.contactmechanismid
                                                     WHEN 77
                                                     THEN 7
                                                     WHEN 46
                                                     THEN 4
                                                     WHEN 47
                                                     THEN 6
                                                     WHEN 45
                                                     THEN 5
                                                     ELSE 4
                                                 END
                FROM Enterprise.PartyContactMechanism pcm
                     INNER JOIN Enterprise.Organization o ON(pcm.PartyId = o.PartyId)
                     INNER JOIN Enterprise.Party p ON(o.PartyId = p.PartyId)
                     INNER JOIN Ident.ContactMechanismIdentity cmi ON(pcm.ContactMechanismId = cmi.ContactMechanismId)
                     INNER JOIN Ident.IdentityProviderSetting ips ON ips.IdentityProviderSettingId = cmi.IdentityProviderSettingId
                     INNER JOIN Ident.IdentityProviderSettingType ipst ON ipst.IdentityProviderSettingTypeId = ips.IdentityProviderSettingTypeId
                     INNER JOIN Ident.IdentityProviderType ipt ON ipt.IdentityProviderTypeId = ipst.IdentityProviderTypeId
                WHERE ipst.Name = 'AuthenticationType'
                      AND ips.value <> 'local'
                      AND p.RealPageId = @RealPageId;
                FETCH rpguid INTO @realpageid;
END;
        CLOSE rpguid;
        DEALLOCATE rpguid;
END;
IF(@DBName = 'IDENTITY'
   AND @ServerName = 'RCPGBKDBSQL005A' OR @ServerName = 'RCPGBKDBSQL005B')
    BEGIN
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'AuthenticationType');
        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'oktacamden'
        ),
        (@IdentityProviderSettingTypeID,
         'Local'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'Caption');
        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'Sign in with Okta'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'AuthorityUri');
        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'https://identitydev.corp.realpage.com/identity/camden'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'RedirectUri');
        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'https://landing.local/auth.aspx?idp=oktacamden'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'AuthenticationMode');
        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         '0'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'ValidateIssuer');
        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         '1'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'OktaMetadataLocation');
        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'https://dev-489082.oktapreview.com/app/exk8ypmtyrcEEXYKi0h7/sso/saml/metadata'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaCamden'
                                                                      AND IPST.Name IN(N'OktaEntityId'); --, N'', N'', N'', N'', N'', N'')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'exk8ypmtyrcEEXYKi0h7'
        );
	   --End OctaCamden

	   --Start Amli

        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'AuthenticationType'); --, N'Caption', N'AuthorityUri', N'RedirectUri', N'AuthenticationMode', N'ValidateIssuer', N'OktaMetadataLocation')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'OktaAmli'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'Caption'); --, N'', N'AuthorityUri', N'RedirectUri', N'AuthenticationMode', N'ValidateIssuer', N'OktaMetadataLocation')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'Sign in with Okta'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'AuthorityUri'); --, N'', N'', N'RedirectUri', N'AuthenticationMode', N'ValidateIssuer', N'OktaMetadataLocation')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'https://identitydev.corp.realpage.com/identity/client1'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'RedirectUri'); --, N'', N'', N'', N'AuthenticationMode', N'ValidateIssuer', N'OktaMetadataLocation')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'https://landing.local/auth.aspx?idp=oktaamli'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'AuthenticationMode'); --, N'', N'', N'', N'', N'ValidateIssuer', N'OktaMetadataLocation')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         '1'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'ValidateIssuer'); --, N'', N'', N'', N'', N'', N'OktaMetadataLocation')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         '1'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'OktaMetadataLocation'); --, N'', N'', N'', N'', N'', N'')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'https://dev-489082.oktapreview.com/app/exk8y9pu8jOUTyViW0h7/sso/saml/metadata'
        );
        SELECT @IdentityProviderSettingTypeID = IPST.IdentityProviderSettingTypeID
        FROM Ident.IdentityProviderType IPT
             INNER JOIN [Ident].[IdentityProviderSettingType] IPST ON IPT.IdentityProviderTypeId = IPST.IdentityProviderTypeId
                                                                      AND IPT.Name = 'OktaAmli'
                                                                      AND IPST.Name IN(N'OktaEntityId'); --, N'', N'', N'', N'', N'', N'')

        INSERT INTO Ident.IdentityProviderSetting
        (IdentityProviderSettingTypeID,
         Value
        )
        VALUES
        (@IdentityProviderSettingTypeID,
         'exk8y9pu8jOUTyViW0h7'
        );
        
--EndAmli

        UPDATE A
          SET
              A.UserId = B.UserId
        FROM Person.Persona A
             INNER JOIN [Enterprise].[PersonaIdentityUserLogin] B ON A.PersonaId = B.PersonaID;
        UPDATE a
          SET
              a.IdentityProviderTypeId = CASE b.contactmechanismid
                                             WHEN 77
                                             THEN 7
                                             WHEN 46
                                             THEN 4
                                             WHEN 47
                                             THEN 6
                                             WHEN 45
                                             THEN 5
                                             WHEN 48
                                             THEN 6
                                         END
        FROM Ident.UserLogin a
             INNER JOIN [Enterprise].[PersonaIdentityUserLogin] b ON a.userid = b.userid;


--UPDATE IDENTITYPROVIDERTYPE Table with COntactMechanismId

        UPDATE a
          SET
              a.ContactMechaNIsmId = d.COntactMechanismId
        FROM Ident.IdentityProviderType a
             INNER JOIN Ident.IdentityProviderSettingType b ON a.IdentityProviderTypeId = b.IdentityProviderTypeId
             INNER JOIN Ident.IdentityProviderSetting c ON b.IdentityProviderSettingTypeId = c.IdentityProviderSettingTypeId
             INNER JOIN ident.ContactMechanismIdentity d ON d.IdentityProviderSettingId = c.IdentityProviderSettingId
        WHERE B.Name = 'AuthenticationType'
              AND a.Name NOT IN('OktaCamden', 'OktaAmli');

--UPDATE ORGANIZATION TABLE WITH IdentityProviderTypeId



        DECLARE rpguid CURSOR
        FOR
            SELECT p.realpageid
            FROM Enterprise.Organization O
                 INNER JOIN enterprise.party p ON o.partyid = p.partyid;
        OPEN rpguid;
        FETCH rpguid INTO @realpageid;
        WHILE @@FETCH_STATUS = 0
            BEGIN
                UPDATE o
                  SET
                      o.IdentityProviderTypeId = CASE cmi.contactmechanismid
                                                     WHEN 77
                                                     THEN 7
                                                     WHEN 46
                                                     THEN 4
                                                     WHEN 47
                                                     THEN 6
                                                     WHEN 45
                                                     THEN 5
                                                     ELSE 4
                                                 END
                FROM Enterprise.PartyContactMechanism pcm
                     INNER JOIN Enterprise.Organization o ON(pcm.PartyId = o.PartyId)
                     INNER JOIN Enterprise.Party p ON(o.PartyId = p.PartyId)
                     INNER JOIN Ident.ContactMechanismIdentity cmi ON(pcm.ContactMechanismId = cmi.ContactMechanismId)
                     INNER JOIN Ident.IdentityProviderSetting ips ON ips.IdentityProviderSettingId = cmi.IdentityProviderSettingId
                     INNER JOIN Ident.IdentityProviderSettingType ipst ON ipst.IdentityProviderSettingTypeId = ips.IdentityProviderSettingTypeId
                     INNER JOIN Ident.IdentityProviderType ipt ON ipt.IdentityProviderTypeId = ipst.IdentityProviderTypeId
                WHERE ipst.Name = 'AuthenticationType'
                      AND ips.value = 'local'
                      AND p.RealPageId = @RealPageId;
                UPDATE o
                  SET
                      o.IdentityProviderTypeId = CASE cmi.contactmechanismid
                                                     WHEN 77
                                                     THEN 7
                                                     WHEN 46
                                                     THEN 4
                                                     WHEN 47
                                                     THEN 6
                                                     WHEN 45
                                                     THEN 5
                                                     ELSE 4
                                                 END
                FROM Enterprise.PartyContactMechanism pcm
                     INNER JOIN Enterprise.Organization o ON(pcm.PartyId = o.PartyId)
                     INNER JOIN Enterprise.Party p ON(o.PartyId = p.PartyId)
                     INNER JOIN Ident.ContactMechanismIdentity cmi ON(pcm.ContactMechanismId = cmi.ContactMechanismId)
                     INNER JOIN Ident.IdentityProviderSetting ips ON ips.IdentityProviderSettingId = cmi.IdentityProviderSettingId
                     INNER JOIN Ident.IdentityProviderSettingType ipst ON ipst.IdentityProviderSettingTypeId = ips.IdentityProviderSettingTypeId
                     INNER JOIN Ident.IdentityProviderType ipt ON ipt.IdentityProviderTypeId = ipst.IdentityProviderTypeId
                WHERE ipst.Name = 'AuthenticationType'
                      AND ips.value <> 'local'
                      AND p.RealPageId = @RealPageId;
                FETCH rpguid INTO @realpageid;
END;
        CLOSE rpguid;
        DEALLOCATE rpguid;
END;
--UPDATE USER LOGIN TABLE WITH IdentityProviderType



--Update Organizations with new Indentityproviders

SELECT @identityProviderTypeId = IdentityProviderTypeID
FROM Ident.IdentityProviderType
WHERE name = 'OktaCamden';
UPDATE Enterprise.Organization
  SET
      IdentityProviderTypeId = @identityProviderTypeId
WHERE Name = 'CAMDEN DEVELOPMENT, INC.';
SELECT @identityProviderTypeId = IdentityProviderTypeID
FROM Ident.IdentityProviderType
WHERE name = 'OktaAmli';
UPDATE Enterprise.Organization
  SET
      IdentityProviderTypeId = @identityProviderTypeId
WHERE Name = 'AMLI';

--Update PartyContact information

DECLARE @CMId INT;
SELECT @PartyId = PartyId
FROM Enterprise.Organization
WHERE Name = 'CAMDEN DEVELOPMENT, INC.';
SELECT @CMId = ContactMechanismId
FROM Ident.IdentityProviderType
WHERE Name = 'OktaCamden';
UPDATE Enterprise.PartyContactMechanism
  SET
      COntactMechanismId = @CMid
WHERE PartyId = @PartyId
      AND ContactMechanismID <> 46;
SELECT @PartyId = PartyId
FROM Enterprise.Organization
WHERE Name = 'AMLI';
SELECT @CMId = ContactMechanismId
FROM Ident.IdentityProviderType
WHERE Name = 'OktaAmli';
UPDATE Enterprise.PartyContactMechanism
  SET
      COntactMechanismId = @CMid
WHERE PartyId = @PartyId
      AND ContactMechanismID <> 46;

--UPDATE UserLogin WITH newly added IdentityProviderTypeId

UPDATE u
  SET
      u.IdentityProviderTypeId = o.IdentityProviderTypeId
FROM ident.userlogin u
     INNER JOIN person.persona p ON p.userid = u.userid
     INNER JOIN enterprise.organization o ON o.partyid = p.organizationpartyid
WHERE u.IdentityProviderTypeId <> o.IdentityProviderTypeId;
EXEC sys.sp_updateextendedproperty
     @name = N'Build',
     @value = '26';