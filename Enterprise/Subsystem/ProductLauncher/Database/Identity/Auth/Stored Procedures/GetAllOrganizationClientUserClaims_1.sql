CREATE PROCEDURE [Auth].[GetAllOrganizationClientUserClaims]
	@OrganizationId  int ,
    @ClientCode NVARCHAR (200) ,
    @UserID   bigint 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--SELECT OCC.ID,OCC.OrganizationID,C.ClientID,OCC.UserID,OCC.Type,OCC.Value
	--FROM Auth.OrganizationClientUserClaims OCC INNER JOIN Auth.Clients C ON C.ClientId = OCC.ClientId
	--WHERE OCC.OrganizationID = @OrganizationID AND C.ClientCode = @ClientCode AND OCC.UserID = @UserID

END