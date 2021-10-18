CREATE PROCEDURE [Security].[AddRoleDetailsInternal]
	@RoleName nvarchar(255),
	@Description nvarchar(255),
	@ProductId int,
	@RoleTypeId int,
	@OrgPartyId  bigint NULL,
	@ShortName nvarchar(255),
	@CreatedBy nvarchar(255)
AS
    BEGIN
		BEGIN TRY
		BEGIN TRANSACTION;
			INSERT INTO [Security].[Role]
					(RoleName,
					 [Description],
					 ProductId,
					 RoleTypeID,
					 OrgPartyID,
					 ShortName,
					 CreatedBy,
					 CreatedDate
					)
			     VALUES
					(@RoleName,
					 @Description,
					 @ProductId,
					 @RoleTypeId,
					 @OrgPartyId,
					 @ShortName,
					 @CreatedBy,
					 GETUTCDATE()
					);
		COMMIT;
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