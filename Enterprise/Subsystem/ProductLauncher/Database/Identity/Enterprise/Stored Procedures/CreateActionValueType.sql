CREATE PROCEDURE Enterprise.CreateActionValueType
(
	@ActionValueName NVARCHAR(50),
	@Description NVARCHAR(200) NULL,
	@ActionValueTypeId INT OUTPUT
)
AS
BEGIN
	IF NOT EXISTS (SELECT 1 FROM Enterprise.ActionValueType WHERE Value = @ActionValueName)
	BEGIN TRY
		INSERT INTO Enterprise.ActionValueType(Value, Description)
			VALUES (@ActionValueName, @Description)
		
		SELECT @ActionValueTypeId = SCOPE_IDENTITY()
		
		SELECT @ActionValueTypeId AS ActionValueTypeId, '' as ErrorMessage
	END TRY
	BEGIN CATCH
		DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
	END CATCH
	ELSE 
		BEGIN
			SELECT 'Action Value already exists.'
		END
END
GO