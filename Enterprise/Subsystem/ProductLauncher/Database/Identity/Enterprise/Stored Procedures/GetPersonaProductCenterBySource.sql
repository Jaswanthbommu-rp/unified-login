CREATE PROCEDURE [Enterprise].[GetPersonaProductCenters]
(@PersonaId	BIGINT,
 @Source NVARCHAR(25) = NULL)
AS
BEGIN
SET NOCOUNT ON;
	SELECT 
		ppc.CacheExpirationDate,
		pc.Source,
		pc.ProductCenterId,
		pc.ProductCenterSourceId
	FROM
		Enterprise.PersonaProductCenter ppc JOIN
		Enterprise.ProductCenter pc ON ppc.ProductCenterId = pc.ProductCenterId
	WHERE
		(@Source IS NULL OR pc.Source = @Source) AND
		ppc.PersonaId = @PersonaId
	ORDER BY 
		pc.Source, pc.ProductCenterId
END;
GO