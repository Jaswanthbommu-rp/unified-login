CREATE PROCEDURE [Enterprise].[UpdatePersonaProductCenter]
(@PersonaId	BIGINT,
 @Source NVARCHAR (25),
 @ProductCenterSourceIds [Enterprise].[SourceList]  READONLY,
 @CacheExpirationDate DATETIME)
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @NOW DATETIME = GETUTCDATE();
	MERGE Enterprise.PersonaProductCenter TARGET
	USING	
	(SELECT 
		pc.ProductCenterId
	FROM 
		Enterprise.ProductCenter pc JOIN
		@ProductCenterSourceIds productCenterSourceIds ON productCenterSourceIds.Source = pc.ProductCenterSourceId AND PC.Source = @Source) SOURCE
	ON 
		SOURCE.ProductCenterId = TARGET.ProductCenterId AND 
		TARGET.PersonaId = @PersonaId
	WHEN MATCHED THEN	
		UPDATE SET 
			TARGET.ModifiedDate = @NOW,
			TARGET.CacheExpirationDate = @CacheExpirationDate
	WHEN NOT MATCHED BY TARGET THEN
		INSERT (PersonaId, ProductCenterId, CacheExpirationDate)
		VALUES (@PersonaId, SOURCE.ProductCenterId, @CacheExpirationDate);

	DELETE ppc
	FROM 
		Enterprise.PersonaProductCenter ppc JOIN
		Enterprise.ProductCenter pc ON ppc.ProductCenterId = pc.ProductCenterId LEFT JOIN
		@ProductCenterSourceIds productCenterSourceIds ON productCenterSourceIds.Source = pc.ProductCenterSourceId
	WHERE
		ppc.PersonaId = @PersonaId AND
		pc.Source = @Source AND
		productCenterSourceIds.Source IS NULL
END;
GO