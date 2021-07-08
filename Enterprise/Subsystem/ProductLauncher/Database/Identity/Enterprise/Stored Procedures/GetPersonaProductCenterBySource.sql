CREATE PROCEDURE [Enterprise].[GetPersonaProductCenterBySource]
(@PersonaId	BIGINT,
 @Source NVARCHAR(25))
AS
BEGIN
SET NOCOUNT ON;
	SELECT 
		pc.ProductCenterId,
		pc.ProductCenterSourceId,
		ppc.CacheExpirationDate
	FROM
		Enterprise.PersonaProductCenter ppc JOIN
		Enterprise.ProductCenter pc ON ppc.ProductCenterId = pc.ProductCenterId
	WHERE
		pc.Source = @Source AND
		ppc.PersonaId = @PersonaId
END;
GO