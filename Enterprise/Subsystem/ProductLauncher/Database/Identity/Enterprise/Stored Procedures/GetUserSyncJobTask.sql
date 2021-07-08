CREATE PROCEDURE [Enterprise].[GetUserSyncJobTask]
(@UserSyncJobTaskId	BIGINT)
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @NOW DATETIME= GETUTCDATE();
	SELECT 
		DISTINCT TOP (1)
		sj.UserSyncJobId,
		sua.PersonaId, 
		sua.Value AS LoginName, 
		ISNULL(p.UDMSourceCode, p.BooksProductCode) AS Source,
		'https://swaggerhub.realpage.com/virts/Realpage/UnifiedLoginProductIntegration/1.0.2' AS SyncUrl
		FROM	
			Ident.SamlUserAttribute sua
			INNER JOIN Ident.SamlAttribute sa ON sua.SamlAttributeId = sa.SamlAttributeId
			INNER JOIN Enterprise.UserSyncJob sj ON sj.PersonaId = sua.PersonaId
			INNER JOIN Enterprise.UserSyncJobTask sjt ON sjt.UserSyncJobId = sj.UserSyncJobId
			INNER JOIN Enterprise.Product p ON p.ProductId = sua.ProductId
		WHERE 
			sa.Name = 'productUsername' AND 
			((@NOW BETWEEN sua.FromDate AND sua.ThruDate) OR (@NOW >= sua.FromDate AND sua.ThruDate IS NULL)) AND
			sjt.UserSyncJobTaskId = @UserSyncJobTaskId AND
			sjt.Source = ISNULL(p.UDMSourceCode, p.BooksProductCode)
END;
GO
