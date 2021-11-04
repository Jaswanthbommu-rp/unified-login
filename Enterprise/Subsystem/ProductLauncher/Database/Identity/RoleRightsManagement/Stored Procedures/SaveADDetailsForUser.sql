CREATE PROCEDURE [Security].[SaveADDetailsForUser]
	@UserId BIGINT,
	@SamAccountName VARCHAR(200) 
AS
BEGIN
	SET NOCOUNT ON
	BEGIN TRY  
		IF EXISTS (SELECT TOP (1) 1 FROM Security.ADUserDetails WHERE UserId = @UserId )
		BEGIN
			UPDATE Security.ADUserDetails SET SamAccountName = @SamAccountName WHERE UserId = @UserId AND SamAccountName <> @SamAccountName
		END
		ELSE
		BEGIN
			INSERT INTO Security.ADUserDetails ( UserId, SamAccountName ) VALUES ( @UserId, @SamAccountName )
		END
		SELECT 1
		RETURN 1
	END TRY
	BEGIN CATCH
		SELECT 0
		RETURN 0
	END CATCH
END