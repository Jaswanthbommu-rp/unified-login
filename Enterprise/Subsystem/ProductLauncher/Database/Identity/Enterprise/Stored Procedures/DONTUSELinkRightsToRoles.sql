CREATE PROCEDURE [Enterprise].[LinkRightsToRoles]
( 
				@ManageRight typrole READONLY,@CreatedBy nvarchar(50) NULL, @NewRightID int OUTPUT
)
AS
BEGIN
	DECLARE @RoleID int;
	DECLARE @ProductId int;
	DECLARE @RightValueTypeId int;
	DECLARE @RightId int;
	DECLARE @IsDeleted bit= 0;
	DECLARE @Description nvarchar(200);
	DECLARE @Rightname nvarchar(200);
	DECLARE @ErrorLogID int;
	DECLARE @RowNum int;
	DECLARE @DelRIghtId int;
	DECLARE @ActionId int;
	DECLARE @Status int;
	DECLARE @UserActionId int;
	DECLARE @PartyId int;
	DECLARE @DefaultRoute nvarchar(200);
	DECLARE @ActionValueID int;
	DECLARE @SchemaName varchar(25);
	
	SELECT	@SchemaName = ps.Value				
	FROM	Enterprise.GlobalProductConfiguration gpc
			JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
			JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId
			JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
	WHERE  gpc.ProductId = 3
	AND (gpc.ThruDate IS NULL)
	AND ( pc.ThruDate IS NULL)
	AND ( ps.ThruDate IS NULL)
	And PST.Name = 'RolesRightsSchemaName'

	SELECT @Status = StatusType.StatusTypeID
	FROM Enterprise.StatusTypeCategoryType
		 JOIN
		 Enterprise.StatusTypeCategory
		 ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
		 JOIN
		 Enterprise.StatusTypeCategoryClassification
		 ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
		 JOIN
		 Enterprise.StatusType
		 ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
	WHERE StatusType.name = 'ALL';
	CREATE TABLE #ProcessedList
		( 
		 RoleId int, RightValueTypeId int, RightId int, Success bit
		);
	IF
	(
		SELECT COUNT(*)
		FROM @ManageRight
	) > 0
	BEGIN
		SELECT IDENTITY(int, 1, 1) AS RowNum, RoleId, RightId, IsDeleted, 0 AS 'PStatus'
		INTO #LinkRights
		FROM @ManageRight;
	END;
	WHILE EXISTS
	(
		SELECT 1
		FROM #LinkRights
		WHERE PStatus = 0
	)
	BEGIN
		SELECT TOP 1 @RowNum = Rownum, @RoleId = RoleId, @RightValueTypeId = RightId, @IsDeleted = IsDeleted
		FROM #LinkRights
		WHERE PStatus = 0;
		
		SELECT @PartyId = PartyId
		FROM Enterprise.Role
		WHERE RoleId = @RoleId;
		
		SELECT @RightName =  Value
		FROM Enterprise.RightValueType
		WHERE RightValueTypeId = @RightValueTypeId;
		
		IF NOT EXISTS
			(
				SELECT 1
				FROM Enterprise.ACTION AS A
					 INNER JOIN
					 Enterprise.UserActions AS UA
					 ON UA.ActionId = A.ActionId
					 INNER JOIN
					 Enterprise.[Right] AS R
					 ON UA.RightId = R.RightId
					 INNER JOIN
					 Enterprise.RightValueType AS RVT
					 ON RVT.RightValueTypeId = R.RightValueTypeId
					 INNER JOIN
					 Enterprise.Role AS RR
					 ON RR.RoleId = R.RoleId
				WHERE RR.PartyId = @PartyId AND 
					  RVT.RightValueTypeId = @RightValueTypeId AND 
					  a.objectvalue LIKE 'Default%'
			)
		BEGIN
			SET @DefaultRoute = 'DefaultRouteFor_RVT_'+CONVERT(nvarchar, @RightValueTypeId) + '_' + CONVERT(nvarchar, @PartyId);
			SELECT @ActionValueID = [ActionValueTypeID]
			FROM Enterprise.ActionValueType
			WHERE Value = 'ROUTE';
			SELECT @ProductId = ProductId FROM Enterprise.RightValueType WHERE value = @RightName
			EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = @DefaultRoute, @ActionTarget = N'Route', @ActionbValueTypeId = @ActionValueID, @Description = 'DefaultRoute', @ActionID = @ActionID OUTPUT;
		END;
		ELSE
		BEGIN
			SELECT TOP 1 @DefaultRoute = A.ObjectValue
			FROM Enterprise.ACTION A
				 INNER JOIN
				 Enterprise.UserActions UA
				 ON UA.ActionId = A.ActionId
				 INNER JOIN
				 Enterprise.[Right] R
				 ON UA.RightId = R.RightId
				 INNER JOIN
				 Enterprise.RightValueType RVT
				 ON RVT.RightValueTypeId = R.RightValueTypeId
				 INNER JOIN
				 Enterprise.Role RR
				 ON RR.RoleId = R.RoleId
			WHERE RR.PartyId = @PartyId AND 
				  RVT.RightValueTypeId = @RightValueTypeId AND 
				  a.objectvalue LIKE 'Default%';
		SELECT @ActionId = A.ActionId, @ProductId = A.ProductId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RVT
			 ON RVT.RightValueTypeID = R.RightValueTypeID
			 INNER JOIN
			 Enterprise.UserActions AS UA
			 ON UA.RightId = R.RightId
			 INNER JOIN
			 Enterprise.ACTION AS A
			 ON UA.ActionId = A.ActionId
		WHERE R.PartyId = @PartyId AND 
			  A.ObjectValue = @DefaultRoute;
		END;
		
		IF(@IsDeleted = 0)
		BEGIN
			IF NOT EXISTS
			(
				SELECT 1
				FROM Enterprise.[Right]
				WHERE RoleId = @RoleId AND 
					  RightValueTypeId = @RightValueTypeId
			)
			BEGIN
				BEGIN TRY
					INSERT INTO Enterprise.[Right]( RoleID, RightValueTypeId, PartyId )
					VALUES( @RoleID, @RightValueTypeid, @PartyId );
					SELECT @NewRightID = SCOPE_IDENTITY();
					SELECT @NewRightID AS Id, '' AS ErrorMessage;
					EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @NewRightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
					INSERT INTO #ProcessedList( RoleId, RIghtValueTypeId, RightId, Success )
					VALUES( @RoleId, @RightValueTypeId, @NewRightId, 1 );
				END TRY
				BEGIN CATCH
					EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;
					SELECT 0 AS Id, ErrorMessage
					FROM dbo.ErrorLog
					WHERE ErrorLogID = @ErrorLogID;
					INSERT INTO #ProcessedList( RoleId, RIghtValueTypeId, RightId, Success )
					VALUES( @RoleId, @RightValueTypeId, 0, 0 );
				END CATCH;
			END;
		END;
		IF(@IsDeleted = 1)
		BEGIN
			IF EXISTS
				(
					SELECT 1
					FROM Enterprise.[Right]
					WHERE RoleId = @RoleId AND 
							RightValueTypeId = @RightValueTypeId
				)
			BEGIN
				BEGIN TRY
					DECLARE @DeletedRightId TABLE
					( 
						  DeletedRightId int
					);
					DELETE FROM Enterprise.UserActions
					WHERE RightId = @RightId;
					DELETE FROM Enterprise.[Right]
					OUTPUT DELETED.RightId
						   INTO @DeletedRightId
					WHERE RoleId = @RoleID AND 
						  RightValueTypeId = @RightValueTypeId;
					SELECT @DelRightId = DeletedRightId
					FROM @DeletedRightId;
					INSERT INTO #ProcessedList( RoleId, RIghtValueTypeId, RightId, Success )
					VALUES( @RoleId, @RightValueTypeId, @DelRightId, 1 );
				END TRY
				BEGIN CATCH
					EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;
					SELECT 0 AS Id, ErrorMessage
					FROM dbo.ErrorLog
					WHERE ErrorLogID = @ErrorLogID;
					INSERT INTO #ProcessedList( RoleId, RIghtValueTypeId, RightId, Success )
					VALUES( @RoleId, @RightValueTypeId, 0, 0 );
				END CATCH;
			END;
		END;
		UPDATE #LinkRights
		  SET PStatus = 1
		WHERE RowNum = @RowNum;
	END;
	IF (@SchemaName = 'Enterprise')
	BEGIN
		EXEC [Security].[LinkRightsToRoles] @ManageRight,@CreatedBy, @NewRightID OUTPUT
	END

	SELECT *
	FROM #ProcessedList;
END;