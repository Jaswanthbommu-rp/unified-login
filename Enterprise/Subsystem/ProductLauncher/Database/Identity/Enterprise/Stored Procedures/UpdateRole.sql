CREATE PROCEDURE [Enterprise].UpdateRole
(@RoleId      INT,
 @Rolename    NVARCHAR(50),
 @Description NVARCHAR(200) NULL,
 @CreatedBy nvarchar(50) NULL
)
AS
BEGIN
	DECLARE @RoleValueTypeId int;
	DECLARE @SchemaName varchar(25);
	Declare @OrgPartyId INT,@SecurityRoleId INT

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

	SELECT @RoleValueTypeId = RoleValueTypeId,@OrgPartyId = PartyID
	FROM Enterprise.Role
	WHERE RoleId = @RoleId;
	BEGIN TRY
		BEGIN
			SET NOCOUNT ON;
			IF EXISTS
			(
				SELECT 1
				FROM Enterprise.RoleValueType
				WHERE Value = LTRIM(RTRIM(@RoleName))

			)
			BEGIN
				SELECT @RoleId AS RoleId, '' AS ErrorMessage;
			END;
			ELSE
			BEGIN
				UPDATE Enterprise.RoleValueType
				  SET value = @Rolename, Description = @Description
				WHERE RoleValueTypeId = @RoleValueTypeId;
				SELECT @RoleId AS ID, '' AS ErrorMessage;
			END;
			
		END;
	END TRY
	BEGIN CATCH
		DECLARE @ErrorLogID int;
		EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;
		SELECT 0 AS Id, ErrorMessage
		FROM dbo.ErrorLog
		WHERE ErrorLogID = @ErrorLogID;
	END CATCH;
	--update from security schema 
		IF (@SchemaName = 'Enterprise')
		BEGIN
			
			Select @SecurityRoleId = R.RoleID From Security.Role R 							
			Where R.RoleName = @Rolename
			AND R.OrgPartyID = @OrgPartyId

			EXEC [Security].[UpdateRole] @SecurityRoleId,@Rolename,@Description,@CreatedBy
		END
END;