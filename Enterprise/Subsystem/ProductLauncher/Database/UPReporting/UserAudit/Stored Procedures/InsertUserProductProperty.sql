CREATE PROCEDURE [UserAudit].[InsertUserProductProperty]
    @UserProductId BIGINT,
    @Items         [UserAudit].[NameIdList] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [UserAudit].[UserProperty] (UserProductId, PropertyName, PropertyId)
    SELECT @UserProductId, t.[Name], t.[Id]
    FROM   @Items AS t;
END