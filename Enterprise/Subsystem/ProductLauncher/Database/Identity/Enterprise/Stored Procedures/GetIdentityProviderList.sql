CREATE PROCEDURE [Enterprise].[GetIdentityProviderList]
AS

BEGIN
	SET NOCOUNT ON;
	SELECT IdentityProviderTypeId, Description FROM Ident.IdentityProviderType
END;
