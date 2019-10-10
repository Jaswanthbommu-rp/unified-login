--Add Edit My Own Profile Right to Basic user


SELECT @ParentActionID = ActionID,
       @ProductID = ProductID
FROM ENterprise.ACTION
WHERE ObjectValue = 'Sidemenu'
      AND ObjectType = 'Route'
      AND Description = 'User';
SELECT @ParentActionID,
       @ProductID;
IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.ACTION
    WHERE ObjectValue = 'Edit Own Profile'
          AND ParentActionId = @parentActionid
)
    BEGIN
        EXEC [Enterprise].[CreateAction]
             @ProductID = @ProductId,
             @Action = N'Edit Own Profile',
             @ActionTarget = N'Right',
             @ActionbValueTypeId = 1,
             @Description = '',
             @ParentActionID = @ParentActionId,
             @ActionID = @ActionID OUTPUT;
        SELECT @ActionID AS N'@ActionID';
    END;

    EXEC sys.sp_updateextendedproperty @name=N'Build', @value='29'