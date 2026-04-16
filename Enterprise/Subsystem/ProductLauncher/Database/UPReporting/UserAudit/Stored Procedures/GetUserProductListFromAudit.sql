CREATE PROCEDURE [UserAudit].[GetUserProductListFromAudit]
    @UserAuditId BIGINT,
    @UlDBName    NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @sql NVARCHAR(MAX);

    SET @sql = N'
        SELECT
            UP.UserProductId,
            UP.AuditUserId,
            UP.PersonaId,
            UP.ProductId,
            UP.UserName,
            UP.CreatedDate,
            UP.CompletedDate,
            UP.[Status]
        FROM [UserAudit].[UserProduct] UP
        LEFT JOIN ' + QUOTENAME(@UlDBName) + N'.[Enterprise].[PersonaConfiguration] PC
            ON  PC.ProductId = UP.ProductId
            AND PC.PersonaId = UP.PersonaId
        WHERE UP.AuditUserId = @UserAuditId;';

    EXEC sp_executesql
        @sql,
        N'@UserAuditId BIGINT',
        @UserAuditId = @UserAuditId;
END