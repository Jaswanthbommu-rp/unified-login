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
	DECLARE @RoleId INT
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
	SELECT @RoleId = SCOPE_IDENTITY();
	SELECT @RoleId AS Id, '' AS ErrorMessage
END;