--DECLARE @OrgRowNum INT
--Declare @ActionID INT
--DECLARE @RightID INT
--DECLARE @RoleID INT
--DECLARE @Status INT
--DECLARE @ActionValueID INT
--DECLARE @OrgID INT
--DECLARE @ProductID INT
--DECLARE @ParentActionId INT
--DECLARE @UserActionId INT

IF OBJECT_ID('tempdb..#HoldOrgs_JRR') IS NOT NULL
    DROP TABLE #HoldOrgs_JRR

SELECT DISTINCT IDENTITY(INT, 1,1) RowNumber, OrganizationPartyID, 0 PStatus INTO #HoldOrgs_JRR FROM Person.Persona-- WHERE Person.Persona.OrganizationPartyId = 353


SELECT @Status = StatusType.StatusTypeID
FROM Enterprise.StatusTypeCategoryType
JOIN Enterprise.StatusTypeCategory ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
JOIN Enterprise.StatusTypeCategoryClassification ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
JOIN Enterprise.StatusType ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
	   WHERE StatusType.name = 'ALL'
		  AND StatusTypeCategoryType.Name = 'Security'

--SET @OrgID = 350;
WHILE EXISTS(SELECT 1 FROM #HoldOrgs_JRR WHERE PStatus = 0)
BEGIN
    SELECT TOP 1 @OrgRowNum = Rownumber, @OrgID = OrganizationPartyID 
	   FROM #HoldOrgs_JRR
         WHERE PStatus = 0

    SELECT @ActionId = A1.ActionId FROM Enterprise.Action A1
	   INNER JOIN Enterprise.Action A2
		  ON A1.ActionId = A2.ActionId
			WHERE A2.ObjectValue = 'Edit Own Profile' and A1.ParentActionId = 
			 (SELECT ActionId FROM Enterprise.Action WHERE ObjectValue = 'SideMenu' and Description  = 'User')
    
    SELECT @ProductID = ProductId FROM Enterprise.Product WHERE Name = 'Unified Login'

    


    SELECT @RoleID = RoleID FROM Enterprise.Role R inner join enterprise.RoleValueType RR ON 
		RR.RoleValueTypeId = R.RoleValueTYpeId WHere value IN ('Basic End User') AND PartyID = @OrgID
    SELECT   @RightID = RightId FROM Enterprise.[Right] R
		INNER JOIN Enterprise.RightValueType RR on RR.RightValueTypeId = R.RightValueTypeId
			 WHERE Value = 'Edit Profile' and RoleId = @RoleID
    EXEC	[Enterprise].[LinkActionToRights]  @ActionID = @ActionID, @RightId = @RightId,	@StatusId = @Status,	@UserActionId = @UserActionId OUTPUT

    UPDATE #HoldOrgs_JRR
    SET
       PStatus = 1
    WHERE RowNumber = @OrgRowNum
END

EXEC sys.sp_updateextendedproperty @name=N'Build', @value='49'