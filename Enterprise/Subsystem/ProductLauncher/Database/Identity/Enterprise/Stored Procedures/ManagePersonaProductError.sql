CREATE PROCEDURE [Enterprise].[ManagePersonaProductError]
(
	@PersonaId BigInt
)
AS
BEGIN
BEGIN TRY
	IF EXISTS(SELECT 1 FROM Enterprise.PersonaConfiguration WHERE PersonaId = @PersonaId AND StatusTypeId = 7)
	BEGIN
		IF NOT EXISTS(SELECT 1 FROM Enterprise.PersonaProductError WHERE PersonaId = @PersonaId)
		BEGIN
			INSERT INTO Enterprise.PersonaProductError(PersonaId)
			select @PersonaId
		END
	END
	ELSE
	BEGIN
		DELETE FROM Enterprise.PersonaProductError WHERE PersonaId =@PersonaId
	END

	SELECT @PersonaId AS Id ,  
                '' AS ErrorMessage 
END TRY
BEGIN CATCH  
        DECLARE @ErrorLogID INT;  
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;  
  
        SELECT 0 AS Id ,  
                ErrorMessage  
        FROM dbo.ErrorLog  
        WHERE ErrorLogID = @ErrorLogID;  
 END CATCH 
END