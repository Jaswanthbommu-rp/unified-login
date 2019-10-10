CREATE PROCEDURE [Auth].[GetClientByClientCode]
	@ClientCode nvarchar(200)
AS
BEGIN
	SET NOCOUNT ON;
	SELECT * FROM Auth.Clients WHERE ClientCode = @ClientCode
END
GO
