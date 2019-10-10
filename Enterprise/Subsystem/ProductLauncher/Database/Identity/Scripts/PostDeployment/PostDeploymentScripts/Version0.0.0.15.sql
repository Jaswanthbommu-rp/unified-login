

SELECT  @ParentActionID = ActionID , @ProductID = ProductID
    FROM ENterprise.ACTION
    WHERE ObjectValue = 'UsersList'
	   AND ObjectType = 'Route'
	   AND Description = 'SuperUser'
SELECT @ParentActionID, @ProductID


IF NOT EXISTS (SELECT 1 FROM Enterprise.ACTION WHERE ObjectValue = 'Clone User' AND ParentActionId = @parentActionid)
BEGIN
	   EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Clone User', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT
	   SELECT	@ActionID as N'@ActionID'

END

EXEC sys.sp_updateextendedproperty @name=N'Build', @value='16'