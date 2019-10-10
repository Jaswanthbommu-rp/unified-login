CREATE PROCEDURE [Auth].[GetAllScopes]
	AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT * FROM Auth.Scopes

END
GO
