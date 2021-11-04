CREATE PROCEDURE [Security].[GetADDetailsForUser]
	@UserId BIGINT
AS
BEGIN
	SET NOCOUNT ON
	SELECT 
		UserId, 
		SamAccountName
	FROM 
		Security.ADUserDetails WHERE UserId = @UserId
END
