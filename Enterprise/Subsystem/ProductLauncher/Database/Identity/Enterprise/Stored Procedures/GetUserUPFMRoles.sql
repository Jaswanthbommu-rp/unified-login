CREATE PROCEDURE [Enterprise].[GetUserUPFMRoles]
 @ProductId int,
 @PartyId bigint
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        R.RoleName AS [Role],
        R.ShortName AS [RoleNickName],
        R.RoleID AS [RoleId],
        P.ProductId AS [Product],
        RT.Value AS RoleType,
        PE.PersonaId,
        ULP.OrganizationPartyId,
        ERT.Name AS [UserType]
    FROM Security.Role R
    INNER JOIN Security.RoleRight RR ON R.RoleId = RR.RoleId
    INNER JOIN Security.[Right] RG ON RR.RightID = RG.RightID
    INNER JOIN Security.RoleType RT ON RT.RoleTypeId = R.RoleTypeID
    INNER JOIN Enterprise.Product P ON P.ProductId = R.ProductId
    INNER JOIN Security.PersonaRole PR ON PR.RoleID = R.RoleID
    INNER JOIN Person.Persona PE ON PE.PersonaId = PR.PersonaId
    INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = PE.UserLoginPersonaId
    INNER JOIN Ident.UserLogin UL ON UL.UserId = ULP.UserLoginId
    INNER JOIN Enterprise.PartyRelationShip PRS ON PRS.PartyIdFrom = UL.PersonPartyId
        AND PRS.PartyIdTo = ULP.OrganizationPartyId
        AND PRS.ThruDate IS NULL
    INNER JOIN Enterprise.RoleType ERT ON ERT.PartyRoleTypeId = PRS.RoleTypeIdFrom
        AND ERT.ParentPartyRoleTypeId = 400
    WHERE P.ProductId = @ProductId
        AND ULP.OrganizationPartyId = @PartyId
END;
