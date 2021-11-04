CREATE PROCEDURE [Enterprise].[AddUpdateEmployeeProductADGroupMapping]
	@ProductId INT,
	@PersonaId BIGINT,
	@ADGroupId INT
AS
BEGIN
	DECLARE @ResultTable TABLE ( ID INT NOT NULL )
	BEGIN TRY
		IF NOT EXISTS ( SELECT TOP (1) 1 FROM Enterprise.EmployeeProductMapping WHERE PersonaId = @PersonaId AND ProductId = @ProductId )
		BEGIN		 
			INSERT INTO Enterprise.EmployeeProductMapping ( PersonaId, ProductId, ADGroupId, CreatedDate )
			OUTPUT Inserted.EmployeeProductMappingId INTO @ResultTable
			VALUES
				( @PersonaId, @ProductId, @ADGroupId, GETUTCDATE() )
			SELECT ID, '' [ErrorMessage] FROM @ResultTable
			RETURN
		END
		ELSE
		BEGIN
			UPDATE Enterprise.EmployeeProductMapping
				SET AdGroupId = @ADGroupId,
					CreatedDate = GETUTCDATE()
			OUTPUT Inserted.EmployeeProductMappingId INTO @ResultTable
			WHERE
				PersonaId = @PersonaId AND ProductId = @ProductId
			SELECT ID, '' [ErrorMessage] FROM @ResultTable
			RETURN
		END
		SELECT 1 [ID], '' [ErrorMessage]
		RETURN
	END TRY
	BEGIN CATCH
		SELECT 0 [ID], 'There was an error adding/updating the employee product details' [ErrorMessage]
	END CATCH
END
