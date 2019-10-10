CREATE PROCEDURE Enterprise.CreateAction
(
	@ParentActionID INT = NULL,
	@ProductID INT,
	@Action NVARCHAR(50),
	@ActionTarget NVARCHAR(50),
	@ActionbValueTypeId INT,
	@Description nVARCHAR(200),
	@ActionID INT OUTPUT
)
AS
BEGIN
	DECLARE @ErrorLogID INT;
	
	IF @ParentActionID IS NULL or @ParentActionId = ''
	BEGIN TRY
		INSERT INTO Enterprise.Action (ParentActionId, ProductId, ObjectValue, ObjectType, ActionvalueTypeId, Description)
			VALUES (NULL, @ProductId, @Action, @ActionTarget, @ActionbValueTypeId, @Description)
	
		SELECT @ActionID = SCOPE_IDENTITY()

		SELECT @ActionId AS ActionId, '' as ErrorMessage

	END TRY
	BEGIN CATCH

        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
	END CATCH 
	
	IF @ParentActionID IS NOT NULL or @ParentActionId <> ''
	BEGIN TRY
		INSERT INTO Enterprise.Action (ParentActionId, ProductId, ObjectValue, ObjectType, ActionvalueTypeId, Description)
			VALUES (@ParentActionID, @ProductId, @Action, @ActionTarget, @ActionbValueTypeId, @Description)
	
		SELECT @ActionID = SCOPE_IDENTITY()

		SELECT @ActionId AS ActionId, '' as ErrorMessage

	END TRY
	BEGIN CATCH
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
	END CATCH 
END
GO