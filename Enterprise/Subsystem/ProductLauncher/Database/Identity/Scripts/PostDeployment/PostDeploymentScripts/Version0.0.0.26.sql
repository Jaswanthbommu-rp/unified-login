
SELECT @ParentActionID = ActionID,
       @ProductID = ProductID
FROM ENterprise.ACTION
WHERE ObjectValue = 'RolesAndRights'
      AND ObjectType = 'Route'
      AND Description = 'SuperUser';
SELECT @ParentActionID,
       @ProductID;
IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.ACTION
    WHERE ObjectValue = 'View Roles'
          AND ParentActionId = @parentActionid
)
    BEGIN
        EXEC [Enterprise].[CreateAction]
             @ProductID = @ProductId,
             @Action = N'View Roles',
             @ActionTarget = N'Right',
             @ActionbValueTypeId = 1,
             @Description = '',
             @ParentActionID = @ParentActionId,
             @ActionID = @ActionID OUTPUT;
        SELECT @ActionID AS N'@ActionID';
END;
IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.ACTION
    WHERE ObjectValue = 'View Rights'
          AND ParentActionId = @parentActionid
)
    BEGIN
        EXEC [Enterprise].[CreateAction]
             @ProductID = @ProductId,
             @Action = N'View Rights',
             @ActionTarget = N'Right',
             @ActionbValueTypeId = 1,
             @Description = '',
             @ParentActionID = @ParentActionId,
             @ActionID = @ActionID OUTPUT;
        SELECT @ActionID AS N'@ActionID';
END;

EXEC sys.sp_updateextendedproperty
     @name = N'Build',
     @value = '27';