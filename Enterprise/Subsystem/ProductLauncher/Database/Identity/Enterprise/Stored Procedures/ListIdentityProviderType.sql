CREATE PROCEDURE Enterprise.ListIdentityProviderType
AS
BEGIN
/*
DJ: 1/23/2018 - Procedure to List All the IdentityProviderTypes

Revision History:

*/
    SELECT IdentityProviderTypeId,
        Name,
        ContactMechanismId
    FROM Ident.IdentityProviderType;
END;