CREATE PROCEDURE [Auth].[GetAllScopeSecrets]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT * FROM Auth.ScopeSecrets

END
GO