CREATE PROCEDURE [Ident].[RemoveSamlUserAttributeBySamlAttributeId]
    @PersonaId INT ,
    @ProductId INT,
	@SamlAttributeId INT
AS
BEGIN
	BEGIN TRY
		DECLARE @SamlUserAttributeId INT, @ErrorLogID INT;
		
		SELECT @SamlUserAttributeId = SamlUserAttributeId FROM ident.SamlUserAttribute 
		WHERE PersonaId=@PersonaId AND ProductId = @ProductId AND SamlAttributeId=@SamlAttributeId
		
		DELETE FROM ident.SamlUserAttribute WHERE PersonaId=@PersonaId AND ProductId = @ProductId AND SamlAttributeId=@SamlAttributeId;
		SELECT @SamlUserAttributeId as Id, '' AS ErrorMessage
	END TRY
	BEGIN CATCH
		EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;
		SELECT 0 AS Id, ErrorMessage FROM dbo.ErrorLog WHERE ErrorLogID = @ErrorLogID;
    END CATCH;
END;