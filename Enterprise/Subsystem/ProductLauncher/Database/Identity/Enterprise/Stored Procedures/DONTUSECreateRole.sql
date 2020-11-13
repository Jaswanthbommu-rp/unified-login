CREATE PROCEDURE [Enterprise].[CreateRole]
(@RoleName       NVARCHAR(50),
 @ShortName      NVARCHAR(50)  = NULL,
 @Description    NVARCHAR(200),
 @RoleTypeID     INT,
 @RoleCategoryId INT,
 @PartyID        INT,
 @CreatedBy nvarchar(50) NULL,
 @RoleID         INT OUTPUT
)
AS;
BEGIN
	DECLARE @RoleValueTypeId int;
	DECLARE @ErrorLogID int;
	DECLARE @DefaultRightValueTypeId1 int;
	DECLARE @DefaultRightValueTypeId2 int;
	DECLARE @RightId int;
	DECLARE @ActionId1 int;
	DECLARE @ActionId2 int;
	DECLARE @Status int;
	DECLARE @UserActionId int;
	DECLARE @SchemaName varchar(25);
	DECLARE @EnterpriseRoleID INT;

	SET @RoleId = 0
	SELECT @RoleId = ISNULL(RoleId,0)
	FROM ENterprise.Role R
			INNER JOIN
			Enterprise.RoleValueType RVT
			ON RVT.RoleValueTypeId = R.RoleValuetypeId
	WHERE RVT.Value = @RoleName AND 
			R.PartyId = @PartyId;
	IF (@RoleId > 0)
	BEGIN
		SELECT @RoleId AS RoleID, 'Role already exists under Organization.' as ErrorMessage;
		RETURN;
	END

	IF @RoleName IN ('SystemRole', 'SystemRight')
	BEGIN
		SELECT 0 AS RoleID, 'Role with this name cannot be created.' as ErrorMessage;
		RETURN;
	END

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
	
	BEGIN TRY
		IF Exists(SELECT 1 RoleValueTypeId
				FROM Enterprise.RoleValueType
				WHERE Value = @RoleName AND @RoleCategoryId = 13)
			BEGIN
				SELECT @RoleValueTypeID = RoleValueTypeId
				FROM Enterprise.RoleValueType
				WHERE Value = @RoleName;
			END
		ELSE
			BEGIN
				INSERT INTO Enterprise.RoleValueType( Value, ShortName, Description, StatusTypeId )
				VALUES( @RoleName, @ShortName, @Description, @RoleCategoryId );
				SELECT @RoleValueTypeId = SCOPE_IDENTITY();
			END

		INSERT INTO Enterprise.Role( RoleTypeID, RoleValueTypeId, PartyID )
		VALUES( @RoleTypeID, @RoleValueTypeId, @PartyID );
		SELECT @RoleID = SCOPE_IDENTITY();

		SELECT @RoleID AS RoleID, '' AS ErrorMessage;
	END TRY
	BEGIN CATCH
		EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;
		SELECT 0 AS RoleID, ErrorMessage
		FROM dbo.ErrorLog
		WHERE ErrorLogID = @ErrorLogID;
	END CATCH;	
	    
/*Assign Default Right*/

	IF
(
	SELECT ST.StatusTypeId
	FROM Enterprise.StatusTypeCategoryType AS STCT
		 JOIN
		 Enterprise.StatusTypeCategory AS STC
		 ON STC.StatusTypeCategoryTypeId = STCT.StatusTypeCategoryTypeId
		 JOIN
		 Enterprise.StatusTypeCategoryClassification AS STCC
		 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
		 JOIN
		 Enterprise.StatusType AS ST
		 ON ST.StatusTypeId = STCC.StatusTypeId
	WHERE STCT.name = 'Security' AND 
		  STC.Name = 'Role Type' AND 
		  ST.Name = 'Custom'
) = @RoleCategoryId
	BEGIN
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
		WHERE StatusType.name = 'ALL' AND 
			  StatusTypeCategoryType.Name = 'Security';
		SELECT DISTINCT 
			   @DefaultRightValueTypeId1 = RVT.RightValueTypeID
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RVT
			 ON RVT.RightValueTypeID = R.RightValueTypeID
		WHERE R.PartyId = @PartyId AND 
			  RVT.Value = 'Default_Dashboard_Users';
		SELECT DISTINCT 
			   @DefaultRightValueTypeId2 = RVT.RightValueTypeID
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RVT
			 ON RVT.RightValueTypeID = R.RightValueTypeID
		WHERE R.PartyId = @PartyId AND 
			  RVT.Value = 'Default_SideMenu_Users';
		SELECT @ActionId1 = ActionId
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'DashBoard' AND 
			  Description = 'User';
		SELECT @ActionId2 = ActionId
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'SideMenu' AND 
			  Description = 'User';
	IF NOT EXISTS
	(
		SELECT 1
		FROM Enterprise.[Right]
		WHERE RoleId = @RoleId AND 
		  PartyId = @PartyId AND 
		  RightValueTypeID IN( @DefaultRightValueTypeId1, @DefaultRightValueTypeId2 )
	)
		BEGIN
			INSERT INTO Enterprise.[Right]( RoleId, PartyId, RightValueTypeId )
			VALUES( @RoleId, @PartyId, @DefaultRightValueTypeId1 );
			SELECT @RightId = SCOPE_IDENTITY();
			EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID1, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
			INSERT INTO Enterprise.[Right]( RoleId, PartyId, RightValueTypeId )
			VALUES( @RoleId, @PartyId, @DefaultRightValueTypeId2 );
			SELECT @RightId = SCOPE_IDENTITY();
			EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID2, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
		END;
	END;
	IF (@SchemaName = 'Enterprise')
	BEGIN
		EXEC [Security].[CreateRole] @RoleName, @ShortName, @Description, @RoleTypeID, @RoleCategoryId, @PartyID, @CreatedBy, @EnterpriseRoleID OUTPUT
	END
END;