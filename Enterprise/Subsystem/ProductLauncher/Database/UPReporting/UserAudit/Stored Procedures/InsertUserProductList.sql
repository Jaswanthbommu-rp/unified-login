CREATE PROCEDURE [UserAudit].[InsertUserProductList]
    @AuditUserId  BIGINT,
    @UserProducts [UserAudit].[UserProductInfo] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [UserAudit].[UserProduct] (AuditUserId, PersonaId, ProductId, UserName)
    SELECT @AuditUserId, t.PersonaId, t.ProductId, t.UserName
    FROM   @UserProducts AS t
    WHERE  NOT EXISTS (
        SELECT 1
        FROM   [UserAudit].[UserProduct] up
        WHERE  up.AuditUserId = @AuditUserId
          AND  up.PersonaId   = t.PersonaId
          AND  up.ProductId   = t.ProductId
    );
END