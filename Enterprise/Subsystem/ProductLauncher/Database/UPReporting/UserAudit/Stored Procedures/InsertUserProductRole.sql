CREATE PROCEDURE [UserAudit].[InsertUserProductRole]
    @UserProductId BIGINT,
    @Items         [UserAudit].[NameIdList] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [UserAudit].[UserRole] (UserProductId, RoleName)
    SELECT @UserProductId, t.[Name]
    FROM   @Items AS t;
END