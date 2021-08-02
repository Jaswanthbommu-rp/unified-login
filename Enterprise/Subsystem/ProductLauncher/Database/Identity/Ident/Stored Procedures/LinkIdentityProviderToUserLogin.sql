CREATE PROCEDURE [Ident].[LinkIdentityProviderToUserLogin]
(
	@UserId BIGINT,
	@ContactMechanismID INT
)
AS
BEGIN
	DECLARE @ErrorLogID INT;
	BEGIN TRY

		DECLARE @IdentityProviderTypeId INT
		SELECT @IdentityProviderTypeId = IdentityProviderTypeID from Ident.IdentityProviderType
			WHERE ContactMechanismId = @ContactMechanismId

		-- SEE IF THE USER BEING UPDATED IS A SUPPORT TOOL USER AND IF IT IS FORCE TO AZURE
		IF EXISTS ( SELECT TOP(1) 1 FROM 
			Ident.UserLogin UL
			INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId
			INNER JOIN Enterprise.OrganizationAdminUser OAU ON OAU.UserLoginPersonaId = ULP.UserLoginPersonaId
		WHERE
			UL.UserId = @UserId
		)
		BEGIN
			SELECT @IdentityProviderTypeId = IdentityProviderTypeID from Ident.IdentityProviderType
			WHERE [Name] = 'AzureActiveDirectory'
		END

		UPDATE Ident.UserLogin
			SET IdentityProviderTypeId = @IdentityProviderTypeId
		WHERE UserId = @UserId
		
		
		SELECT	@UserId AS Id ,
						'' AS ErrorMessage
		END TRY
		BEGIN CATCH
			EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

			SELECT  0 AS Id,
					ErrorMessage
			FROM    dbo.ErrorLog
			WHERE   ErrorLogID = @ErrorLogID;
		END CATCH
END