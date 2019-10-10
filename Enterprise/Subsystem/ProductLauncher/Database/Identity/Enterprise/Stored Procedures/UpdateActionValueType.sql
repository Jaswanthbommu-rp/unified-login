CREATE PROCEDURE [Enterprise].UpdateActionValueType
(
	@ActionValueTypeID INT,
    @ActionvalueName NVARCHAR(50),
	@Description NVARCHAR(200) NULL
)
AS
BEGIN
	IF @Description IS NULL
	BEGIN
		SELECT @Description = Description FROM Enterprise.ActionValueType where ActionValueTypeID = @ActionValueTypeID
	END

	IF @ActionValueName IS NULL
	BEGIN
		SELECT @ActionValueName = Value from Enterprise.ActionValueType Where ActionValueTypeID = @ActionValueTypeID
	END
		
	BEGIN TRY
		SET NOCOUNT ON;
		UPDATE Enterprise.ActionValueType
			SET value = @ActionvalueName,
				Description = @Description
		Where ActionValueTypeID = @ActionValueTypeID 
	END TRY
	BEGIN CATCH

		DECLARE @ErrorLogID INT;
		EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

		SELECT  0 AS Id,
				ErrorMessage
		FROM    [dbo].ErrorLog
		WHERE   ErrorLogID = @ErrorLogID;
	END CATCH
END;	
