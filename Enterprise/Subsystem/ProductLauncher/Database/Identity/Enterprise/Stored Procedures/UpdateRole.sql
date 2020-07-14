CREATE PROCEDURE [Enterprise].UpdateRole
(@RoleId      INT,
 @Rolename    NVARCHAR(50),
 @Description NVARCHAR(200) NULL,
 @CreatedBy nvarchar(50) NULL
)
AS
BEGIN
	DECLARE @RoleValueTypeId int;
	SELECT @RoleValueTypeId = RoleValueTypeId
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
END;