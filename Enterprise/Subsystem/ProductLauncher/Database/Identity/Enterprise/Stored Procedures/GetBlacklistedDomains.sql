
CREATE PROCEDURE [Enterprise].[GetBlacklistedDomains]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT EmailAddress
    FROM [Enterprise].[EmailDomain]
    WHERE IsActive = 1;
END;