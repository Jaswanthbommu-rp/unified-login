CREATE PROCEDURE [Enterprise].[InsertProductLoginActivitybyUser]
(    
	@ProductId INT,
	@PersonaId BIGINT,
	@ImpersonatorUserId BIGINT
)     
AS    
BEGIN
	BEGIN TRY
		DECLARE @Now DATETIME = GETUTCDATE();
		IF EXISTS(SELECT 1 FROM [Enterprise].[ProductLoginUserActivitySummary] WHERE ProductId = @ProductId AND PersonaId = @PersonaId)
		BEGIN
			UPDATE [Enterprise].[ProductLoginUserActivitySummary]
			SET [LoginDate] = @Now, ImpersonatorUserId = @ImpersonatorUserId
			WHERE ProductId = @ProductId AND PersonaId = @PersonaId
		END
		ELSE
		BEGIN
			INSERT INTO [Enterprise].[ProductLoginUserActivitySummary](ProductId,PersonaId,ImpersonatorUserId,[LoginDate])      
			VALUES (@ProductId,@PersonaId,@ImpersonatorUserId,@Now)
		END

		--Insert only to History table
		INSERT INTO [Enterprise].[ProductLoginActivitybyUser](ProductId,PersonaId,ImpersonatorUserId,CreateDate)    
		VALUES (@ProductId,@PersonaId,@ImpersonatorUserId,@Now)

	END TRY
	BEGIN CATCH    
		DECLARE @ErrorLogID INT;
		EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;    
		SELECT 0 AS Id, ErrorMessage  FROM dbo.ErrorLog WHERE ErrorLogID = @ErrorLogID;
	END CATCH   
END;