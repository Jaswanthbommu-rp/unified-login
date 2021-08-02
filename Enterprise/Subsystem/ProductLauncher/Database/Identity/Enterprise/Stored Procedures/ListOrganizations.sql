CREATE PROCEDURE [Enterprise].[ListOrganizations] @RealPageId UNIQUEIDENTIFIER = NULL
AS
     BEGIN
         SELECT O.PartyId,
                O.Name,
                P.RealPageId 'OrganizationRealPageId',
                D.SourceId AS 'BooksMasterId',
                'RealPageEmployeeAccessID' AS 'SettingName',
                P2.RealPageId AS 'PersonRealPageId',
                UL.LoginName
         FROM Enterprise.Organization O
              INNER JOIN Enterprise.Party P ON O.PartyId = P.PartyId
              INNER JOIN Enterprise.DataImportMapping D ON O.PartyId = D.PartyId
              INNER JOIN Enterprise.OrganizationAdminUser OAU ON OAU.OrganizationPartyId = O.PartyId
			  INNER JOIN Ident.UserLoginPersona ULP ON oau.UserLoginPersonaId = ulp.UserLoginPersonaId
			  INNER JOIN Ident.UserLogin UL ON UL.UserId = ulp.UserLoginId
			  INNER JOIN Enterprise.Party P2 ON P2.PartyId = UL.PersonPartyId
			   AND (P.RealPageId = @RealPageId OR @RealPageID IS NULL)
     END;

