CREATE PROCEDURE [Auth].[CreateUser] (
		@Firstname nvarchar(100),
		@LastName nvarchar(100),
		@IsActive bit,
		@PasswordHash nvarchar(510),
		@IdentityProvider nvarchar(200),
		@Title nvarchar(100) = NULL,
		@Email nvarchar(100) = NULL,
		@Phone nvarchar(40) = NULL,
		@IsLocked bit
)
AS
BEGIN
	/*
		In reference to the [Auth].[User] table, the following columns are 
		Required to add a User
			LoginId
			Firstname
			LastName
			IsActive
			PasswordHash
			
		Optional
			Title
			Email
			Phone
			IsLocked
		Notes:
			IdentityProvider should be a lookup table
	*/
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	INSERT INTO Auth.Users (
		Firstname,
		LastName,
		IsActive,
		PasswordHash,
		IdentityProvider,
		Title,
		Email,
		Phone,
		IsLocked
	)
	VALUES (
		@Firstname,
		@LastName,
		@IsActive,
		@PasswordHash,
		@IdentityProvider,
		@Title,
		@Email,
		@Phone,
		@IsLocked
	)
END
GO
