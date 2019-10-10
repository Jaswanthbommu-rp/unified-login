
IF OBJECT_ID('tempdb..#HoldOrgs2') IS NOT NULL
    DROP TABLE #HoldOrgs2

SELECT DISTINCT IDENTITY(INT, 1,1) RowNumber, OrganizationPartyID, 0 PStatus INTO #HoldOrgs2 FROM Person.Persona-- WHERE Person.Persona.OrganizationPartyId = 353


SELECT @Status = StatusType.StatusTypeID
FROM Enterprise.StatusTypeCategoryType
JOIN Enterprise.StatusTypeCategory ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
JOIN Enterprise.StatusTypeCategoryClassification ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
JOIN Enterprise.StatusType ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
	   WHERE StatusType.name = 'ALL'
		  AND StatusTypeCategoryType.Name = 'Security'

--SET @OrgID = 350;
WHILE EXISTS(SELECT 1 FROM #HoldOrgs2 WHERE PStatus = 0)
BEGIN
    SELECT TOP 1 @OrgRowNum = Rownumber, @OrgID = OrganizationPartyID 
	   FROM #HoldOrgs2 
         WHERE PStatus = 0


    SELECT @ActionValueID = [ActionValueTypeID] FROM Enterprise.ActionValueType WHERE Value = 'ROUTE'
    SELECT @ProductID = ProductId FROM Enterprise.Product WHERE Name = 'Unified Login'

    IF NOT EXISTS (SELECT 1 FROM Enterprise.Action WHERE ObjectValue = 'EditUser'and Description = 'User')
    BEGIN
		  EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'EditUser', @ActionTarget = N'Route', @ActionbValueTypeId = @ActionValueID, @Description = 'User', @ActionID = @ActionID OUTPUT
		  SELECT	@ActionID as N'@ActionID'
    END

    SELECT @ParentActionId = ActionID  FROM Enterprise.Action Where ObjectValue = 'EditUser' and ParentActionID is NULL and Description = 'User'
    IF NOT EXISTS (SELECT 1 FROM Enterprise.Action WHERE ObjectValue = 'Edit Own Profile' and ParentActionID = @ParentActionId)
    BEGIN
			 EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Edit Own Profile', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT
			 SELECT	@ActionID as N'@ActionID'
    END



    SELECT @ActionID = ActionID FROM Enterprise.Action WHERE ObjectValue = 'EditUser' and ObjectType = 'ROUTE' and Description = 'User'
    SELECT @RoleID = RoleID FROM Enterprise.Role R inner join enterprise.RoleValueType RR ON 
		RR.RoleValueTypeId = R.RoleValueTYpeId WHere value IN ('Basic End User') AND PartyID = @OrgID
    SELECT @RightID = RightId FROM Enterprise.[Right] R
		INNER JOIN Enterprise.RightValueType RR on RR.RightValueTypeId = R.RightValueTypeId
			 WHERE Value = 'Ability to edit my own profile' and RoleId = @RoleID
    EXEC	[Enterprise].[LinkActionToRights]  @ActionID = @ActionID, @RightId = @RightId,	@StatusId = @Status,	@UserActionId = @UserActionId OUTPUT

    UPDATE #HoldOrgs2
    SET
       #HoldOrgs2.PStatus = 1
    WHERE RowNumber = @OrgRowNum
END

EXEC sys.sp_updateextendedproperty @name=N'Build', @value='50'