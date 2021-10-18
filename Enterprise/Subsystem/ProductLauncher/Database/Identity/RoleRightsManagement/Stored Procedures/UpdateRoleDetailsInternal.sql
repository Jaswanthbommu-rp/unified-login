CREATE PROCEDURE [Security].[UpdateRoleDetailsInternal]
    @RoleId int,
	@RoleName nvarchar(255),
	@Description nvarchar(255),
	@ProductId int NULL,
	@RoleTypeId int NULL,
	@OrgPartyId  bigint NULL,
	@ShortName nvarchar(255) 
AS
    BEGIN
		BEGIN TRY
			SET NOCOUNT ON;
			BEGIN TRANSACTION;

			UPDATE R
			SET [RoleName] = CASE WHEN @RoleName IS NULL THEN R.RoleName ELSE LTRIM(RTRIM(@RoleName)) END,
				[Description] = CASE WHEN @Description IS NULL THEN R.[Description] ELSE LTRIM(RTRIM(@Description)) END,
				ProductId = CASE WHEN @ProductId IS NULL THEN R.[ProductId] ELSE @ProductId END,
				RoleTypeID = CASE WHEN @RoleTypeId IS NULL THEN R.RoleTypeID ELSE @RoleTypeId END,
				ShortName = CASE WHEN @ShortName IS NULL THEN R.ShortName ELSE @ShortName END,
				OrgPartyID =  @OrgPartyId
			FROM [Security].[Role] R
			WHERE R.RoleId = @RoleId

			SET NOCOUNT OFF;
			COMMIT;
			SELECT @RoleId AS RoleId
		END TRY
		BEGIN CATCH
			ROLLBACK;

			DECLARE @ErrorLogID INT;
			EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

			SELECT  0 AS Id,
					ErrorMessage
			FROM    [dbo].ErrorLog
			WHERE   ErrorLogID = @ErrorLogID;
		END CATCH
    END;