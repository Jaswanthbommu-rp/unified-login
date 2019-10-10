CREATE PROCEDURE [Enterprise].[CreateRight]
(@RoleID          INT          ,
 @RightName       NVARCHAR(200),
 @ShortName	 NVARCHAR(50) = NULL, 
 @RightCategoryId INT,
 @VisibilityStatusId INT = NULL,
 @PartyId         INT,
 @ProductId       INT,
 @TargetProductId INT = NULL, 
 @Description     NVARCHAR(200),
 @RightID         INT OUTPUT
)
AS
     BEGIN
         DECLARE @RightValueTypeId INT;
         DECLARE @ErrorLogID INT;
         DECLARE @DefaultRoute NVARCHAR(50);
         DECLARE @ActionValueID INT;
         DECLARE @ActionID INT;
         DECLARE @Status INT;
         DECLARE @UserActionId INT;
         IF NOT EXISTS
         (
             SELECT 1
             FROM Enterprise.RightValueType
             WHERE Value = @Rightname
         )
             BEGIN TRY
                 INSERT INTO Enterprise.RightValueType
                 (Value,
				  ShortName, 
                  Description,
                  StatusTypeId,
				  ProductId,
				  TargetProductId,
				  VisibilityStatusId

                 )
                 VALUES
                 (@RightName,
			      @ShortName,
                  @Description,
                  @RightCategoryId,
				  @ProductId,
				  @TargetProductId,
				  @VisibilityStatusId
                 );
                 SELECT @RightValueTypeId = SCOPE_IDENTITY();
         END TRY
             BEGIN CATCH
                 EXEC dbo.LogError
                      @ErrorLogID = @ErrorLogID OUTPUT;
                 SELECT 0 AS Id,
                        ErrorMessage
                 FROM dbo.ErrorLog
                 WHERE ErrorLogID = @ErrorLogID;
         END CATCH;
             ELSE
             BEGIN
                 SELECT @RightValueTypeID = RightValueTypeId
                 FROM Enterprise.RightValueType
                 WHERE Value = @RightName;
         END;
         IF NOT EXISTS
         (
             SELECT 1
             FROM Enterprise.[Right] R
                  INNER JOIN EnterPrise.RightValueType RV ON R.RightValueTypeId = RV.RightValueTypeId
             WHERE RV.Value = @RightName
                   AND R.RoleID = @RoleID
                   AND R.PartyID = @PartyId
         )
             BEGIN TRY
                 INSERT INTO Enterprise.[Right]
                 (RoleID,
                  RightValueTypeId,
                  PartyId
                 )
                 VALUES
                 (@RoleID,
                  @RightValueTypeID,
                  @PartyId
                 );
                 SELECT @RightID = SCOPE_IDENTITY();
                 SELECT @RightID AS RightID,
                        '' AS ErrorMessage;
         END TRY
             BEGIN CATCH
                 EXEC dbo.LogError
                      @ErrorLogID = @ErrorLogID OUTPUT;
                 SELECT 0 AS Id,
                        ErrorMessage
                 FROM dbo.ErrorLog
                 WHERE ErrorLogID = @ErrorLogID;
         END CATCH;
             ELSE
             BEGIN
                 SELECT @RightId = R.RightId
             FROM Enterprise.[Right] R
                  INNER JOIN EnterPrise.RightValueType RV ON R.RightValueTypeId = RV.RightValueTypeId
					WHERE RV.Value = @RightName
                   AND R.RoleID = @RoleID
                   AND R.PartyID = @PartyId
				 SELECT 'Right already exists.';
         END;
         --In case role name is not provided, create a new route for the role.
           SET @DefaultRoute = 'DefaultRouteFor_RVT_'+ CONVERT(NVARCHAR, @RightValueTypeId) + '_' + CONVERT(NVARCHAR, @RightId) + '_' + CONVERT(NVARCHAR, @PartyId);
		 IF NOT EXISTS (SELECT 1 FROM Enterprise.Action WHERE ObjectValue = @DefaultRoute)
             BEGIN
                 SELECT @ActionValueID = [ActionValueTypeID]
                 FROM Enterprise.ActionValueType
                 WHERE Value = 'ROUTE';
                 EXEC [Enterprise].[CreateAction]
                      @ProductID = @ProductId,
                      @Action = @DefaultRoute,
                      @ActionTarget = N'Route',
                      @ActionbValueTypeId = @ActionValueID,
                      @Description = 'DefaultRoute',
                      @ActionID = @ActionID OUTPUT;
                 SELECT @ActionID AS N'@ActionID';
                 -- Link newly created right and action to complete the chain.
			  SELECT @Status = StatusType.StatusTypeID
                 FROM Enterprise.StatusTypeCategoryType
                      JOIN Enterprise.StatusTypeCategory ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
                      JOIN Enterprise.StatusTypeCategoryClassification ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
                      JOIN Enterprise.StatusType ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
                 WHERE StatusType.name = 'ALL'
                       AND StatusTypeCategoryType.Name = 'Security';
                 EXEC [Enterprise].[LinkActionToRights]
                      @ActionID = @ActionID,
                      @RightId = @RightId,
                      @StatusId = @Status,
                      @UserActionId = @UserActionId OUTPUT;
         END;
     END;